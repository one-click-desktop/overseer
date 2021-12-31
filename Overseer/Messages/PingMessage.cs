using OneClickDesktop.BackendClasses.Communication.MessagesTemplates;
using OneClickDesktop.RabbitModule.Common.RabbitMessage;

namespace OneClickDesktop.Overseer.Messages
{
    public class PingMessage: PingTemplate, IRabbitMessage
    {
        public string SenderIdentifier { get; set; }
        public string Type { get; set; } = MessageTypeName;
        public object Body { get; set; } = null;
    }
}