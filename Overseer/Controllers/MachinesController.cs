using Microsoft.AspNetCore.Mvc;
using System;

using OneClickDesktop.Api.Controllers;

namespace OneClickDesktop.Overseer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MachinesController : MachinesApiController
    {
        public override IActionResult GetMachines()
        {
            throw new NotImplementedException();
        }
    }
}
