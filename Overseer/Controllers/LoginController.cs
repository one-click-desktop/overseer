using Microsoft.AspNetCore.Mvc;
using System;

using OneClickDesktop.Api.Controllers;
using OneClickDesktop.Api.Models;

namespace OneClickDesktop.Overseer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : LoginApiController
    {
        public override IActionResult Login([FromBody] Login login)
        {
            throw new NotImplementedException();
        }
    }
}
