namespace NServiceBus
{
    using System;
    using System.Threading.Tasks;
    using NServiceBus.Pipeline.Contexts;

    class MutateOutgoingPhysicalMessageBehavior : PhysicalOutgoingContextStageBehavior
    {
        public override Task Invoke(Context context, Func<Task> next)
        {
            foreach (var mutator in context.Builder.BuildAll<IMutateOutgoingPhysicalContext>())
            {
                mutator.MutateOutgoing(new OutgoingPhysicalMutatorContext(context.Body,context.Headers));
            }

            return next();
        }
    }
}