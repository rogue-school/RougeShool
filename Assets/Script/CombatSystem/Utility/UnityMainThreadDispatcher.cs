using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.CombatSystem.Interface;
using Zenject;

namespace Game.CombatSystem.Utility
{
    /// <summary>
    /// Unity 메인 스레드에서 작업을 실행하기 위한 Dispatcher (Zenject DI).
    /// 비동기 흐름에서 UI 또는 Unity API에 접근할 때 안전하게 사용합니다.
    /// </summary>
    public class UnityMainThreadDispatcher : MonoBehaviour
    {
        private readonly Queue<Action> _executionQueue = new();
        private readonly Queue<IEnumerator> _coroutineQueue = new();
        private readonly Queue<(Action action, float delay)> _delayedQueue = new();
        
        // IMainThreadDispatcher 구현 - 프로퍼티
        public bool IsMainThread => System.Threading.Thread.CurrentThread.ManagedThreadId == 1;
        public int QueueCount => _executionQueue.Count + _coroutineQueue.Count + _delayedQueue.Count;
        
        // IMainThreadDispatcher 구현 - 이벤트
        public event Action<int> OnQueueCountChanged;
        
        private void Awake()
        {
            // Zenject DI 기반으로 초기화
        }

        /// <summary>
        /// 메인 스레드에서 실행할 작업을 큐에 추가합니다.
        /// </summary>
        public void Enqueue(Action action)
        {
            if (action == null) return;

            lock (_executionQueue)
            {
                _executionQueue.Enqueue(action);
                OnQueueCountChanged?.Invoke(QueueCount);
            }
        }
        
        /// <summary>
        /// 메인 스레드에서 실행할 코루틴을 큐에 추가합니다.
        /// </summary>
        public void EnqueueCoroutine(IEnumerator routine)
        {
            if (routine == null) return;

            lock (_coroutineQueue)
            {
                _coroutineQueue.Enqueue(routine);
                OnQueueCountChanged?.Invoke(QueueCount);
            }
        }
        
        /// <summary>
        /// 지연된 작업을 메인 스레드에서 실행합니다.
        /// </summary>
        public void EnqueueDelayed(Action action, float delay)
        {
            if (action == null) return;

            lock (_delayedQueue)
            {
                _delayedQueue.Enqueue((action, delay));
                OnQueueCountChanged?.Invoke(QueueCount);
            }
        }
        
        /// <summary>
        /// 큐에 있는 모든 작업을 즉시 실행합니다.
        /// </summary>
        public void ExecuteAll()
        {
            // 일반 작업 실행
            lock (_executionQueue)
            {
                while (_executionQueue.Count > 0)
                {
                    _executionQueue.Dequeue()?.Invoke();
                }
            }
            
            // 코루틴 실행
            lock (_coroutineQueue)
            {
                while (_coroutineQueue.Count > 0)
                {
                    StartCoroutine(_coroutineQueue.Dequeue());
                }
            }
            
            // 지연된 작업 실행
            lock (_delayedQueue)
            {
                while (_delayedQueue.Count > 0)
                {
                    var (action, delay) = _delayedQueue.Dequeue();
                    StartCoroutine(ExecuteDelayed(action, delay));
                }
            }
            
            OnQueueCountChanged?.Invoke(QueueCount);
        }
        
        /// <summary>
        /// 큐를 비웁니다.
        /// </summary>
        public void ClearQueue()
        {
            lock (_executionQueue) { _executionQueue.Clear(); }
            lock (_coroutineQueue) { _coroutineQueue.Clear(); }
            lock (_delayedQueue) { _delayedQueue.Clear(); }
            
            OnQueueCountChanged?.Invoke(0);
        }
        
        private void Update()
        {
            ExecuteAll();
        }
        
        private IEnumerator ExecuteDelayed(Action action, float delay)
        {
            yield return new WaitForSeconds(delay);
            action?.Invoke();
        }

    }
}
