using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace StatLight.Core.Events.Aggregation
{
    public interface IEventAggregator
    {
        // Sending messages
        void SendMessage<T>(T message);
        void SendMessage<T>() where T : new();

        // Explicit registration
        //void AddListener(object listener);
        void AddListener<T>(Action<T> handler);
        void AddListener<T>(Action handler);
        void RemoveListener(object listener);

        // Filtered registration, experimental
        //If<T> If<T>(Func<T, bool> filter);
    }
    public interface IListener<T>
    {
        void Handle(T message);
    }

    public class DelegateListener<T> : IListener<T>
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

    
    public class EventAggregator : IEventAggregator
    {
        private readonly SynchronizationContext _context;
        private readonly List<object> _listeners = new List<object>();
        private readonly object _locker = new object();

        public EventAggregator(SynchronizationContext context)
        {
            _context = context;
        }

        #region IEventAggregator Members

        public void SendMessage<T>(T message)
        {
            sendAction(() => CallOnEach<IListener<T>>(all(), x => x.Handle(message)));
        }

        public void SendMessage<T>() where T : new()
        {
            SendMessage(new T());
        }

        public void AddListener(object listener)
        {
            lock (_locker)
            {
                if (_listeners.Contains(listener)) return;
                _listeners.Add(listener);
            }
        }

        public void AddListener<T>(Action<T> handler)
        {
            var delegateListener = new DelegateListener<T>(handler);
            AddListener(delegateListener);
        }

        public void AddListener<T>(Action handler)
        {
            var delegateListener = new DelegateListener<T>(msg => handler());
            AddListener(delegateListener);
        }

        public void RemoveListener(object listener)
        {
            lock (_locker)
            {
                _listeners.Remove(listener);
            }
        }

        //public If<T> If<T>(Func<T, bool> filter)
        //{
        //    return new IfExpression<T>(filter, this);
        //}

        #endregion

        private IEnumerable<object> all()
        {
            lock (_locker)
            {
                return _listeners.ToArray();
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

        #region Nested type: IfExpression

        //internal class IfExpression<T> : If<T>
        //{
        //    private readonly EventAggregator _aggregator;
        //    private readonly Func<T, bool> _filter;

        //    public IfExpression(Func<T, bool> filter, EventAggregator aggregator)
        //    {
        //        _filter = filter;
        //        _aggregator = aggregator;
        //    }

        //    #region If<T> Members

        //    public object PublishTo(Action<T> action)
        //    {
        //        var listener = new FilteredListener<T>(_filter, action);
        //        _aggregator.AddListener(listener);

        //        return listener;
        //    }

        //    #endregion
        //}

        #endregion

        public static void CallOn<T>(object target, Action<T> action)
            where T : class
        {
            var subject = target as T;
            if (subject != null)
            {
                action(subject);
            }
        }

        public static void CallOnEach<T>(IEnumerable enumerable, Action<T> action)
            where T : class
        {
            foreach (object o in enumerable)
            {
                CallOn(o, action);
            }
        }
    }
}
