using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using OneClickDesktop.RabbitModule.Common.RabbitMessage;
using OneClickDesktop.RabbitModule.Overseer;

namespace OneClickDesktop.Overseer.IndependentServices
{    
    //Tutaj kilka bardzo waznych uwag nt. wielowątkowości ASP.NET
    //https://volosoft.com/blog/ASP.NET-Core-Dependency-Injection-Best-Practices,-Tips-Tricks

    public class VirtualizationServersCommunicationParameters
    {
        public string RabbitMQHostname;
        public int RabbitMQPort;
        public IReadOnlyDictionary<string, Type> MessageTypeMappings;
    }
    
    /// <summary>
    /// Klasa odpowiada za utowrzenie wątku do obsługi komuniakcji z RabbitMQ.
    /// Otrzymuje 
    /// </summary>
    public  class VirtualizationServersCommunication : IDisposable
    {
        private OverseerClient connection;
        private CancellationTokenSource tokenSrc = new CancellationTokenSource();
        private Thread senderThread = null;
        private Thread readerThread = null;
        private CancellationToken token;

        public ConcurrentQueue<IRabbitMessage> Requests { get; } = new ConcurrentQueue<IRabbitMessage>();

        private VirtualizationServersCommunication(VirtualizationServersCommunicationParameters parameters)
        {
            connection = new OverseerClient(parameters.RabbitMQHostname, parameters.RabbitMQPort,
                parameters.MessageTypeMappings);
            
            token = tokenSrc.Token;
            senderThread = new Thread(new ThreadStart(this.ConsumeRequests));
            senderThread.Start();
            readerThread = new Thread(new ThreadStart(this.ConsumeRequests));
            senderThread.Start();
        }

        private void ConsumeRequests()
        {
            while (true)
            {
                Thread.Sleep(1000);
            }
        }


        public void Dispose()
        {
            
            if (tokenSrc != null)
            {
                tokenSrc.Cancel();
                senderThread?.Join();
                readerThread?.Join();
                tokenSrc.Dispose();
            }
            
            connection?.Dispose();
        }
    }
}