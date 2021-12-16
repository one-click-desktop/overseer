using System;
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
            return state switch
            {
                SessionState.Pending => SessionStatusDTO.Pending,
                SessionState.Running => SessionStatusDTO.Ready,
                SessionState.Cancelled => SessionStatusDTO.Cancelled,
                SessionState.WaitingForRemoval => SessionStatusDTO.Ready,
                _ => throw new ArgumentOutOfRangeException(nameof(state), state, null)
            };
        }

        public static IpAddressDTO MapAddressToDTO(MachineAddress? address)
        {
            return address == null
                ? null
                : new IpAddressDTO()
                {
                    Address = address.Value.Address,
                    Port = address.Value.Port
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
            // TODO: change to code after model change
            return new SessionType() { Type = machineType.Name };
        }
    }
}