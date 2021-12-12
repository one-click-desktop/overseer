using OneClickDesktop.BackendClasses.Communication.MessagesTemplates;
using OneClickDesktop.RabbitModule.Common.RabbitMessage;

namespace OneClickDesktop.Overseer.Messages
{
    public class ModelReportMessage: ModelReportTemplate, IRabbitMessage
    {
        public string AppId { get; set; } = "NaRazieTestowoUstawmyCosTakiego";
        public string Type { get; set; } = ModelReportTemplate.MessageTypeName;
        public object Message { get; set; }
        
        public ModelReportMessage(BackendClasses.Model.VirtualizationServer model): base(model)
        {
            Message = model;
        }
    }
}