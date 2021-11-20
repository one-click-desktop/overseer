using Microsoft.AspNetCore.Mvc;
using System;

using OneClickDesktop.Api.Controllers;
using OneClickDesktop.Api.Models;
using OneClickDesktop.Overseer.Services.Interfaces;
using OneClickDesktop.Overseer.Authorization;

namespace OneClickDesktop.Overseer.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : LoginApiController
    {
        private readonly IUserService loginService;

        public LoginController(IUserService loginService)
        {
            this.loginService = loginService;
        }

        [AllowAnonymous]
        public override IActionResult Login([FromBody] LoginDTO login)
        {
            TokenDTO token = loginService.Login(login);

            return Ok(token);
        }
    }
}
