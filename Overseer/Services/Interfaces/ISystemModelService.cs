using System;
using System.Collections.Generic;
using OneClickDesktop.BackendClasses.Model;

namespace OneClickDesktop.Overseer.Services.Interfaces
{
    public interface ISystemModelService
    {
        public event EventHandler<Guid> ServerUpdated;

        public void UpdateServerInfo(VirtualizationServer serverInfo);

        public IEnumerable<VirtualizationServer> GetServers();

        public Session GetSession(Guid sessionGuid);

        public Session CreateSession(User user, SessionType sessionType);

        public bool TryFindSession(User user, SessionType sessionType, out Session session);

        public Machine GetMachineForSession(Session session);

        public IEnumerable<(VirtualizationServer server, string domainName, MachineType machineType)>
            GetDomainsForStartup();

        public Machine GetMachine(Guid serverGuid, string machineName);

        public void CancelSession(Session session);
    }
}