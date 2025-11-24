using System;
using System.Collections.Generic;

namespace Game.Application.Services
{
    /// <summary>
    /// 쿼리를 전송하고 결과를 반환받는 쿼리 버스 인터페이스입니다.
    /// </summary>
    public interface IQueryBus
    {
        /// <summary>
        /// 특정 쿼리 타입과 결과 타입에 대한 핸들러를 등록합니다.
        /// </summary>
        /// <typeparam name="TQuery">쿼리 타입</typeparam>
        /// <typeparam name="TResult">결과 타입</typeparam>
        /// <param name="handler">쿼리를 처리할 핸들러</param>
        void RegisterHandler<TQuery, TResult>(Func<TQuery, TResult> handler);

        /// <summary>
        /// 특정 쿼리 타입과 결과 타입에 대한 핸들러를 해제합니다.
        /// </summary>
        /// <typeparam name="TQuery">쿼리 타입</typeparam>
        /// <typeparam name="TResult">결과 타입</typeparam>
        void UnregisterHandler<TQuery, TResult>();

        /// <summary>
        /// 쿼리를 전송하고 결과를 반환받습니다.
        /// </summary>
        /// <typeparam name="TQuery">쿼리 타입</typeparam>
        /// <typeparam name="TResult">결과 타입</typeparam>
        /// <param name="query">전송할 쿼리</param>
        /// <returns>쿼리 처리 결과</returns>
        TResult Query<TQuery, TResult>(TQuery query);
    }

    /// <summary>
    /// 인메모리 구현을 사용하는 기본 쿼리 버스입니다.
    /// </summary>
    public sealed class QueryBus : IQueryBus
    {
        private readonly Dictionary<(Type queryType, Type resultType), Delegate> _handlers
            = new Dictionary<(Type queryType, Type resultType), Delegate>();

        private readonly object _lock = new object();

        public void RegisterHandler<TQuery, TResult>(Func<TQuery, TResult> handler)
        {
            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler), "쿼리 핸들러는 null일 수 없습니다.");
            }

            var key = (typeof(TQuery), typeof(TResult));

            lock (_lock)
            {
                if (_handlers.ContainsKey(key))
                {
                    throw new InvalidOperationException($"쿼리 타입 {key.Item1.Name}, 결과 타입 {key.Item2.Name} 에 대한 핸들러가 이미 등록되어 있습니다.");
                }

                _handlers[key] = handler;
            }
        }

        public void UnregisterHandler<TQuery, TResult>()
        {
            var key = (typeof(TQuery), typeof(TResult));

            lock (_lock)
            {
                _handlers.Remove(key);
            }
        }

        public TResult Query<TQuery, TResult>(TQuery query)
        {
            var key = (typeof(TQuery), typeof(TResult));
            Delegate handler;

            lock (_lock)
            {
                if (!_handlers.TryGetValue(key, out handler))
                {
                    throw new InvalidOperationException($"쿼리 타입 {key.Item1.Name}, 결과 타입 {key.Item2.Name} 에 대한 핸들러가 등록되어 있지 않습니다.");
                }
            }

            if (handler is Func<TQuery, TResult> typedHandler)
            {
                return typedHandler(query);
            }

            throw new InvalidOperationException("등록된 쿼리 핸들러의 시그니처가 올바르지 않습니다.");
        }
    }
}


