using System.Net;
using OneClickDesktop.Api.Models;
using OneClickDesktop.BackendClasses.Model;
using OneClickDesktop.BackendClasses.Model.States;

namespace OneClickDesktop.Overseer.Helpers
{
    public static class ClassMapUtils
    {
        public static SessionStatusDTO MapSessionStateToDTO(SessionState state)
        {
            // TODO
            return SessionStatusDTO.Pending;
        }

        public static IpAddressDTO MapAddressToDTO(IPAddress address)
        {
            return new IpAddressDTO()
            {
                Address = address.ToString(),
                // TODO: add port to model
                Port = 1234
            };
        }

        public static MachineTypeDTO MapSessionTypeToDTO(SessionType type)
        {
            return new MachineTypeDTO()
            {
                // TODO: resolve name based on type
                Name = type.Type,
                // TODO: change code to string and assign type to code
                Code = type.Type.GetHashCode()
            };
        }

        public static MachineTypeDTO MapMachineTypeToDTO(MachineType type)
        {
            return new MachineTypeDTO()
            {
                // TODO: resolve name based on type
                Name = type.Type,
                // TODO: change code to string and assign type to code
                Code = type.Type.GetHashCode()
            };
        }
        
        public static SessionType MapMachineTypeDTOToSessionType(MachineTypeDTO machineType)
        {
            return new SessionType() { Type = machineType.Name };
        }
    }
}