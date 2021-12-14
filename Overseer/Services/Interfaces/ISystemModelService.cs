using System;
using System.Collections.Generic;
using OneClickDesktop.BackendClasses.Model;
using OneClickDesktop.Overseer.Entities;

namespace OneClickDesktop.Overseer.Services.Interfaces
{
    public interface ISystemModelService
    {
        public event EventHandler<Guid> ServerUpdated;

        public void UpdateServerInfo(VirtualizationServer serverInfo);

        public IEnumerable<VirtualizationServer> GetServers();

        public Session GetSession(Guid sessionGuid);
    }
}