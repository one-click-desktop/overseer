using OneClickDesktop.RabbitModule.Common.RabbitMessage;

namespace OneClickDesktop.Overseer.Services.Interfaces
{
    public interface IVirtualizationServerConnectionService
    {
        public void SendRequest(IRabbitMessage message, string queue = null);
    }
}