using UnityEngine;
using System;
using Game.CoreSystem.Utility;
using Game.CharacterSystem.Manager;
using Game.CombatSystem.Manager;
using Game.CombatSystem.Interface;
using Game.SkillCardSystem.Manager;
using Zenject;

namespace Game.CombatSystem.State
{
    /// <summary>
    /// 전투 상태 머신
    /// - 전투의 모든 상태(PlayerTurn, EnemyTurn, CardExecution 등)를 관리합니다
    /// - 상태 전환 로직을 중앙에서 제어합니다
    /// - 각 상태별 허용/차단 액션을 명확히 정의합니다
    /// </summary>
    public class CombatStateMachine : MonoBehaviour
    {
        #region 의존성 주입

        [Inject] private CombatExecutionManager executionManager;
        [Inject] private TurnManager turnManager;
        [Inject] private PlayerManager playerManager;
        [Inject] private EnemyManager enemyManager;

        // 새로운 분리된 인터페이스들 (리팩토링)
        [Inject] private ITurnController turnController;
        [Inject] private ICardSlotRegistry slotRegistry;
        [Inject] private ISlotMovementController slotMovement;

        // PlayerHandManager는 Optional (씬에 없을 수 있음)
        private PlayerHandManager handManager;

        #endregion

        #region 상태 관리

        private ICombatState _currentState;
        private CombatStateContext _context;

        [Header("디버그 설정")]
        [SerializeField] private bool enableDebugLogging = true;
        [SerializeField] private string currentStateName = "None";

        #endregion

        #region 이벤트

        /// <summary>
        /// 상태가 변경될 때 발생하는 이벤트
        /// </summary>
        public event Action<ICombatState, ICombatState> OnStateChanged;

        /// <summary>
        /// 전투가 시작될 때 발생하는 이벤트
        /// </summary>
        public event Action OnCombatStarted;

        /// <summary>
        /// 전투가 종료될 때 발생하는 이벤트
        /// </summary>
        public event Action<bool> OnCombatEnded; // bool: isVictory

        #endregion

        #region Unity 생명주기

        private void Awake()
        {
            // 컨텍스트 생성
            _context = new CombatStateContext();

            if (enableDebugLogging)
            {
                GameLogger.LogInfo("[CombatStateMachine] 생성 완료", GameLogger.LogCategory.Combat);
            }
        }

        private void Start()
        {
            // PlayerHandManager 찾기 (Optional)
            handManager = FindFirstObjectByType<PlayerHandManager>();

            // 지연 초기화 (다른 매니저들이 준비될 때까지 대기)
            Invoke(nameof(DelayedInitialize), 0.1f);
        }

        private void Update()
        {
            // 현재 상태 업데이트
            _currentState?.OnUpdate(_context);

            // 캐릭터 사망 확인 (전투 중에만)
            // 소환 확인은 제거 - 특정 상태에서만 체크하도록 변경
            if (_currentState != null && _context?.IsInitialized == true)
            {
                CheckCharacterDeath();
            }
        }

        /// <summary>
        /// 캐릭터 사망 확인 및 처리
        /// </summary>
        private void CheckCharacterDeath()
        {
            // BattleEndState에서는 체크하지 않음
            if (_currentState is BattleEndState)
                return;

            // 적 사망 확인
            if (enemyManager != null)
            {
                var enemy = enemyManager.GetCharacter();
                if (enemy != null && enemy.IsDead())
                {
                    // EnemyDefeatedState가 아닐 때만 전환
                    if (!(_currentState is EnemyDefeatedState))
                    {
                        OnEnemyDeath();
                    }
                }
            }

            // 플레이어 사망 확인
            if (playerManager != null)
            {
                var player = playerManager.GetCharacter();
                if (player != null && player.IsDead())
                {
                    OnPlayerDeath();
                }
            }
        }

