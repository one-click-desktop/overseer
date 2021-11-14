using Microsoft.AspNetCore.Mvc;

using OneClickDesktop.Api.Controllers;
using OneClickDesktop.Api.Models;
using OneClickDesktop.Overseer.Authorization;
using OneClickDesktop.Overseer.Entities;
using OneClickDesktop.Overseer.Services.Interfaces;

namespace OneClickDesktop.Overseer.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ResourcesController : ResourcesApiController
    {
        private readonly IResourcesService resourcesService;

        public ResourcesController(IResourcesService machinesService)
        {
            this.resourcesService = machinesService;
        }

        [Authorize(Role.Admin)]
        public override IActionResult GetResources()
        {
            TotalResources result = resourcesService.GetAllResources();
            return Ok(result);
        }
    }
}
