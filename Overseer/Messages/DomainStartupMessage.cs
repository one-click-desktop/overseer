using OneClickDesktop.BackendClasses.Communication.MessagesTemplates;
using OneClickDesktop.BackendClasses.Communication.RabbitDTOs;
using OneClickDesktop.RabbitModule.Common.RabbitMessage;

namespace OneClickDesktop.Overseer.Messages
{
    public class DomainStartupMessage: DomainStartupTemplate, IRabbitMessage
    {
        public string SenderIdentifier { get; set; } = Configuration.AppId;
        public string Type { get; set; } = MessageTypeName;
        public object Body { get; set; }

        public DomainStartupMessage(DomainStartupRDTO data)
        {
           Body = data;
        }
    }
}