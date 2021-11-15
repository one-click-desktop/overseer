

using OneClickDesktop.Api.Models;
using OneClickDesktop.Overseer.Services.Interfaces;
using System.Collections.Generic;

namespace OneClickDesktop.Overseer.Services.Classes
{
    public class ResourcesService : IResourcesService
    {
        private List<MachineDTO> fakeMachines;
        private TotalResourcesDTO fakeTotalResources;

        public ResourcesService()
        {
            fakeMachines = new List<MachineDTO>();
            fakeMachines.Add(new MachineDTO()
            {
                Amount = 2,
                Type = MachineTypeDTO.CpuEnum
            });
            fakeMachines.Add(new MachineDTO()
            {
                Amount = 0,
                Type = MachineTypeDTO.GpuEnum
            });

            fakeTotalResources = new TotalResourcesDTO();
            fakeTotalResources.Servers = new List<ServerDTO>()
            {
                new ServerDTO()
                {
                    Address = new IpAddressDTO()
                    {
                        Address = "10.0.10.99",
                        Port = 3389
                    },
                    Name = "fakeServer1",
                    Free = new List<MachineDTO>()
                    {
                        new MachineDTO() { Amount = 2, Type = MachineTypeDTO.CpuEnum },
                        new MachineDTO() { Amount = 0, Type = MachineTypeDTO.GpuEnum },
                    },
                    Running = new List<MachineDTO>()
                    {
                        new MachineDTO() { Amount = 0, Type = MachineTypeDTO.CpuEnum },
                        new MachineDTO() { Amount = 1, Type = MachineTypeDTO.GpuEnum },
                    },
                    Resources = new ResourcesDTO()
                    {
                        Cpu = new ResourceDTO() { Total = 8, Free = 4},
                        Gpu = new ResourceDTO() { Total = 1, Free = 0},
                        Memory = new ResourceDTO() { Total = 8192, Free = 4096 },
                        Storage = new ResourceDTO() { Total = 500, Free = 450 }
                    }
                }
            };
            fakeTotalResources.Total = new ResourcesDTO()
            {
                Cpu = new ResourceDTO() { Total = 8, Free = 4 },
                Gpu = new ResourceDTO() { Total = 1, Free = 0 },
                Memory = new ResourceDTO() { Total = 8192, Free = 4096 },
                Storage = new ResourceDTO() { Total = 500, Free = 450 }
            };
        }

        public TotalResourcesDTO GetAllResources()
        {
            return fakeTotalResources;
        }

        public IEnumerable<MachineDTO> GetMachinesInfo()
        {
            return fakeMachines;
        }
    }
}
