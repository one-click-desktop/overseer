using System;
using System.Collections.Generic;
using System.Linq;
using OneClickDesktop.Api.Models;
using OneClickDesktop.BackendClasses.Model;
using OneClickDesktop.BackendClasses.Model.Resources;
using OneClickDesktop.Overseer.Helpers;
using OneClickDesktop.Overseer.Services.Interfaces;

namespace OneClickDesktop.Overseer.Services.Classes
{
    public class ResourcesService : IResourcesService
    {
        private readonly ISystemModelService modelService;

        public ResourcesService(ISystemModelService modelService)
        {
            this.modelService = modelService;
        }

        /// <summary>
        /// Get information about total resources and each server resources
        /// </summary>
        public TotalResourcesDTO GetAllResources()
        {
            var servers = modelService.GetServers();
            var virtualizationServers = servers.ToList();
            var (totalServerResources, freeServerResources) = virtualizationServers
                                                              .Select(server =>
                                                                          (server.TotalServerResources,
                                                                           server.FreeServerResources)
                                                              )
                                                              .Aggregate((a1, a2) =>
                                                                             (a1.TotalServerResources + a2.TotalServerResources,
                                                                              a1.FreeServerResources +
                                                                              a2.FreeServerResources)
                                                              );
            return new TotalResourcesDTO()
            {
                Total = ConstructResourcesDTO(totalServerResources,
                                              freeServerResources),
                Servers = virtualizationServers.Select(server => new ServerDTO()
                {
                    Name = server.ServerGuid.ToString(),
                    Resources = ConstructResourcesDTO(server.TotalServerResources, server.FreeServerResources),
                    Free = CalculateFreeMachines(server),
                    Running = GetRunningMachines(server),
                    // TODO: we need to change this in DTO, as we don't store server address
                    Address = null
                }).ToList()
            };
        }

        /// <summary>
        /// Get amount of available machines by type
        /// </summary>
        public IEnumerable<MachineDTO> GetMachinesInfo()
        {
            return modelService.GetServers()
                               .SelectMany(CalculateFreeMachines)
                               .GroupBy(machine => machine.Type)
                               .Select(group => new MachineDTO()
                               {
                                   Type = group.Key,
                                   Amount = group.Sum(machine => machine.Amount)
                               });
        }

        /// <summary>
        /// Calculate amount of machines that can be created on server by type
        /// </summary>
        private static List<MachineDTO> CalculateFreeMachines(VirtualizationServer server)
        {
            var freeResources = server.FreeServerResources;

            return server.TemplateResources.Select(pair =>
            {
                var (type, templateResources) = pair;
                return new MachineDTO()
                {
                    Type = ClassMapUtils.MapMachineTypeToDTO(new MachineType() { Type = type }),
                    Amount = CalculateAvailableMachinesForTemplate(freeResources, templateResources)
                };
            }).ToList();
        }

        /// <summary>
        /// Get list of machines running on server by machine type
        /// </summary>
        private static List<MachineDTO> GetRunningMachines(VirtualizationServer server)
        {
            return server.RunningMachines.Values.GroupBy(machine => machine.MachineType)
                         .Select(group => new MachineDTO()
                         {
                             Amount = group.Count(),
                             Type = ClassMapUtils.MapMachineTypeToDTO(group.Key)
                         }).ToList();
        }

        /// <summary>
        /// Calculates amount of machines that can be created from template resources
        /// </summary>
        private static int CalculateAvailableMachinesForTemplate(ServerResources serverResources,
                                                                 TemplateResources templateResources)
        {
            return Math.Min(serverResources.CpuCores / templateResources.CpuCores,
                            Math.Min(serverResources.GpuCount / (templateResources.AttachGpu ? 1 : 0),
                                     Math.Min(serverResources.Memory / templateResources.Memory,
                                              serverResources.Storage / templateResources.Storage)));
        }

        private static ResourcesDTO ConstructResourcesDTO(ServerResources totalResources, ServerResources freeResources)
        {
            return new ResourcesDTO()
            {
                Cpu = ConstructResourceDTO(totalResources.CpuCores, freeResources.CpuCores),
                Gpu = ConstructResourceDTO(totalResources.GpuCount, freeResources.GpuCount),
                Memory = ConstructResourceDTO(totalResources.Memory, freeResources.Memory),
                Storage = ConstructResourceDTO(totalResources.Storage, freeResources.Storage),
            };
        }

        private static ResourceDTO ConstructResourceDTO(int total, int free)
        {
            return new ResourceDTO()
            {
                Free = free,
                Total = total
            };
        }
    }
}