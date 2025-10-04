using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Game.CoreSystem.Utility;
using Game.CharacterSystem.Interface;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.Data;
using Game.CombatSystem.Slot;
using Game.CombatSystem.Interface;
using Game.CombatSystem.Utility;
using Game.CharacterSystem.Manager;
using Zenject;

namespace Game.CombatSystem.Manager
{
    /// <summary>
    /// 전투 실행을 관리하는 통합 매니저
    /// SlotExecutionSystem, CardManager, CharacterManager의 기능을 통합합니다.
    /// </summary>
    public class CombatExecutionManager : MonoBehaviour, ICombatExecutionManager
    {
        #region 기본 설정

        [Header("매니저 기본 설정")]
        [Tooltip("디버그 로깅 활성화")]
        [SerializeField] private bool enableDebugLogging = true;


        #endregion

        #region 실행 설정

        [Header("실행 설정")]
        [Tooltip("즉시 실행 활성화")]
        [SerializeField] private bool enableImmediateExecution = true;

        [Tooltip("실행 지연 시간")]
        [SerializeField] private float executionDelay = 0.1f;

        [Tooltip("애니메이션 대기 시간")]
        [SerializeField] private float animationWaitTime = 0.5f;

        #endregion

        #region 의존성 주입

        [Inject] private PlayerManager playerManager;
        [Inject] private EnemyManager enemyManager;
        // CombatSlotManager 제거됨 - 슬롯 관리 기능을 CombatFlowManager로 이전
        [Inject] private TurnManager turnManager;

        #endregion

        #region 내부 상태

        private bool isExecuting = false;
        private bool isInitialized = false;
        private Queue<ExecutionCommand> executionQueue = new Queue<ExecutionCommand>();

        // FindObjectOfType 캐싱
        private Game.SkillCardSystem.Manager.PlayerHandManager cachedPlayerHandManager;
        private ICombatFlowManager cachedCombatFlowManager;

        #endregion

        #region 이벤트

        public System.Action<ISkillCard, ICharacter, ICharacter> OnCardExecuted { get; set; }
        public System.Action<ExecutionResult> OnExecutionCompleted { get; set; }

        #endregion

        #region Unity 생명주기

        private void Awake()
        {
            if (enableDebugLogging)
            {
                GameLogger.LogInfo("CombatExecutionManager 초기화 시작", GameLogger.LogCategory.Combat);
            }
        }

        private void Start()
        {
            // 의존성 검증을 지연시켜 다른 매니저들이 완전히 초기화될 때까지 대기
            // CombatFlowManager에서 필요할 때 초기화됨
        }

        #endregion

        #region 초기화 및 캐싱

        /// <summary>
        /// PlayerHandManager 캐시 가져오기 (지연 초기화)
        /// </summary>
        private Game.SkillCardSystem.Manager.PlayerHandManager GetCachedPlayerHandManager()
        {
            if (cachedPlayerHandManager == null)
            {
                cachedPlayerHandManager = FindFirstObjectByType<Game.SkillCardSystem.Manager.PlayerHandManager>();
            }
            return cachedPlayerHandManager;
        }

        /// <summary>
        /// CombatFlowManager 캐시 가져오기 (지연 초기화)
        /// </summary>
        private ICombatFlowManager GetCachedCombatFlowManager()
        {
            if (cachedCombatFlowManager == null)
            {
                // 인터페이스는 UnityEngine.Object가 아니므로 구체 타입으로 조회 후 인터페이스로 캐스팅
                var concrete = FindFirstObjectByType<CombatFlowManager>();
                cachedCombatFlowManager = concrete as ICombatFlowManager;
            }
            return cachedCombatFlowManager;
        }

        private IEnumerator InitializeExecution()
        {
            if (isInitialized)
            {
                yield break;
            }

            if (enableDebugLogging)
            {
                // GameLogger.LogInfo("실행 시스템 초기화 중...", GameLogger.LogCategory.Combat);
            }

            // 의존성 검증
            if (!ValidateDependencies())
            {
                GameLogger.LogError("실행 시스템 의존성 검증 실패", GameLogger.LogCategory.Error);
                yield break;
            }

            // 초기 상태 설정
            isExecuting = false;
            executionQueue.Clear();

            isInitialized = true;

            if (enableDebugLogging)
            {
                // GameLogger.LogInfo("실행 시스템 초기화 완료", GameLogger.LogCategory.Combat);
            }
        }

        private IEnumerator InitializeAndExecute(ISkillCard card, CombatSlotPosition slotPosition)
        {
            yield return InitializeExecution();
            if (isInitialized)
            {
                ExecuteCardImmediately(card, slotPosition);
            }
        }

