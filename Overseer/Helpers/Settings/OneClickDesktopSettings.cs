

namespace OneClickDesktop.Overseer.Helpers.Settings
{
    /// <summary>
    /// Configuration for OneClickDesktop system
    /// </summary>
    public class OneClickDesktopSettings
    {
        /// <summary>
        /// Overseer system-wide unique id
        /// </summary>
        public string OverseerId { get; set; } = "overseer-test";

        /// <summary>
        /// Internal RabbitMQ broker access address
        /// </summary>
        public string RabbitMQHostname { get; set; } = "localhost";

        /// <summary>
        /// Internal RabbitMQ broker access port
        /// </summary>
        public int RabbitMQPort { get; set; } = 5672;

        /// <summary>
        /// Model requests to virtualization servers interval (in seconds)
        /// </summary>
        public int ModelUpdateInterval { get; set; } = 60;
        /// <summary>
        /// Timeout in minutes for machine shoutdown after lossing connection with client
        /// </summary>
        public int DomainShutdownTimeout { get; set; } = 15;

        /// <summary>
        /// Domain Shutdown interval checking - should be divider of DomainShutdownTimeout (in seconds)
        /// </summary>
        public int DomainShutdownCounterInterval { get; set; } = 30;
    }
}