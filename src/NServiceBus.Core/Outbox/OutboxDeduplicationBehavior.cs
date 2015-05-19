﻿namespace NServiceBus
{
    using System;
    using System.Collections.Generic;
    using System.Transactions;
    using NServiceBus.Outbox;
    using NServiceBus.Pipeline;
    using NServiceBus.Transports;
    using NServiceBus.Unicast.Transport;

    class OutboxDeduplicationBehavior : PhysicalMessageProcessingStageBehavior
    {
        public OutboxDeduplicationBehavior(IOutboxStorage outboxStorage,TransactionSettings transactionSettings, RoutingStrategyFactory routingStrategyFactory)
        {
            this.outboxStorage = outboxStorage;
            this.transactionSettings = transactionSettings;
            this.routingStrategyFactory = routingStrategyFactory;
        }

        public override void Invoke(Context context, Action next)
        {
            var messageId = context.PhysicalMessage.Id;
            OutboxMessage outboxMessage;

            if (!outboxStorage.TryGet(messageId, out outboxMessage))
            {
                outboxMessage = new OutboxMessage(messageId);

                context.Set(outboxMessage);

                //we use this scope to make sure that we escalate to DTC if the user is talking to another resource by misstake
                using (var checkForEscalationScope = new TransactionScope(TransactionScopeOption.RequiresNew, new TransactionOptions { IsolationLevel = transactionSettings.IsolationLevel, Timeout = transactionSettings.TransactionTimeout }))
                {
                    next();
                    checkForEscalationScope.Complete();
                }


                if (context.handleCurrentMessageLaterWasCalled)
                {
                    return;
                }
            }

            DispatchOperationToTransport(outboxMessage.TransportOperations);

            outboxStorage.SetAsDispatched(messageId);
        }

        void DispatchOperationToTransport(IEnumerable<TransportOperation> operations)
        {
            foreach (var transportOperation in operations)
            {
                var deliveryOptions = transportOperation.Options.ToDeliveryOptions();

                deliveryOptions.EnlistInReceiveTransaction = false;

                var message = new OutgoingMessage(transportOperation.MessageId, transportOperation.Headers, transportOperation.Body);

                var routingStrategy = routingStrategyFactory.Create(transportOperation.Options);

                routingStrategy.Dispatch(message);
            }
        }

        readonly IOutboxStorage outboxStorage;
        readonly TransactionSettings transactionSettings;
        RoutingStrategyFactory routingStrategyFactory;

        public class OutboxDeduplicationRegistration : RegisterStep
        {
            public OutboxDeduplicationRegistration()
                : base("OutboxDeduplication", typeof(OutboxDeduplicationBehavior), "Deduplication for the outbox feature")
            {
                InsertBeforeIfExists(WellKnownStep.AuditProcessedMessage);
            }
        }
    }
}
