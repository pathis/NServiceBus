namespace NServiceBus
{
    using System;
    using System.Threading.Tasks;
    using NServiceBus.Features;
    using NServiceBus.Pipeline;
    using NServiceBus.Pipeline.Contexts;

    class SetLegacyReturnCodeBehavior : Behavior<OutgoingContext>
    {
        public override Task Invoke(OutgoingContext context, Func<Task> next)
        {
            if (CallbackSupport.IsLegacyEnumResponse(context.MessageType))
            {
                context.Headers[Headers.ReturnMessageErrorCodeHeader] = ((dynamic)context.MessageInstance).ReturnCode;
            }

            return next();
        }

        public class Registration : RegisterStep
        {
            public Registration()
                : base("SetLegacyReturnCode", typeof(SetLegacyReturnCodeBehavior), "Promotes the legacy return code to a header in order to be backwards compatible with v5 and below")
            {

            }
        }
    }
}