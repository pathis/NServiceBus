namespace NServiceBus.Core.Tests.Audit
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using NServiceBus.Pipeline.Contexts;
    using NUnit.Framework;

    [TestFixture]
    public class AttachCausationHeadersBehaviorTests
    {
        [Test]
        public async void Should_set_the_conversation_id_to_new_guid_when_not_sent_from_handler()
        {
            var behavior = new AttachCausationHeadersBehavior();
            var context = new PhysicalOutgoingContextStageBehavior.Context(null, new OutgoingContext(null, null, new Dictionary<string, string>(), null, MessageIntentEnum.Send, null, null, null));

            await behavior.Invoke(context,()=> Task.FromResult(true));

            Assert.AreNotEqual(Guid.Empty,Guid.Parse(context.Headers[Headers.ConversationId]));
        }

        [Test]
        public async void Should_set_the_conversation_id_to_conversation_id_of_incoming_message()
        {
            var incomingConvId = Guid.NewGuid().ToString();

            var behavior = new AttachCausationHeadersBehavior();
            var context = new PhysicalOutgoingContextStageBehavior.Context(null, new OutgoingContext(null, null, new Dictionary<string, string>(), null, MessageIntentEnum.Send, null, null,null));

            context.Set(TransportReceiveContext.IncomingPhysicalMessageKey,new TransportMessage("xyz",new Dictionary<string, string>{{Headers.ConversationId,incomingConvId}}));

            await behavior.Invoke(context, () => Task.FromResult(true));

            Assert.AreEqual(incomingConvId, context.Headers[Headers.ConversationId]);
        }

        [Test]
        public async void Should_not_override_a_conversation_id_specified_by_the_user()
        {
            var userConvId = Guid.NewGuid().ToString();

            var behavior = new AttachCausationHeadersBehavior();
            var context = new PhysicalOutgoingContextStageBehavior.Context(null, new OutgoingContext(null, null, new Dictionary<string, string> { { Headers.ConversationId, userConvId } }, null, MessageIntentEnum.Send, null, null, null));

            await behavior.Invoke(context, () => Task.FromResult(true));

            Assert.AreEqual(userConvId, context.Headers[Headers.ConversationId]);
            
        }

        [Test]
        public async void Should_set_the_related_to_header_with_the_id_of_the_current_message()
        {
            
            var behavior = new AttachCausationHeadersBehavior();
            var context = new PhysicalOutgoingContextStageBehavior.Context(null, new OutgoingContext(null, null, new Dictionary<string, string>(), null, MessageIntentEnum.Send, null, null, null));

            context.Set(TransportReceiveContext.IncomingPhysicalMessageKey, new TransportMessage("the message id", new Dictionary<string, string>()));

            await behavior.Invoke(context, () => Task.FromResult(true));

            Assert.AreEqual("the message id", context.Headers[Headers.RelatedTo]);
        
        }
    }
}