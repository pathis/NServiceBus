namespace NServiceBus
{
    using System;
    using NServiceBus.Pipeline;
    using NServiceBus.Pipeline.Contexts;
    using NServiceBus.StaticHeaders;

    class ApplyStaticHeadersBehavior:Behavior<OutgoingContext>
    {
        readonly StaticOutgoingHeaders staticHeaders;

        public ApplyStaticHeadersBehavior(StaticOutgoingHeaders staticHeaders)
        {
            this.staticHeaders = staticHeaders;
        }

        public override void Invoke(OutgoingContext context, Action next)
        {
            foreach (var staticHeader in staticHeaders)
            {
                context.SetHeader(staticHeader.Key,staticHeader.Value);
            }

            next();
        }
    }
}