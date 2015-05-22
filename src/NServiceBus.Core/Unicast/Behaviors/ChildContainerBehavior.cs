namespace NServiceBus
{
    using System;
    using System.Threading.Tasks;
    using ObjectBuilder;

    class ChildContainerBehavior : PhysicalMessageProcessingStageBehavior
    {
        public override async Task Invoke(Context context, Func<Task> next)
        {
            using (var childBuilder = context.Builder.CreateChildBuilder())
            {
                context.Set(childBuilder);
                try
                {
                    await next().ConfigureAwait(false);
                }
                finally
                {
                    context.Remove<IBuilder>();
                }
            }
        }
    }
}