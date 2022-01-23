using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Loader;
using System.Threading;
using System.Timers;
using Microsoft.AspNetCore.Server.HttpSys;
using Microsoft.Extensions.Options;
using NLog.LayoutRenderers;
using OneClickDesktop.BackendClasses.Communication.RabbitDTOs;
using OneClickDesktop.BackendClasses.Model;
using OneClickDesktop.BackendClasses.Model.States;
using OneClickDesktop.Overseer.Helpers.Settings;
using OneClickDesktop.Overseer.Messages;
using OneClickDesktop.Overseer.Services.Interfaces;
using Timer = System.Timers.Timer;

namespace OneClickDesktop.Overseer.Services.Classes
{
    public class SessionProcessService : ISessionProcessService, IDisposable
    {
        private readonly IOptions<OneClickDesktopSettings> ocdConfiguration;
        
        private readonly ISystemModelService modelService;
        private readonly IVirtualizationServerConnectionService connectionService;

        private readonly CancellationTokenSource tokenSrc = new CancellationTokenSource();
        private CancellationToken token;
        private Thread sessionSearchThread = null;
        private Thread sessionDeleteThread = null;

        private ConcurrentDictionary<Guid, IList<(Guid session, string machine)>> waitingForServerChange =
            new ConcurrentDictionary<Guid, IList<(Guid session, string machine)>>();

        private BlockingCollection<Guid> changes = new BlockingCollection<Guid>();

        private BlockingCollection<(string, string, MachineState)> changedMachines =
            new BlockingCollection<(string, string, MachineState)>();

        public SessionProcessService(ISystemModelService modelService,
            IVirtualizationServerConnectionService connectionService,
            IOptions<OneClickDesktopSettings> ocdConfiguration)
        {
            this.ocdConfiguration = ocdConfiguration;
            this.modelService = modelService;
            this.connectionService = connectionService;
            token = tokenSrc.Token;

            sessionSearchThread = new Thread(SessionSearch);
            sessionDeleteThread = new Thread(SessionDelete);

            modelService.ServerUpdated += OnServerUpdated;

            sessionSearchThread.Start();
            sessionDeleteThread.Start();
        }

        public void StartSessionSearchProcess(Session session)
        {
            // look for machine for session
            CheckSession(session.SessionGuid, session, null);
        }

        private void OnServerUpdated(object sender, Guid serverGuid)
        {
            changes.Add(serverGuid);

            //dodaj maszyny, ktore zmienily stan w aktualizacji do kolekcji
            //Watek badajacy czas do wylaczenia zadecyduje co z nimi zrobic
            //Potrzeba tylko guida zmienionej maszyny, nazwy kolejki servera oraz stanu jaki byl przy zmianie
            foreach (Machine m in modelService.GetMachinesFromServer(serverGuid))
                changedMachines.Add((m.Name, m.ParentServer.Queue, m.State));
        }

        private void SessionSearch()
        {
            while (true)
            {
                Guid serverGuid;
                try
                {
                    serverGuid = changes.Take(token);
                }
                catch (OperationCanceledException e)
                {
                    return;
                }

                // check if sessions waiting for machine on this server
                if (!waitingForServerChange.TryRemove(serverGuid, out var waiting)) continue;

                foreach (var item in waiting)
                {
                    var session = modelService.GetSession(item.session);
                    var machine = modelService.GetMachine(serverGuid, item.machine);
                    CheckSession(item.session, session, machine);
                }
            }
        }

        private void CheckSession(Guid sessionGuid, Session session, Machine machineCrush)
        {
            if (session?.SessionState is SessionState.Running or SessionState.Cancelled)
            {
                return;
            }

            // was waiting for machine, and it didnt change
            if (machineCrush != null && machineCrush.State == MachineState.Free)
            {
                AddToList(sessionGuid, machineCrush);
                return;
            }

            //Session is not already propagated but model update has come
            if (session == null)
                return;

            var machine = modelService.GetMachineForSession(session);
            if (machine == null)
            {
                modelService.CancelSession(session);
                return;
            }

            AddToList(session.SessionGuid, machine);

            connectionService.SendRequest(
                new SessionCreationMessage(new SessionCreationRDTO()
                {
                    DomainName = machine.Name,
                    PartialSession = session
                }),
                machine.ParentServer.Queue
            );
        }

