namespace NServiceBus
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;
    using Janitor;
    using NServiceBus.Pipeline;

    class BehaviorChain : IDisposable
    {
        public BehaviorChain(IEnumerable<BehaviorInstance> behaviorList, BehaviorContext context, Dictionary<Type, string> lookupSteps, BusNotifications notifications)
        {
            this.context = context;
            this.lookupSteps = lookupSteps;
            this.notifications = notifications;

            itemDescriptors = behaviorList.ToArray();
        }

        public async Task Invoke(BehaviorContextStacker contextStacker)
        {
            var outerPipe = false;

            try
            {
                if (!context.TryGet("Diagnostics.Pipe", out steps))
                {
                    outerPipe = true;
                    steps = new Observable<StepStarted>();
                    context.Set("Diagnostics.Pipe", steps);
                    notifications.Pipeline.InvokeReceiveStarted(steps);
                }

                await InvokeNext(context, contextStacker, 0).ConfigureAwait(false);

                if (outerPipe)
                {
                    steps.OnCompleted();
                }
            }
            catch (Exception ex)
            {
                if (outerPipe)
                {
                    steps.OnError(ex);
                }

                throw;
            }
            finally
            {
                if (outerPipe)
                {
                    context.Remove("Diagnostics.Pipe");
                }
            }
        }

        public void Dispose()
        {
            
        }

        async Task<BehaviorContext> InvokeNext(BehaviorContext context, BehaviorContextStacker contextStacker, int currentIndex)
        {
            Guard.AgainstNull(context, "context");

            if (currentIndex == itemDescriptors.Length)
            {
                return context;
            }

            var behavior = itemDescriptors[currentIndex];
            var stepEnded = new Observable<StepEnded>();
            contextStacker.Push(context);
            try
            {
                steps.OnNext(new StepStarted(lookupSteps[behavior.Type], behavior.Type, stepEnded));

                var duration = Stopwatch.StartNew();

                BehaviorContext innermostContext = null;
                await behavior.Invoke(context, async newContext =>
                {
                    duration.Stop();
                    innermostContext = await InvokeNext(newContext, contextStacker, currentIndex + 1).ConfigureAwait(false);
                    duration.Start();
                }).ConfigureAwait(false);

                duration.Stop();

                stepEnded.OnNext(new StepEnded(duration.Elapsed));
                stepEnded.OnCompleted();

                return innermostContext ?? context;
            }
            catch (Exception ex)
            {
                stepEnded.OnError(ex);

                throw;
            }
            finally
            {
                contextStacker.Pop();
            }
        }

        [SkipWeaving]
        readonly BusNotifications notifications;
        readonly BehaviorContext context;
        [SkipWeaving]
        readonly BehaviorInstance[] itemDescriptors;
        readonly Dictionary<Type, string> lookupSteps;
        Observable<StepStarted> steps;
    }
}