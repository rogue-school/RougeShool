using UnityEngine;

namespace Game.Utility
{
    /// <summary>
    /// 키보드 단축키로 로그를 제어하는 스크립트
    /// 개발 중 빠른 로그 제어를 위한 유틸리티
    /// </summary>
    public class LogKeyboardControl : MonoBehaviour
    {
        [Header("단축키 설정")]
        [SerializeField] private KeyCode toggleAllLogsKey = KeyCode.F1;
        [SerializeField] private KeyCode toggleCombatLogsKey = KeyCode.F2;
        [SerializeField] private KeyCode toggleAnimationLogsKey = KeyCode.F3;
        [SerializeField] private KeyCode toggleAnimationVerboseKey = KeyCode.F4;
        [SerializeField] private KeyCode toggleDatabaseLogsKey = KeyCode.F5;
        [SerializeField] private KeyCode togglePerformanceLogsKey = KeyCode.F6;
        [SerializeField] private KeyCode printLogStatusKey = KeyCode.F7;
        [SerializeField] private KeyCode enableAllLogsKey = KeyCode.F8;
        [SerializeField] private KeyCode disableAllLogsKey = KeyCode.F9;

        [Header("디버그 정보")]
        [SerializeField] private bool showKeyBindings = true;

        private void Start()
        {
            if (showKeyBindings)
            {
                GameLogger.LogInfo("=== 로그 단축키 ===");
                GameLogger.LogInfo($"F1: 모든 로그 토글");
                GameLogger.LogInfo($"F2: 전투 로그 토글");
                GameLogger.LogInfo($"F3: 애니메이션 로그 토글");
                GameLogger.LogInfo($"F4: 상세 애니메이션 로그 토글");
                GameLogger.LogInfo($"F5: 데이터베이스 로그 토글");
                GameLogger.LogInfo($"F6: 성능 로그 토글");
                GameLogger.LogInfo($"F7: 로그 상태 출력");
                GameLogger.LogInfo($"F8: 모든 로그 활성화");
                GameLogger.LogInfo($"F9: 모든 로그 비활성화");
            }
        }

        private void Update()
        {
            HandleKeyboardInput();
        }

        private void HandleKeyboardInput()
        {
            // F1: 모든 로그 토글
            if (Input.GetKeyDown(toggleAllLogsKey))
            {
                ToggleAllLogs();
            }

            // F2: 전투 로그 토글
            if (Input.GetKeyDown(toggleCombatLogsKey))
            {
                ToggleLogCategory(GameLogger.LogCategory.Combat, "전투");
            }

            // F3: 애니메이션 로그 토글
            if (Input.GetKeyDown(toggleAnimationLogsKey))
            {
                ToggleLogCategory(GameLogger.LogCategory.Animation, "애니메이션");
            }

            // F4: 상세 애니메이션 로그 토글
            if (Input.GetKeyDown(toggleAnimationVerboseKey))
            {
                ToggleLogCategory(GameLogger.LogCategory.AnimationVerbose, "상세 애니메이션");
            }

            // F5: 데이터베이스 로그 토글
            if (Input.GetKeyDown(toggleDatabaseLogsKey))
            {
                ToggleLogCategory(GameLogger.LogCategory.Database, "데이터베이스");
            }

            // F6: 성능 로그 토글
            if (Input.GetKeyDown(togglePerformanceLogsKey))
            {
                ToggleLogCategory(GameLogger.LogCategory.Performance, "성능");
            }

            // F7: 로그 상태 출력
            if (Input.GetKeyDown(printLogStatusKey))
            {
                GameLogger.PrintLogStatus();
            }

            // F8: 모든 로그 활성화
            if (Input.GetKeyDown(enableAllLogsKey))
            {
                GameLogger.EnableAllLogs();
                GameLogger.LogInfo("모든 로그 활성화됨");
            }

            // F9: 모든 로그 비활성화
            if (Input.GetKeyDown(disableAllLogsKey))
            {
                GameLogger.DisableAllLogs();
                GameLogger.LogInfo("모든 로그 비활성화됨 (에러 제외)");
            }
        }

        private void ToggleAllLogs()
        {
            bool allEnabled = GameLogger.IsLogCategoryEnabled(GameLogger.LogCategory.Combat) &&
                             GameLogger.IsLogCategoryEnabled(GameLogger.LogCategory.Animation) &&
                             GameLogger.IsLogCategoryEnabled(GameLogger.LogCategory.Slot) &&
                             GameLogger.IsLogCategoryEnabled(GameLogger.LogCategory.Character) &&
                             GameLogger.IsLogCategoryEnabled(GameLogger.LogCategory.SkillCard);

            if (allEnabled)
            {
                GameLogger.DisableAllLogs();
                GameLogger.LogInfo("모든 로그 비활성화됨");
            }
            else
            {
                GameLogger.EnableAllLogs();
                GameLogger.LogInfo("모든 로그 활성화됨");
            }
        }

        private void ToggleLogCategory(GameLogger.LogCategory category, string categoryName)
        {
            bool currentState = GameLogger.IsLogCategoryEnabled(category);
            bool newState = !currentState;
            
            GameLogger.ToggleLogCategory(category, newState);
            GameLogger.LogInfo($"{categoryName} 로그 {(newState ? "활성화" : "비활성화")}됨");
        }

        /// <summary>
        /// 현재 로그 상태를 UI에 표시하기 위한 메서드
        /// </summary>
        public string GetLogStatusText()
        {
            var states = GameLogger.GetLogCategoryStates();
            string status = "=== 로그 상태 ===\n";
            
            foreach (var kvp in states)
            {
                string statusIcon = kvp.Value ? "●" : "○";
                status += $"{statusIcon} {kvp.Key}\n";
            }
            
            return status;
        }

        /// <summary>
        /// 특정 카테고리의 로그 상태 반환
        /// </summary>
        public bool IsCategoryEnabled(GameLogger.LogCategory category)
        {
            return GameLogger.IsLogCategoryEnabled(category);
        }
    }
} 