namespace NServiceBus.Transports
{
    using System.Threading.Tasks;

    /// <summary>
    /// Requests a message to be published
    /// </summary>
    public interface IPublishMessages
    {
        /// <summary>
        /// Publishes the given messages to all known subscribers
        /// </summary>
        Task Publish(OutgoingMessage message,TransportPublishOptions publishOptions);
    }
}