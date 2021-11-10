using Microsoft.AspNetCore.Mvc;
using System;

using OneClickDesktop.Api.Controllers;
using OneClickDesktop.Overseer.Services.Interfaces;

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

        public override IActionResult GetMachines()
        {
            throw new NotImplementedException();
        }
    }
}
