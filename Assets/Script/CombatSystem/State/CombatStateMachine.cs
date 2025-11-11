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

        // 부활 관련 플래그
        private bool hasUsedReviveThisDeath = false;
        private bool isWaitingForDeathEffect = false;
        private bool isProcessingEnemyDeath = false;

		[Header("부활 트리거 설정")]
		[Tooltip("치명타격(사망을 유발한 공격) 이펙트가 끝난 뒤 부활을 시도하기 위해 대기할 시간(초)")]
		[SerializeField] private float reviveAfterFatalHitDelay = 0.6f;

		[Header("처치 트리거 설정")]
		[Tooltip("적 처치(사망을 유발한 공격) 이펙트가 끝난 뒤 처치 처리를 진행하기 위해 대기할 시간(초)")]
		[SerializeField] private float enemyAfterFatalHitDelay = 0.5f;
        
        // 게임 오버 플래그 (완전히 사망했을 때 설정 - 이후 모든 액션 차단)
        private bool isGameOver = false;

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

            // 사망 이펙트 완료 이벤트 구독
            CombatEvents.OnPlayerDeathEffectComplete += OnPlayerDeathEffectComplete;

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
            // 게임 오버 상태이면 사망 체크를 하지 않음
            if (isGameOver)
                return;

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
            // 이벤트 구독 해제
            CombatEvents.OnPlayerDeathEffectComplete -= OnPlayerDeathEffectComplete;

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

            // 부활 플래그 리셋 (새 전투 시작)
            hasUsedReviveThisDeath = false;
            isGameOver = false;

            GameLogger.LogInfo(
                $"[CombatStateMachine] 전투 시작{(enemyData != null ? $" - 적: {enemyName}" : "")}",
                GameLogger.LogCategory.Combat);

            // 인스턴스 이벤트 발생
            OnCombatStarted?.Invoke();

            // 정적 이벤트 발생 (통계 시스템 등 전역 구독자용)
            CombatEvents.RaiseCombatStarted();

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

			// 게임 오버 시에는 BattleEndState로의 전환만 허용
			if (isGameOver && newState is not BattleEndState)
			{
				GameLogger.LogWarning("[CombatStateMachine] 게임 오버 상태입니다 - 상태 전환 차단", GameLogger.LogCategory.Combat);
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
			// 게임 오버 시에는 BattleEndState로의 전환만 허용
			if (isGameOver && newState is not BattleEndState)
			{
				GameLogger.LogWarning("[CombatStateMachine] 게임 오버 상태입니다 - 상태 전환 차단", GameLogger.LogCategory.Combat);
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

            // EnemyDefeatedState로 전환되었으면 적 사망 처리 플래그는 유지,
            // 그 외 상태로 전환될 때에는 리셋하여 다음 라운드에서 정상 동작하도록 함
            if (_currentState is not EnemyDefeatedState)
            {
                isProcessingEnemyDeath = false;
            }

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
			// 게임 오버 시에는 BattleEndState로의 전환만 허용
			if (isGameOver && newState is not BattleEndState)
			{
				GameLogger.LogWarning("[CombatStateMachine] 게임 오버 상태입니다 - 상태 전환 차단", GameLogger.LogCategory.Combat);
				return;
			}

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
			// 게임 오버 시에는 BattleEndState로의 전환만 허용
			if (isGameOver && newState is not BattleEndState)
			{
				GameLogger.LogWarning("[CombatStateMachine] 게임 오버 상태입니다 - 상태 전환 차단", GameLogger.LogCategory.Combat);
				return;
			}

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
        /// 게임 오버 플래그를 설정하고 전투를 종료합니다 (플레이어 완전 사망 시 사용)
        /// </summary>
        private void SetGameOverAndEndCombat(bool isVictory)
        {
            // 게임 오버 플래그 설정 (이후 모든 사망 체크 차단)
            isGameOver = true;
            
            GameLogger.LogInfo(
                $"[CombatStateMachine] 게임 오버 설정 및 전투 종료 - {(isVictory ? "승리" : "패배")}",
                GameLogger.LogCategory.Combat);

            // 즉시 BattleEndState로 전환하여 모든 액션 차단
            var endState = new BattleEndState(isVictory);
            ChangeStateImmediate(endState);

            OnCombatEnded?.Invoke(isVictory);
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
            // 중복 처리 방지: 사망 처리 진행 중이면 무시
            if (isProcessingEnemyDeath)
            {
                return;
            }

            isProcessingEnemyDeath = true;
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

			// 일반 적 사망: 치명타격 이펙트가 끝날 시간을 잠시 대기한 뒤 처치 상태로 전환
			if (this != null)
			{
				StartCoroutine(WaitForEnemyFatalHitEffectThenProceed());
			}
			else
			{
				// 폴백: 코루틴 불가 시 즉시 전환
				var enemyDefeatedState = new EnemyDefeatedState();
				ChangeState(enemyDefeatedState);
			}
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
            // 게임 오버 상태이면 처리하지 않음
            if (isGameOver)
            {
                return;
            }

            GameLogger.LogInfo(
                "[CombatStateMachine] 플레이어 사망 감지",
                GameLogger.LogCategory.Combat);

            // 이미 이번 사망에서 부활을 사용했다면 더 이상 부활하지 않음
            if (hasUsedReviveThisDeath)
            {
                GameLogger.LogInfo("[CombatStateMachine] 이미 부활을 사용했으므로 게임 종료", GameLogger.LogCategory.Combat);
                SetGameOverAndEndCombat(false);
                return;
            }

            // 부활 아이템이 있는지 확인
            bool hasReviveItem = CheckReviveItemExists();
            if (!hasReviveItem)
            {
                GameLogger.LogInfo("[CombatStateMachine] 부활 아이템이 없어 게임 종료", GameLogger.LogCategory.Combat);
                SetGameOverAndEndCombat(false);
                return;
            }

            // 사망 이펙트 완료를 기다리는 플래그 설정
            isWaitingForDeathEffect = true;
            GameLogger.LogInfo("[CombatStateMachine] 사망 이펙트 완료 대기 중...", GameLogger.LogCategory.Combat);

			// 공격(히트) 이펙트가 끝난 뒤 부활하도록 약간 대기 후 완료 이벤트를 발생시킵니다
			// 외부에서 별도 완료 신호가 없으므로 설정값 기반의 안전한 지연을 둡니다
			StartCoroutine(WaitForFatalHitEffectThenComplete());
        }

        /// <summary>
        /// 사망 이펙트 완료를 기다리거나 타임아웃 시 완료 처리
        /// </summary>
        private System.Collections.IEnumerator WaitForDeathEffectOrTimeout()
        {
            // 사망 이펙트가 없는 경우를 대비해 짧은 지연 후 즉시 완료 처리
            // OnPlayerCharacterDeath 이벤트를 구독하는 곳에서 사망 이펙트를 재생하고 완료 시 RaisePlayerDeathEffectComplete()를 호출해야 함
            yield return new WaitForSeconds(0.1f); // 사망 이펙트 재생 시작을 위한 짧은 지연

            // 게임 오버 상태이면 처리하지 않음
            if (isGameOver)
            {
                yield break;
            }

            // 여전히 대기 중이면 사망 이펙트가 없는 것으로 간주하고 즉시 완료 처리
            if (isWaitingForDeathEffect)
            {
                GameLogger.LogWarning("[CombatStateMachine] 사망 이펙트가 없거나 완료되지 않음 - 즉시 부활 처리 진행", GameLogger.LogCategory.Combat);
                CombatEvents.RaisePlayerDeathEffectComplete();
            }
        }

		/// <summary>
		/// 치명타격(사망을 유발한 공격)의 히트 이펙트가 끝날 시간을 대기한 뒤
		/// 사망 이펙트 완료로 간주하고 부활 처리를 진행합니다.
		/// </summary>
		private System.Collections.IEnumerator WaitForFatalHitEffectThenComplete()
		{
			// 플레이어 캐릭터의 현재 재생 중인 파티클/애니메이션 기반으로 남은 시간을 추정
			float dynamicRemaining = 0f;
			var player = playerManager != null ? playerManager.GetCharacter() as MonoBehaviour : null;
			if (player != null)
			{
				dynamicRemaining = ComputeApproxEffectRemainingTime(player.transform, reviveAfterFatalHitDelay);
			}

			// 설정값과 동적으로 계산한 값 중 더 큰 값을 사용
			float delay = Mathf.Max(0f, Mathf.Max(reviveAfterFatalHitDelay, dynamicRemaining));
			if (delay > 0f)
			{
				yield return new WaitForSeconds(delay);
			}

			// 게임 오버 상태이면 처리하지 않음
			if (isGameOver)
			{
				yield break;
			}

			// 아직 사망 이펙트 완료를 기다리고 있으면 완료 이벤트 발생
			if (isWaitingForDeathEffect)
			{
				GameLogger.LogInfo("[CombatStateMachine] 치명타격 이펙트 대기 후 부활 처리 진행", GameLogger.LogCategory.Combat);
				CombatEvents.RaisePlayerDeathEffectComplete();
			}
		}

		/// <summary>
		/// 적 처치(사망 유발) 히트 이펙트 종료 시간을 대기한 뒤 적 처치 상태로 전환합니다.
		/// </summary>
		private System.Collections.IEnumerator WaitForEnemyFatalHitEffectThenProceed()
		{
			// 현재 적 캐릭터 기준으로 남은 이펙트 시간을 추정
			float dynamicRemaining = 0f;
			var enemy = enemyManager != null ? enemyManager.GetCharacter() as MonoBehaviour : null;
			if (enemy != null)
			{
				dynamicRemaining = ComputeApproxEffectRemainingTime(enemy.transform, enemyAfterFatalHitDelay);
			}

			float delay = Mathf.Max(0f, Mathf.Max(enemyAfterFatalHitDelay, dynamicRemaining));
			if (delay > 0f)
			{
				yield return new WaitForSeconds(delay);
			}

			// 게임 오버 상태이면 더 이상 진행하지 않음
			if (isGameOver)
			{
				yield break;
			}

			var enemyDefeatedState = new EnemyDefeatedState();
			ChangeState(enemyDefeatedState);
		}

		/// <summary>
		/// 대상 캐릭터 하위에서 재생 중인 파티클/애니메이션을 탐색하여
		/// 대략적인 남은 이펙트 시간을 계산합니다. 최소 보장값으로 fallback을 사용합니다.
		/// </summary>
		private float ComputeApproxEffectRemainingTime(Transform characterRoot, float fallbackSeconds)
		{
			if (characterRoot == null) return Mathf.Max(0f, fallbackSeconds);

			float maxSeconds = 0f;

			// 파티클 기반 추정
			var particleSystems = characterRoot.GetComponentsInChildren<ParticleSystem>(true);
			for (int i = 0; i < particleSystems.Length; i++)
			{
				var ps = particleSystems[i];
				if (ps == null) continue;
				var main = ps.main;
				// 대략: duration + 최대 수명
				float approx = main.duration + main.startLifetime.constantMax;
				if (main.loop)
				{
					approx = Mathf.Min(approx, 3f);
				}
				if (approx > maxSeconds) maxSeconds = approx;
			}

			// 애니메이션 기반 추정
			var animator = characterRoot.GetComponentInChildren<Animator>(true);
			if (animator != null && animator.runtimeAnimatorController != null)
			{
				var clips = animator.runtimeAnimatorController.animationClips;
				for (int i = 0; i < clips.Length; i++)
				{
					var clip = clips[i];
					if (clip != null && clip.length > maxSeconds)
					{
						maxSeconds = clip.length;
					}
				}
			}

			// 최소 보장값과 상한 적용
			float result = Mathf.Max(fallbackSeconds, maxSeconds);
			// 안전 상한 (지나치게 긴 루프/클립으로 인한 과도 대기 방지)
			return Mathf.Clamp(result, 0f, 2.0f);
		}

        /// <summary>
        /// 플레이어 사망 이펙트 완료 시 호출
        /// </summary>
        private void OnPlayerDeathEffectComplete()
        {
            // 게임 오버 상태이면 처리하지 않음
            if (isGameOver)
            {
                return;
            }

            // 사망 이펙트 완료를 기다리고 있지 않으면 무시
            if (!isWaitingForDeathEffect)
            {
                return;
            }

            isWaitingForDeathEffect = false;
            GameLogger.LogInfo("[CombatStateMachine] 사망 이펙트 완료 - 부활 아이템 효과 발동", GameLogger.LogCategory.Combat);

            // 부활 아이템 효과 발동
            if (TryAutoRevive())
            {
                hasUsedReviveThisDeath = true; // 부활 사용 플래그 설정 (이번 사망에서만 사용)
                GameLogger.LogInfo("[CombatStateMachine] 부활 아이템 사용으로 부활 성공", GameLogger.LogCategory.Combat);
                
                // 플레이어가 살아나는 것을 대기한 후 플래그 리셋
                StartCoroutine(ResetReviveFlagAfterRevive());
            }
            else
            {
                // 부활 아이템 사용 실패 시 패배 상태로 전환
                GameLogger.LogWarning("[CombatStateMachine] 부활 아이템 사용 실패 - 게임 종료", GameLogger.LogCategory.Combat);
                SetGameOverAndEndCombat(false);
            }
        }

        /// <summary>
        /// 부활 아이템 존재 여부를 확인합니다 (아이템 사용 전 체크)
        /// </summary>
        /// <returns>부활 아이템 존재 여부</returns>
        private bool CheckReviveItemExists()
        {
            try
            {
                var itemService = FindFirstObjectByType<Game.ItemSystem.Service.ItemService>();
                if (itemService == null)
                {
                    return false;
                }

                var reviveItemSlot = FindReviveItemSlot(itemService);
                return reviveItemSlot != -1;
            }
            catch (System.Exception ex)
            {
                GameLogger.LogError($"[CombatStateMachine] 부활 아이템 확인 중 오류: {ex.Message}", GameLogger.LogCategory.Error);
                return false;
            }
        }

        /// <summary>
        /// 자동 부활을 시도합니다.
        /// </summary>
        /// <returns>부활 성공 여부</returns>
        private bool TryAutoRevive()
        {
            // 게임 오버 상태이면 부활 시도하지 않음
            if (isGameOver)
            {
                GameLogger.LogWarning("[CombatStateMachine] 게임 오버 상태이므로 부활 시도하지 않음", GameLogger.LogCategory.Combat);
                return false;
            }

            try
            {
                // ItemService 찾기
                var itemService = FindFirstObjectByType<Game.ItemSystem.Service.ItemService>();
                if (itemService == null)
                {
                    GameLogger.LogWarning("[CombatStateMachine] ItemService를 찾을 수 없습니다", GameLogger.LogCategory.Combat);
                    return false;
                }

                // 플레이어 캐릭터 가져오기
                var playerCharacter = playerManager?.GetCharacter();
                if (playerCharacter == null)
                {
                    GameLogger.LogWarning("[CombatStateMachine] 플레이어 캐릭터를 찾을 수 없습니다", GameLogger.LogCategory.Combat);
                    return false;
                }

                // 부활 아이템 찾기 (모든 슬롯에서 첫 번째 부활 아이템만 사용)
                var reviveItemSlot = FindReviveItemSlot(itemService);
                if (reviveItemSlot == -1)
                {
                    GameLogger.LogInfo("[CombatStateMachine] 부활 아이템이 없습니다", GameLogger.LogCategory.Combat);
                    return false;
                }

                GameLogger.LogInfo($"[CombatStateMachine] 부활 아이템 발견: 슬롯 {reviveItemSlot}", GameLogger.LogCategory.Combat);

                // 부활 아이템 사용 (사용 후 자동으로 슬롯에서 제거됨)
                bool success = itemService.UseActiveItem(reviveItemSlot);
                
                if (success)
                {
                    GameLogger.LogInfo("[CombatStateMachine] 부활 아이템 사용 성공 - 한 번만 부활됩니다", GameLogger.LogCategory.Combat);
                    return true;
                }
                else
                {
                    GameLogger.LogWarning("[CombatStateMachine] 부활 아이템 사용 실패", GameLogger.LogCategory.Combat);
                    return false;
                }
            }
            catch (System.Exception ex)
            {
                GameLogger.LogError($"[CombatStateMachine] 자동 부활 중 오류 발생: {ex.Message}", GameLogger.LogCategory.Combat);
                return false;
            }
        }

        /// <summary>
        /// 부활 아이템이 있는 슬롯을 찾습니다.
        /// </summary>
        /// <param name="itemService">아이템 서비스</param>
        /// <returns>부활 아이템 슬롯 인덱스, 없으면 -1</returns>
        private int FindReviveItemSlot(Game.ItemSystem.Service.ItemService itemService)
        {
            // ItemService의 activeSlots에 접근하기 위해 리플렉션 사용
            var activeSlotsField = typeof(Game.ItemSystem.Service.ItemService).GetField("activeSlots", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (activeSlotsField != null)
            {
                var activeSlots = activeSlotsField.GetValue(itemService) as Game.ItemSystem.Interface.ActiveItemSlotData[];
                
                if (activeSlots != null)
                {
                    for (int i = 0; i < activeSlots.Length; i++)
                    {
                        var slot = activeSlots[i];
                        if (!slot.isEmpty && slot.item != null)
                        {
                            // 부활 관련 아이템인지 확인
                            if (slot.item.DisplayName.Contains("부활") || 
                                slot.item.DisplayName.Contains("Revive") ||
                                slot.item.DisplayName.Contains("징표"))
                            {
                                return i;
                            }
                        }
                    }
                }
            }

            return -1;
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

        /// <summary>
        /// 부활 후 플래그를 리셋합니다 (다음 사망 시 다시 부활 가능하도록)
        /// </summary>
        private System.Collections.IEnumerator ResetReviveFlagAfterRevive()
        {
            // 플레이어가 부활하는 시간 대기
            yield return new WaitForSeconds(0.5f);
            
            // 플레이어가 살아있는지 확인
            var playerCharacter = playerManager?.GetCharacter();
            if (playerCharacter != null && !playerCharacter.IsDead())
            {
                hasUsedReviveThisDeath = false; // 플래그 리셋 (다음 사망에서 다시 부활 가능)
                GameLogger.LogInfo("[CombatStateMachine] 부활 플래그 리셋 완료 - 다음 사망에서도 부활 가능", GameLogger.LogCategory.Combat);
            }
        }

        #endregion
    }
}
