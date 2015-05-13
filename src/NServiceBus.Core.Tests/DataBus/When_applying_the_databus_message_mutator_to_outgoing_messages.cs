namespace NServiceBus.Core.Tests.DataBus
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;
    using NServiceBus.DataBus;
    using NServiceBus.Pipeline.Contexts;
    using NServiceBus.Unicast;
    using NUnit.Framework;
    using Conventions = NServiceBus.Conventions;

    [TestFixture]
    class When_applying_the_databus_message_mutator_to_outgoing_messages
    {
        [Test]
        public async void Outgoing_databus_properties_should_be_dehydrated()
        {
            var message = new MessageWithDataBusProperty
            {
                DataBusProperty = new DataBusProperty<string>("test")
            };

            var context = new OutgoingContext(null, new SendMessageOptions("MyEndpoint"), new Dictionary<string, string>(), "msg id", MessageIntentEnum.Send, null, message, null);
            
            var fakeDatabus = new FakeDataBus();
           
            var sendBehavior = new DataBusSendBehavior
            {
                DataBus = fakeDatabus,
                Conventions = new Conventions(),
                DataBusSerializer = new DefaultDataBusSerializer(),
            };
            await sendBehavior.Invoke(context, () => Task.FromResult(true));

            Assert.AreEqual(TimeSpan.MaxValue, fakeDatabus.TTBRUsed);
        }

        [Test]
        public async void Time_to_live_should_be_passed_on_the_databus()
        {
           var message = new MessageWithExplicitTimeToLive
            {
                DataBusProperty = new DataBusProperty<string>("test")
            };

           var context = new OutgoingContext(null, new SendMessageOptions("MyEndpoint") { TimeToBeReceived = TimeSpan.FromMinutes(1) }, new Dictionary<string, string>(), "msg id", MessageIntentEnum.Send, null, message, null);

           var fakeDatabus = new FakeDataBus();
           
           var sendBehavior = new DataBusSendBehavior
           {
               DataBus = fakeDatabus,
               Conventions = new Conventions(),
               DataBusSerializer = new DefaultDataBusSerializer(),
           };

           await sendBehavior.Invoke(context, () => Task.FromResult(true));

           Assert.AreEqual(TimeSpan.FromMinutes(1),fakeDatabus.TTBRUsed);
        }

        class FakeDataBus:IDataBus
        {
            public TimeSpan TTBRUsed;

            public Stream Get(string key)
            {
                throw new NotImplementedException();
            }

            public string Put(Stream stream, TimeSpan timeToBeReceived)
            {
                TTBRUsed = timeToBeReceived;
                return Guid.NewGuid().ToString();
            }

            public void Start()
            {
                throw new NotImplementedException();
            }
        }
    }
}