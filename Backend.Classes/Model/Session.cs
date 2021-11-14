using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneClickDesktop.Backend.Classes.Model
{
    class Session
    {
        public Machine CorrelatedMachine { get; set; }
        public User CorreletedUser { get; set; }
        public string SessionGuid { get; set; }

        public Session(string guid, User user, Machine machine = null)
        {
            SessionGuid = guid;
            CorreletedUser = user;
            CorrelatedMachine = machine;
        }

        public void AttachMachine(Machine machine)
        {
            CorrelatedMachine = machine;
        }


    }
}
