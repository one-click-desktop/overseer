using System;
using System.Net;
using OneClickDesktop.Api.Models;
using OneClickDesktop.BackendClasses.Communication.RabbitDTOs;
using OneClickDesktop.BackendClasses.Model;
using OneClickDesktop.BackendClasses.Model.States;
using OneClickDesktop.Overseer.Helpers;
using OneClickDesktop.Overseer.Helpers.Exceptions;
using OneClickDesktop.Overseer.Messages;
using OneClickDesktop.Overseer.Services.Interfaces;

namespace OneClickDesktop.Overseer.Services.Classes
{
    public class SessionService : ISessionService
    {
        private readonly IVirtualizationServerConnectionService virtSrvConnection;
        private readonly ISystemModelService modelService;
        private readonly ISessionProcessService sessionProcessService;

        public SessionService(IVirtualizationServerConnectionService virtSrvConnection,
        ISystemModelService modelService,
            ISessionProcessService sessionProcessService)
        {
            this.virtSrvConnection = virtSrvConnection;
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

            if (session == null)
                return;

            //Wyslij do wszystkich virtserverów informacje o cancelacji sesji.
            //Informacje o udanej cancelacji przechwycimy w odbiorze modelu - co przerwie proces szukania maszyny
            //lub anuluje juz znalezioną maszynę

            virtSrvConnection.SendRequest(
                new SessionCancelMessage(
                    new SessionCancelRDTO()
                    {
                        SessionGuid = sessionGuid
                    }
                ));

            return;
            SessionState oldState = session.SessionState;
            modelService.CancelSession(session);
            if (oldState == SessionState.Pending)
            {
                //znajdz poproszona maszyne i mowimy serwerowi aby oznaczyl maszyne jako wolna oraz usunal sesje z modelu

                //Jezeli zostala przypisana do niej dana sesja

                //Wysylij do jednego servera
            }
            else if (oldState == SessionState.Running)
            {
                //jezeli maszyna jest occupied (nie mozna) - ignoruj na virtsrv

                //jezeli maszyna zarezerwowana - oznacz jako wolna i usun sesje z modelu (virtsrv)

                //Wyślij do jednego servera
            }
        }

        public SessionDTO RequestSession(MachineTypeDTO type, Guid userGuid)
        {
            var sessionType = ClassMapUtils.MapMachineTypeDTOToSessionType(type);
            if (modelService.TryFindSession(new User(userGuid), sessionType, out var session))
                return CreateSessionDTO(session);

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
                Address = ClassMapUtils.MapAddressToDTO(session.CorrelatedMachine?.IpAddress),
                Type = ClassMapUtils.MapSessionTypeToDTO(session.SessionType)
            };
        }
    }
}