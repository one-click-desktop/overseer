

using OneClickDesktop.Api.Models;
using OneClickDesktop.Overseer.Services.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace OneClickDesktop.Overseer.Services.Classes
{
    public class ResourcesService : IResourcesService
    {
        private readonly ISystemModelService modelService;
        
        public ResourcesService(ISystemModelService modelService)
        {
            this.modelService = modelService;
        }

        public TotalResourcesDTO GetAllResources()
        {
            // TODO: add data transformation
            //var resourcesInfo = modelService.GetServersResources()
            return null;
        }

        public IEnumerable<MachineDTO> GetMachinesInfo()
        {
            return modelService.GetMachines()
                               .GroupBy(machine => machine.MachineType)
                               .Select(group => new MachineDTO()
                               {
                                   Amount = group.Count(),
                                   Type = new MachineTypeDTO()
                                   {
                                       Name = group.Key.Type,
                                       //Code = group.Key.Code
                                   }
                               });
        }
    }
}
