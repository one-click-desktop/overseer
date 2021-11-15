

using OneClickDesktop.Api.Models;
using OneClickDesktop.Overseer.Services.Interfaces;
using System.Collections.Generic;

namespace OneClickDesktop.Overseer.Services.Classes
{
    public class ResourcesService : IResourcesService
    {
        private List<Machine> fakeMachines;
        private TotalResources fakeTotalResources;

        public ResourcesService()
        {
            fakeMachines = new List<Machine>();
            fakeMachines.Add(new Machine()
            {
                Amount = 2,
                Type = MachineType.CpuEnum
            });
            fakeMachines.Add(new Machine()
            {
                Amount = 0,
                Type = MachineType.GpuEnum
            });

            fakeTotalResources = new TotalResources();
            fakeTotalResources.Servers = new List<Server>()
            {
                new Server()
                {
                    Address = new IpAddress()
                    {
                        Address = "10.0.10.99",
                        Port = 3389
                    },
                    Name = "fakeServer1",
                    Free = new List<Machine>()
                    {
                        new Machine() { Amount = 2, Type = MachineType.CpuEnum },
                        new Machine() { Amount = 0, Type = MachineType.GpuEnum },
                    },
                    Running = new List<Machine>()
                    {
                        new Machine() { Amount = 0, Type = MachineType.CpuEnum },
                        new Machine() { Amount = 1, Type = MachineType.GpuEnum },
                    },
                    Resources = new Resources()
                    {
                        Cpu = new Resource() { Total = 8, Free = 4},
                        Gpu = new Resource() { Total = 1, Free = 0},
                        Memory = new Resource() { Total = 8192, Free = 4096 },
                        Storage = new Resource() { Total = 500, Free = 450 }
                    }
                }
            };
            fakeTotalResources.Total = new Resources()
            {
                Cpu = new Resource() { Total = 8, Free = 4 },
                Gpu = new Resource() { Total = 1, Free = 0 },
                Memory = new Resource() { Total = 8192, Free = 4096 },
                Storage = new Resource() { Total = 500, Free = 450 }
            };
        }

        public TotalResources GetAllResources()
        {
            return fakeTotalResources;
        }

        public IEnumerable<Machine> GetMachinesInfo()
        {
            return fakeMachines;
        }
    }
}
