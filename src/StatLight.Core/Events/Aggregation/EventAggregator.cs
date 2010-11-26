using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using StatLight.Core.Common;

namespace StatLight.Core.Events.Aggregation
{
    public interface IListener<T>
    {
        void Handle(T message);
    }

    public interface IEventSubscriptionManager
    {
        // Explicit registration
        IEventSubscriptionManager AddListener<T>(Action<T> listener);
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
        IEventSubscriptionManager AddListener<T>(Action listener);

        IEventSubscriptionManager AddListener(Action<object> listener, Predicate<object> filter);
        IEventSubscriptionManager AddListener(Action listener, Predicate<object> filter);

        // Listeners that apply a filter before handling
        IEventSubscriptionManager AddListener<T>(Action<T> listener, Predicate<T> filter);
        IEventSubscriptionManager AddListener<T>(Action listener, Predicate<T> filter);

        IEventSubscriptionManager AddListener(object listener);

        IEventSubscriptionManager RemoveListener(object listener);
    }

    public interface IEventPublisher
    {
        // Sending messages
        void SendMessage<T>(T message);
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
        void SendMessage<T>() where T : new();
    }

    public interface IEventAggregator : IEventPublisher, IEventSubscriptionManager
    {

    }

    public class EventAggregator : IEventAggregator
    {
        private readonly List<FilteredListener<object>> _filteredListeners = new List<FilteredListener<object>>();
        private readonly SynchronizationContext _context;
        public ILogger Logger { get; set; }
        private readonly List<object> _listeners = new List<object>();
        private readonly object _locker = new object();

        public EventAggregator()
            : this(new SynchronizationContext())
        { }

        public EventAggregator(SynchronizationContext context)
        {
            _context = context;
        }

        #region IEventAggregator Members

        public void SendMessage<T>(T message)
        {
            //DEBUG
            //if (Logger != null)
            //    Logger.Debug(typeof(T).Name);
            SendAction(() => CallOnEach<IListener<T>>(all(), x => x.Handle(message)));
            SendAction(() => SendToAllFilteredListeners(message));
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
        public void SendMessage<T>() where T : new()
        {
            SendMessage(new T());
        }

        public IEventSubscriptionManager AddListener(object listener)
        {
            lock (_locker)
            {
                if (_listeners.Contains(listener))
                    return this;
                _listeners.Add(listener);
            }
            return this;
        }



        public IEventSubscriptionManager AddListener<T>(Action<T> listener)
        {
            var delegateListener = new DelegateListener<T>(listener);
            AddListener(delegateListener);
            return this;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
        public IEventSubscriptionManager AddListener<T>(Action listener)
        {
            var delegateListener = new DelegateListener<T>(msg => listener());
            AddListener(delegateListener);
            return this;
        }

        #region FilteredListener

        private void SendToAllFilteredListeners(object message)
        {
            foreach (FilteredListener<object> listener in allFiltered())
            {
                listener.Handle(message);
            }
        }

        public IEventSubscriptionManager AddListener(Action<object> listener, Predicate<object> filter)
        {
            lock (_locker)
            {
                var filteredListener = new FilteredListener<object>(listener, filter);
                if (_filteredListeners.Contains(filteredListener))
                    return this;
                _filteredListeners.Add(filteredListener);
            }
            return this;
        }
        public IEventSubscriptionManager AddListener(Action listener, Predicate<object> filter)
        {
            return AddListener(listener, filter);
        }

        //TODO:
        //public IEventAggregator RemoveFilteredListener(object listener)
        //{
        //    lock (_locker)
        //    {
        //        _filteredListeners.Remove(listener);
        //    }
        //    return this;
        //}
        #endregion

        public IEventSubscriptionManager RemoveListener(object listener)
        {
            lock (_locker)
            {
                _listeners.Remove(listener);
            }
            return this;
        }

        public IEventSubscriptionManager AddListener<T>(Action<T> listener, Predicate<T> filter)
        {
            AddListener(new FilteredListener<T>(listener, filter));
            return this;
        }

        public IEventSubscriptionManager AddListener<T>(Action listener, Predicate<T> filter)
        {
            AddListener(new FilteredListener<T>(msg => listener(), filter));
            return this;
        }

        #endregion

        private IEnumerable<object> all()
        {
            lock (_locker)
            {
                return _listeners.ToArray();
            }
        }

        private IEnumerable<FilteredListener<object>> allFiltered()
        {
            lock (_locker)
            {
                return _filteredListeners.ToArray();
            }
        }

        protected virtual void SendAction(Action action)
        {
            _context.Send(state => action(), null);
        }

        public void AddListeners(params object[] listeners)
        {
            if (listeners == null) throw new ArgumentNullException("listeners");
            foreach (object listener in listeners)
            {
                AddListener(listener);
            }
        }

        public bool HasListener(object listener)
        {
            if (listener == null) throw new ArgumentNullException("listener");
            return _listeners.Contains(listener);
        }

        public void RemoveAllListeners()
        {
            _listeners.Clear();
        }

        public void RemoveAllListeners(Predicate<object> filter)
        {
            var itemsToRemove = _listeners.Where(w => filter(w)).ToList();
            foreach (var item in itemsToRemove)
            {
                _listeners.Remove(item);
            }
        }

        public static void CallOn<T>(object target, Action<T> action)
            where T : class
        {
            if (action == null) throw new ArgumentNullException("action");
            var subject = target as T;
            if (subject != null)
            {
                action(subject);
            }
        }

        public static void CallOnEach<TListener>(IEnumerable enumerable, Action<TListener> action)
            where TListener : class
        {
            if (enumerable == null) throw new ArgumentNullException("enumerable");
            if (action == null) throw new ArgumentNullException("action");
            foreach (object o in enumerable)
            {
                CallOn(o, action);
            }
        }


        private class DelegateListener<T> : IListener<T>
        {
            private readonly Action<T> _listener;

            public DelegateListener(Action<T> listener)
            {
                _listener = listener;
            }

            public void Handle(T message)
            {
                _listener(message);
            }
        }


        private class FilteredListener<T>
            : IListener<T>
        {
            private readonly Predicate<T> _filter;
            private readonly Action<T> _listener;

            public FilteredListener(Action<T> listener, Predicate<T> filter)
            {
                _listener = listener;
                _filter = filter;
            }

            public void Handle(T message)
            {
                if (_filter(message))
                    _listener(message);
            }

            public override int GetHashCode()
            {
                return _listener.GetHashCode();
            }
        }

    }
}
