using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NLog;
using OneClickDesktop.BackendClasses.Model;
using OneClickDesktop.BackendClasses.Model.Resources;
using OneClickDesktop.BackendClasses.Model.States;
using OneClickDesktop.Overseer.Entities;
using OneClickDesktop.Overseer.Helpers;
using OneClickDesktop.Overseer.Services.Interfaces;
using User = OneClickDesktop.BackendClasses.Model.User;

namespace OneClickDesktop.Overseer.Services.Classes
{
    public class SystemModelService : ISystemModelService
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private SystemModel model = new SystemModel();
        private ReaderWriterLock rwLock = new ReaderWriterLock();

        public event EventHandler<Guid> ServerUpdated;

        public void UpdateServerInfo(VirtualizationServer serverInfo)
        {
            if (serverInfo == null)
                return;

            try
            {
                rwLock.AcquireWriterLock(Timeout.Infinite);
                model.UpdateOrAddServer(serverInfo);
                UpdateModelSessions(serverInfo);
                ServerUpdated?.Invoke(this, serverInfo.ServerGuid);
            }
            catch (Exception e)
            {
                logger.Warn(e, "Error on updating Server information in model");
            }
            finally
            {
                rwLock.ReleaseWriterLock();
            }
        }

        private void UpdateModelSessions(VirtualizationServer server)
        {
            foreach (var session in server.Sessions.Values)
            {
                switch (session.SessionState)
                {
                    case SessionState.Cancelled:
                        model.DeleteSession(session.SessionGuid);
                        break;
                    case SessionState.WaitingForRemoval
                        when (session.CorrelatedMachine?.State ?? MachineState.TurnedOff) == MachineState.TurnedOff:
                        model.DeleteSession(session.SessionGuid);
                        break;
                    default:
                        model.UpdateOrAddSession(session);
                        break;
                }
            }
        }

        public IEnumerable<VirtualizationServer> GetServers()
        {
            IEnumerable<VirtualizationServer> servers = null;
            try
            {
                rwLock.AcquireReaderLock(Timeout.Infinite);
                servers = model.Servers.Values;
            }
            catch (Exception e)
            {
                logger.Warn(e, "Error on reading servers resources from model");
            }
            finally
            {
                rwLock.ReleaseReaderLock();
            }

            return servers;
        }

        public Session CreateSession(User user, SessionType sessionType)
        {
            Session session = null;
            try
            {
                rwLock.AcquireWriterLock(Timeout.Infinite);
                session = model.CreateSession(user, sessionType);
            }
            catch (Exception e)
            {
                logger.Warn(e, "Error on creating session in model");
            }
            finally
            {
                rwLock.ReleaseWriterLock();
            }

            return session;
        }

        public bool TryFindSession(User user, SessionType sessionType, out Session session)
        {
            session = null;
            try
            {
                rwLock.AcquireReaderLock(Timeout.Infinite);
                session = model.Sessions.Values
                               .FirstOrDefault(s => s.CorrelatedUser.Equals(user)
                                                    && s.SessionType.Equals(sessionType)
                                                    && Constants.State.SessionAvailableForUser.Contains(
                                                        s.SessionState)
                               );
            }
            catch (Exception e)
            {
                logger.Warn(e, "Error on looking for session in model");
            }
            finally
            {
                rwLock.ReleaseReaderLock();
            }

            return session != null;
        }

        public Machine GetMachineForSession(Session session)
        {
            Machine machineForSession = null;
            try
            {
                rwLock.AcquireReaderLock(Timeout.Infinite);
                var machineType = ClassMapUtils.MapSessionTypeToMachineType(session.SessionType);
                // find free machine of type
                machineForSession = model.Servers.Values
                                         .SelectMany(
                                             server => server.RunningMachines.Values.Where(
                                                 machine => machine.MachineType.Equals(machineType)
                                                            && machine.State == MachineState.Free)
                                         ).FirstOrDefault();
            }
            catch (Exception e)
            {
                logger.Warn(e, "Error on reading machine details from model");
            }
            finally
            {
                rwLock.ReleaseReaderLock();
            }

            return machineForSession;
        }

