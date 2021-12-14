using System.Collections.Generic;
using OneClickDesktop.BackendClasses.Model;
using OneClickDesktop.BackendClasses.Model.Resources;

namespace OneClickDesktop.Overseer.Entities
{
    public struct ServerResourcesInfo
    {
        public ServerResources TotalResources { get; set; }
        
        public ServerResources FreeResources { get; set; }
        
        public IEnumerable<Machine> Machines { get; set; }
    }
}