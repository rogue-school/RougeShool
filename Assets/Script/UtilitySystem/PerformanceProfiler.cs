using UnityEngine;
using System.Collections.Generic;
using System.Diagnostics;

namespace Game.Utility
{
    /// <summary>
    /// 성능 측정을 위한 전문적인 프로파일러
    /// 메서드 실행 시간, 메모리 사용량 등을 측정
    /// </summary>
    public static class PerformanceProfiler
    {
        #region Performance Measurement
        private static readonly Dictionary<string, Stopwatch> stopwatches = new Dictionary<string, Stopwatch>();
        private static readonly Dictionary<string, float> measurements = new Dictionary<string, float>();

        /// <summary>
        /// 성능 측정 시작
        /// </summary>
        [Conditional("UNITY_EDITOR")]
        [Conditional("DEVELOPMENT_BUILD")]
        public static void StartMeasurement(string operationName)
        {
            if (!GameLogger.IsLogCategoryEnabled(GameLogger.LogCategory.Performance))
                return;

            if (stopwatches.ContainsKey(operationName))
            {
                stopwatches[operationName].Restart();
            }
            else
            {
                stopwatches[operationName] = Stopwatch.StartNew();
            }

            GameLogger.LogPerformanceStart(operationName);
        }

        /// <summary>
        /// 성능 측정 완료
        /// </summary>
        [Conditional("UNITY_EDITOR")]
        [Conditional("DEVELOPMENT_BUILD")]
        public static void EndMeasurement(string operationName)
        {
            if (!GameLogger.IsLogCategoryEnabled(GameLogger.LogCategory.Performance))
                return;

            if (stopwatches.TryGetValue(operationName, out Stopwatch stopwatch))
            {
                stopwatch.Stop();
                float elapsedMs = stopwatch.ElapsedMilliseconds;
                measurements[operationName] = elapsedMs;
                
                GameLogger.LogPerformanceEnd(operationName, elapsedMs);
            }
        }

        /// <summary>
        /// 성능 측정 결과 반환
        /// </summary>
        public static float GetMeasurement(string operationName)
        {
            return measurements.TryGetValue(operationName, out float value) ? value : 0f;
        }

        /// <summary>
        /// 모든 측정 결과 반환
        /// </summary>
        public static Dictionary<string, float> GetAllMeasurements()
        {
            return new Dictionary<string, float>(measurements);
        }

        /// <summary>
        /// 측정 결과 초기화
        /// </summary>
        public static void ClearMeasurements()
        {
            stopwatches.Clear();
            measurements.Clear();
        }
        #endregion

        #region Memory Profiling
        /// <summary>
        /// 현재 메모리 사용량 측정
        /// </summary>
        [Conditional("UNITY_EDITOR")]
        [Conditional("DEVELOPMENT_BUILD")]
        public static void LogMemoryUsage(string context = "")
        {
            if (!GameLogger.IsLogCategoryEnabled(GameLogger.LogCategory.Performance))
                return;

            long totalMemory = System.GC.GetTotalMemory(false);
            long managedMemory = System.GC.GetTotalMemory(true);
            
            string contextText = string.IsNullOrEmpty(context) ? "" : $" [{context}]";
            GameLogger.LogPerformance($"메모리 사용량{contextText}: 총 {totalMemory / 1024 / 1024}MB, 관리 {managedMemory / 1024 / 1024}MB");
        }

        /// <summary>
        /// 가비지 컬렉션 강제 실행
        /// </summary>
        [Conditional("UNITY_EDITOR")]
        [Conditional("DEVELOPMENT_BUILD")]
        public static void ForceGarbageCollection()
        {
            if (!GameLogger.IsLogCategoryEnabled(GameLogger.LogCategory.Performance))
                return;

            long beforeMemory = System.GC.GetTotalMemory(false);
            System.GC.Collect();
            long afterMemory = System.GC.GetTotalMemory(false);
            
            GameLogger.LogPerformance($"가비지 컬렉션 실행: {beforeMemory / 1024 / 1024}MB → {afterMemory / 1024 / 1024}MB ({(beforeMemory - afterMemory) / 1024 / 1024}MB 해제)");
        }
        #endregion

        #region Frame Rate Monitoring
        private static float frameTimeSum = 0f;
        private static int frameCount = 0;
        private static float lastFrameTime = 0f;

