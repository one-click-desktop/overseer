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
    public class MachinesController : MachinesApiController
    {
        private readonly IResourcesService resourcesService;

        private User RequestUser => (User)HttpContext.Items["User"];

        public MachinesController(IResourcesService machinesService)
        {
            this.resourcesService = machinesService;
        }

        [Authorize]
        public override IActionResult GetMachines()
        {
            var machines = resourcesService.GetMachinesInfo(RequestUser.Id);
            return Ok(machines);
        }
    }
}
