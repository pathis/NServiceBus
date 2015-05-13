namespace NServiceBus
{
    using System;
    using System.Threading.Tasks;
    using NServiceBus.Pipeline;

    class EnforceMessageIdBehavior : PhysicalMessageProcessingStageBehavior
    {
        public override Task Invoke(Context context, Func<Task> next)
        {
            if (string.IsNullOrWhiteSpace(context.PhysicalMessage.Id))
            {     
                throw new MessageDeserializationException("Message without message id detected");    
            }

            return next();
        }

        public class Registration : RegisterStep
        {
            public Registration()
                : base("EnforceMessageId", typeof(EnforceMessageIdBehavior), "Makes sure that the message pulled from the transport contains a message id")
            {
                InsertBeforeIfExists("SecondLevelRetries");
                InsertBeforeIfExists("FirstLevelRetries");
                InsertBeforeIfExists("ReceivePerformanceDiagnosticsBehavior");
            }
        }
    }
}