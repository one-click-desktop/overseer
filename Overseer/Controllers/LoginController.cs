using Microsoft.AspNetCore.Mvc;
using System;

using OneClickDesktop.Api.Controllers;
using OneClickDesktop.Api.Models;
using OneClickDesktop.Overseer.Services.Interfaces;

namespace OneClickDesktop.Overseer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : LoginApiController
    {
        private readonly ILoginService loginService;

        public LoginController(ILoginService loginService)
        {
            this.loginService = loginService;
        }

        public override IActionResult Login([FromBody] Login login)
        {
            throw new NotImplementedException();
        }
    }
}
