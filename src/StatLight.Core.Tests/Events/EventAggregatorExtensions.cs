using System;

// ReSharper disable InconsistentNaming
namespace StatLight.Core.Events
{
    public static class EventAggregatorExtensions
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        public static IDisposable AddListenerAction<T>(this IEventSubscriptionManager eventAggregator, Action<T> listener)
        {
            if (eventAggregator == null) throw new ArgumentNullException("eventAggregator");
            if (listener == null) throw new ArgumentNullException("listener");

            var delegateListener = new DelegateListener<T>(listener, eventAggregator);
            eventAggregator.AddListener(delegateListener);

            return delegateListener;
        }
    }

    public class DelegateListener<T> : IListener<T>, IDisposable
    {
        private readonly Action<T> _listener;
        private readonly IEventSubscriptionManager _eventSubscriptionManager;

        public DelegateListener(Action<T> listener, IEventSubscriptionManager eventSubscriptionManager)
        {
            _listener = listener;
            _eventSubscriptionManager = eventSubscriptionManager;
        }

        public void Handle(T message)
        {
            _listener(message);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _eventSubscriptionManager.RemoveListener(this);
            }
        }
    }
}
// ReSharper enable InconsistentNaming