        /// <summary>
        /// 소환 트리거 확인 (턴 전환 시점에만 호출)
        /// 매 프레임 체크하지 않고 안전한 시점에만 체크
        /// </summary>
        public void CheckSummonTriggerAtSafePoint()
        {
            // BattleEndState나 EnemyDefeatedState에서는 체크하지 않음
            if (_currentState is BattleEndState || _currentState is EnemyDefeatedState || _currentState is SummonState || _currentState is SummonReturnState)
            {
                GameLogger.LogInfo(
                    $"[CombatStateMachine] 소환 체크 건너뜀 - 현재 상태: {_currentState.StateName}",
                    GameLogger.LogCategory.Combat);
                return;
            }

            // 적이 있고 살아있을 때만 체크
            if (enemyManager != null)
            {
                var enemy = enemyManager.GetCharacter();
                if (enemy != null && !enemy.IsDead())
                {
                    // StageManager에서 소환 플래그 확인
                    var stageManager = FindFirstObjectByType<Game.StageSystem.Manager.StageManager>();
                    if (stageManager != null && stageManager.IsSummonedEnemyActive())
                    {
                        GameLogger.LogInfo(
                            "[CombatStateMachine] 소환 트리거 감지 - SummonState로 전환",
                            GameLogger.LogCategory.Combat);

                        // SummonState로 전환
                        var summonData = stageManager.GetSummonTarget();
                        var originalHP = stageManager.GetOriginalEnemyHP();

                        if (summonData != null)
                        {
                            var summonState = new SummonState(summonData, originalHP);
                            ChangeState(summonState);
                        }
                        else
                        {
                            GameLogger.LogError(
                                "[CombatStateMachine] 소환 대상 데이터가 없습니다",
                                GameLogger.LogCategory.Error);
                        }
                    }
                }
            }
        }

        private void OnDestroy()
        {
            // 현재 상태 정리
            if (_currentState != null && _context != null)
            {
                _currentState.OnExit(_context);
            }
        }

        #endregion

        #region 초기화

        /// <summary>
        /// 지연 초기화 (의존성 주입 완료 후)
        /// </summary>
        private void DelayedInitialize()
        {
            // 컨텍스트 초기화 (레거시 + 리팩토링된 인터페이스)
            _context.Initialize(
                this,
                executionManager,
                turnManager,
                playerManager,
                enemyManager,
                handManager,
                turnController,
                slotRegistry,
                slotMovement);

            // 매니저 검증
            if (!_context.ValidateManagers())
            {
                GameLogger.LogError(
                    "[CombatStateMachine] 필수 매니저 검증 실패",
                    GameLogger.LogCategory.Error);
                return;
            }

            if (enableDebugLogging)
            {
                GameLogger.LogInfo(
                    "[CombatStateMachine] 초기화 완료 (레거시 + 리팩토링)",
                    GameLogger.LogCategory.Combat);
            }

            GameLogger.LogInfo(
                "[CombatStateMachine] 초기화 완료 - Update()에서 캐릭터 사망 체크",
                GameLogger.LogCategory.Combat);
        }

        #endregion

        #region 상태 전환

        /// <summary>
        /// 전투를 시작합니다
        /// </summary>
        public void StartCombat()
        {
            StartCombat(null, null);
        }

        /// <summary>
        /// 전투를 시작합니다 (적 데이터 포함)
        /// </summary>
        public void StartCombat(Game.CharacterSystem.Data.EnemyCharacterData enemyData, string enemyName)
        {
            if (_currentState != null)
            {
                GameLogger.LogWarning(
                    "[CombatStateMachine] 이미 전투가 진행 중입니다",
                    GameLogger.LogCategory.Combat);
                return;
            }

            GameLogger.LogInfo(
                $"[CombatStateMachine] 전투 시작{(enemyData != null ? $" - 적: {enemyName}" : "")}",
                GameLogger.LogCategory.Combat);

            OnCombatStarted?.Invoke();

            // 초기화 상태로 시작 (적 데이터 전달)
            var initState = new CombatInitState();
            if (enemyData != null)
            {
                initState.SetEnemyData(enemyData, enemyName);
            }
            ChangeState(initState);
        }

