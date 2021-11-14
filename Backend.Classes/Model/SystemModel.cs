using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneClickDesktop.Backend.Classes.Model
{
    /// <summary>
    /// KLasa opisująca abstrakcyjny model systemu posiadany przez każdego nadrządce.
    /// </summary>
    class SystemModel
    {
        private List<VirtualisationServer> servers;
        private Dictionary<string, Session> sessions;

        #region Sessions
        public void GetSessionInfo(string sessionGuid) { }
        public void CreateSession(User user, string sessionType) { }
        #endregion

    }
}
