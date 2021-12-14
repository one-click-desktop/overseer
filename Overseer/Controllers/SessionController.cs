using System;
using System.ComponentModel.DataAnnotations;
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
    public class SessionController : SessionApiController
    {
        private readonly ISessionService sessionService;

        private User RequestUser => (User)HttpContext.Items["User"];

        public SessionController(ISessionService sessionService)
        {
            this.sessionService = sessionService;
        }

        public override IActionResult DeleteSession([FromRoute(Name = "sessionId"), Required] string sessionId)
        {
            sessionService.CancelSession(Guid.Parse(sessionId), RequestUser.Id);
            return Ok("Sessions successfully canceled");
        }

        public override IActionResult GetSessionStatus([FromRoute(Name = "sessionId"), Required] string sessionId)
        {
            var res = sessionService.AskForSession(Guid.Parse(sessionId), RequestUser.Id);
            return Ok(res);
        }

        public override IActionResult GetSession([FromBody] MachineTypeDTO machineTypeDTO)
        {
            var sessionId = sessionService.RequestSession(machineTypeDTO, RequestUser.Id);
            return Ok(new SessionDTO()
            {
                Address = null,
                Id = sessionId.ToString(),
                Status = SessionStatusDTO.Pending,
                Type = machineTypeDTO
            });
        }
    }
}
