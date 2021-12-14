using Microsoft.AspNetCore.Mvc;
using System;
using System.ComponentModel.DataAnnotations;

using OneClickDesktop.Api.Controllers;
using OneClickDesktop.Overseer.Services.Interfaces;
using OneClickDesktop.Overseer.Authorization;
using OneClickDesktop.Overseer.Entities;
using OneClickDesktop.Api.Models;

using SessionDTO  = OneClickDesktop.Api.Models.SessionDTO ;
using OneClickDesktop.Overseer.Helpers.Exceptions;

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
            sessionService.CancelSession(sessionId, RequestUser.Id.ToString());
            return Ok("Sessions successfully canceled");
        }

        public override IActionResult GetSessionStatus([FromRoute(Name = "sessionId"), Required] string sessionId)
        {
            SessionDTO  res = sessionService.AskForSession(sessionId, RequestUser.Id.ToString());
            return Ok(res);
        }

        public override IActionResult GetSession([FromBody] MachineTypeDTO machineTypeDTO)
        {
            string sessionId = sessionService.RequestSession(machineTypeDTO, RequestUser.Id.ToString());

            return Ok(new SessionDTO()
            {
                Address = null,
                Id = sessionId,
                Status = SessionStatusDTO.Pending,
                Type = machineTypeDTO
            });
        }
    }
}
