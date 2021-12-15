using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NLog;
using OneClickDesktop.BackendClasses.Model;
using OneClickDesktop.BackendClasses.Model.States;
using OneClickDesktop.Overseer.Services.Interfaces;

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
                model.UpdateOrAddSession(session);
            }
            // TODO: delete dead sessions
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
            Machine machine = null;
            try
            {
                rwLock.AcquireReaderLock(Timeout.Infinite);
                // TODO: add proper implemetation
                machine = model.Servers.Values.FirstOrDefault()?.RunningMachines.Values.FirstOrDefault(
                    m => m.State == MachineState.Free &&
                         m.MachineType.Type == session.SessionType.Type);
            }
            catch (Exception e)
            {
                logger.Warn(e, "Error on reading machine details from model");
            }
            finally
            {
                rwLock.ReleaseReaderLock();
            }

            return machine;
        }

        public IEnumerable<(VirtualizationServer server, string domainName, MachineType machineType)>
            GetDomainsForStartup()
        {
            VirtualizationServer server = null;
            string domainName = null;
            MachineType machineType = null;
            try
            {
                rwLock.AcquireReaderLock(Timeout.Infinite);
                // TODO: add proper implementation
                server = model.Servers.Values.FirstOrDefault(server => server.Managable);
                if (server == null || server.RunningMachines.Values.Any())
                    return new List<(VirtualizationServer server, string domainName, MachineType machineType)>();
                domainName = $"{server?.ServerGuid}:{server.RunningMachines.Count}";
                machineType = new MachineType() { Type = server.TemplateResources.Keys.First() };
            }
            catch (Exception e)
            {
                logger.Warn(e, "Error on getting domains for startup from model");
            }
            finally
            {
                rwLock.ReleaseReaderLock();
            }

            return new List<(VirtualizationServer server, string domainName, MachineType machineType)>()
            {
                (server, domainName, machineType)
            };
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
                // TODO: get session from model
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