using System;
using System.Threading;
using OneClickDesktop.BackendClasses.Model;
using OneClickDesktop.Overseer.Services.Interfaces;

namespace OneClickDesktop.Overseer.Services.Classes
{
    public class SystemModelService: ISystemModelService
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private SystemModel model = new SystemModel();
        private ReaderWriterLock rwLock = new ReaderWriterLock();
        
        public void UpdateServerInfo(VirtualizationServer serverInfo)
        {
            try
            {
                rwLock.AcquireWriterLock(Timeout.Infinite);
                model.UpdateOrAddServer(serverInfo);
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
    }
}