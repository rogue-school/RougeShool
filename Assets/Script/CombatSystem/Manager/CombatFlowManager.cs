using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Game.CoreSystem.Utility;
using Game.CharacterSystem.Interface;
using Game.CharacterSystem.Manager;
using Game.SkillCardSystem.Manager;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.Data;
using Game.SkillCardSystem.Factory;
using Game.SkillCardSystem.UI;
using Game.CombatSystem.Slot;
using Game.CombatSystem.Utility;
using Game.CombatSystem.Interface;
using Game.StageSystem.Manager;
using Game.StageSystem.Interface;
using Zenject;

namespace Game.CombatSystem.Manager
{
    /// <summary>
    /// 전투 플로우 전체를 관리하는 통합 매니저입니다.
    /// 
    /// 주요 기능:
    /// - 전투 시작부터 종료까지의 전체 플로우 관리
    /// - 턴 기반 전투 시스템 제어
    /// - 캐릭터 및 카드 시스템과의 통합 관리
    /// - 상태 기반 전투 진행 제어
    /// 
    /// 사용 예시:
    /// ```csharp
    /// // 전투 시작
    /// combatFlowManager.StartCombat();
    /// 
    /// // 턴 진행
    /// combatFlowManager.ProgressTurn();
    /// 
    /// // 전투 종료
    /// combatFlowManager.EndCombat();
    /// ```
    /// </summary>
    public class CombatFlowManager : MonoBehaviour, ICombatFlowManager
    {
        #region 기본 설정

        [Header("매니저 기본 설정")]
        [Tooltip("디버그 로깅 활성화")]
        [SerializeField] private bool enableDebugLogging = true;

        [Tooltip("자동 초기화 활성화")]
        [SerializeField] private bool autoInitialize = true;

        #endregion

        #region 전투 설정

        [Header("전투 설정")]
        [Tooltip("초기 전투 상태")]
        [SerializeField] private CombatPhase initialPhase = CombatPhase.Preparation;

        [Tooltip("턴 기반 전투 활성화")]
        [SerializeField] private bool enableTurnBasedCombat = true;

        [Tooltip("자동 턴 진행")]
        [SerializeField] private bool autoProgressTurn = false;

        #endregion

        #region 의존성 주입

        [Inject] private IPlayerManager playerManager;
        [Inject] private IEnemyManager enemyManager;
        [Inject] private IPlayerHandManager playerHandManager;
        [Inject] private IStageManager stageManager;
        [Inject] private CombatSlotManager slotManager;
        [Inject] private TurnManager turnManager;
        [Inject] private CombatExecutionManager executionManager;

        #endregion

        #region 내부 상태

        private CombatPhase currentPhase;
        private bool isCombatActive = false;
        private bool isInitialized = false;

        #endregion

        #region 이벤트

        public System.Action<CombatPhase> OnCombatPhaseChanged { get; set; }
        public System.Action OnCombatStarted { get; set; }
        public System.Action OnCombatEnded { get; set; }

        #endregion

        #region Unity 생명주기

        private void Awake()
        {
            if (enableDebugLogging)
            {
                GameLogger.LogInfo("CombatFlowManager 초기화 시작", GameLogger.LogCategory.Combat);
            }
        }

        private void Start()
        {
            if (autoInitialize)
            {
                StartCoroutine(InitializeCombat());
            }
        }

        #endregion

        #region 초기화

        private IEnumerator InitializeCombat()
        {
            if (isInitialized)
            {
                yield break;
            }

            if (enableDebugLogging)
            {
                GameLogger.LogInfo("전투 시스템 초기화 중...", GameLogger.LogCategory.Combat);
            }

            // 의존성 검증
            if (!ValidateDependencies())
            {
                GameLogger.LogError("전투 시스템 의존성 검증 실패", GameLogger.LogCategory.Error);
                yield break;
            }

            // 초기 상태 설정
            currentPhase = initialPhase;
            isCombatActive = false;

            // 서브 매니저들 초기화
            yield return StartCoroutine(InitializeSubManagers());

            isInitialized = true;

            if (enableDebugLogging)
            {
                GameLogger.LogInfo("전투 시스템 초기화 완료", GameLogger.LogCategory.Combat);
            }
        }

        private IEnumerator InitializeSubManagers()
        {
            // 슬롯 매니저 초기화 (CombatSlotManager는 싱글톤으로 자동 초기화됨)
            if (slotManager != null)
            {
                // CombatSlotManager는 Awake에서 자동 초기화되므로 별도 초기화 불필요
                yield return null;
            }

            // 턴 매니저 초기화 (TurnManager는 싱글톤으로 자동 초기화됨)
            if (turnManager != null)
            {
                // TurnManager는 Awake에서 자동 초기화되므로 별도 초기화 불필요
                yield return null;
            }

            // 실행 매니저 초기화 (CombatExecutionManager는 별도 초기화 불필요)
            if (executionManager != null)
            {
                // CombatExecutionManager는 Start에서 자동 초기화되므로 별도 초기화 불필요
                yield return null;
            }
        }

        #endregion

        #region 전투 플로우 관리

        /// <summary>
        /// 전투 시작
        /// </summary>
        public void StartCombat()
        {
            if (!isInitialized)
            {
                GameLogger.LogWarning("전투 시스템이 초기화되지 않았습니다.", GameLogger.LogCategory.Combat);
                return;
            }

            if (isCombatActive)
            {
                GameLogger.LogWarning("전투가 이미 진행 중입니다.", GameLogger.LogCategory.Combat);
                return;
            }

            StartCoroutine(StartCombatCoroutine());
        }

