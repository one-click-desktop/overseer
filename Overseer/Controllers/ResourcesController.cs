using Microsoft.AspNetCore.Mvc;

using OneClickDesktop.Api.Controllers;
using OneClickDesktop.Overseer.Services.Interfaces;

namespace OneClickDesktop.Overseer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ResourcesController : ResourcesApiController
    {
        private readonly IResourcesService resourcesService;

        public ResourcesController(IResourcesService machinesService)
        {
            this.resourcesService = machinesService;
        }

        public override IActionResult GetResources()
        {
            throw new System.NotImplementedException();
        }
    }
}
