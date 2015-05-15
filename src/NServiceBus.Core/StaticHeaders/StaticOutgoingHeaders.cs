namespace NServiceBus.StaticHeaders
{
    using System.Collections.Generic;
    using NServiceBus.Features;

    class StaticHeaders:Feature
    {
        public StaticHeaders()
        {
            EnableByDefault();
            Prerequisite(c=>c.Settings.HasSetting<StaticOutgoingHeaders>(),"No static headers registered");

        }

        protected internal override void Setup(FeatureConfigurationContext context)
        {
            var headers = context.Settings.Get<StaticOutgoingHeaders>();

            context.Container.ConfigureComponent(b => new ApplyStaticHeadersBehavior(headers), DependencyLifecycle.SingleInstance);
            context.MainPipeline.Register("ApplyStaticHeaders", typeof(ApplyStaticHeadersBehavior), "Applies static headers to outgoing messages");
        }
    }

    class StaticOutgoingHeaders:Dictionary<string,string>
    {
       
    }
}