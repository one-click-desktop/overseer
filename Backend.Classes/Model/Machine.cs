using OneClickDesktop.Backend.Classes.Model.Resources;
using OneClickDesktop.Backend.Classes.Model.States;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneClickDesktop.Backend.Classes.Model
{
    /// <summary>
    /// Klasa reprezentują jedną instancję wirtualnej maszyny.
    /// </summary>
    class Machine
    {
        public string Name { get; private set; }
        public MachineState State { get; set; }

        public string MachineType { get; private set; }

        public MachineResources UsingResources { get; private set; }

        public User ConnectedUser { get; private set; }

        public VirtualisationServer ParentServer { get; private set; }

        /// <summary>
        /// Tworzy maszynę w stanie wyłączonym bez połączonego użytkownika.
        /// </summary>
        /// <param name="name">Nazwa maszyny</param>
        /// <param name="type">Typ maszyny</param>
        /// <param name="resources">Zajmowane zasoby przez maszynę</param>
        /// <param name="parent">Serwer wirtualizacji, na którym jest uruchamiana.</param>
        public Machine(string name, string type, MachineResources resources, VirtualisationServer parent)
        {
            Name = name;
            MachineType = type;
            UsingResources = resources;
            ParentServer = parent;
            ConnectedUser = null;
            State = MachineState.TurnedOff;
        }



    }
}
