namespace NServiceBus
{
    using System;
    using System.Runtime.ExceptionServices;
    using System.Threading.Tasks;
    using NServiceBus.Hosting;
    using NServiceBus.Logging;
    using NServiceBus.Pipeline;
    using NServiceBus.Transports;

    class MoveFaultsToErrorQueueBehavior : PhysicalMessageProcessingStageBehavior
    {
        public MoveFaultsToErrorQueueBehavior(CriticalError criticalError, ISendMessages sender, HostInformation hostInformation, BusNotifications notifications, string errorQueueAddress)
        {
            this.criticalError = criticalError;
            this.sender = sender;
            this.hostInformation = hostInformation;
            this.notifications = notifications;
            this.errorQueueAddress = errorQueueAddress;
        }

        public override async Task Invoke(Context context, Func<Task> next)
        {
            ExceptionDispatchInfo exceptionInfo = null;
            try
            {
                await next().ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                // Need to do it like that until we have CSharp 6 support. Cannot wait inside catch
                exceptionInfo = ExceptionDispatchInfo.Capture(exception);
            }

            if (exceptionInfo != null)
            {
                try
                {
                    var exception = exceptionInfo.SourceException;
                    var message = context.PhysicalMessage;

                    Logger.Error("Failed to process message with ID: " + message.Id, exception);
                    message.RevertToOriginalBodyIfNeeded();

                    message.SetExceptionHeaders(exception, PipelineInfo.PublicAddress);

                    message.Headers.Remove(Headers.Retries);


                    message.Headers[Headers.HostId] = hostInformation.HostId.ToString("N");
                    message.Headers[Headers.HostDisplayName] = hostInformation.DisplayName;

                    await sender.Send(new OutgoingMessage("msg id", message.Headers, message.Body), new TransportSendOptions(errorQueueAddress))
                        .ConfigureAwait(false);

                    notifications.Errors.InvokeMessageHasBeenSentToErrorQueue(message, exception);
                }
                catch (Exception ex)
                {
                    criticalError.Raise("Failed to forward message to error queue", ex);
                    throw;
                }
            }
        }

        readonly CriticalError criticalError;
        readonly ISendMessages sender;
        readonly HostInformation hostInformation;
        readonly BusNotifications notifications;
        readonly string errorQueueAddress;
        static ILog Logger = LogManager.GetLogger<MoveFaultsToErrorQueueBehavior>();

        public class Registration : RegisterStep
        {
            public Registration()
                : base("MoveFaultsToErrorQueue", typeof(MoveFaultsToErrorQueueBehavior), "Invokes the configured fault manager for messages that fails processing (and any retries)")
            {
                InsertBeforeIfExists("HandlerTransactionScopeWrapper");
                InsertBeforeIfExists("FirstLevelRetries");
                InsertBeforeIfExists("SecondLevelRetries");

                InsertBeforeIfExists("ReceivePerformanceDiagnosticsBehavior");

            }
        }
    }
}