using OneClickDesktop.BackendClasses.Model;

namespace OneClickDesktop.Overseer.Entities
{
    public class DomainStartup
    {
        public VirtualizationServer Server { get; set; }
        public string DomainName { get; set; } 
        public MachineType MachineType { get; set; }

        public DomainStartup(VirtualizationServer server, string domainName, MachineType machineType)
        {
            Server = server;
            DomainName = domainName;
            MachineType = machineType;
        }
    }
}