        public IEnumerable<DomainStartup>
            GetDomainsForStartup()
        {
            var domains = new List<DomainStartup>();
            try
            {
                rwLock.AcquireReaderLock(Timeout.Infinite);

                var servers = model.Servers.Values.Where(server => server.Managable).ToList();

                // find all machine types
                var machineTypes = servers
                                   .SelectMany(server => server.TemplateResources.Keys)
                                   .Select(key => new MachineType() { Type = key })
                                   .Distinct();
                // find machine types with machines
                var machinesStarted = servers
                                      .SelectMany(server => server.RunningMachines.Values)
                                      .Where(machine => Constants.State.MachineAvailable.Contains(machine.State))
                                      .Select(machine => machine.MachineType)
                                      .Distinct();
                // machine types with no available machines
                var machinesToStart = machineTypes.Except(machinesStarted);

                // free server resources, updated as machine startups are processed
                var serverResources = new Dictionary<Guid, ServerResources>(servers
                                                                            .Select(
                                                                                server => (
                                                                                    server, server.FreeResources))
                                                                            .Select(
                                                                                pair =>
                                                                                    new KeyValuePair<Guid,
                                                                                        ServerResources>(
                                                                                        pair.server.ServerGuid,
                                                                                        new ServerResources(
                                                                                            pair.FreeResources,
                                                                                            pair.FreeResources
                                                                                                .GpuIds)))
                );

                // for each machine type select server and generate domain name
                foreach (var machineType in machinesToStart)
                {
                    var server = servers
                                 .Where(server => server.TemplateResources.ContainsKey(machineType.Type))
                                 .OrderByDescending(
                                     server => CanCreateMachines(server, machineType, serverResources))
                                 .FirstOrDefault();

                    if (server == null) continue;

                    DecreaseServerResources(serverResources, server, machineType);
                    domains.Add(new DomainStartup(server, GenerateMachineDomainName(server, machineType), machineType));
                }
            }
            catch (Exception e)
            {
                logger.Warn(e, "Error on getting domains for startup from model");
            }
            finally
            {
                rwLock.ReleaseReaderLock();
            }

            return domains;
        }

        private int CanCreateMachines(VirtualizationServer server, MachineType type,
                                      Dictionary<Guid, ServerResources> serverResourcesMap)
        {
            var template = server.TemplateResources[type.Type];
            var resources = serverResourcesMap[server.ServerGuid];

            var count = Math.Min(resources.CpuCores / template.CpuCores,
                                 Math.Min(resources.Memory / template.Memory,
                                          resources.Storage / template.Storage));
            return template.AttachGpu ? Math.Min(count, resources.GpuCount) : count;
        }

        private void DecreaseServerResources(Dictionary<Guid, ServerResources> serverResourcesMap,
                                             VirtualizationServer server, MachineType type)
        {
            // update map: decrease resources needed to create this machine
            var template = server.TemplateResources[type.Type];
            var resources = serverResourcesMap[server.ServerGuid];
            serverResourcesMap[server.ServerGuid] = new ServerResources(
                resources.Memory - template.Memory,
                resources.CpuCores - template.CpuCores,
                resources.Storage - template.Storage,
                resources.GpuIds.Skip(template.AttachGpu ? 1 : 0)
            );
        }

        private string GenerateMachineDomainName(VirtualizationServer server, MachineType machineType)
        {
            var lastDomainNumber = server.RunningMachines.Values
                                         .Where(machine => machine.MachineType.Equals(machineType))
                                         .Select(machine => (int?)int.Parse(
                                                     machine.Name[(machine.Name.LastIndexOf('-') + 1)..]))
                                         .Max() ?? -1;
            return $"{server.ServerGuid.ToString()}-{machineType.Type}-{lastDomainNumber + 1}";
        }

        public Machine GetMachine(Guid serverGuid, string machineName)
        {
            Machine machine = null;
            try
            {
                rwLock.AcquireReaderLock(Timeout.Infinite);
                if (model.Servers.TryGetValue(serverGuid, out var server))
                {
                    if (server.RunningMachines.TryGetValue(machineName, out var m))
                    {
                        machine = m;
                    }
                }
            }
            catch (Exception e)
            {
                logger.Warn(e, "Error on reading looking for server machine in model");
            }
            finally
            {
                rwLock.ReleaseReaderLock();
            }

            return machine;
        }

        public void CancelSession(Session session)
        {
            try
            {
                rwLock.AcquireWriterLock(Timeout.Infinite);
                session.SessionState = SessionState.Cancelled;
            }
            catch (Exception e)
            {
                logger.Warn(e, "Error on changing session state in model");
            }
            finally
            {
                rwLock.ReleaseWriterLock();
            }
        }

        public Session GetSession(Guid sessionGuid)
        {
            Session session = null;
            try
            {
                rwLock.AcquireReaderLock(Timeout.Infinite);
                session = model.GetSessionInfo(sessionGuid);
            }
            catch (Exception e)
            {
                logger.Warn(e, "Error on reading session details from model");
            }
            finally
            {
                rwLock.ReleaseReaderLock();
            }

            return session;
        }
    }
}