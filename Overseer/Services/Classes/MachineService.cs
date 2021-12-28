using System;
using OneClickDesktop.BackendClasses.Communication.RabbitDTOs;
using OneClickDesktop.Overseer.Messages;
using OneClickDesktop.Overseer.Services.Interfaces;

namespace OneClickDesktop.Overseer.Services.Classes
{
    public class MachineService : IMachineService
    {
        private readonly ISystemModelService modelService;
        private readonly IVirtualizationServerConnectionService connectionService;

        public MachineService(ISystemModelService modelService,
                              IVirtualizationServerConnectionService connectionService)
        {
            this.modelService = modelService;
            this.connectionService = connectionService;

            this.modelService.ServerUpdated += StartMachineIfNeeded;
        }

        private void StartMachineIfNeeded(object sender, Guid serverGuid)
        {
            var domains = modelService.GetDomainsForStartup();
            foreach (var domain in domains)
            {
                connectionService.SendRequest(
                    new DomainStartupMessage(new DomainStartupRDTO()
                    {
                        DomainName = domain.DomainName,
                        DomainType = domain.MachineType
                    }),
                    domain.Server.Queue
                );
            }
        }
    }
}