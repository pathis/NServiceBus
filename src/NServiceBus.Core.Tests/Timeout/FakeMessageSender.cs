namespace NServiceBus.Core.Tests.Timeout
{
    using System.Threading.Tasks;
    using NServiceBus.Transports;

    public class FakeMessageSender : ISendMessages
    {
        private volatile int messagesSent;

        public int MessagesSent
        {
            get { return messagesSent; }
            set { messagesSent = value; }
        }

        public Task Send(OutgoingMessage message, TransportSendOptions sendOptions)
        {
            MessagesSent++;
            return Task.FromResult(true);
        }
    }
}