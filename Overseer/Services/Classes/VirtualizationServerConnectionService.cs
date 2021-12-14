using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Text.Json;
using OneClickDesktop.BackendClasses.Communication;
using OneClickDesktop.BackendClasses.Model;
using OneClickDesktop.Overseer.Messages;
using OneClickDesktop.Overseer.Services.Interfaces;
using OneClickDesktop.RabbitModule.Common.EventArgs;
using OneClickDesktop.RabbitModule.Common.RabbitMessage;
using OneClickDesktop.RabbitModule.Overseer;

namespace OneClickDesktop.Overseer.Services.Classes
{
    public class VirtualizationServerConnectionParameters
    {
        public string RabbitMQHostname;
        public int RabbitMQPort;
        public IReadOnlyDictionary<string, Type> MessageTypeMappings;
    }
    
    public class VirtualizationServerConnectionService: IVirtualizationServerConnectionService, IDisposable
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private readonly ISystemModelService modelService;
        private OverseerClient connection;
        
        private BlockingCollection<(IRabbitMessage message, string queue)> requests  = new BlockingCollection<(IRabbitMessage message, string queue)>();
        private CancellationTokenSource tokenSrc = new CancellationTokenSource();
        private Thread senderThread = null;
        private Thread modelRequestThread = null;
        private CancellationToken token;

        public VirtualizationServerConnectionService(ISystemModelService modelService)
        {
            logger.Info("Starting VirtualizationServerConnectionService");
            
            this.modelService = modelService;
            //[TODO][CONFIG] Wynieść do konfiguracji!
            VirtualizationServerConnectionParameters parameters = new VirtualizationServerConnectionParameters()
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
                VirtualizationServer data = ModelReportMessage.ConversionReceivedData(args.RabbitMessage.Message);
                modelService.UpdateServerInfo(data);
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
                }
                else
                {
                    connection.SendToAllVirtServers(msg.message);
                }
                
                // imo lepiej przeciążyć ToString zamiast używać serializera do logowania wiadomości
                logger.Info($"Message sent to virtualization servers {JsonSerializer.Serialize(msg)}");
            }
        }

        private void RequestModelUpdate()
        {
            while (true)
            {
                if (token.IsCancellationRequested)
                    return;
                
                SendRequest(new ModelReportMessage(null), null);
                Thread.Sleep(Configuration.ModelUpdateWait);
                
                logger.Info($"Requesting model update from virtualization servers");
            }
        }

        public void Dispose()
        {
            if (tokenSrc != null)
            {
                tokenSrc.Cancel();
                senderThread?.Join();
            }

            connection?.Dispose();
        }
    }
}