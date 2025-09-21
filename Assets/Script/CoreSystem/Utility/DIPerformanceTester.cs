using UnityEngine;
using Zenject;
using System.Diagnostics;
using System.Collections.Generic;
using Game.CoreSystem.Utility;

namespace Game.CoreSystem.Utility
{
    /// <summary>
    /// DI 성능을 테스트하고 측정하는 클래스
    /// 개발 중에 DI 최적화 효과를 확인할 수 있습니다.
    /// </summary>
    public class DIPerformanceTester : MonoBehaviour
    {
        [Header("성능 테스트 설정")]
        [SerializeField] private bool enablePerformanceTesting = true;
        [SerializeField] private bool enableDetailedLogging = false;
        [SerializeField] private int testIterations = 100;

        [Header("테스트 결과")]
        [SerializeField] private float averageBindingTime;
        [SerializeField] private float averageResolutionTime;
        [SerializeField] private int totalBindings;
        [SerializeField] private int totalResolutions;

        private DiContainer container;
        private List<long> bindingTimes;
        private List<long> resolutionTimes;

        private void Awake()
        {
            if (enablePerformanceTesting)
            {
                InitializeTesting();
            }
        }

        private void InitializeTesting()
        {
            bindingTimes = new List<long>();
            resolutionTimes = new List<long>();
            
            // Zenject 컨테이너 참조 가져오기
            var installer = GetComponent<MonoInstaller>();
            if (installer != null)
            {
                // 리플렉션을 통해 Container 접근
                var containerField = typeof(MonoInstaller).GetField("_container", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                container = containerField?.GetValue(installer) as DiContainer;
            }
            
            if (container == null)
            {
                GameLogger.LogWarning("DIPerformanceTester: Zenject 컨테이너를 찾을 수 없습니다.", GameLogger.LogCategory.Core);
            }
        }

        /// <summary>
        /// DI 바인딩 성능 테스트
        /// </summary>
        [ContextMenu("테스트 DI 바인딩 성능")]
        public void TestBindingPerformance()
        {
            if (!enablePerformanceTesting || container == null)
            {
                GameLogger.LogWarning("DI 성능 테스트가 비활성화되었거나 컨테이너가 없습니다.", GameLogger.LogCategory.Core);
                return;
            }

            GameLogger.LogInfo("=== DI 바인딩 성능 테스트 시작 ===", GameLogger.LogCategory.Core);
            
            var stopwatch = new Stopwatch();
            long totalTime = 0;

            for (int i = 0; i < testIterations; i++)
            {
                stopwatch.Restart();
                
                // 테스트용 바인딩들
                TestBindings();
                
                stopwatch.Stop();
                long elapsed = stopwatch.ElapsedMilliseconds;
                bindingTimes.Add(elapsed);
                totalTime += elapsed;
            }

            averageBindingTime = (float)totalTime / testIterations;
            totalBindings = testIterations;

            GameLogger.LogInfo($"DI 바인딩 성능 테스트 완료:", GameLogger.LogCategory.Core);
            GameLogger.LogInfo($"- 평균 바인딩 시간: {averageBindingTime:F2}ms", GameLogger.LogCategory.Core);
            GameLogger.LogInfo($"- 총 테스트 횟수: {testIterations}", GameLogger.LogCategory.Core);
            GameLogger.LogInfo($"- 총 바인딩 시간: {totalTime}ms", GameLogger.LogCategory.Core);
        }

        /// <summary>
        /// DI 해결 성능 테스트
        /// </summary>
        [ContextMenu("테스트 DI 해결 성능")]
        public void TestResolutionPerformance()
        {
            if (!enablePerformanceTesting || container == null)
            {
                GameLogger.LogWarning("DI 성능 테스트가 비활성화되었거나 컨테이너가 없습니다.", GameLogger.LogCategory.Core);
                return;
            }

            GameLogger.LogInfo("=== DI 해결 성능 테스트 시작 ===", GameLogger.LogCategory.Core);
            
            var stopwatch = new Stopwatch();
            long totalTime = 0;

            for (int i = 0; i < testIterations; i++)
            {
                stopwatch.Restart();
                
                // 테스트용 해결들
                TestResolutions();
                
                stopwatch.Stop();
                long elapsed = stopwatch.ElapsedMilliseconds;
                resolutionTimes.Add(elapsed);
                totalTime += elapsed;
            }

            averageResolutionTime = (float)totalTime / testIterations;
            totalResolutions = testIterations;

            GameLogger.LogInfo($"DI 해결 성능 테스트 완료:", GameLogger.LogCategory.Core);
            GameLogger.LogInfo($"- 평균 해결 시간: {averageResolutionTime:F2}ms", GameLogger.LogCategory.Core);
            GameLogger.LogInfo($"- 총 테스트 횟수: {testIterations}", GameLogger.LogCategory.Core);
            GameLogger.LogInfo($"- 총 해결 시간: {totalTime}ms", GameLogger.LogCategory.Core);
        }

        /// <summary>
        /// 전체 DI 성능 테스트
        /// </summary>
        [ContextMenu("전체 DI 성능 테스트")]
        public void TestFullDIPerformance()
        {
            TestBindingPerformance();
            TestResolutionPerformance();
            
            GameLogger.LogInfo("=== 전체 DI 성능 테스트 결과 ===", GameLogger.LogCategory.Core);
            GameLogger.LogInfo($"바인딩 성능: {averageBindingTime:F2}ms (평균)", GameLogger.LogCategory.Core);
            GameLogger.LogInfo($"해결 성능: {averageResolutionTime:F2}ms (평균)", GameLogger.LogCategory.Core);
            
            if (enableDetailedLogging)
            {
                LogDetailedResults();
            }
        }

        private void TestBindings()
        {
            // 테스트용 바인딩들 (실제 프로젝트의 바인딩 패턴 시뮬레이션)
            try
            {
                // 서비스 바인딩
                container.Bind<ITestService>().To<TestService>().AsSingle();
                container.Bind<ITestManager>().To<TestManager>().AsSingle();
                
                // 팩토리 바인딩
                container.Bind<ITestFactory>().To<TestFactory>().AsSingle();
                
                // MonoBehaviour 바인딩
                var testMono = new GameObject("TestMono").AddComponent<TestMonoBehaviour>();
                container.Bind<TestMonoBehaviour>().FromInstance(testMono).AsSingle();
                
                // 인터페이스 바인딩
                container.Bind<ITestInterface>().To<TestImplementation>().AsSingle();
            }
            catch (System.Exception ex)
            {
                if (enableDetailedLogging)
                {
                    GameLogger.LogWarning($"테스트 바인딩 중 오류: {ex.Message}", GameLogger.LogCategory.Core);
                }
            }
        }

        private void TestResolutions()
        {
            // 테스트용 해결들
            try
            {
                var service = container.Resolve<ITestService>();
                var manager = container.Resolve<ITestManager>();
                var factory = container.Resolve<ITestFactory>();
                var mono = container.Resolve<TestMonoBehaviour>();
                var interfaceImpl = container.Resolve<ITestInterface>();
            }
            catch (System.Exception ex)
            {
                if (enableDetailedLogging)
                {
                    GameLogger.LogWarning($"테스트 해결 중 오류: {ex.Message}", GameLogger.LogCategory.Core);
                }
            }
        }

        private void LogDetailedResults()
        {
            GameLogger.LogInfo("=== 상세 테스트 결과 ===", GameLogger.LogCategory.Core);
            
            if (bindingTimes != null && bindingTimes.Count > 0)
            {
                GameLogger.LogInfo($"바인딩 시간 분포:", GameLogger.LogCategory.Core);
                LogTimeDistribution(bindingTimes);
            }
            
            if (resolutionTimes != null && resolutionTimes.Count > 0)
            {
                GameLogger.LogInfo($"해결 시간 분포:", GameLogger.LogCategory.Core);
                LogTimeDistribution(resolutionTimes);
            }
        }

        private void LogTimeDistribution(List<long> times)
        {
            times.Sort();
            int count = times.Count;
            
            GameLogger.LogInfo($"- 최소: {times[0]}ms", GameLogger.LogCategory.Core);
            GameLogger.LogInfo($"- 최대: {times[count - 1]}ms", GameLogger.LogCategory.Core);
            GameLogger.LogInfo($"- 중간값: {times[count / 2]}ms", GameLogger.LogCategory.Core);
            GameLogger.LogInfo($"- 25%: {times[count / 4]}ms", GameLogger.LogCategory.Core);
            GameLogger.LogInfo($"- 75%: {times[count * 3 / 4]}ms", GameLogger.LogCategory.Core);
        }

        #region 테스트용 인터페이스 및 클래스들

        // 테스트용 인터페이스들
        public interface ITestService { }
        public interface ITestManager { }
        public interface ITestFactory { }
        public interface ITestInterface { }

        // 테스트용 구현체들
        public class TestService : ITestService { }
        public class TestManager : ITestManager { }
        public class TestFactory : ITestFactory { }
        public class TestImplementation : ITestInterface { }
        public class TestMonoBehaviour : MonoBehaviour { }

        #endregion
    }
}
