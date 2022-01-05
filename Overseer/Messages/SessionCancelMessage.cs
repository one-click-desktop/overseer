using OneClickDesktop.BackendClasses.Communication.MessagesTemplates;
using OneClickDesktop.BackendClasses.Communication.RabbitDTOs;
using OneClickDesktop.RabbitModule.Common.RabbitMessage;

namespace OneClickDesktop.Overseer.Messages
{
    public class SessionCancelMessage: SessionCancelTemplate, IRabbitMessage
    {
        public string SenderIdentifier { get; set; }
        public string Type { get; set; } = MessageTypeName;
        public object Body { get; set; }

        public SessionCancelMessage(SessionCancelRDTO data)
        {
            Body = data;
        }
    }
}