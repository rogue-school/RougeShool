using UnityEngine;
using Zenject;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Game.CoreSystem.Utility;

namespace Game.CoreSystem.Utility
{
    /// <summary>
    /// Zenject DI 최적화를 위한 유틸리티 클래스
    /// 성능 측정, 중복 바인딩 방지, 순환 의존성 검사 등을 제공합니다.
    /// </summary>
    public static class DIOptimizationUtility
    {
        #region 성능 측정

        /// <summary>
        /// DI 바인딩 성능을 측정하는 클래스
        /// </summary>
        public class BindingPerformanceTracker
        {
            private readonly Stopwatch stopwatch;
            private readonly Dictionary<string, long> bindingTimes;
            private readonly bool enableLogging;

            public BindingPerformanceTracker(bool enableLogging = false)
            {
                this.enableLogging = enableLogging;
                this.stopwatch = new Stopwatch();
                this.bindingTimes = new Dictionary<string, long>();
            }

            public void StartTracking(string operationName)
            {
                stopwatch.Restart();
                if (enableLogging)
                {
                    GameLogger.LogInfo($"DI 바인딩 시작: {operationName}", GameLogger.LogCategory.Core);
                }
            }

            public void EndTracking(string operationName)
            {
                stopwatch.Stop();
                bindingTimes[operationName] = stopwatch.ElapsedMilliseconds;
                
                if (enableLogging)
                {
                    GameLogger.LogInfo($"DI 바인딩 완료: {operationName} ({stopwatch.ElapsedMilliseconds}ms)", GameLogger.LogCategory.Core);
                }
            }

            public void LogSummary()
            {
                if (!enableLogging) return;

                GameLogger.LogInfo("=== DI 바인딩 성능 요약 ===", GameLogger.LogCategory.Core);
                foreach (var kvp in bindingTimes)
                {
                    GameLogger.LogInfo($"{kvp.Key}: {kvp.Value}ms", GameLogger.LogCategory.Core);
                }
                
                long totalTime = 0;
                foreach (var time in bindingTimes.Values)
                {
                    totalTime += time;
                }
                GameLogger.LogInfo($"총 바인딩 시간: {totalTime}ms", GameLogger.LogCategory.Core);
            }
        }

        #endregion

        #region 중복 바인딩 방지

        /// <summary>
        /// 중복 바인딩을 방지하는 헬퍼 클래스
        /// </summary>
        public class DuplicateBindingPreventer
        {
            private readonly HashSet<string> boundTypes;
            private readonly DiContainer container;

            public DuplicateBindingPreventer(DiContainer container)
            {
                this.container = container;
                this.boundTypes = new HashSet<string>();
            }

            /// <summary>
            /// 타입이 이미 바인딩되었는지 확인
            /// </summary>
            public bool IsAlreadyBound<T>()
            {
                string typeName = typeof(T).FullName;
                return boundTypes.Contains(typeName);
            }

            /// <summary>
            /// 안전한 바인딩 (중복 방지)
            /// </summary>
            public void SafeBind<TInterface, TImplementation>(bool asSingle = true) 
                where TImplementation : class, TInterface
            {
                if (IsAlreadyBound<TInterface>())
                {
                    GameLogger.LogWarning($"{typeof(TInterface).Name}는 이미 바인딩되었습니다. 중복 바인딩을 건너뜁니다.", GameLogger.LogCategory.Core);
                    return;
                }

                if (asSingle)
                {
                    container.Bind<TInterface>().To<TImplementation>().AsSingle();
                }
                else
                {
                    container.Bind<TInterface>().To<TImplementation>();
                }

                boundTypes.Add(typeof(TInterface).FullName);
            }

            /// <summary>
            /// 안전한 인스턴스 바인딩
            /// </summary>
            public void SafeBindInstance<T>(T instance, bool asSingle = true)
            {
                if (IsAlreadyBound<T>())
                {
                    GameLogger.LogWarning($"{typeof(T).Name}는 이미 바인딩되었습니다. 중복 바인딩을 건너뜁니다.", GameLogger.LogCategory.Core);
                    return;
                }

                if (asSingle)
                {
                    container.Bind<T>().FromInstance(instance).AsSingle();
                }
                else
                {
                    container.Bind<T>().FromInstance(instance);
                }

                boundTypes.Add(typeof(T).FullName);
            }
        }

