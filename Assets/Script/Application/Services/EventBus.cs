using System;
using System.Collections.Generic;

namespace Game.Application.Services
{
    /// <summary>
    /// 애플리케이션 전역에서 사용할 수 있는 이벤트 버스 인터페이스입니다.
    /// </summary>
    public interface IEventBus
    {
        /// <summary>
        /// 특정 타입의 이벤트를 구독합니다.
        /// </summary>
        /// <typeparam name="TEvent">이벤트 타입</typeparam>
        /// <param name="handler">이벤트가 발생했을 때 호출될 핸들러</param>
        void Subscribe<TEvent>(Action<TEvent> handler);

        /// <summary>
        /// 특정 타입의 이벤트 구독을 해제합니다.
        /// </summary>
        /// <typeparam name="TEvent">이벤트 타입</typeparam>
        /// <param name="handler">해제할 핸들러</param>
        void Unsubscribe<TEvent>(Action<TEvent> handler);

        /// <summary>
        /// 이벤트를 발행합니다.
        /// </summary>
        /// <typeparam name="TEvent">이벤트 타입</typeparam>
        /// <param name="eventData">이벤트 데이터</param>
        void Publish<TEvent>(TEvent eventData);
    }

    /// <summary>
    /// 인메모리 구현을 사용하는 기본 이벤트 버스입니다.
    /// </summary>
    public sealed class EventBus : IEventBus
    {
        private readonly Dictionary<Type, List<Delegate>> _handlers = new Dictionary<Type, List<Delegate>>();
        private readonly object _lock = new object();

        public void Subscribe<TEvent>(Action<TEvent> handler)
        {
            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler), "이벤트 핸들러는 null일 수 없습니다.");
            }

            var eventType = typeof(TEvent);

            lock (_lock)
            {
                if (!_handlers.TryGetValue(eventType, out var list))
                {
                    list = new List<Delegate>();
                    _handlers[eventType] = list;
                }

                if (!list.Contains(handler))
                {
                    list.Add(handler);
                }
            }
        }

        public void Unsubscribe<TEvent>(Action<TEvent> handler)
        {
            if (handler == null)
            {
                return;
            }

            var eventType = typeof(TEvent);

            lock (_lock)
            {
                if (!_handlers.TryGetValue(eventType, out var list))
                {
                    return;
                }

                list.Remove(handler);

                if (list.Count == 0)
                {
                    _handlers.Remove(eventType);
                }
            }
        }

        public void Publish<TEvent>(TEvent eventData)
        {
            if (eventData == null)
            {
                throw new ArgumentNullException(nameof(eventData), "이벤트 데이터는 null일 수 없습니다.");
            }

            Delegate[] snapshot;
            var eventType = typeof(TEvent);

            lock (_lock)
            {
                if (!_handlers.TryGetValue(eventType, out var list) || list.Count == 0)
                {
                    return;
                }

                snapshot = list.ToArray();
            }

            foreach (var handler in snapshot)
            {
                if (handler is Action<TEvent> typedHandler)
                {
                    typedHandler(eventData);
                }
            }
        }
    }
}


