namespace NServiceBus
{
    using System;
    using System.Threading.Tasks;
    using NServiceBus.Outbox;
    using Pipeline;

    class OutboxRecordBehavior : PhysicalMessageProcessingStageBehavior
    {
        public IOutboxStorage OutboxStorage { get; set; }

        public override async Task Invoke(Context context, Func<Task> next)
        {
            await next();
            
            if (context.handleCurrentMessageLaterWasCalled)
            {
                return;
            }

            var outboxMessage = context.Get<OutboxMessage>();

            OutboxStorage.Store(outboxMessage.MessageId, outboxMessage.TransportOperations);
        }

        public class OutboxRecorderRegistration : RegisterStep
        {
            public OutboxRecorderRegistration()
                : base("OutboxRecorder", typeof(OutboxRecordBehavior), "Records all action to the outbox storage")
            {
                InsertBefore(WellKnownStep.MutateIncomingTransportMessage);
                InsertAfter(WellKnownStep.ExecuteUnitOfWork);
            }
        }
    }
}