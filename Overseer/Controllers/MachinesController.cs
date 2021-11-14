using Microsoft.AspNetCore.Mvc;
using System;

using OneClickDesktop.Api.Controllers;
using OneClickDesktop.Overseer.Services.Interfaces;
using OneClickDesktop.Overseer.Helpers;
using OneClickDesktop.Overseer.Entities;
using OneClickDesktop.Overseer.Authorization;

namespace OneClickDesktop.Overseer.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class MachinesController : ControllerBase// : MachinesApiController
    {
        private readonly IResourcesService resourcesService;

        public MachinesController(IResourcesService machinesService)
        {
            this.resourcesService = machinesService;
        }

        [Authorize(Role.User, Role.Admin)]
        [HttpGet]
        [Route("machines")]
        public IActionResult GetMachines()
        {
            var machines = resourcesService.GetMachinesInfo();
            return Ok(machines);
        }
    }
}
