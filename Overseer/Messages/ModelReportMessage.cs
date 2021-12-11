using OneClickDesktop.RabbitModule.Common.RabbitMessage;

namespace OneClickDesktop.Overseer.Messages
{
    public class ModelReportMessage: IRabbitMessage
    {
        public static string StaticType = "ModelReport";
        
        public string AppId { get; set; } = "NaRazieTestowoUstawmyCosTakiego";
        public string Type { get; set; } = StaticType;
        public object Message { get; set; }


        public ModelReportMessage(BackendClasses.Model.VirtualizationServer model)
        {
            Message = model;
        }

        public ModelReportMessage()
        {
            Message = null;
        }
    }
}