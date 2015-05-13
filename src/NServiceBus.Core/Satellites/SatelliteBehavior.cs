namespace NServiceBus
{
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// 
    /// </summary>
    public abstract class SatelliteBehavior: PhysicalMessageProcessingStageBehavior
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="next"></param>
        public override Task Invoke(Context context, Func<Task> next)
        {
            context.Set("TransportReceiver.MessageHandledSuccessfully", Handle(context.PhysicalMessage));

            return Task.FromResult(true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="physicalMessage"></param>
        /// <returns></returns>
        protected abstract bool Handle(TransportMessage physicalMessage);
    }
}