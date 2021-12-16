using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using OneClickDesktop.BackendClasses.Communication.RabbitDTOs;
using OneClickDesktop.BackendClasses.Model;
using OneClickDesktop.BackendClasses.Model.States;
using OneClickDesktop.Overseer.Messages;
using OneClickDesktop.Overseer.Services.Interfaces;

namespace OneClickDesktop.Overseer.Services.Classes
{
    public class SessionProcessService : ISessionProcessService, IDisposable
    {
        private readonly ISystemModelService modelService;
        private readonly IVirtualizationServerConnectionService connectionService;

        private readonly CancellationTokenSource tokenSrc = new CancellationTokenSource();
        private CancellationToken token;
        private Thread sessionSearchThread = null;
        private Thread sessionCancelThread = null;

        private ConcurrentDictionary<Guid, IList<(Guid session, string machine)>> waitingForServerChange =
            new ConcurrentDictionary<Guid, IList<(Guid session, string machine)>>();

        private BlockingCollection<Guid> changes = new BlockingCollection<Guid>();

        public SessionProcessService(ISystemModelService modelService,
                                     IVirtualizationServerConnectionService connectionService)
        {
            this.modelService = modelService;
            this.connectionService = connectionService;
            token = tokenSrc.Token;

            sessionSearchThread = new Thread(SessionSearch);
            sessionCancelThread = new Thread(SessionCancel);

            modelService.ServerUpdated += OnServerUpdated;

            sessionSearchThread.Start();
            //sessionCancelThread.Start();
        }

        public void StartSessionSearchProcess(Session session)
        {
            CheckSession(session, null);
        }

        private void OnServerUpdated(object sender, Guid serverGuid)
        {
            changes.Add(serverGuid);
            // TODO: detect session waiting for delete and start process
        }

        private void SessionSearch()
        {
            while (true)
            {
                if (token.IsCancellationRequested)
                    return;

                // wait for change
                var serverGuid = changes.Take();

                if (!waitingForServerChange.TryRemove(serverGuid, out var waiting)) continue;

                foreach (var item in waiting)
                {
                    var session = modelService.GetSession(item.session);
                    var machine = modelService.GetMachine(serverGuid, item.machine);
                    CheckSession(session, machine);
                }
            }
        }

        private void CheckSession(Session session, Machine machineCrush)
        {
            if (session.SessionState == SessionState.Running)
            {
                return;
            }

            // was waiting for machine, and it didnt change
            if (machineCrush != null && machineCrush.State == MachineState.Free)
            {
                AddToList(session, machineCrush);
            }

            var machine = modelService.GetMachineForSession(session);
            if (machine == null)
            {
                modelService.CancelSession(session);
                // TODO: send servers info about session cancellation
                return;
            }

            AddToList(session, machine);

            connectionService.SendRequest(
                new SessionCreationMessage(new SessionCreationRDTO()
                {
                    DomainName = machine.Name,
                    PartialSession = session
                }),
                machine.ParentServer.Queue
            );
        }

        private void AddToList(Session session, Machine machine)
        {
            var serverGuid = machine.ParentServer.ServerGuid;
            var item = (session.SessionGuid, machine.Name);
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

        private void SessionCancel()
        {
        }

        public void Dispose()
        {
            if (tokenSrc != null)
            {
                tokenSrc.Cancel();
                sessionSearchThread?.Join();
                sessionCancelThread?.Join();
            }
        }
    }
}