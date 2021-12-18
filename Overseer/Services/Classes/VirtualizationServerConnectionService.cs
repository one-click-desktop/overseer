using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OneClickDesktop.BackendClasses.Communication;
using OneClickDesktop.Overseer.Helpers.Settings;
using OneClickDesktop.Overseer.Messages;
using OneClickDesktop.Overseer.Services.Interfaces;
using OneClickDesktop.RabbitModule.Common.EventArgs;
using OneClickDesktop.RabbitModule.Common.RabbitMessage;
using OneClickDesktop.RabbitModule.Overseer;

namespace OneClickDesktop.Overseer.Services.Classes
{
    public struct VirtualizationServerConnectionParameters
    {
        public string RabbitMQHostname { get; set; }
        public int RabbitMQPort { get; set; }
        public IReadOnlyDictionary<string, Type> MessageTypeMappings { get; set; }
    }
    
    public class VirtualizationServerConnectionService: IVirtualizationServerConnectionService, IDisposable
    {
        private readonly ILogger<VirtualizationServerConnectionService> logger;
        private readonly ISystemModelService modelService;
        private readonly IOptions<OneClickDesktopSettings> conf;
        private OverseerClient connection;
        
        private BlockingCollection<(IRabbitMessage message, string queue)> requests  = new BlockingCollection<(IRabbitMessage message, string queue)>();
        private CancellationTokenSource tokenSrc = new CancellationTokenSource();
        private CancellationToken token;
        private Thread senderThread = null;
        private Thread modelRequestThread = null;

        public VirtualizationServerConnectionService(
            ISystemModelService modelService,
            ILogger<VirtualizationServerConnectionService> logger,
            IOptions<OneClickDesktopSettings> ocdConfiguration)
        {
            this.logger = logger;
            conf = ocdConfiguration;

            this.logger.LogInformation("Starting VirtualizationServerConnectionService");
            
            this.modelService = modelService;
            var parameters = new VirtualizationServerConnectionParameters()
            {
                MessageTypeMappings = TypeMappings.OverseerReceiveMapping,
                RabbitMQHostname = conf.Value.RabbitMQHostname,
                RabbitMQPort = conf.Value.RabbitMQPort
            };
            connection = new OverseerClient(parameters.RabbitMQHostname, parameters.RabbitMQPort,
                parameters.MessageTypeMappings);
            
            //Zarejestruj metode do otrzymywania inforacji o modelu (otrzymywanie w oddzielnym wątku)
            connection.Received += ReceiveModelReport;
            
            //Wystartuj wątek do wysyłania wiadomości
            token = tokenSrc.Token;
            senderThread = new Thread(ConsumeMultithreadRequests);
            modelRequestThread = new Thread(RequestModelUpdate);
            senderThread.Start();
            modelRequestThread.Start();
        }

        /// <summary>
        /// Add request to message queue
        /// </summary>
        /// <param name="message">Message to send</param>
        /// <param name="queue">Target RabbitMQ queue, null to end to all servers</param>
        public void SendRequest(IRabbitMessage message, string queue)
        {
            if (message != null)
            {
                requests.Add((message, queue));
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void ReceiveModelReport(object sender, MessageEventArgs args)
        {
            if (args.RabbitMessage.Type == ModelReportMessage.MessageTypeName)
            {
                var data = ModelReportMessage.ConversionReceivedData(args.RabbitMessage.Body);
                modelService.UpdateServerInfo(data);
                logger.LogInformation($"Updated server model");
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        private void ConsumeMultithreadRequests()
        {
            while (true)
            {
                if (token.IsCancellationRequested)
                    return;
                var msg = requests.Take();

                if (msg.queue != null)
                {
                    msg.message.SenderIdentifier = conf.Value.OverseerId;
                    connection.SendToVirtServer(msg.queue, msg.message);
                    logger.LogDebug($"Message sent to virtualization server {msg.queue} {JsonSerializer.Serialize(msg.message)}");
                }
                else
                {
                    connection.SendToAllVirtServers(msg.message);
                    logger.LogDebug($"Message sent to virtualization servers {JsonSerializer.Serialize(msg.message)}");
                }
                logger.LogInformation($"Send message {msg.message.Type}");
            }
        }

        private void RequestModelUpdate()
        {
            while (true)
            {
                if (token.IsCancellationRequested)
                    return;
                
                SendRequest(new ModelReportMessage(null), null);
                logger.LogInformation($"Requesting model update from virtualization servers");
                
                Thread.Sleep(conf.Value.SessionWaitingTimeout * 60000);//from minutes to miliseconds
            }
        }

        public void Dispose()
        {
            if (tokenSrc != null)
            {
                tokenSrc.Cancel();
                senderThread?.Join();
                modelRequestThread?.Join();
            }

            connection?.Dispose();
        }
    }
}