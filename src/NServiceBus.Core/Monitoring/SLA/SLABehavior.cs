namespace NServiceBus
{
    using System;
    using System.Threading.Tasks;
    using Pipeline;

    class SLABehavior : PhysicalMessageProcessingStageBehavior
    {
        EstimatedTimeToSLABreachCalculator breachCalculator;

        public SLABehavior(EstimatedTimeToSLABreachCalculator breachCalculator)
        {
            this.breachCalculator = breachCalculator;
        }

        public override async Task Invoke(Context context, Func<Task> next)
        {
            await next().ConfigureAwait(false);

            DateTime timeSent;

            if (!context.TryGet("IncomingMessage.TimeSent", out timeSent))
            {
                return;
            }

            breachCalculator.Update(timeSent, context.Get<DateTime>("IncomingMessage.ProcessingStarted"), context.Get<DateTime>("IncomingMessage.ProcessingEnded"));
        }

        public class Registration:RegisterStep
        {
            public Registration()
                : base("SLA", typeof(SLABehavior), "Updates the SLA performance counter")
            {
                InsertBefore(WellKnownStep.ProcessingStatistics);
            }
        }
    }
}