        private void AddToList(Guid sessionGuid, Machine machine)
        {
            var serverGuid = machine.ParentServer.ServerGuid;
            var item = (sessionGuid, machine.Name);
            if (waitingForServerChange.ContainsKey(serverGuid))
            {
                waitingForServerChange[serverGuid].Add(item);
                return;
            }

            waitingForServerChange[serverGuid] = new List<(Guid session, string machine)>()
            {
                item
            };
        }

        private class ShutdownCounter
        {
            public string MachineName;
            public string VirtSrvQueuename;
            public uint Counter;

            public ShutdownCounter(string machineName, string virtSrvQueueName)
            {
                Counter = 0;
                MachineName = machineName;
                VirtSrvQueuename = virtSrvQueueName;
            }

            /// <summary>
            /// Tell if counter is over waiting threshold
            /// </summary>
            /// <param name="intervalMs">Interval of incrementation</param>
            /// <param name="maxTimeMs">Waiting threshold</param>
            /// <returns>True - timer is expired</returns>
            public bool IsExpired(ulong intervalMs, ulong maxTimeMs)
            {
                return intervalMs * Counter >= maxTimeMs;
            }

            public override bool Equals(object obj)
            {
                if (!(obj is ShutdownCounter))
                    return false;

                return ((ShutdownCounter) obj).MachineName == MachineName;
            }

            public override int GetHashCode()
            {
                return MachineName.GetHashCode();
            }
        }

        private void SessionDelete()
        {
            //co jakis czas sprawdz, czy sesja + maszyna do smieci (to juz jest watek)
            HashSet<ShutdownCounter> shutdownCounters = new HashSet<ShutdownCounter>();
            object counterMutex = new object();

            System.Timers.Timer shutdownCounterTimer = new System.Timers.Timer();
            shutdownCounterTimer.Enabled = true;
            shutdownCounterTimer.Interval = ocdConfiguration.Value.DomainShutdownCounterInterval * 1000;
            shutdownCounterTimer.AutoReset = true;

            //Zwiekszanie liczników dla maszyn w wątku timera
            shutdownCounterTimer.Elapsed += (sender, args) =>
            {
                lock (counterMutex)
                {
                    if (shutdownCounters.Count == 0)
                        return;

                    foreach (ShutdownCounter c in shutdownCounters)
                        c.Counter++;

                    //Jeżeli jakies maszyny przekrocza dozwolony ca soczekiwania zgłoś je do wyłaczenia
                    var toShutdown =
                        shutdownCounters.Where(c =>
                            c.IsExpired((ulong)ocdConfiguration.Value.DomainShutdownCounterInterval * 1000,//seconds -> miliseconds
                                (ulong)ocdConfiguration.Value.DomainShutdownTimeout * 60 * 1000));//minutes -> miliseconds
                    if (toShutdown?.Count() > 0)
                    {
                        foreach (var machineInfo in toShutdown)
                        {
                            connectionService.SendRequest(
                                new DomainShutdownMessage(
                                    new DomainShutdownRDTO()
                                    {
                                        DomainName = machineInfo.MachineName
                                    }),
                                machineInfo.VirtSrvQueuename
                            );

                            shutdownCounters.Remove(machineInfo);
                        }
                    }
                }
            };

            shutdownCounterTimer.Start();

            while (true)
            {
                string machineName;
                string queueName;
                MachineState state;
                try
                {
                    (machineName, queueName, state) = changedMachines.Take(token);
                }
                catch (OperationCanceledException e)
                {
                    shutdownCounterTimer.Enabled = false;
                    shutdownCounterTimer.Stop();
                    return;
                }

                lock (counterMutex)
                {
                    //Dodaj maszyny oczekujące na zamknięcie jeżeli juz jeszcze jej nie ma
                    if (state == MachineState.WaitingForShutdown)
                    {
                        ShutdownCounter counter = new ShutdownCounter(machineName, queueName);
                        if (!shutdownCounters.Contains(counter))
                            shutdownCounters.Add(counter);
                    }

                    //Znanjdz maszyny, ktore juz oczekiwaly na zamkniecie i ponownie ktos sie do nich podlaczyl
                    if (state != MachineState.WaitingForShutdown)
                    {
                        ShutdownCounter counter = new ShutdownCounter(machineName, queueName);
                        if (shutdownCounters.Contains(counter))
                            shutdownCounters.Remove(counter);
                    }
                }
            }
        }

        public void Dispose()
        {
            if (tokenSrc != null)
            {
                tokenSrc.Cancel();
                sessionSearchThread?.Join();
                sessionDeleteThread?.Join();
            }
        }
    }
}