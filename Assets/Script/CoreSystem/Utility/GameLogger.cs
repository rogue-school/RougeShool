using UnityEngine;
using System.Collections.Generic;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

namespace Game.CoreSystem.Utility
{
    /// <summary>
    /// 현업 수준의 전문적인 디버그 로그 관리 시스템
    /// 조건부 컴파일, 로그 레벨, 카테고리별 제어, 성능 최적화 포함
    /// </summary>
    public static class GameLogger
    {
        #region Log Levels
        public enum LogLevel
        {
            None = 0,
            Error = 1,
            Warning = 2,
            Info = 3,
            Debug = 4,
            Verbose = 5
        }

        private static LogLevel currentLevel = LogLevel.Info;
        #endregion

        #region Log Categories
        public enum LogCategory
        {
            Core,
            Combat,
            Animation,
            AnimationVerbose,
            Slot,
            Character,
            SkillCard,
            Database,
            Performance,
            Network,
            UI,
            Audio,
            Save,
            Error
        }

        private static readonly Dictionary<LogCategory, bool> logCategories = new Dictionary<LogCategory, bool>
        {
            [LogCategory.Core] = true,
            [LogCategory.Combat] = true,
            [LogCategory.Animation] = false,
            [LogCategory.AnimationVerbose] = false,
            [LogCategory.Slot] = true,
            [LogCategory.Character] = true,
            [LogCategory.SkillCard] = false,
            [LogCategory.Database] = false,
            [LogCategory.Performance] = false,
            [LogCategory.Network] = false,
            [LogCategory.UI] = true,
            [LogCategory.Audio] = false,
            [LogCategory.Save] = false,
            [LogCategory.Error] = true
        };
        #endregion

        #region Initialization
        /// <summary>
        /// 로거 초기화
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            #if UNITY_EDITOR
            currentLevel = LogLevel.Debug;
            #elif DEVELOPMENT_BUILD
            currentLevel = LogLevel.Info;
            #else
            currentLevel = LogLevel.Error;
            #endif
        }
        #endregion

        #region Log Level Control
        /// <summary>
        /// 로그 레벨 설정
        /// </summary>
        public static void SetLogLevel(LogLevel level)
        {
            currentLevel = level;
        }

        /// <summary>
        /// 현재 로그 레벨 반환
        /// </summary>
        public static LogLevel GetCurrentLogLevel() => currentLevel;
        #endregion

        #region Category Control
        /// <summary>
        /// 로그 카테고리 토글
        /// </summary>
        public static void ToggleLogCategory(LogCategory category, bool enabled)
        {
            if (logCategories.ContainsKey(category))
            {
                logCategories[category] = enabled;
                LogInfo($"{category} 로그 {(enabled ? "활성화" : "비활성화")}", LogCategory.Combat);
            }
        }

        /// <summary>
        /// 로그 카테고리 상태 확인
        /// </summary>
        public static bool IsLogCategoryEnabled(LogCategory category)
        {
            return logCategories.ContainsKey(category) && logCategories[category];
        }

        /// <summary>
        /// 모든 로그 카테고리 비활성화 (에러 제외)
        /// </summary>
        public static void DisableAllLogs()
        {
            foreach (var key in logCategories.Keys)
            {
                if (key != LogCategory.Error)
                    logCategories[key] = false;
            }
            LogInfo("모든 로그 비활성화 (에러 제외)", LogCategory.Combat);
        }

        /// <summary>
        /// 모든 로그 카테고리 활성화
        /// </summary>
        public static void EnableAllLogs()
        {
            foreach (var key in logCategories.Keys)
            {
                logCategories[key] = true;
            }
            LogInfo("모든 로그 활성화", LogCategory.Combat);
        }

        /// <summary>
        /// 로그 카테고리 상태 반환
        /// </summary>
        public static Dictionary<LogCategory, bool> GetLogCategoryStates()
        {
            return new Dictionary<LogCategory, bool>(logCategories);
        }
        #endregion

        #region Log Methods
        /// <summary>
        /// 에러 로그 (항상 출력)
        /// </summary>
        public static void LogError(string message, LogCategory category = LogCategory.Error)
        {
            Debug.LogError($"[{category}] {message}");
        }

        /// <summary>
        /// 경고 로그
        /// </summary>
        [Conditional("UNITY_EDITOR")]
        [Conditional("DEVELOPMENT_BUILD")]
        public static void LogWarning(string message, LogCategory category = LogCategory.Combat)
        {
            if (currentLevel >= LogLevel.Warning && IsLogCategoryEnabled(category))
                Debug.LogWarning($"[{category}] {message}");
        }

        /// <summary>
        /// 정보 로그
        /// </summary>
        [Conditional("UNITY_EDITOR")]
        [Conditional("DEVELOPMENT_BUILD")]
        public static void LogInfo(string message, LogCategory category = LogCategory.Combat)
        {
            if (currentLevel >= LogLevel.Info && IsLogCategoryEnabled(category))
                Debug.Log($"[{category}] {message}");
        }

        /// <summary>
        /// 디버그 로그
        /// </summary>
        [Conditional("UNITY_EDITOR")]
        [Conditional("DEVELOPMENT_BUILD")]
        public static void LogDebug(string message, LogCategory category = LogCategory.Combat)
        {
            if (currentLevel >= LogLevel.Debug && IsLogCategoryEnabled(category))
                Debug.Log($"[{category}] {message}");
        }

