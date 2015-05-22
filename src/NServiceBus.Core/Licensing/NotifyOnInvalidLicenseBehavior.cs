namespace NServiceBus
{
    using System;
    using System.Diagnostics;
    using System.Threading.Tasks;
    using Logging;
    using Pipeline;

    class NotifyOnInvalidLicenseBehavior : PhysicalMessageProcessingStageBehavior
    {
        public override async Task Invoke(Context context, Func<Task> next)
        {
            context.PhysicalMessage.Headers[Headers.HasLicenseExpired] = true.ToString().ToLower();

            await next().ConfigureAwait(false);

            if (Debugger.IsAttached)
            {
                Log.Error("Your license has expired");
            }
        }

        static ILog Log = LogManager.GetLogger<NotifyOnInvalidLicenseBehavior>();

        public class Registration : RegisterStep
        {
            public Registration()
                : base("LicenseReminder", typeof(NotifyOnInvalidLicenseBehavior), "Enforces the licensing policy")
            {
                InsertBeforeIfExists(WellKnownStep.AuditProcessedMessage);
            }
        }
    }
}