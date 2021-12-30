using System;
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
                Code = type.Type
            };
        }

        public static MachineTypeDTO MapMachineTypeToDTO(MachineType type)
        {
            return new MachineTypeDTO()
            {
                // TODO: resolve name based on type
                Name = type.Type,
                Code = type.Type
            };
        }

        public static SessionType MapMachineTypeDTOToSessionType(MachineTypeDTO machineType)
        {
            return new SessionType() { Type = machineType.Code };
        }

        public static MachineType MapSessionTypeToMachineType(SessionType sessionType)
        {
            return new MachineType() { Type = sessionType.Type };
        }
    }
}