        /// <summary>
        /// 상세 로그 (에디터에서만)
        /// </summary>
        [Conditional("UNITY_EDITOR")]
        public static void LogVerbose(string message, LogCategory category = LogCategory.Combat)
        {
            if (currentLevel >= LogLevel.Verbose && IsLogCategoryEnabled(category))
                Debug.Log($"[{category}] {message}");
        }
        #endregion

        #region Category-Specific Log Methods
        /// <summary>
        /// 전투 시스템 로그
        /// </summary>
        [Conditional("UNITY_EDITOR")]
        [Conditional("DEVELOPMENT_BUILD")]
        public static void LogCombat(string message)
        {
            LogInfo(message, LogCategory.Combat);
        }

        /// <summary>
        /// 애니메이션 시스템 로그
        /// </summary>
        [Conditional("UNITY_EDITOR")]
        [Conditional("DEVELOPMENT_BUILD")]
        public static void LogAnimation(string message)
        {
            LogInfo(message, LogCategory.Animation);
        }

        /// <summary>
        /// 상세 애니메이션 로그 (에디터에서만)
        /// </summary>
        [Conditional("UNITY_EDITOR")]
        public static void LogAnimationVerbose(string message)
        {
            LogVerbose(message, LogCategory.AnimationVerbose);
        }

        /// <summary>
        /// 슬롯 시스템 로그
        /// </summary>
        [Conditional("UNITY_EDITOR")]
        [Conditional("DEVELOPMENT_BUILD")]
        public static void LogSlot(string message)
        {
            LogInfo(message, LogCategory.Slot);
        }

        /// <summary>
        /// 캐릭터 시스템 로그
        /// </summary>
        [Conditional("UNITY_EDITOR")]
        [Conditional("DEVELOPMENT_BUILD")]
        public static void LogCharacter(string message)
        {
            LogInfo(message, LogCategory.Character);
        }

        /// <summary>
        /// 스킬카드 시스템 로그
        /// </summary>
        [Conditional("UNITY_EDITOR")]
        [Conditional("DEVELOPMENT_BUILD")]
        public static void LogSkillCard(string message)
        {
            LogInfo(message, LogCategory.SkillCard);
        }

        /// <summary>
        /// 데이터베이스 로그
        /// </summary>
        [Conditional("UNITY_EDITOR")]
        [Conditional("DEVELOPMENT_BUILD")]
        public static void LogDatabase(string message)
        {
            LogInfo(message, LogCategory.Database);
        }

        /// <summary>
        /// 성능 로그
        /// </summary>
        [Conditional("UNITY_EDITOR")]
        [Conditional("DEVELOPMENT_BUILD")]
        public static void LogPerformance(string message)
        {
            LogInfo(message, LogCategory.Performance);
        }

        /// <summary>
        /// 네트워크 로그
        /// </summary>
        [Conditional("UNITY_EDITOR")]
        [Conditional("DEVELOPMENT_BUILD")]
        public static void LogNetwork(string message)
        {
            LogInfo(message, LogCategory.Network);
        }

        /// <summary>
        /// UI 로그
        /// </summary>
        [Conditional("UNITY_EDITOR")]
        [Conditional("DEVELOPMENT_BUILD")]
        public static void LogUI(string message)
        {
            LogInfo(message, LogCategory.UI);
        }

        /// <summary>
        /// 오디오 로그
        /// </summary>
        [Conditional("UNITY_EDITOR")]
        [Conditional("DEVELOPMENT_BUILD")]
        public static void LogAudio(string message)
        {
            LogInfo(message, LogCategory.Audio);
        }
        #endregion

        #region Performance Logging
        /// <summary>
        /// 성능 측정 시작
        /// </summary>
        [Conditional("UNITY_EDITOR")]
        [Conditional("DEVELOPMENT_BUILD")]
        public static void LogPerformanceStart(string operationName)
        {
            if (IsLogCategoryEnabled(LogCategory.Performance))
            {
                LogPerformance($"시작: {operationName}");
            }
        }

        /// <summary>
        /// 성능 측정 완료
        /// </summary>
        [Conditional("UNITY_EDITOR")]
        [Conditional("DEVELOPMENT_BUILD")]
        public static void LogPerformanceEnd(string operationName, float elapsedTime)
        {
            if (IsLogCategoryEnabled(LogCategory.Performance))
            {
                LogPerformance($"완료: {operationName} ({elapsedTime:F3}ms)");
            }
        }
        #endregion

        #region Utility Methods
        /// <summary>
        /// 현재 로그 상태 출력
        /// </summary>
        [Conditional("UNITY_EDITOR")]
        [Conditional("DEVELOPMENT_BUILD")]
        public static void PrintLogStatus()
        {
            LogInfo($"=== 로그 상태 ===");
            LogInfo($"현재 레벨: {currentLevel}");
            LogInfo($"활성화된 카테고리:");
            foreach (var kvp in logCategories)
            {
                if (kvp.Value)
                    LogInfo($"  - {kvp.Key}");
            }
        }

        /// <summary>
        /// 로그 카테고리별 통계 반환
        /// </summary>
        public static Dictionary<LogCategory, int> GetLogStatistics()
        {
            // 실제 구현에서는 로그 카운터를 추가할 수 있음
            return new Dictionary<LogCategory, int>();
        }
        #endregion
    }
} 