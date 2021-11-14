

using OneClickDesktop.Api.Models;
using OneClickDesktop.Overseer.Services.Interfaces;
using System.Collections.Generic;

namespace OneClickDesktop.Overseer.Services.Classes
{
    public class ResourcesService : IResourcesService
    {
        private List<Machine> fakeMachines;
        public ResourcesService()
        {
            fakeMachines = new List<Machines>();
            fakeMachines.Add(new Machines()
            {
                Amount = 3,
                Type = MachineType.CpuEnum
            });
            fakeMachines.Add(new Machines()
            {
                Amount = 1,
                Type = MachineType.GpuEnum
            });
        }

        public TotalResources GetAllResources()
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<Machines> GetMachinesInfo()
        {
            return fakeMachines;
        }
    }
}
