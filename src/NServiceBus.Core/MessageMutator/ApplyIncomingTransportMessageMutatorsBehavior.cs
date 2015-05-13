namespace NServiceBus
{
    using System;
    using System.Threading.Tasks;
    using NServiceBus.MessageMutator;


    class ApplyIncomingTransportMessageMutatorsBehavior : PhysicalMessageProcessingStageBehavior
    {
        public override Task Invoke(Context context, Func<Task> next)
        {
            var mutators = context.Builder.BuildAll<IMutateIncomingTransportMessages>();

            foreach (var mutator in mutators)
            {
                mutator.MutateIncoming(context.PhysicalMessage);
            }

            return next();
        }
    }
}