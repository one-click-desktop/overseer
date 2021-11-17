using System;
using System.Collections.Generic;
using OneClickDesktop.Api.Models;
using OneClickDesktop.Overseer.Helpers.Exceptions;
using OneClickDesktop.Overseer.Services.Interfaces;

using SessionDTO  = OneClickDesktop.Api.Models.SessionDTO ;

namespace OneClickDesktop.Overseer.Services.Classes
{
    public class SessionService : ISessionService
    {
        private static Dictionary<string, (SessionDTO  ses, string user)> fakeSessionList = new Dictionary<string, (SessionDTO  ses, string user)>();

        private SessionDTO  FindSession(string sessionGuid, string userGuid)
        {
            if (!fakeSessionList.TryGetValue(sessionGuid, out (SessionDTO  ses, string user) session) || session.user != userGuid)
                throw new ErrorHttpException("SessionDTO  not found", System.Net.HttpStatusCode.NotFound);
            return session.ses;
        }

        public SessionDTO  AskForSession(string sessionGuid, string userGuid)
        {
            return FindSession(sessionGuid, userGuid);
        }

        public void CancelSession(string sessionGuid, string userGuid)
        {
            SessionDTO  session = FindSession(sessionGuid, userGuid);

            fakeSessionList.Remove(session.Id);
        }

        public string RequestSession(MachineTypeDTO  type, string userGuid)
        {
            //TODO: Dodac validace czy typ sesji jest dopuszczalny w systemie
            string sessionGuid = Guid.NewGuid().ToString();
            fakeSessionList.Add(sessionGuid, (new SessionDTO ()
            {
                Address = new IpAddressDTO()
                {
                    Address = "1.2.3.4",
                    Port = 12345
                },
                Id = sessionGuid,
                Status = SessionStatusDTO.Ready,
                Type = type
            }, userGuid));

            return sessionGuid;
        }
    }
}
