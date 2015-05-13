namespace NServiceBus
{
    using System;
    using System.Threading.Tasks;
    using NServiceBus.MessageMutator;
    using NServiceBus.Pipeline.Contexts;


    class ApplyIncomingMessageMutatorsBehavior : LogicalMessageProcessingStageBehavior
    {
        public override Task Invoke(Context context, Func<Task> next)
        {
            var current = context.IncomingLogicalMessage.Instance;

            foreach (var mutator in context.Builder.BuildAll<IMutateIncomingMessages>())
            {
                current = mutator.MutateIncoming(current);
                context.IncomingLogicalMessage.UpdateMessageInstance(current);
            }

            context.MessageType = context.IncomingLogicalMessage.Metadata.MessageType;
            return next();
        }
    }
}