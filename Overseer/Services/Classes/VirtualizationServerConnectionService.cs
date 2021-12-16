using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using NLog;
using OneClickDesktop.BackendClasses.Communication;
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
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private readonly ISystemModelService modelService;
        private OverseerClient connection;
        
        private BlockingCollection<(IRabbitMessage message, string queue)> requests  = new BlockingCollection<(IRabbitMessage message, string queue)>();
        private CancellationTokenSource tokenSrc = new CancellationTokenSource();
        private CancellationToken token;
        private Thread senderThread = null;
        private Thread modelRequestThread = null;

        public VirtualizationServerConnectionService(ISystemModelService modelService)
        {
            logger.Info("Starting VirtualizationServerConnectionService");
            
            this.modelService = modelService;
            //[TODO][CONFIG] Wynieść do konfiguracji!
            var parameters = new VirtualizationServerConnectionParameters()
            {
                MessageTypeMappings = TypeMappings.OverseerReceiveMapping,
                RabbitMQHostname = "localhost",
                RabbitMQPort = 5672
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
            if (args.RabbitMessage.MessageType == ModelReportMessage.MessageTypeName)
            {
                var data = ModelReportMessage.ConversionReceivedData(args.RabbitMessage.MessageBody);
                modelService.UpdateServerInfo(data);
                logger.Info($"Updated server model");
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
                    connection.SendToVirtServer(msg.queue, msg.message);
                    logger.Debug($"Message sent to virtualization server {msg.queue} {JsonSerializer.Serialize(msg.message)}");
                }
                else
                {
                    connection.SendToAllVirtServers(msg.message);
                    logger.Debug($"Message sent to virtualization servers {JsonSerializer.Serialize(msg.message)}");
                }
            }
        }

        private void RequestModelUpdate()
        {
            while (true)
            {
                if (token.IsCancellationRequested)
                    return;
                
                SendRequest(new ModelReportMessage(null), null);
                logger.Info($"Requesting model update from virtualization servers");
                
                Thread.Sleep(Configuration.ModelUpdateWait);
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