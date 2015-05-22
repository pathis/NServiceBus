namespace NServiceBus.Transports
{
    using System.Threading.Tasks;

    /// <summary>
    /// Abstraction of the capability to send messages.
    /// </summary>
    public interface ISendMessages
    {
        /// <summary>
        /// Sends the given <paramref name="message"/>
        /// </summary>
        Task Send(OutgoingMessage message, TransportSendOptions sendOptions);
    }
}