
using OneClickDesktop.Api.Models;
using OneClickDesktop.Overseer.Services.Interfaces;

namespace OneClickDesktop.Overseer.Services.Classes
{
    public class SessionService : ISessionService
    {
        public Session AskForSession(string sessionGuid, string userGuid)
        {
            throw new System.NotImplementedException();
        }

        public void CancelSession(string sessionGuid, string userGuid)
        {
            throw new System.NotImplementedException();
        }

        public string RequestSession(MachineType type, string userGuid)
        {
            throw new System.NotImplementedException();
        }
    }
}
