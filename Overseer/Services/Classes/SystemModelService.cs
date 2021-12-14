using System;
using System.Collections.Generic;
using System.Threading;
using NLog;
using OneClickDesktop.BackendClasses.Model;
using OneClickDesktop.Overseer.Services.Interfaces;

namespace OneClickDesktop.Overseer.Services.Classes
{
    public class SystemModelService: ISystemModelService
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
            foreach (var (_, session) in server.Sessions)
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

        public Session GetSession(Guid sessionGuid)
        {
            return model.GetSessionInfo(sessionGuid);
        }
    }
}