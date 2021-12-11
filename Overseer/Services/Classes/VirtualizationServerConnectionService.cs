using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Text.Json;
using OneClickDesktop.BackendClasses.Model;
using OneClickDesktop.Overseer.IndependentServices;
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
        
        private CancellationTokenSource tokenSrc = new CancellationTokenSource();
        private Thread senderThread = null;
        private CancellationToken token;
        
        public BlockingCollection<IRabbitMessage> Requests { get; } = new BlockingCollection<IRabbitMessage>();

        public VirtualizationServerConnectionService(ISystemModelService modelService)
        {
            logger.Info("Starting VirtualizationServerConnectionService");
            
            this.modelService = modelService;
            //[TODO][CONFIG] Wynieść do konfiguracji!
            VirtualizationServerConnectionParameters parameters = new VirtualizationServerConnectionParameters()
            {
                MessageTypeMappings = new Dictionary<string, Type>()
                {
                    {ModelReportMessage.StaticType, typeof(VirtualizationServer)}
                },
                RabbitMQHostname = "localhost",
                RabbitMQPort = 5672
            };
            connection = new OverseerClient(parameters.RabbitMQHostname, parameters.RabbitMQPort,
                parameters.MessageTypeMappings);
            
            //Zarejestruj metode do otrzymywania inforacji o modelu (otrzymywanie w oddzielnym wątku)
            connection.Received += ReceiveModelReport;
            
            //Wystartuj wątek do wysyłania wiadomości
            token = tokenSrc.Token;
            senderThread = new Thread(new ThreadStart(ConsumeMultithreadRequests));
            senderThread.Start();
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void ReceiveModelReport(object sender, MessageEventArgs args)
        {
            if (args.RabbitMessage.Type == ModelReportMessage.StaticType)
            {
                VirtualizationServer data = args.RabbitMessage.Message as VirtualizationServer;
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
                IRabbitMessage msg = Requests.Take();

                //logger.Warn("Wrong message to sent");

                logger.Info($"Message sent to virtualization servers {JsonSerializer.Serialize(msg)}");
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