        /// <summary>
        /// 안전한 상태 변경 (완료 검증 포함)
        /// 모든 비동기 작업과 검증이 완료된 후에만 상태 전환
        /// </summary>
        /// <param name="newState">새로운 상태</param>
        public void ChangeStateSafe(ICombatState newState)
        {
            if (newState == null)
            {
                GameLogger.LogError(
                    "[CombatStateMachine] null 상태로 전환할 수 없습니다",
                    GameLogger.LogCategory.Error);
                return;
            }

            // 현재 상태가 있으면 완료 검증 및 대기
            if (_currentState != null)
            {
                // 1단계: 전환 가능 여부 검증
                if (!_currentState.CanTransitionToNextState(_context))
                {
                    GameLogger.LogWarning(
                        $"[CombatStateMachine] {_currentState.StateName}에서 전환 불가능",
                        GameLogger.LogCategory.Combat);
                    return;
                }

                // 2단계: 완료 대기 코루틴 시작
                StartCoroutine(WaitAndChangeState(newState));
            }
            else
            {
                // 현재 상태가 없으면 즉시 전환
                ChangeStateImmediate(newState);
            }
        }

        /// <summary>
        /// 완료 대기 후 상태 변경 코루틴
        /// </summary>
        private System.Collections.IEnumerator WaitAndChangeState(ICombatState newState)
        {
            GameLogger.LogInfo(
                $"[CombatStateMachine] {_currentState.StateName} 완료 대기 시작",
                GameLogger.LogCategory.Combat);

            // 현재 상태의 완료 대기
            yield return _currentState.WaitForCompletion(_context);

            GameLogger.LogInfo(
                $"[CombatStateMachine] {_currentState.StateName} 완료 확인됨 - 상태 전환 시작",
                GameLogger.LogCategory.Combat);

            // 상태 전환 실행
            ChangeStateImmediate(newState);
        }

        /// <summary>
        /// 즉시 상태 변경 (내부용)
        /// </summary>
        private void ChangeStateImmediate(ICombatState newState)
        {
            var previousState = _currentState;

            // 이전 상태 종료
            if (_currentState != null)
            {
                _currentState.OnExit(_context);
            }

            // 새 상태로 전환
            _currentState = newState;
            currentStateName = _currentState.StateName;

            // 새 상태 시작
            _currentState.OnEnter(_context);

            // 이벤트 발생
            OnStateChanged?.Invoke(previousState, _currentState);

            if (enableDebugLogging)
            {
                var prevName = previousState?.StateName ?? "None";
                GameLogger.LogInfo(
                    $"[CombatStateMachine] 상태 전환: {prevName} → {_currentState.StateName}",
                    GameLogger.LogCategory.Combat);
            }
        }

        /// <summary>
        /// 상태를 변경합니다 (기존 메서드 - 호환성 유지)
        /// </summary>
        public void ChangeState(ICombatState newState)
        {
            if (newState == null)
            {
                GameLogger.LogError(
                    "[CombatStateMachine] null 상태로 전환할 수 없습니다",
                    GameLogger.LogCategory.Error);
                return;
            }

            var previousState = _currentState;

            // 이전 상태 종료
            if (_currentState != null)
            {
                _currentState.OnExit(_context);
            }

            // 새 상태로 전환
            _currentState = newState;
            currentStateName = _currentState.StateName;

            // 새 상태 시작
            _currentState.OnEnter(_context);

            // 이벤트 발생
            OnStateChanged?.Invoke(previousState, _currentState);

            if (enableDebugLogging)
            {
                var prevName = previousState?.StateName ?? "None";
                GameLogger.LogInfo(
                    $"[CombatStateMachine] 상태 전환: {prevName} → {_currentState.StateName}",
                    GameLogger.LogCategory.Combat);
            }
        }

