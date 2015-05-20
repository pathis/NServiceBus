namespace NServiceBus.Core.Tests.AutomaticSubscriptions
{
    using System;
    using System.Collections.Generic;
    using NServiceBus.Routing.AutomaticSubscriptions;
    using NServiceBus.Unicast;
    using NServiceBus.Unicast.Routing;
    using NUnit.Framework;
    using Conventions = NServiceBus.Conventions;

    public class AutoSubscriptionContext
    {
        [SetUp]
        public void SetUp()
        {
            autoSubscriptionStrategy = new AutoSubscriptionStrategy
            {
                HandlerRegistry = new MessageHandlerRegistry(new Conventions()),
                MessageRouter = new StaticMessageRouter(KnownMessageTypes()),
                Conventions = new Conventions()
            };
        }

        protected virtual IEnumerable<Type> KnownMessageTypes()
        {
            return new List<Type>();
        }

        protected void RegisterMessageHandlerType<T>()
        {
            autoSubscriptionStrategy.HandlerRegistry.RegisterHandler(typeof(T));
        }

        protected void RegisterMessageType<T>(string address)
        {
            autoSubscriptionStrategy.MessageRouter.RegisterMessageRoute(typeof(T), address);
        }

        internal AutoSubscriptionStrategy autoSubscriptionStrategy;
    }
}