        #endregion

        #region 순환 의존성 검사

        /// <summary>
        /// 순환 의존성을 검사하는 클래스
        /// </summary>
        public class CircularDependencyChecker
        {
            private readonly Dictionary<Type, List<Type>> dependencyGraph;
            private readonly HashSet<Type> visited;
            private readonly HashSet<Type> recursionStack;

            public CircularDependencyChecker()
            {
                dependencyGraph = new Dictionary<Type, List<Type>>();
                visited = new HashSet<Type>();
                recursionStack = new HashSet<Type>();
            }

            /// <summary>
            /// 의존성 관계 추가
            /// </summary>
            public void AddDependency(Type dependent, Type dependency)
            {
                if (!dependencyGraph.ContainsKey(dependent))
                {
                    dependencyGraph[dependent] = new List<Type>();
                }
                dependencyGraph[dependent].Add(dependency);
            }

            /// <summary>
            /// 순환 의존성 검사
            /// </summary>
            public bool HasCircularDependency()
            {
                visited.Clear();
                recursionStack.Clear();

                foreach (var type in dependencyGraph.Keys)
                {
                    if (!visited.Contains(type))
                    {
                        if (HasCircularDependencyDFS(type))
                        {
                            return true;
                        }
                    }
                }

                return false;
            }

            private bool HasCircularDependencyDFS(Type type)
            {
                visited.Add(type);
                recursionStack.Add(type);

                if (dependencyGraph.ContainsKey(type))
                {
                    foreach (var dependency in dependencyGraph[type])
                    {
                        if (!visited.Contains(dependency))
                        {
                            if (HasCircularDependencyDFS(dependency))
                            {
                                return true;
                            }
                        }
                        else if (recursionStack.Contains(dependency))
                        {
                            GameLogger.LogError($"순환 의존성 발견: {type.Name} -> {dependency.Name}", GameLogger.LogCategory.Core);
                            return true;
                        }
                    }
                }

                recursionStack.Remove(type);
                return false;
            }
        }

        #endregion

        #region 바인딩 최적화 헬퍼

        /// <summary>
        /// 바인딩 최적화를 위한 헬퍼 메서드들
        /// </summary>
        public static class BindingHelper
        {
            /// <summary>
            /// 여러 인터페이스를 한 번에 바인딩
            /// </summary>
            public static void BindMultipleInterfaces<TImplementation>(DiContainer container, params Type[] interfaces)
            {
                foreach (var interfaceType in interfaces)
                {
                    container.Bind(interfaceType).To<TImplementation>().AsSingle();
                }
            }

            /// <summary>
            /// MonoBehaviour 컴포넌트를 안전하게 바인딩
            /// </summary>
            public static T SafeBindMonoBehaviour<T>(DiContainer container, string gameObjectName = null) 
                where T : MonoBehaviour
            {
                var instance = UnityEngine.Object.FindFirstObjectByType<T>();
                
                if (instance == null)
                {
                    var go = new GameObject(gameObjectName ?? typeof(T).Name);
                    instance = go.AddComponent<T>();
                    UnityEngine.Object.DontDestroyOnLoad(go);
                }

                container.Bind<T>().FromInstance(instance).AsSingle();
                return instance;
            }

            /// <summary>
            /// 인터페이스와 구현체를 함께 바인딩
            /// </summary>
            public static void BindInterfaceAndImplementation<TInterface, TImplementation>(DiContainer container, bool asSingle = true)
            {
                if (asSingle)
                {
                    container.BindInterfacesAndSelfTo<TImplementation>().AsSingle();
                }
                else
                {
                    container.BindInterfacesAndSelfTo<TImplementation>();
                }
            }
        }

        #endregion
    }
}
