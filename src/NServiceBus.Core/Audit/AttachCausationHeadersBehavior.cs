namespace NServiceBus
{
    using System;
    using System.Threading.Tasks;
    using NServiceBus.Pipeline;
    using NServiceBus.Pipeline.Contexts;

    class AttachCausationHeadersBehavior : PhysicalOutgoingContextStageBehavior
    {
        public override Task Invoke(Context context, Func<Task> next)
        {
            ApplyHeaders(context);

            return next();
         }

        void ApplyHeaders(Context context)
        {
            if (context.Headers.ContainsKey(Headers.ConversationId))
                return;

            var conversationId = CombGuid.Generate().ToString();

            TransportMessage incomingMessage;

            if (context.TryGet(TransportReceiveContext.IncomingPhysicalMessageKey,out incomingMessage))
            {
                context.Headers[Headers.RelatedTo] = incomingMessage.Id;

                string conversationIdFromCurrentMessageContext;
                if (incomingMessage.Headers.TryGetValue(Headers.ConversationId, out conversationIdFromCurrentMessageContext))
                {
                    conversationId = conversationIdFromCurrentMessageContext;
                }
            }

            context.Headers[Headers.ConversationId] = conversationId;
        }

        public class Registration : RegisterStep
        {
            public Registration()
                : base("AttachCausationHeaders", typeof(AttachCausationHeadersBehavior), "Adds related to and conversation id headers to outgoing messages")
            {
            }
        }
    }
}