        /// <summary>
        /// 상태를 강제로 변경합니다 (소환/복귀 시 사용)
        /// </summary>
        /// <param name="newState">새로운 상태</param>
        public void ForceChangeState(ICombatState newState)
        {
            if (newState == null)
            {
                GameLogger.LogError(
                    "[CombatStateMachine] null 상태로 전환할 수 없습니다",
                    GameLogger.LogCategory.Error);
                return;
            }

            GameLogger.LogInfo(
                $"[CombatStateMachine] 강제 상태 전환: {_currentState?.StateName ?? "None"} → {newState.StateName}",
                GameLogger.LogCategory.Combat);

            var previousState = _currentState;

            // 이전 상태 종료
            if (_currentState != null)
            {
                _currentState.OnExit(_context);
            }

            // 새 상태로 전환
            _currentState = newState;
            currentStateName = _currentState.StateName;

            // 새 상태 시작
            _currentState.OnEnter(_context);

            // 이벤트 발생
            OnStateChanged?.Invoke(previousState, _currentState);
        }

        /// <summary>
        /// 전투를 종료합니다
        /// </summary>
        public void EndCombat(bool isVictory)
        {
            GameLogger.LogInfo(
                $"[CombatStateMachine] 전투 종료 - {(isVictory ? "승리" : "패배")}",
                GameLogger.LogCategory.Combat);

            var endState = new BattleEndState(isVictory);
            ChangeState(endState);

            OnCombatEnded?.Invoke(isVictory);
        }

        #endregion

        #region 상태 쿼리

        /// <summary>
        /// 현재 상태를 반환합니다
        /// </summary>
        public ICombatState GetCurrentState()
        {
            return _currentState;
        }

        /// <summary>
        /// 플레이어 카드 드래그가 허용되는지 확인
        /// </summary>
        public bool CanPlayerDragCard()
        {
            return _currentState?.AllowPlayerCardDrag ?? false;
        }

        /// <summary>
        /// 적 카드 자동 실행이 허용되는지 확인
        /// </summary>
        public bool CanEnemyAutoExecute()
        {
            return _currentState?.AllowEnemyAutoExecution ?? false;
        }

        /// <summary>
        /// 슬롯 이동이 허용되는지 확인
        /// </summary>
        public bool CanMoveSlots()
        {
            return _currentState?.AllowSlotMovement ?? false;
        }

        /// <summary>
        /// 턴 전환이 허용되는지 확인
        /// </summary>
        public bool CanSwitchTurn()
        {
            return _currentState?.AllowTurnSwitch ?? false;
        }

        #endregion

        #region 외부 이벤트 핸들러

        /// <summary>
        /// 플레이어가 카드를 배치했을 때 호출
        /// </summary>
        public void OnPlayerCardPlaced(
            Game.SkillCardSystem.Interface.ISkillCard card,
            Game.CombatSystem.Slot.CombatSlotPosition slot)
        {
            if (!CanPlayerDragCard())
            {
                GameLogger.LogWarning(
                    "[CombatStateMachine] 현재 상태에서 카드 배치가 허용되지 않습니다",
                    GameLogger.LogCategory.Combat);
                return;
            }

            // PlayerTurnState에 알림
            if (_currentState is PlayerTurnState playerTurn)
            {
                playerTurn.OnCardPlaced(_context, card, slot);
            }
        }

        /// <summary>
        /// 적 카드가 배틀 슬롯에 도달했을 때 호출
        /// 슬롯 이동 중이 아니면 즉시 실행하지 않음 (SlotMovingState에서 처리)
        /// </summary>
        public void OnEnemyCardReady(Game.SkillCardSystem.Interface.ISkillCard card)
        {
            // 슬롯 이동 중이면 아무것도 하지 않음
            // (SlotMovingState가 슬롯 이동 완료 후 자동으로 적 카드를 확인하고 실행함)
            if (_currentState is SlotMovingState)
            {
                GameLogger.LogInfo(
                    "[CombatStateMachine] 슬롯 이동 중 - 적 카드는 이동 완료 후 자동 실행됨",
                    GameLogger.LogCategory.Combat);
                return;
            }

            // 다른 상태에서는 경고만 출력 (정상적으로는 SlotMovingState에서만 처리)
            GameLogger.LogWarning(
                $"[CombatStateMachine] 적 카드 도달 알림 - 현재 상태: {_currentState?.StateName ?? "None"}",
                GameLogger.LogCategory.Combat);
        }

