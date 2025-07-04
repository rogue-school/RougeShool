using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Unity 메인 스레드에서 작업을 실행하기 위한 Dispatcher.
/// 비동기 흐름에서 UI 또는 Unity API에 접근할 때 안전하게 사용합니다.
/// </summary>
public class UnityMainThreadDispatcher : MonoBehaviour
{
    private static readonly Queue<Action> _executionQueue = new();

    public static void Enqueue(Action action)
    {
        if (action == null) return;

        lock (_executionQueue)
        {
            _executionQueue.Enqueue(action);
        }
    }

    void Update()
    {
        lock (_executionQueue)
        {
            while (_executionQueue.Count > 0)
            {
                _executionQueue.Dequeue()?.Invoke();
            }
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
#if UNITY_2023_1_OR_NEWER
        if (FindFirstObjectByType<UnityMainThreadDispatcher>() == null)
#else
        if (FindObjectOfType<UnityMainThreadDispatcher>() == null)
#endif
        {
            var go = new GameObject("UnityMainThreadDispatcher");
            DontDestroyOnLoad(go);
            go.AddComponent<UnityMainThreadDispatcher>();
        }
    }
}
