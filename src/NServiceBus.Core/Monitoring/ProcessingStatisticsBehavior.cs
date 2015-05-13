namespace NServiceBus
{
    using System;
    using System.Threading.Tasks;

    class ProcessingStatisticsBehavior : PhysicalMessageProcessingStageBehavior
    {
        public override Task Invoke(Context context, Func<Task> next)
        {
            string timeSentString;
            var headers = context.PhysicalMessage.Headers;

            if (headers.TryGetValue(Headers.TimeSent, out timeSentString))
            {
                context.Set("IncomingMessage.TimeSent", DateTimeExtensions.ToUtcDateTime(timeSentString));
            }

            context.Set("IncomingMessage.ProcessingStarted", DateTime.UtcNow);

            try
            {
                return next();
            }
            finally
            {
                context.Set("IncomingMessage.ProcessingEnded", DateTime.UtcNow);
            }
        }
    }
}