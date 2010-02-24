using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using StatLight.Core.Reporting.Providers.Console;

namespace StatLight.Core.Events.Aggregation
{
    public interface IListener<T>
    {
        void Handle(T message);
    }

    public interface IEventAggregator
    {
        // Sending messages
        void SendMessage<T>(T message);
        void SendMessage<T>() where T : new();

        // Explicit registration
        IEventAggregator AddListener<T>(Action<T> handler);
        IEventAggregator AddListener<T>(Action handler);

        IEventAggregator AddListener(Action<object> handler, Predicate<object> filter);
        IEventAggregator AddListener(Action handler, Predicate<object> filter);

        // Listeners that apply a filter before handling
        IEventAggregator AddListener<T>(Action<T> handler, Predicate<T> filter);
        IEventAggregator AddListener<T>(Action handler, Predicate<T> filter);

        IEventAggregator AddListener(object listener);

        IEventAggregator RemoveListener(object listener);
    }

    public class EventAggregator : IEventAggregator
    {
        private readonly List<FilteredHandler<object>> _filteredHandlers = new List<FilteredHandler<object>>();
        private readonly SynchronizationContext _context;
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
            sendAction(() => CallOnEach<IListener<T>>(all(), x => x.Handle(message)));
            sendAction(() => SendToAllFilteredListeners(message));
        }

        public void SendMessage<T>() where T : new()
        {
            SendMessage(new T());
        }

        public IEventAggregator AddListener(object listener)
        {
            lock (_locker)
            {
                if (_listeners.Contains(listener))
                    return this;
                _listeners.Add(listener);
            }
            return this;
        }



        public IEventAggregator AddListener<T>(Action<T> handler)
        {
            var delegateListener = new DelegateListener<T>(handler);
            AddListener(delegateListener);
            return this;
        }

        public IEventAggregator AddListener<T>(Action handler)
        {
            var delegateListener = new DelegateListener<T>(msg => handler());
            AddListener(delegateListener);
            return this;
        }

        #region FilteredListener

        private void SendToAllFilteredListeners(object message)
        {
            foreach (FilteredHandler<object> listener in allFiltered())
            {
                listener.Handle(message);
            }
        }

        public IEventAggregator AddListener(Action<object> listener, Predicate<object> filter)
        {
            lock (_locker)
            {
                var filteredHandler = new FilteredHandler<object>(listener, filter);
                if (_filteredHandlers.Contains(filteredHandler))
                    return this;
                _filteredHandlers.Add(filteredHandler);
            }
            return this;
        }
        public IEventAggregator AddListener(Action listener, Predicate<object> filter)
        {
            return AddListener(listener, filter);
        }

        //TODO:
        //public IEventAggregator RemoveFilteredListener(object listener)
        //{
        //    lock (_locker)
        //    {
        //        _filteredHandlers.Remove(listener);
        //    }
        //    return this;
        //}
        #endregion

        public IEventAggregator RemoveListener(object listener)
        {
            lock (_locker)
            {
                _listeners.Remove(listener);
            }
            return this;
        }

        public IEventAggregator AddListener<T>(Action<T> handler, Predicate<T> filter)
        {
            AddListener(new FilteredHandler<T>(handler, filter));
            return this;
        }

        public IEventAggregator AddListener<T>(Action handler, Predicate<T> filter)
        {
            AddListener(new FilteredHandler<T>(msg => handler(), filter));
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

        private IEnumerable<FilteredHandler<object>> allFiltered()
        {
            lock (_locker)
            {
                return _filteredHandlers.ToArray();
            }
        }

        protected virtual void sendAction(Action action)
        {
            _context.Send(state => action(), null);
        }

        public void AddListeners(params object[] listeners)
        {
            foreach (object listener in listeners)
            {
                AddListener(listener);
            }
        }

        public bool HasListener(object listener)
        {
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
            var subject = target as T;
            if (subject != null)
            {
                action(subject);
            }
        }

        public static void CallOnEach<TListener>(IEnumerable enumerable, Action<TListener> action)
            where TListener : class
        {
            foreach (object o in enumerable)
            {
                CallOn(o, action);
            }
        }


        private class DelegateListener<T> : IListener<T>
        {
            private readonly Action<T> _handler;

            public DelegateListener(Action<T> handler)
            {
                _handler = handler;
            }

            public void Handle(T message)
            {
                _handler(message);
            }
        }


        private class FilteredHandler<T>
            : IListener<T>
        {
            private readonly Predicate<T> _filter;
            private readonly Action<T> _handler;

            public FilteredHandler(Action<T> handler, Predicate<T> filter)
            {
                _handler = handler;
                _filter = filter;
            }

            public void Handle(T message)
            {
                if (_filter(message))
                    _handler(message);
            }

            public override int GetHashCode()
            {
                return _handler.GetHashCode();
            }
        }

    }
}
