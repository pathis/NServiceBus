namespace NServiceBus
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using NServiceBus.Pipeline.Contexts;
    using NServiceBus.Transports;
    using NServiceBus.Unicast;
    using NServiceBus.Unicast.Queuing;

    class DispatchMessageToTransportBehavior : PhysicalOutgoingContextStageBehavior
    {
        public ISendMessages MessageSender { get; set; }

        public IPublishMessages MessagePublisher { get; set; }

        public IDeferMessages MessageDeferral { get; set; }

        public override async Task Invoke(Context context, Func<Task> next)
        {
            await InvokeNative(context);

            await next().ConfigureAwait(false);
        }

        public Task InvokeNative(Context context)
        {
            var message = new OutgoingMessage(context.MessageId, context.Headers, context.Body);

            if (context.Intent == MessageIntentEnum.Publish)
            {
                return NativePublish(new TransportPublishOptions(context.MessageType, context.DeliveryMessageOptions.TimeToBeReceived, context.DeliveryMessageOptions.NonDurable ?? false), message);
            }

            return NativeSendOrDefer(context.DeliveryMessageOptions, message);
        }

        public async Task NativePublish(TransportPublishOptions publishOptions, OutgoingMessage message)
        {
            SetTransportHeaders(publishOptions.TimeToBeReceived, publishOptions.NonDurable, message);

            try
            {
                await Publish(message, publishOptions);
            }
            catch (QueueNotFoundException ex)
            {
                var messageDescription = "ControlMessage";

                string enclosedMessageTypes;

                if (message.Headers.TryGetValue(Headers.EnclosedMessageTypes, out enclosedMessageTypes))
                {
                    messageDescription = enclosedMessageTypes;
                }
                throw new Exception(string.Format("The destination queue '{0}' could not be found. You may have misconfigured the destination for this kind of message ({1}) in the MessageEndpointMappings of the UnicastBusConfig section in your configuration file. " + "It may also be the case that the given queue just hasn't been created yet, or has been deleted.", ex.Queue, messageDescription), ex);
            }
        }

        public async Task NativeSendOrDefer(DeliveryMessageOptions deliveryMessageOptions, OutgoingMessage message)
        {
            SetTransportHeaders(deliveryMessageOptions.TimeToBeReceived, deliveryMessageOptions.NonDurable, message);

            try
            {
                await SendOrDefer(message, deliveryMessageOptions as SendMessageOptions);
            }
            catch (QueueNotFoundException ex)
            {
                var messageDescription = "ControlMessage";

                string enclosedMessageTypes;

                if (message.Headers.TryGetValue(Headers.EnclosedMessageTypes, out enclosedMessageTypes))
                {
                    messageDescription = enclosedMessageTypes;
                }
                throw new Exception(string.Format("The destination queue '{0}' could not be found. You may have misconfigured the destination for this kind of message ({1}) in the MessageEndpointMappings of the UnicastBusConfig section in your configuration file. " + "It may also be the case that the given queue just hasn't been created yet, or has been deleted.", ex.Queue, messageDescription), ex);
            }
        }

        void SetTransportHeaders(TimeSpan? timeToBeReceived, bool? nonDurable, OutgoingMessage message)
        {
            message.Headers[Headers.MessageId] = message.MessageId;


            if (timeToBeReceived.HasValue)
            {
                message.Headers[Headers.TimeToBeReceived] = timeToBeReceived.Value.ToString("c");
            }

            if (nonDurable.HasValue && nonDurable.Value)
            {
                message.Headers[Headers.NonDurableMessage] = true.ToString();
            }
        }

        Task SendOrDefer(OutgoingMessage message, SendMessageOptions options)
        {
            if ((options.DelayDeliveryFor.HasValue && options.DelayDeliveryFor > TimeSpan.Zero) ||
                (options.DeliverAt.HasValue && options.DeliverAt.Value.ToUniversalTime() > DateTime.UtcNow))
            {
                SetIsDeferredHeader(message.Headers);
                MessageDeferral.Defer(message, new TransportDeferOptions(
                    options.Destination,
                    options.DelayDeliveryFor,
                    options.DeliverAt,
                    options.NonDurable ?? true,
                    options.EnlistInReceiveTransaction));

                return Task.FromResult(true);
            }

            return MessageSender.Send(message, new TransportSendOptions(options.Destination,
                                                                    options.TimeToBeReceived,
                                                                    options.NonDurable ?? true,
                                                                    options.EnlistInReceiveTransaction));
        }

        static void SetIsDeferredHeader(Dictionary<string, string> headers)
        {
            headers[Headers.IsDeferredMessage] = true.ToString();
        }

        Task Publish(OutgoingMessage message, TransportPublishOptions publishOptions)
        {
            if (MessagePublisher == null)
            {
                throw new InvalidOperationException("No message publisher has been registered. If you're using a transport without native support for pub/sub please enable the message driven publishing feature by calling config.EnableFeature<MessageDrivenSubscriptions>() in your configuration");
            }
            return MessagePublisher.Publish(message, publishOptions);
        }


    }
}