        #endregion

        #region 카드 실행

        /// <summary>
        /// 카드 즉시 실행
        /// </summary>
        public void ExecuteCardImmediately(ISkillCard card, CombatSlotPosition slotPosition)
        {
            if (!isInitialized)
            {
                // 지연 초기화 후 실행
                StartCoroutine(InitializeAndExecute(card, slotPosition));
                return;
            }

            if (isExecuting)
            {
                GameLogger.LogWarning("이미 실행 중입니다.", GameLogger.LogCategory.Combat);
                return;
            }

            if (!enableImmediateExecution)
            {
                GameLogger.LogWarning("즉시 실행이 비활성화되어 있습니다.", GameLogger.LogCategory.Combat);
                return;
            }

            StartCoroutine(ExecuteCardCoroutine(card, slotPosition));
        }

        private IEnumerator ExecuteCardCoroutine(ISkillCard card, CombatSlotPosition slotPosition)
        {
            isExecuting = true;

            if (enableDebugLogging)
            {
                // GameLogger.LogInfo($"카드 실행 시작: {FormatCardTag(card)} at {slotPosition}", GameLogger.LogCategory.Combat);
            }

            // 실행 지연
            if (executionDelay > 0)
            {
                yield return new WaitForSeconds(executionDelay);
            }

            // 카드 실행
            ExecutionResult result = ExecuteCard(card, slotPosition);

            // 애니메이션 대기
            if (animationWaitTime > 0)
            {
                yield return new WaitForSeconds(animationWaitTime);
            }

            // 실행 완료 이벤트 발생
            OnExecutionCompleted?.Invoke(result);

            // 실행 후 처리: 배틀 슬롯에서 사용된 카드는 소유자와 무관하게 정리
            // (상태 머신이 있으면 턴 진행은 상태 머신이 담당)
            if (card != null && slotPosition == CombatSlotPosition.BATTLE_SLOT)
            {
                try
                {
                    if (card.IsFromPlayer())
                    {
                        // 플레이어 핸드에서 해당 카드 제거 (실행 완료 후)
                        var handMgr = GetCachedPlayerHandManager();
                        if (handMgr != null)
                        {
                            handMgr.RemoveCard(card);
                            GameLogger.LogInfo($"플레이어 카드 실행 완료 - 핸드에서 제거: {FormatCardTag(card)}", GameLogger.LogCategory.SkillCard);
                        }
                        else
                        {
                            GameLogger.LogWarning("PlayerHandManager를 찾을 수 없습니다 - 플레이어 카드 핸드 제거 실패", GameLogger.LogCategory.Combat);
                        }
                    }
                    else
                    {
                        // 적 카드는 핸드에 없으므로 로그만 출력
                        GameLogger.LogInfo($"적 카드 실행 완료: {FormatCardTag(card)}", GameLogger.LogCategory.Combat);
                    }

                    // 배틀 슬롯 정리 (UI 포함)
                    turnManager?.ClearSlot(CombatSlotPosition.BATTLE_SLOT);
                    GameLogger.LogInfo("배틀 슬롯 정리 완료", GameLogger.LogCategory.Combat);

                    // 턴 진행은 상태 패턴(CombatStateMachine)이 담당
                }
                catch (System.Exception ex)
                {
                    GameLogger.LogError($"카드 실행 후 처리 중 예외: {ex.Message}", GameLogger.LogCategory.Error);
                }
            }

            isExecuting = false;

            if (enableDebugLogging)
            {
                GameLogger.LogInfo($"카드 실행 완료: {FormatCardTag(card)}", GameLogger.LogCategory.Combat);
            }
        }

        private ExecutionResult ExecuteCard(ISkillCard card, CombatSlotPosition slotPosition)
        {
            // 소스와 타겟 캐릭터 결정 (카드 소유자 기준)
            ICharacter sourceCharacter = GetSourceCharacter(card);
            ICharacter targetCharacter = GetTargetCharacter(card);

            if (sourceCharacter == null || targetCharacter == null)
            {
                GameLogger.LogError("소스 또는 타겟 캐릭터를 찾을 수 없습니다.", GameLogger.LogCategory.Error);
                return new ExecutionResult(false, null, "캐릭터를 찾을 수 없음");
            }

            // 카드 실행
            try
            {
                card.ExecuteSkill(sourceCharacter, targetCharacter);

                // 실행 이벤트 발생
                OnCardExecuted?.Invoke(card, sourceCharacter, targetCharacter);

                return new ExecutionResult(true, null, "카드 실행 성공");
            }
            catch (System.Exception ex)
            {
                GameLogger.LogError($"카드 실행 중 오류 발생: {ex.Message}", GameLogger.LogCategory.Error);
                return new ExecutionResult(false, null, ex.Message);
            }
        }

