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
        IEventSubscriptionManager AddListener(object listener);

        IEventSubscriptionManager RemoveListener(object listener);
    }

    public interface IEventPublisher
    {
        void SendMessage<T>(T message);
        void SendMessage<T>() where T : new();
    }

    public class EventAggregator : IEventPublisher, IEventSubscriptionManager
    {
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
        }

        public void SendMessage<T>() where T : new()
        {
            SendMessage(new T());
        }

        public IEventSubscriptionManager AddListener(object listener)
        {
            lock (_locker)
            {
                if (HasListener(listener))
                    return this;
                _listeners.Add(listener);
            }
            return this;
        }

        public IEventSubscriptionManager RemoveListener(object listener)
        {
            lock (_locker)
            {
                _listeners.Remove(listener);
            }
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
    }
}
