using System;
using System.Collections.Generic;

namespace Game.Application.Services
{
    /// <summary>
    /// 명령을 전송하고 처리하는 커맨드 버스 인터페이스입니다.
    /// </summary>
    public interface ICommandBus
    {
        /// <summary>
        /// 특정 타입의 명령 핸들러를 등록합니다.
        /// </summary>
        /// <typeparam name="TCommand">명령 타입</typeparam>
        /// <param name="handler">명령을 처리할 핸들러</param>
        void RegisterHandler<TCommand>(Action<TCommand> handler);

        /// <summary>
        /// 특정 타입의 명령 핸들러 등록을 해제합니다.
        /// </summary>
        /// <typeparam name="TCommand">명령 타입</typeparam>
        void UnregisterHandler<TCommand>();

        /// <summary>
        /// 명령을 전송합니다.
        /// </summary>
        /// <typeparam name="TCommand">명령 타입</typeparam>
        /// <param name="command">전송할 명령</param>
        void Send<TCommand>(TCommand command);
    }

    /// <summary>
    /// 인메모리 구현을 사용하는 기본 커맨드 버스입니다.
    /// </summary>
    public sealed class CommandBus : ICommandBus
    {
        private readonly Dictionary<Type, Delegate> _handlers = new Dictionary<Type, Delegate>();
        private readonly object _lock = new object();

        public void RegisterHandler<TCommand>(Action<TCommand> handler)
        {
            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler), "명령 핸들러는 null일 수 없습니다.");
            }

            var commandType = typeof(TCommand);

            lock (_lock)
            {
                if (_handlers.ContainsKey(commandType))
                {
                    throw new InvalidOperationException($"명령 타입 {commandType.Name} 에 대한 핸들러가 이미 등록되어 있습니다.");
                }

                _handlers[commandType] = handler;
            }
        }

        public void UnregisterHandler<TCommand>()
        {
            var commandType = typeof(TCommand);

            lock (_lock)
            {
                _handlers.Remove(commandType);
            }
        }

        public void Send<TCommand>(TCommand command)
        {
            var commandType = typeof(TCommand);
            Delegate handler;

            lock (_lock)
            {
                if (!_handlers.TryGetValue(commandType, out handler))
                {
                    throw new InvalidOperationException($"명령 타입 {commandType.Name} 에 대한 핸들러가 등록되어 있지 않습니다.");
                }
            }

            if (handler is Action<TCommand> typedHandler)
            {
                typedHandler(command);
            }
        }
    }
}


