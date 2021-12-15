using System;
using System.Net;
using OneClickDesktop.Api.Models;
using OneClickDesktop.BackendClasses.Model;
using OneClickDesktop.Overseer.Helpers;
using OneClickDesktop.Overseer.Helpers.Exceptions;
using OneClickDesktop.Overseer.Services.Interfaces;

namespace OneClickDesktop.Overseer.Services.Classes
{
    public class SessionService : ISessionService
    {
        private readonly IVirtualizationServerConnectionService virtSrvConnection;
        private readonly ISystemModelService modelService;
        private readonly ISessionProcessService sessionProcessService;

        public SessionService(ISystemModelService modelService,
                              ISessionProcessService sessionProcessService)
        {
            this.modelService = modelService;
            this.sessionProcessService = sessionProcessService;
        }

        private Session FindSession(Guid sessionGuid, Guid userGuid)
        {
            var session = modelService.GetSession(sessionGuid);
            if (session == null || session.CorrelatedUser.Guid != userGuid)
            {
                throw new ErrorHttpException("SessionDTO  not found", HttpStatusCode.NotFound);
            }
            return session;
        }

        public SessionDTO AskAboutSession(Guid sessionGuid, Guid userGuid)
        {
            return CreateSessionDTO(FindSession(sessionGuid, userGuid));
        }

        public void CancelSession(Guid sessionGuid, Guid userGuid)
        {
            var session = FindSession(sessionGuid, userGuid);
            // TODO: cancel session
        }

        public SessionDTO RequestSession(MachineTypeDTO type, Guid userGuid)
        {
            var sessionType = ClassMapUtils.MapMachineTypeDTOToSessionType(type);
            if (modelService.TryFindSession(new User(userGuid), sessionType, out var session)) return CreateSessionDTO(session);
            
            // [WARNING] we may want to store users
            session = modelService.CreateSession(new User(userGuid), sessionType);
            sessionProcessService.StartSessionSearchProcess(session);

            return CreateSessionDTO(session);
        }

        private SessionDTO CreateSessionDTO(Session session)
        {
            return new SessionDTO()
            {
                Id = session.SessionGuid.ToString(),
                Status = ClassMapUtils.MapSessionStateToDTO(session.SessionState),
                //Address = ClassMapUtils.MapAddressToDTO(session.CorrelatedMachine?.IpAddress),
                // TODO: fix
                Address = new IpAddressDTO()
                {
                    Address = "localhost",
                    Port = 3389
                },
                Type = ClassMapUtils.MapSessionTypeToDTO(session.SessionType)
            };
        }
    }
}