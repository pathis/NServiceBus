namespace NServiceBus
{
    using System;
    using System.Threading.Tasks;
    using NServiceBus.Hosting;
    using NServiceBus.Pipeline;

    class HostInformationBehavior : PhysicalMessageProcessingStageBehavior
    {
        readonly HostInformation hostInfo;

        public HostInformationBehavior(HostInformation hostInfo)
        {
            this.hostInfo = hostInfo;
        }

        public override Task Invoke(Context context, Func<Task> next)
        {
            context.PhysicalMessage.Headers[Headers.HostId] = hostInfo.HostId.ToString("N");
            context.PhysicalMessage.Headers[Headers.HostDisplayName] = hostInfo.DisplayName;

            return next();
        }

        public class Registration : RegisterStep
        {
            public Registration()
                : base("AddHostInformation", typeof(HostInformationBehavior), "Adds host information")
            {
            }
        }
    }
}