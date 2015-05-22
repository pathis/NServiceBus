namespace NServiceBus
{
    using System;
    using System.Threading.Tasks;
    using NServiceBus.Pipeline;
    using NServiceBus.Transports;

    class ForwardBehavior : PhysicalMessageProcessingStageBehavior
    {
        public IAuditMessages MessageAuditer { get; set; }

        public string ForwardReceivedMessagesTo { get; set; }


        public override async Task Invoke(Context context, Func<Task> next)
        {
            await next().ConfigureAwait(false);

            context.PhysicalMessage.RevertToOriginalBodyIfNeeded();

            MessageAuditer.Audit(new OutgoingMessage(context.PhysicalMessage.Id,context.PhysicalMessage.Headers,context.PhysicalMessage.Body),new TransportSendOptions(ForwardReceivedMessagesTo));
        }

        public class Registration : RegisterStep
        {
            public Registration()
                : base("ForwardMessageTo", typeof(ForwardBehavior), "Forwards message to the specified queue in the UnicastBus config section.")
            {
                InsertBefore(WellKnownStep.ExecuteUnitOfWork);
            }
        }
    }
}