        /// <summary>
        /// 적이 사망했을 때 호출
        /// </summary>
        private void OnEnemyDeath()
        {
            GameLogger.LogInfo(
                "[CombatStateMachine] 적 사망 감지",
                GameLogger.LogCategory.Combat);

            // 소환된 적인지 확인
            var stageManager = FindFirstObjectByType<Game.StageSystem.Manager.StageManager>();
            if (stageManager != null && stageManager.IsSummonedEnemyActive())
            {
                GameLogger.LogInfo(
                    "[CombatStateMachine] 소환된 적 사망 - EnemyDefeatedState로 전환하지 않음 (원본 적 복귀 처리)",
                    GameLogger.LogCategory.Combat);
                return; // 소환된 적이 죽으면 StageManager에서 원본 적 복귀 처리
            }

            // 일반 적 사망 시 적 처치 상태로 전환
            var enemyDefeatedState = new EnemyDefeatedState();
            ChangeState(enemyDefeatedState);
        }

        /// <summary>
        /// StageManager에서 적 사망을 감지했을 때 호출되는 공개 메서드
        /// </summary>
        public void OnEnemyDeathDetected()
        {
            GameLogger.LogInfo(
                "[CombatStateMachine] StageManager로부터 적 사망 알림 수신",
                GameLogger.LogCategory.Combat);

            // EnemyDefeatedState가 아닐 때만 전환
            if (!(_currentState is EnemyDefeatedState))
            {
                OnEnemyDeath();
            }
            else
            {
                GameLogger.LogInfo(
                    "[CombatStateMachine] 이미 EnemyDefeatedState에 있음 - 전환 건너뜀",
                    GameLogger.LogCategory.Combat);
            }
        }

        /// <summary>
        /// StageManager에서 다음 적 생성 완료를 알릴 때 호출되는 공개 메서드
        /// </summary>
        public void OnNextEnemySpawned()
        {
            GameLogger.LogInfo(
                "[CombatStateMachine] StageManager로부터 다음 적 생성 완료 알림 수신",
                GameLogger.LogCategory.Combat);

            // EnemyDefeatedState에서만 처리
            if (_currentState is EnemyDefeatedState enemyDefeatedState)
            {
                GameLogger.LogInfo(
                    "[CombatStateMachine] EnemyDefeatedState에서 다음 적 생성 완료 처리",
                    GameLogger.LogCategory.Combat);
                
                // EnemyDefeatedState의 다음 적 확인 로직을 트리거
                enemyDefeatedState.OnNextEnemyReady();
            }
            else
            {
                GameLogger.LogWarning(
                    $"[CombatStateMachine] 다음 적 생성 완료 알림 - 현재 상태가 EnemyDefeatedState가 아님: {_currentState?.StateName ?? "None"}",
                    GameLogger.LogCategory.Combat);
            }
        }

        /// <summary>
        /// 플레이어가 사망했을 때 호출
        /// </summary>
        private void OnPlayerDeath()
        {
            GameLogger.LogInfo(
                "[CombatStateMachine] 플레이어 사망 감지",
                GameLogger.LogCategory.Combat);

            // 패배 상태로 전환
            EndCombat(false);
        }

        #endregion

        #region 디버그

        /// <summary>
        /// 현재 상태 정보 출력
        /// </summary>
        [ContextMenu("현재 상태 정보 출력")]
        public void LogCurrentState()
        {
            if (_currentState != null)
            {
                GameLogger.LogInfo(
                    $"[CombatStateMachine] 현재 상태: {_currentState.StateName}\n" +
                    $"- 플레이어 드래그 허용: {CanPlayerDragCard()}\n" +
                    $"- 적 자동 실행 허용: {CanEnemyAutoExecute()}\n" +
                    $"- 슬롯 이동 허용: {CanMoveSlots()}\n" +
                    $"- 턴 전환 허용: {CanSwitchTurn()}",
                    GameLogger.LogCategory.Combat);
            }
            else
            {
                GameLogger.LogInfo(
                    "[CombatStateMachine] 현재 상태 없음 (전투 시작 전)",
                    GameLogger.LogCategory.Combat);
            }
        }

        #endregion
    }
}
