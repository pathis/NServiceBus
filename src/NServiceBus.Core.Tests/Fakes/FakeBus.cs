namespace NServiceBus.Core.Tests.Fakes
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    public class FakeBus : IBus
    {
        DateTime _deferProcessAt = DateTime.MinValue;
        int _deferWasCalled;
        TimeSpan deferDelay = TimeSpan.MinValue;

        public int DeferWasCalled
        {
            get { return _deferWasCalled; }
            set { _deferWasCalled = value; }
        }

        public TimeSpan DeferDelay
        {
            get { return deferDelay; }
        }

        public object DeferedMessage { get; set; }

        public DateTime DeferProcessAt
        {
            get { return _deferProcessAt; }
        }

        public IDictionary<string, string> OutgoingHeaders
        {
            get { throw new NotImplementedException(); }
        }

        public Task Publish(object message,PublishOptions options)
        {
            throw new NotImplementedException();
        }

        public Task Publish<T>(Action<T> messageConstructor, PublishOptions options)
        {
            throw new NotImplementedException();
        }

        public void Subscribe(Type messageType)
        {
            throw new NotImplementedException();
        }

        public void Subscribe<T>()
        {
            throw new NotImplementedException();
        }

        public void Unsubscribe(Type messageType)
        {
            throw new NotImplementedException();
        }

        public void Unsubscribe<T>()
        {
            throw new NotImplementedException();
        }

        public Task Send(object message, SendOptions options)
        {
            return Task.FromResult(true);
        }

        public Task Send<T>(Action<T> messageConstructor, SendOptions options)
        {
            throw new NotImplementedException();
        }

        [Obsolete("", true)]
        public ICallback Send(Address address, object message)
        {
            throw new NotImplementedException();
        }

        [Obsolete("", true)]
        public ICallback Send<T>(Address address, Action<T> messageConstructor)
        {
            throw new NotImplementedException();
        }

        [Obsolete("", true)]
        public ICallback Send(string destination, string correlationId, object message)
        {
            throw new NotImplementedException();
        }

        [Obsolete("", true)]
        public ICallback Send(Address address, string correlationId, object message)
        {
            throw new NotImplementedException();
        }

        [Obsolete("", true)]
        public ICallback Send<T>(string destination, string correlationId, Action<T> messageConstructor)
        {
            throw new NotImplementedException();
        }


        [Obsolete("", true)]
        public ICallback Send<T>(Address address, string correlationId, Action<T> messageConstructor)
        {
            throw new NotImplementedException();
        }

        public Task SendLocal(object message, SendLocalOptions options)
        {
            if (options.Delay.HasValue)
            {
                Interlocked.Increment(ref _deferWasCalled);
                deferDelay = options.Delay.Value;
                DeferedMessage = message;
            }

            return Task.FromResult(true);
        }

        public Task SendLocal<T>(Action<T> messageConstructor, SendLocalOptions options)
        {
            throw new NotImplementedException();
        }

        [Obsolete("", true)]
        public ICallback Defer(TimeSpan delay, object message)
        {
            throw new NotImplementedException();
        }

        [Obsolete("", true)]
        public ICallback Defer(DateTime processAt, object message)
        {
            throw new NotImplementedException();
        }

        public Task Reply(object message)
        {
            throw new NotImplementedException();
        }

        public Task Reply<T>(Action<T> messageConstructor)
        {
            throw new NotImplementedException();
        }

        [Obsolete("", true)]
        public void Return<T>(T errorEnum)
        {
            throw new NotImplementedException();
        }

        public Task HandleCurrentMessageLater()
        {
            throw new NotImplementedException();
        }

        public Task ForwardCurrentMessageTo(string destination)
        {
            throw new NotImplementedException();
        }

        public void DoNotContinueDispatchingCurrentMessageToHandlers()
        {
            throw new NotImplementedException();
        }

        public IMessageContext CurrentMessageContext
        {
            get { throw new NotImplementedException(); }
        }

        public void Dispose()
        {
        }
    }
}