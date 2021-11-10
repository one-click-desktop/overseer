using Microsoft.AspNetCore.Mvc;
using System;
using System.ComponentModel.DataAnnotations;

using OneClickDesktop.Api.Controllers;
using OneClickDesktop.Overseer.Services.Interfaces;

namespace OneClickDesktop.Overseer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SessionController : SessionApiController
    {
        private readonly ISessionService sessionService;

        public SessionController(ISessionService sessionService)
        {
            this.sessionService = sessionService;
        }


        public override IActionResult DeleteSession([FromRoute(Name = "sessionId"), Required] string sessionId)
        {
            throw new NotImplementedException();
        }

        public override IActionResult GetSession([FromBody] string body)
        {
            throw new NotImplementedException();
        }

        public override IActionResult GetSessionStatus([FromRoute(Name = "sessionId"), Required] string sessionId)
        {
            throw new NotImplementedException();
        }
    }
}
