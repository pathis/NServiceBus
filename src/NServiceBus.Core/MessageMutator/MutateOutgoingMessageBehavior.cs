namespace NServiceBus
{
    using System;
    using System.Threading.Tasks;
    using NServiceBus.MessageMutator;
    using NServiceBus.Pipeline;
    using NServiceBus.Pipeline.Contexts;

    class MutateOutgoingMessageBehavior : Behavior<OutgoingContext>
    {
        public override Task Invoke(OutgoingContext context, Func<Task> next)
        {
            var instanceType = context.MessageInstance.GetType();

            foreach (var mutator in context.Builder.BuildAll<IMutateOutgoingMessages>())
            {
                context.MessageInstance = mutator.MutateOutgoing(context.MessageInstance);
            }

            //if instance type is different we assumes that the user want to change the type
            // this should be made more explicit when we change the mutator api
            if (instanceType != context.MessageInstance.GetType())
            {
                context.MessageType = context.MessageInstance.GetType();
            }

            return next();
        }
    }
}