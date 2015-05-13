namespace NServiceBus
{
    using System;
    using System.Threading.Tasks;
    using NServiceBus.Features;
    using NServiceBus.Pipeline;
    using NServiceBus.Pipeline.Contexts;

    class ConvertLegacyEnumResponseToLegacyControlMessageBehavior : PhysicalOutgoingContextStageBehavior
    {
        public override Task Invoke(Context context, Func<Task> next)
        {
            if (CallbackSupport.IsLegacyEnumResponse(context.MessageType))
            {
                context.Headers[Headers.ControlMessageHeader] = true.ToString();

                context.Headers.Remove(Headers.ContentType);
                context.Headers.Remove(Headers.EnclosedMessageTypes);

                context.Body = new byte[0];
            }

            return next();
        }

        public class Registration : RegisterStep
        {
            public Registration()
                : base("ConvertLegacyEnumResponseToLegacyControlMessage", typeof(ConvertLegacyEnumResponseToLegacyControlMessageBehavior), "Converts the legacy response message to a control message to support backward compatibiliy")
            {
                InsertAfterIfExists(WellKnownStep.MutateOutgoingTransportMessage);
                InsertBefore(WellKnownStep.DispatchMessageToTransport);
            }
        }
    }
}