        private IEnumerator StartCombatCoroutine()
        {
            if (enableDebugLogging)
            {
                GameLogger.LogInfo("전투 시작", GameLogger.LogCategory.Combat);
            }

            // 전투 준비 단계
            yield return StartCoroutine(PrepareCombat());

            // 전투 활성화
            isCombatActive = true;
            ChangeCombatPhase(CombatPhase.Preparation);

            // 전투 시작 이벤트 발생
            OnCombatStarted?.Invoke();

            if (enableDebugLogging)
            {
                GameLogger.LogInfo("전투 준비 완료", GameLogger.LogCategory.Combat);
            }
        }

        private IEnumerator PrepareCombat()
        {
            // 매니저들 초기화 확인
            if (playerManager != null)
            {
                GameLogger.LogInfo("플레이어 매니저 확인 완료", GameLogger.LogCategory.Combat);
            }

            if (stageManager != null)
            {
                GameLogger.LogInfo("스테이지 매니저 확인 완료", GameLogger.LogCategory.Combat);
            }

            if (playerHandManager != null)
            {
                GameLogger.LogInfo("플레이어 핸드 매니저 확인 완료", GameLogger.LogCategory.Combat);
            }

            yield return null; // 코루틴을 위해 yield return 추가
        }

        /// <summary>
        /// 전투 종료
        /// </summary>
        public void EndCombat()
        {
            if (!isCombatActive)
            {
                return;
            }

            StartCoroutine(EndCombatCoroutine());
        }

        private IEnumerator EndCombatCoroutine()
        {
            if (enableDebugLogging)
            {
                GameLogger.LogInfo("전투 종료", GameLogger.LogCategory.Combat);
            }

            // 전투 비활성화
            isCombatActive = false;
            ChangeCombatPhase(CombatPhase.Ended);

            // 정리 작업
            yield return StartCoroutine(CleanupCombat());

            // 전투 종료 이벤트 발생
            OnCombatEnded?.Invoke();

            if (enableDebugLogging)
            {
                GameLogger.LogInfo("전투 정리 완료", GameLogger.LogCategory.Combat);
            }
        }

        private IEnumerator CleanupCombat()
        {
            // 슬롯 정리
            if (slotManager != null)
            {
                slotManager.ClearAllSlots();
            }

            // 턴 매니저 정리
            if (turnManager != null)
            {
                turnManager.ResetTurn();
            }

            yield return null;
        }

        #endregion

        #region 전투 상태 관리

        /// <summary>
        /// 전투 단계 변경
        /// </summary>
        public void ChangeCombatPhase(CombatPhase newPhase)
        {
            if (currentPhase == newPhase)
            {
                return;
            }

            CombatPhase previousPhase = currentPhase;
            currentPhase = newPhase;

            if (enableDebugLogging)
            {
                GameLogger.LogInfo($"전투 단계 변경: {previousPhase} → {newPhase}", GameLogger.LogCategory.Combat);
            }

            // 단계별 처리
            OnCombatPhaseChanged?.Invoke(newPhase);
        }

        /// <summary>
        /// 다음 턴으로 진행
        /// </summary>
        public void ProgressTurn()
        {
            if (!isCombatActive)
            {
                GameLogger.LogWarning("전투가 진행 중이 아닙니다.", GameLogger.LogCategory.Combat);
                return;
            }

            if (!enableTurnBasedCombat)
            {
                GameLogger.LogWarning("턴 기반 전투가 비활성화되어 있습니다.", GameLogger.LogCategory.Combat);
                return;
            }

            if (turnManager != null)
            {
                turnManager.NextTurn();
            }

            // 자동 턴 진행이 활성화된 경우 다음 턴도 자동으로 진행
            if (autoProgressTurn && turnManager != null)
            {
                StartCoroutine(AutoProgressNextTurn());
            }
        }

        private System.Collections.IEnumerator AutoProgressNextTurn()
        {
            yield return new WaitForSeconds(1.0f); // 1초 대기
            if (isCombatActive && enableTurnBasedCombat)
            {
                turnManager.NextTurn();
            }
        }

        #endregion

        #region 유틸리티

        private bool ValidateDependencies()
        {
            bool isValid = true;

            if (playerManager == null)
            {
                GameLogger.LogError("PlayerManager가 주입되지 않았습니다.", GameLogger.LogCategory.Error);
                isValid = false;
            }

            if (enemyManager == null)
            {
                GameLogger.LogError("EnemyManager가 주입되지 않았습니다.", GameLogger.LogCategory.Error);
                isValid = false;
            }

            if (slotManager == null)
            {
                GameLogger.LogError("CombatSlotManager가 주입되지 않았습니다.", GameLogger.LogCategory.Error);
                isValid = false;
            }

            if (turnManager == null)
            {
                GameLogger.LogError("CombatTurnManager가 주입되지 않았습니다.", GameLogger.LogCategory.Error);
                isValid = false;
            }

            if (executionManager == null)
            {
                GameLogger.LogError("CombatExecutionManager가 주입되지 않았습니다.", GameLogger.LogCategory.Error);
                isValid = false;
            }

            return isValid;
        }

        #endregion

        #region 공개 프로퍼티

        public CombatPhase CurrentPhase => currentPhase;
        public bool IsCombatActive => isCombatActive;
        public bool IsInitialized => isInitialized;

        #endregion

        #region 리셋

        public void ResetCombat()
        {
            isCombatActive = false;
            currentPhase = initialPhase;
            isInitialized = false;

            if (enableDebugLogging)
            {
                GameLogger.LogInfo("CombatFlowManager 리셋 완료", GameLogger.LogCategory.Combat);
            }
        }

        #endregion
    }
}
