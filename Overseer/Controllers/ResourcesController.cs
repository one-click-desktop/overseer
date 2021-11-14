using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using OneClickDesktop.Api.Controllers;
using OneClickDesktop.Overseer.Helpers;
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

        [Authorize(Roles = Role.Admin)]
        public override IActionResult GetResources()
        {
            throw new System.NotImplementedException();
        }
    }
}
