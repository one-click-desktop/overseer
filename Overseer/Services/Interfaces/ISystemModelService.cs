using System.Collections.Generic;
using OneClickDesktop.BackendClasses.Model;
using OneClickDesktop.Overseer.Entities;

namespace OneClickDesktop.Overseer.Services.Interfaces
{
    public interface ISystemModelService
    {
        public void UpdateServerInfo(VirtualizationServer serverInfo);

        public IEnumerable<Machine> GetMachines();

        public IEnumerable<ServerResourcesInfo> GetServersResources();
    }
}