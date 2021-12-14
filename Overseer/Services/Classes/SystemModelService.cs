using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using OneClickDesktop.BackendClasses.Model;
using OneClickDesktop.BackendClasses.Model.Resources;
using OneClickDesktop.Overseer.Entities;
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
            if (serverInfo == null)
                return;
            
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

        public IEnumerable<Machine> GetMachines()
        {
            IEnumerable<Machine> machines = null;
            try
            {
                rwLock.AcquireReaderLock(Timeout.Infinite);
                machines = model.Servers.Values.SelectMany(server => server.RunningMachines.Values);
            }
            catch (Exception e)
            {
                logger.Warn(e, "Error on reading machines from model");
            }
            finally
            {
                rwLock.ReleaseReaderLock();
            }
            
            return machines;
        }
        
        public IEnumerable<ServerResourcesInfo> GetServersResources()
        {
            IEnumerable<ServerResourcesInfo> resources = null;
            try
            {
                rwLock.AcquireReaderLock(Timeout.Infinite);
                resources = model.Servers.Values.Select(server => new ServerResourcesInfo()
                {
                    TotalResources = server.TotalServerResources, 
                    FreeResources = server.FreeServerResources,
                    Machines = server.RunningMachines.Values
                });
            }
            catch (Exception e)
            {
                logger.Warn(e, "Error on reading servers resources from model");
            }
            finally
            {
                rwLock.ReleaseReaderLock();
            }
            
            return resources;
        }
    }
}