        /// <summary>
        /// 소유자_카드ID 형태로 태그 문자열을 반환합니다. 예: 플레이어_test001, 적_test101
        /// </summary>
        private string FormatCardTag(Game.SkillCardSystem.Interface.ISkillCard card)
        {
            if (card == null || card.CardDefinition == null) return "[UnknownCard]";
            string owner = card.IsFromPlayer() ? "플레이어" : "적";
            string id = string.IsNullOrEmpty(card.CardDefinition.cardId) ? (card.GetCardName() ?? "Unknown") : card.CardDefinition.cardId;
            return $"{owner}_{id}";
        }

        #endregion

        #region 슬롯 관리

        /// <summary>
        /// 슬롯 이동 (새로운 5슬롯 시스템)
        /// </summary>
        public void MoveSlotsForwardNew()
        {
            // CombatFlowManager의 슬롯 관리 기능 사용
            var combatFlowManager = FindFirstObjectByType<CombatFlowManager>();
            combatFlowManager?.MoveSlotsForwardNew();
        }

        /// <summary>
        /// 슬롯 이동 (레거시 4슬롯 시스템)
        /// </summary>
        public void MoveSlotsForward()
        {
            // CombatFlowManager의 슬롯 관리 기능 사용
            var combatFlowManager = FindFirstObjectByType<CombatFlowManager>();
            combatFlowManager?.MoveSlotsForward();
        }

        #endregion

        #region 캐릭터 관리

        /// <summary>
        /// 소스 캐릭터 결정
        /// </summary>
        private ICharacter GetSourceCharacter(Game.SkillCardSystem.Interface.ISkillCard card)
        {
            // 카드 소유자 기준으로 소스 결정
            return card != null && card.IsFromPlayer()
                ? playerManager?.GetPlayer()
                : enemyManager?.GetCurrentEnemy();
        }

        /// <summary>
        /// 타겟 캐릭터 결정
        /// </summary>
        private ICharacter GetTargetCharacter(Game.SkillCardSystem.Interface.ISkillCard card)
        {
            // 카드 소유자 기준으로 타겟 결정 (상대편)
            return card != null && card.IsFromPlayer()
                ? enemyManager?.GetCurrentEnemy()
                : playerManager?.GetPlayer();
        }
        

        #endregion

        #region 실행 큐 관리

        /// <summary>
        /// 실행 명령을 큐에 추가
        /// </summary>
        public void QueueExecution(ExecutionCommand command)
        {
            executionQueue.Enqueue(command);

            if (enableDebugLogging)
            {
                GameLogger.LogInfo($"실행 명령 큐에 추가: {command.commandType}", GameLogger.LogCategory.Combat);
            }
        }

        /// <summary>
        /// 큐의 모든 실행 명령 처리
        /// </summary>
        public void ProcessExecutionQueue()
        {
            if (isExecuting)
            {
                return;
            }

            StartCoroutine(ProcessQueueCoroutine());
        }

        private IEnumerator ProcessQueueCoroutine()
        {
            while (executionQueue.Count > 0)
            {
                ExecutionCommand command = executionQueue.Dequeue();
                yield return StartCoroutine(ExecuteCommandCoroutine(command));
            }
        }

        private IEnumerator ExecuteCommandCoroutine(ExecutionCommand command)
        {
            // 명령 실행 로직
            yield return null;
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

            // CombatSlotManager 제거됨 - 슬롯 관리 기능을 CombatFlowManager로 통합

            if (turnManager == null)
            {
                GameLogger.LogError("CombatTurnManager가 주입되지 않았습니다.", GameLogger.LogCategory.Error);
                isValid = false;
            }

            return isValid;
        }

        #endregion

        #region 공개 프로퍼티

        public bool IsExecuting => isExecuting;
        public bool IsInitialized => isInitialized;
        public int QueueCount => executionQueue.Count;

        #endregion

        #region 리셋

        public void ResetExecution()
        {
            isExecuting = false;
            executionQueue.Clear();
            isInitialized = false;

            if (enableDebugLogging)
            {
                GameLogger.LogInfo("CombatExecutionManager 리셋 완료", GameLogger.LogCategory.Combat);
            }
        }

        #endregion
    }

}
