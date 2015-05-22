namespace NServiceBus
{
    using System;
    using System.Threading.Tasks;
    using System.Transactions;
    using NServiceBus.Pipeline;

    class SuppressAmbientTransactionBehavior : PhysicalMessageProcessingStageBehavior
    {
        public override async Task Invoke(Context context, Func<Task> next)
        {
            if (Transaction.Current == null)
            {
                await next().ConfigureAwait(false);
                return;
            }

            // TODO: We need .NET 4.5.1 here!
            using (var tx = new TransactionScope(TransactionScopeOption.Suppress))
            {
                await next().ConfigureAwait(false);

                tx.Complete();
            }
        }

        public class Registration : RegisterStep
        {
            public Registration()
                : base("HandlerTransactionScopeWrapperBehavior", typeof(SuppressAmbientTransactionBehavior), "Make sure that any ambient transaction scope is supressed")
            {
                InsertBeforeIfExists("FirstLevelRetries");
                InsertBeforeIfExists("ReceivePerformanceDiagnosticsBehavior");
            }
        }
    }
}