using OneClickDesktop.BackendClasses.Communication.MessagesTemplates;
using OneClickDesktop.BackendClasses.Communication.RabbitDTOs;
using OneClickDesktop.RabbitModule.Common.RabbitMessage;

namespace OneClickDesktop.Overseer.Messages
{
    public class DomainShutdownMessage: DomainShutdownTemplate, IRabbitMessage
    {
        public string SenderIdentifier { get; set; } = Configuration.AppId;
        public string Type { get; set; } = MessageTypeName;
        public object Body { get; set; }
        
        public DomainShutdownMessage(DomainShutdownRDTO data)
        {
            Body = data;
        }
    }
}