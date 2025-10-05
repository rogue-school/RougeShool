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
using Game.CombatSystem.State;

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


        #endregion

        #region 전투 설정

        [Header("전투 설정")]
        [Tooltip("초기 전투 상태")]
        [SerializeField] private CombatPhase initialPhase = CombatPhase.Preparation;

        [Tooltip("턴 기반 전투 활성화")]
        [SerializeField] private bool enableTurnBasedCombat = true;

        // autoProgressTurn 제거됨 (상태 패턴에서 자동으로 처리)

        #endregion

        #region 의존성 주입

        [Inject] private PlayerManager playerManager;
        [Inject] private EnemyManager enemyManager;
        [Inject] private IPlayerHandManager playerHandManager;
        [Inject] private IStageManager stageManager;
        // CombatSlotManager 제거됨 - 슬롯 관리 기능을 직접 구현
        [Inject] private TurnManager turnManager;
        [Inject] private CombatExecutionManager executionManager;

        #endregion

        #region 내부 상태

        private CombatPhase currentPhase;
        private bool isCombatActive = false;
        private bool isInitialized = false;

        // 메타 플로우 상태 머신 (전투 외부 흐름 관리)
        private enum FlowState
        {
            Prepare,
            InCombat,
            Victory,
            Rewards,
            StageTransition,
            GameOver
        }

        private FlowState currentFlowState = FlowState.Prepare;

        #endregion

        #region 이벤트

        public System.Action<CombatPhase> OnCombatPhaseChanged { get; set; }
        public System.Action OnCombatStarted { get; set; }
        public System.Action OnCombatEnded { get; set; }
        public System.Action<ICharacter> OnEnemyDefeated { get; set; }

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
            // 의존성 검증을 지연시켜 다른 매니저들이 완전히 초기화될 때까지 대기
            // GameStartupController에서 StartCombat() 호출 시 초기화됨
        }

        #endregion

        #region 초기화

        public IEnumerator InitializeCombat()
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

            // 메타 플로우 상태 초기화
            currentFlowState = FlowState.Prepare;

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
            // CombatSlotManager 제거됨 - 슬롯 관리 기능을 직접 구현
            yield return null;

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

        // 메타 플로우 전이 함수
        private void TransitionTo(FlowState next)
        {
            if (currentFlowState == next) return;

            // Exit 훅
            switch (currentFlowState)
            {
                case FlowState.Prepare:
                case FlowState.InCombat:
                case FlowState.Victory:
                case FlowState.Rewards:
                case FlowState.StageTransition:
                case FlowState.GameOver:
                    break;
            }

            currentFlowState = next;

            // Enter 훅
            switch (currentFlowState)
            {
                case FlowState.Prepare:
                    OnEnterPrepare();
                    break;
                case FlowState.InCombat:
                    OnEnterInCombat();
                    break;
                case FlowState.Victory:
                    OnEnterVictory();
                    break;
                case FlowState.Rewards:
                    OnEnterRewards();
                    break;
                case FlowState.StageTransition:
                    OnEnterStageTransition();
                    break;
                case FlowState.GameOver:
                    OnEnterGameOver();
                    break;
            }
        }

        private void OnEnterPrepare()
        {
            GameLogger.LogInfo("[Flow] 준비 상태 진입", GameLogger.LogCategory.Combat);
        }

        private void OnEnterInCombat()
        {
            GameLogger.LogInfo("[Flow] 전투 진행 상태 진입", GameLogger.LogCategory.Combat);
        }

        private void OnEnterVictory()
        {
            GameLogger.LogInfo("[Flow] 승리 상태 진입", GameLogger.LogCategory.Combat);
            // 승리 연출 완료 후 보상 단계로 전이
            TransitionTo(FlowState.Rewards);
        }

        private void OnEnterRewards()
        {
            GameLogger.LogInfo("[Flow] 보상 상태 진입 - 보상 UI 표시", GameLogger.LogCategory.Combat);
            // TODO: 보상 UI 표시 후 선택 완료 시 아래 호출
            // OnRewardsSelected();
        }

        private void OnEnterStageTransition()
        {
            GameLogger.LogInfo("[Flow] 스테이지 전환 상태 진입", GameLogger.LogCategory.Combat);
            if (stageManager != null)
            {
                // 다음 스테이지로 진행 후 시작
                if (stageManager.ProgressToNextStage())
                {
                    GameLogger.LogInfo("다음 스테이지 로드 완료 - 스테이지 시작", GameLogger.LogCategory.Combat);
                    stageManager.StartStage();
                }
                else
                {
                    GameLogger.LogWarning("다음 스테이지로 진행할 수 없습니다.", GameLogger.LogCategory.Combat);
                }
            }
            else
            {
                GameLogger.LogWarning("StageManager가 주입되지 않아 전환을 진행할 수 없습니다.", GameLogger.LogCategory.Combat);
            }

            // 전환 완료 후 준비 상태로 회귀
            TransitionTo(FlowState.Prepare);
        }

        private void OnEnterGameOver()
        {
            GameLogger.LogInfo("[Flow] 게임 오버 상태 진입", GameLogger.LogCategory.Combat);
            // TODO: 게임오버 UI 표시 및 재도전/로비 이동 버튼 처리
        }

        // 외부(보상 UI)에서 호출: 보상 선택 완료
        public void OnRewardsSelected()
        {
            TransitionTo(FlowState.StageTransition);
        }

        // 외부(EnemyManager 등)에서 호출: 모든 적 처치
        public void NotifyVictory()
        {
            TransitionTo(FlowState.Victory);
        }

        // 외부(PlayerManager 등)에서 호출: 플레이어 사망
        public void NotifyGameOver()
        {
            TransitionTo(FlowState.GameOver);
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

            // 메타 플로우: 전투 진행 상태로 전이
            TransitionTo(FlowState.InCombat);

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

        /// <summary>
        /// 다음 턴으로 진행
        /// </summary>
        public void ProgressTurn()
        {
            if (!isInitialized)
            {
                GameLogger.LogWarning("전투 시스템이 초기화되지 않았습니다.", GameLogger.LogCategory.Combat);
                return;
            }

            if (!isCombatActive)
            {
                GameLogger.LogWarning("전투가 활성화되어 있지 않습니다.", GameLogger.LogCategory.Combat);
                return;
            }

            // 사용되지 않는 경고 방지: 턴 기반 모드일 때만 진행
            if (!enableTurnBasedCombat)
            {
                GameLogger.LogWarning("턴 기반 전투가 비활성화되어 있어 진행하지 않습니다.", GameLogger.LogCategory.Combat);
                return;
            }

            if (turnManager == null)
            {
                GameLogger.LogError("TurnManager가 주입되지 않았습니다.", GameLogger.LogCategory.Error);
                return;
            }

            // 슬롯 큐 전진 → 턴 효과 처리 → 턴 전환
            StartCoroutine(ProgressTurnCoroutine());
        }

        private IEnumerator ProgressTurnCoroutine()
        {
            // 1) 배틀 슬롯이 비어 있으면 대기열을 전진시킵니다
            yield return turnManager.AdvanceQueueAtTurnStartRoutine();

            // 2) 모든 캐릭터의 턴 효과 처리
            turnManager.ProcessAllCharacterTurnEffects();

            // 3) 턴 전환: 플레이어 ↔ 적
            var next = turnManager.IsPlayerTurn()
                ? TurnManager.TurnType.Enemy
                : TurnManager.TurnType.Player;
            turnManager.SetTurnAndIncrement(next);
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
            // 턴 매니저 정리
            if (turnManager != null)
            {
                turnManager.ResetTurn();
            }

            yield return null;
        }

        /// <summary>
        /// 적 처치 시 호출되는 메서드
        /// GameStartupController에게 적 사망 알림
        /// </summary>
        public void OnEnemyDeath(ICharacter enemy)
        {
            if (enemy == null) return;

            GameLogger.LogInfo($"적 처치: {enemy.GetCharacterName()}", GameLogger.LogCategory.Combat);

            // 적 처치 이벤트 발생
            OnEnemyDefeated?.Invoke(enemy);

            // 메타 플로우: 승리 진입
            NotifyVictory();

            // 전투 종료
            EndCombat();
        }

        /// <summary>
        /// 플레이어 사망 통지 → 게임오버 처리
        /// </summary>
        public void OnPlayerDeath(ICharacter player)
        {
            if (player == null) return;
            GameLogger.LogInfo($"플레이어 사망: {player.GetCharacterName()}", GameLogger.LogCategory.Combat);
            NotifyGameOver();
            EndCombat();
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

            // CombatSlotManager 제거됨 - 슬롯 관리 기능을 직접 구현

            if (turnManager == null)
            {
                GameLogger.LogError("TurnManager가 주입되지 않았습니다.", GameLogger.LogCategory.Error);
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
        
        #region 세이브/로드 시스템 지원
        
        /// <summary>
        /// 전투 상태를 복원합니다. (저장 시스템용)
        /// </summary>
        /// <param name="flowState">복원할 플로우 상태</param>
        public void RestoreCombatState(string flowState)
        {
            if (string.IsNullOrEmpty(flowState))
            {
                GameLogger.LogWarning("복원할 전투 상태가 비어있습니다", GameLogger.LogCategory.Combat);
                return;
            }
            
            try
            {
                // 문자열을 FlowState로 변환
                if (System.Enum.TryParse<FlowState>(flowState, out var parsedState))
                {
                    currentFlowState = parsedState;
                    GameLogger.LogInfo($"전투 상태 복원: {flowState}", GameLogger.LogCategory.Combat);
                }
                else
                {
                    GameLogger.LogWarning($"알 수 없는 전투 상태: {flowState}", GameLogger.LogCategory.Combat);
                }
            }
            catch (System.Exception ex)
            {
                GameLogger.LogError($"전투 상태 복원 실패: {ex.Message}", GameLogger.LogCategory.Error);
            }
        }
        
        /// <summary>
        /// 현재 전투 상태를 가져옵니다. (저장 시스템용)
        /// </summary>
        /// <returns>현재 전투 상태 문자열</returns>
        public string GetCurrentCombatState()
        {
            return currentFlowState.ToString();
        }
        
        /// <summary>
        /// 전투 활성 상태를 설정합니다. (저장 시스템용)
        /// </summary>
        /// <param name="active">전투 활성 여부</param>
        public void SetCombatActive(bool active)
        {
            isCombatActive = active;
            GameLogger.LogInfo($"전투 활성 상태 설정: {active}", GameLogger.LogCategory.Combat);
        }
        
        #endregion
        
        #region 게임 오버 관리 (GameOverManager 통합)
        
        [Header("게임 오버 설정")]
        [SerializeField] private GameObject gameOverUI;
        
        /// <summary>
        /// 게임 오버 UI를 화면에 표시합니다.
        /// </summary>
        public void ShowGameOverUI()
        {
            GameLogger.LogInfo("게임 오버 UI 호출됨", GameLogger.LogCategory.Combat);
            if (gameOverUI != null)
                gameOverUI.SetActive(true);
        }

        /// <summary>
        /// 게임 오버 처리를 시작합니다.
        /// </summary>
        public void TriggerGameOver()
        {
            GameLogger.LogInfo("게임 오버 처리 시작", GameLogger.LogCategory.Combat);
            ShowGameOverUI();
            EndCombat();
        }
        
        #endregion
    }
}
