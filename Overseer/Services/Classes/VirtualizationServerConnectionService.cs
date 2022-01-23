using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
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
using static OneClickDesktop.RabbitModule.Common.Constants;

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

            //Zarejestruj usuwanie servera wirtualizacji gdy brakuje odpowiedzi
            connection.Return += DeadServerHandler;
            
            //Wystartuj wątek do wysyłania wiadomości
            token = tokenSrc.Token;
            senderThread = new Thread(ConsumeMultithreadRequests);
            modelRequestThread = new Thread(RequestModelUpdate);
            senderThread.Start();
            modelRequestThread.Start();
        }

        #region Thread workers
        /// <summary>
        /// 
        /// </summary>
        private void ConsumeMultithreadRequests()
        {
            while (true)
            {
                IRabbitMessage message;
                string queue;
                
                try
                {
                    (message, queue) = requests.Take(token);
                }
                catch (OperationCanceledException e)
                {
                    return;
                }

                if (queue != null)
                {
                    message.SenderIdentifier = conf.Value.OverseerId;
                    connection.SendToVirtServer(queue, message);
                    logger.LogDebug($"Message sent to virtualization server {queue} {JsonSerializer.Serialize(message)}");
                }
                else
                {
                    connection.SendToAllVirtServers(message);
                    logger.LogDebug($"Message sent to virtualization servers {JsonSerializer.Serialize(message)}");
                }
                logger.LogInformation($"Send message {message.Type}");
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
                
                ProbeDeadServers(modelService.GetServerQueues());
                
                Thread.Sleep(conf.Value.ModelUpdateInterval * 1000);//from seconds to miliseconds
            }
        }
        
        
        #endregion
        
        #region Event handlers
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void ReceiveModelReport(object sender, MessageEventArgs args)
        {
            switch (args.RabbitMessage.Type)
            {
                case ModelReportMessage.MessageTypeName:
                    var data = ModelReportMessage.ConversionReceivedData(args.RabbitMessage.Body);
                    modelService.UpdateServerInfo(data);
                    logger.LogInformation($"Updated server model");
                    break;
                case PingMessage.MessageTypeName:
                    logger.LogDebug("ping message - ignoring");
                    break;
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void DeadServerHandler(object sender, ReturnEventArgs args)
        {
            if (args.Exchange == Exchanges.VirtServersDirect && args.ReturnReason == ReturnEventArgs.Reason.NO_QUEUE)
            {
                modelService.RemoveDeadServer(args.RoutingKey);
                logger.LogInformation($"Server from queue {args.RoutingKey} is dead. Removing from model.");
            }
        }
        #endregion

        public void ProbeDeadServers(IEnumerable<string> queueList)
        {
            foreach (string queue in queueList)
            {
                connection.SendToVirtServer(queue, new PingMessage());
            }
        }
        
        /// <summary>
        /// Add request to message queue
        /// </summary>
        /// <param name="message">Message to send</param>
        /// <param name="queue">Target RabbitMQ queue, null to send to all servers</param>
        public void SendRequest(IRabbitMessage message, string queue = null)
        {
            if (message != null)
            {
                requests.Add((message, queue));
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