        /// <summary>
        /// 프레임 시간 측정 (Update에서 호출)
        /// </summary>
        [Conditional("UNITY_EDITOR")]
        [Conditional("DEVELOPMENT_BUILD")]
        public static void UpdateFrameTime()
        {
            if (!GameLogger.IsLogCategoryEnabled(GameLogger.LogCategory.Performance))
                return;

            float currentFrameTime = Time.deltaTime;
            frameTimeSum += currentFrameTime;
            frameCount++;
            lastFrameTime = currentFrameTime;

            // 1초마다 평균 FPS 출력
            if (frameTimeSum >= 1.0f)
            {
                float averageFPS = frameCount / frameTimeSum;
                float currentFPS = 1f / currentFrameTime;
                
                GameLogger.LogPerformance($"FPS: 현재 {currentFPS:F1}, 평균 {averageFPS:F1}");
                
                frameTimeSum = 0f;
                frameCount = 0;
            }
        }

        /// <summary>
        /// 현재 FPS 반환
        /// </summary>
        public static float GetCurrentFPS()
        {
            return lastFrameTime > 0 ? 1f / lastFrameTime : 0f;
        }
        #endregion

        #region Method Profiling
        /// <summary>
        /// 메서드 실행 시간 측정을 위한 래퍼
        /// </summary>
        public static void ProfileMethod(string methodName, System.Action method)
        {
            StartMeasurement(methodName);
            method?.Invoke();
            EndMeasurement(methodName);
        }

        /// <summary>
        /// 코루틴 실행 시간 측정을 위한 래퍼
        /// </summary>
        public static System.Collections.IEnumerator ProfileCoroutine(string coroutineName, System.Collections.IEnumerator coroutine)
        {
            StartMeasurement(coroutineName);
            
            while (coroutine.MoveNext())
            {
                yield return coroutine.Current;
            }
            
            EndMeasurement(coroutineName);
        }
        #endregion

        #region Performance Warnings
        /// <summary>
        /// 성능 경고 체크
        /// </summary>
        [Conditional("UNITY_EDITOR")]
        [Conditional("DEVELOPMENT_BUILD")]
        public static void CheckPerformanceWarning(string operationName, float thresholdMs = 16.67f) // 60fps 기준
        {
            if (!GameLogger.IsLogCategoryEnabled(GameLogger.LogCategory.Performance))
                return;

            float measurement = GetMeasurement(operationName);
            if (measurement > thresholdMs)
            {
                GameLogger.LogWarning($"성능 경고: {operationName}이 {measurement:F2}ms 소요됨 (임계값: {thresholdMs}ms)", GameLogger.LogCategory.Performance);
            }
        }

        /// <summary>
        /// 메모리 사용량 경고 체크
        /// </summary>
        [Conditional("UNITY_EDITOR")]
        [Conditional("DEVELOPMENT_BUILD")]
        public static void CheckMemoryWarning(long thresholdMB = 1000) // 1GB
        {
            if (!GameLogger.IsLogCategoryEnabled(GameLogger.LogCategory.Performance))
                return;

            long totalMemory = System.GC.GetTotalMemory(false);
            long memoryMB = totalMemory / 1024 / 1024;
            
            if (memoryMB > thresholdMB)
            {
                GameLogger.LogWarning($"메모리 경고: {memoryMB}MB 사용 중 (임계값: {thresholdMB}MB)", GameLogger.LogCategory.Performance);
            }
        }
        #endregion

        #region Utility Methods
        /// <summary>
        /// 성능 통계 출력
        /// </summary>
        [Conditional("UNITY_EDITOR")]
        [Conditional("DEVELOPMENT_BUILD")]
        public static void PrintPerformanceStats()
        {
            if (!GameLogger.IsLogCategoryEnabled(GameLogger.LogCategory.Performance))
                return;

            GameLogger.LogPerformance("=== 성능 통계 ===");
            
            foreach (var kvp in measurements)
            {
                GameLogger.LogPerformance($"{kvp.Key}: {kvp.Value:F2}ms");
            }
            
            LogMemoryUsage("통계");
            GameLogger.LogPerformance($"현재 FPS: {GetCurrentFPS():F1}");
        }

        /// <summary>
        /// 성능 측정 결과를 JSON 형태로 반환
        /// </summary>
        public static string GetPerformanceStatsAsJson()
        {
            var stats = new Dictionary<string, object>
            {
                ["measurements"] = measurements,
                ["currentFPS"] = GetCurrentFPS(),
                ["totalMemory"] = System.GC.GetTotalMemory(false) / 1024 / 1024
            };
            
            return JsonUtility.ToJson(stats, true);
        }
        #endregion
    }
} 