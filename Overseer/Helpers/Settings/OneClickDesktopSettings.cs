

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
        public string OverseerId { get; set; }
        /// <summary>
        /// Internal RabbitMQ broker access address
        /// </summary>
        public string RabbitMQHostname { get; set; }
        /// <summary>
        /// Internal RabbitMQ broker access port
        /// </summary>
        public int RabbitMQPort { get; set; }
        /// <summary>
        /// Timeout in minutes for machine shoutdown after lossing connection with client
        /// </summary>
        public int SessionWaitingTimeout { get; set; }
        /// <summary>
        /// Model requests to virtualization servers interval (in seconds)
        /// </summary>
        public int ModelUpdateInterval { get; set; }
    }
}