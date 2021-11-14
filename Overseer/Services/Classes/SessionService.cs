using System;
using System.Collections.Generic;
using OneClickDesktop.Api.Models;
using OneClickDesktop.Overseer.Helpers.Exceptions;
using OneClickDesktop.Overseer.Services.Interfaces;

using Session = OneClickDesktop.Api.Models.Session;

namespace OneClickDesktop.Overseer.Services.Classes
{
    public class SessionService : ISessionService
    {
        private Dictionary<string, (Session ses, string user)> fakeSessionList;

        public SessionService()
        {
            fakeSessionList = new Dictionary<string, (Session ses, string user)>();
        }

        private Session FindSession(string sessionGuid, string userGuid)
        {
            if (!fakeSessionList.TryGetValue(sessionGuid, out (Session ses, string user) session) || session.user != userGuid)
                throw new ErrorHttpException("Session not found", System.Net.HttpStatusCode.NotFound);
            return session.ses;
        }

        public Session AskForSession(string sessionGuid, string userGuid)
        {
            return FindSession(sessionGuid, userGuid);
        }

        public void CancelSession(string sessionGuid, string userGuid)
        {
            Session session = FindSession(sessionGuid, userGuid);

            fakeSessionList.Remove(session.Id);
        }

        public string RequestSession(MachineType type, string userGuid)
        {
            string sessionGuid = Guid.NewGuid().ToString();
            fakeSessionList.Add(sessionGuid, (new Session()
            {
                Address = new IpAddress()
                {
                    Address = "1.2.3.4",
                    Port = 12345
                },
                Id = sessionGuid,
                Status = SessionStatus.PendingEnum,
                Type = type
            }, userGuid));

            return sessionGuid;
        }
    }
}
