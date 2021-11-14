using Microsoft.AspNetCore.Mvc;
using System;

using OneClickDesktop.Api.Controllers;
using OneClickDesktop.Overseer.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using OneClickDesktop.Overseer.Helpers;

namespace OneClickDesktop.Overseer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MachinesController : MachinesApiController
    {
        private readonly IResourcesService resourcesService;

        public MachinesController(IResourcesService machinesService)
        {
            this.resourcesService = machinesService;
        }

        [Authorize(Roles = Role.User)]
        public override IActionResult GetMachines()
        {
            var machines = resourcesService.GetMachinesInfo();
            return Ok(machines);
        }
    }
}
