using System;
using System.Collections.Generic;
using System.Net;
using OneClickDesktop.Api.Models;
using OneClickDesktop.BackendClasses.Model;
using OneClickDesktop.BackendClasses.Model.States;
using OneClickDesktop.Overseer.Helpers;
using OneClickDesktop.Overseer.Helpers.Exceptions;
using OneClickDesktop.Overseer.Services.Interfaces;
using SessionDTO = OneClickDesktop.Api.Models.SessionDTO;

namespace OneClickDesktop.Overseer.Services.Classes
{
    public class SessionService : ISessionService
    {
        private readonly IVirtualizationServerConnectionService virtSrvConnection;
        private readonly ISystemModelService modelService;

        public SessionService(IVirtualizationServerConnectionService virtSrvConnection,
                              ISystemModelService modelService)
        {
            this.virtSrvConnection = virtSrvConnection;
            this.modelService = modelService;
        }

        private Session FindSession(Guid sessionGuid, Guid userGuid)
        {
            var session = modelService.GetSession(sessionGuid);
            if (session == null || session.CorrelatedUser.Guid != userGuid)
            {
                throw new ErrorHttpException("SessionDTO  not found", System.Net.HttpStatusCode.NotFound);
            }
            return session;
        }

        public SessionDTO AskForSession(Guid sessionGuid, Guid userGuid)
        {
            return CreateSessionDTO(FindSession(sessionGuid, userGuid));
        }

        public void CancelSession(Guid sessionGuid, Guid userGuid)
        {
            var session = FindSession(sessionGuid, userGuid);
            StartSessionCancelProcess(session);
        }

        public Guid RequestSession(MachineTypeDTO type, Guid userGuid)
        {
            var sessionGuid = Guid.NewGuid();
            var session = new Session(new User(userGuid), ClassMapUtils.MapMachineTypeDTOToSessionType(type));
            
            return sessionGuid;
        }

        private SessionDTO CreateSessionDTO(Session session)
        {
            return new SessionDTO()
            {
                Id = session.SessionGuid.ToString(),
                Status = ClassMapUtils.MapSessionStateToDTO(session.SessionState),
                Address = ClassMapUtils.MapAddressToDTO(session.CorrelatedMachine.IpAddress),
                Type = ClassMapUtils.MapSessionTypeToDTO(session.SessionType)
            };
        }

        private void StartSessionSearchProcess(Session session)
        {
            // TODO: add implementation
        }

        private void StartSessionCancelProcess(Session session)
        {
            session.SessionState = SessionState.Cancelled;
            // TODO: add implementation
        }
    }
}