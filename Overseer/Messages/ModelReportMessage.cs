using OneClickDesktop.BackendClasses.Communication.MessagesTemplates;
using OneClickDesktop.RabbitModule.Common.RabbitMessage;

namespace OneClickDesktop.Overseer.Messages
{
    public class ModelReportMessage: ModelReportTemplate, IRabbitMessage
    {
        public string AppId { get; set; } = Configuration.AppId;
        public string Type { get; set; } = MessageTypeName;
        public object Message { get; set; }

        public ModelReportMessage(BackendClasses.Model.VirtualizationServer model)
        {
            Message = model;
        }
    }
}