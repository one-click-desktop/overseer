using System.Collections.Generic;
using OneClickDesktop.BackendClasses.Model.States;

namespace OneClickDesktop.Overseer
{
    public static class Constants
    {
        public static class State
        {
            public static readonly HashSet<SessionState> SessionAvailableForUser = new HashSet<SessionState>()
            {
                SessionState.Pending,
                SessionState.Running,
                SessionState.WaitingForRemoval
            };

            public static readonly HashSet<MachineState> MachineAvailable = new HashSet<MachineState>()
            {
                MachineState.Booting,
                MachineState.Free
            };
        }
    }
}