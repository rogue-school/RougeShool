using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Game.CoreSystem.Utility;
using Game.CharacterSystem.Interface;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.Effect;
using Game.SkillCardSystem.Data;
using Game.SkillCardSystem.Utility;
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
        [Inject] private TurnManager turnManager;
        [Inject] private IPlayerHandManager playerHandManager;
        [Inject] private ICombatFlowManager combatFlowManager;
        [Inject(Optional = true)] private Game.StageSystem.Interface.IStageManager stageManager;

        #endregion

        #region 카드 히스토리

        private class CardHistoryEntry
        {
            public ISkillCard Card;
            public int Turn;
        }

        private readonly List<CardHistoryEntry> _playerCardHistory = new List<CardHistoryEntry>();
        private readonly List<CardHistoryEntry> _enemyCardHistory = new List<CardHistoryEntry>();

        #endregion

        #region 내부 상태

        private bool isExecuting = false;
        private bool isInitialized = false;
        private Queue<ExecutionCommand> executionQueue = new Queue<ExecutionCommand>();

        #endregion

        #region 이벤트

        public System.Action<ISkillCard, ICharacter, ICharacter> OnCardExecuted { get; set; }
        public System.Action<ExecutionResult> OnExecutionCompleted { get; set; }

        #endregion

        #region Unity 생명주기

        private void Awake()
        {
            // CombatExecutionManager 초기화
        }

        private void Start()
        {
            // 의존성 검증을 지연시켜 다른 매니저들이 완전히 초기화될 때까지 대기
            // CombatFlowManager에서 필요할 때 초기화됨
        }

        #endregion

        #region 초기화

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
        /// 카드를 즉시 실행합니다
        /// </summary>
        /// <param name="card">실행할 스킬 카드</param>
        /// <param name="slotPosition">카드가 위치한 슬롯 위치</param>
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

            // 실행 후 처리: 배틀 슬롯에서 사용된 카드는 소유자와 무관하게 정리
            // (상태 머신이 있으면 턴 진행은 상태 머신이 담당)
            if (card != null && slotPosition == CombatSlotPosition.BATTLE_SLOT)
            {
                try
                {
                    if (card.IsFromPlayer())
                    {
                        // 플레이어 핸드에서 해당 카드 제거 (실행 완료 후)
                        if (playerHandManager != null)
                        {
                            playerHandManager.RemoveCard(card);
                            // 플레이어 카드 실행 완료 - 핸드에서 제거
                        }
                        else
                        {
                            GameLogger.LogWarning("PlayerHandManager가 주입되지 않았습니다 - 플레이어 카드 핸드 제거 실패", GameLogger.LogCategory.Combat);
                        }
                    }
                    else
                    {
                        // 적 카드 실행 완료
                    }

                    // 배틀 슬롯 정리 (UI 포함)
                    turnManager?.ClearSlot(CombatSlotPosition.BATTLE_SLOT);
                    // 배틀 슬롯 정리 완료

                    // 턴 진행은 상태 패턴(CombatStateMachine)이 담당
                }
                catch (System.Exception ex)
                {
                    GameLogger.LogError($"카드 실행 후 처리 중 예외: {ex.Message}", GameLogger.LogCategory.Error);
                }
            }

            // 실행 상태를 false로 설정 (OnExecutionCompleted 이벤트 핸들러에서 ExecuteCardImmediately를 호출할 수 있도록)
            isExecuting = false;

            // 실행 완료 이벤트 발생 (isExecuting을 false로 설정한 후 호출)
            OnExecutionCompleted?.Invoke(result);

            // 카드 실행 완료
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

            if (card == null)
            {
                GameLogger.LogError("카드가 null입니다.", GameLogger.LogCategory.Error);
                return new ExecutionResult(false, null, "카드가 null입니다");
            }

            try
            {
                // 플레이어 카드의 자원 소모 처리 (선소모/부족 시 실패)
                var resourceResult = ProcessResourceCost(card);
                if (!resourceResult.isSuccess)
                {
                    return resourceResult;
                }

                // 카드 실행
                card.ExecuteSkill(sourceCharacter, targetCharacter);

                // 카드 사용 히스토리 등록 (연계 스킬은 제외)
                RegisterCardUsage(card);

                // 실행 이벤트 발생
                OnCardExecuted?.Invoke(card, sourceCharacter, targetCharacter);

                // 전투 통계 이벤트 발생
                RaiseCombatStatisticsEvents(card);

                return new ExecutionResult(true, null, "카드 실행 성공");
            }
            catch (System.Exception ex)
            {
                GameLogger.LogError($"카드 실행 중 오류 발생: {ex.Message}", GameLogger.LogCategory.Error);
                return new ExecutionResult(false, null, ex.Message);
            }
        }

        /// <summary>
        /// 플레이어 카드의 자원 소모를 처리합니다
        /// </summary>
        /// <param name="card">실행할 카드</param>
        /// <returns>처리 결과</returns>
        private ExecutionResult ProcessResourceCost(ISkillCard card)
        {
            if (card == null || !card.IsFromPlayer())
            {
                return new ExecutionResult(true, null, "자원 소모 불필요");
            }

            var def = card.CardDefinition;
            var cfg = def?.configuration;
            if (cfg == null || !cfg.HasResourceCost())
            {
                return new ExecutionResult(true, null, "자원 소모 불필요");
            }

            int cost = Mathf.Max(0, cfg.resourceConfig.cost);
            if (cost <= 0)
            {
                return new ExecutionResult(true, null, "자원 소모 불필요");
            }

            if (playerManager == null)
            {
                GameLogger.LogError("PlayerManager가 없어 자원 소모를 처리할 수 없습니다.", GameLogger.LogCategory.Error);
                return new ExecutionResult(false, null, "자원 매니저 없음");
            }

            if (!playerManager.HasEnoughResource(cost))
            {
                GameLogger.LogWarning($"자원이 부족하여 카드를 사용할 수 없습니다. 필요: {cost}, 현재: {playerManager.CurrentResource}", GameLogger.LogCategory.SkillCard);
                return new ExecutionResult(false, null, "자원이 부족합니다");
            }

            // 선소모
            bool consumed = playerManager.ConsumeResource(cost);
            if (!consumed)
            {
                GameLogger.LogWarning($"자원 소모 실패: 필요 {cost}, 현재 {playerManager.CurrentResource}", GameLogger.LogCategory.SkillCard);
                return new ExecutionResult(false, null, "자원 소모 실패");
            }

            return new ExecutionResult(true, null, "자원 소모 완료");
        }

        /// <summary>
        /// 전투 통계 이벤트를 발생시킵니다
        /// </summary>
        /// <param name="card">실행된 카드</param>
        private void RaiseCombatStatisticsEvents(ISkillCard card)
        {
            if (card == null || card.CardDefinition == null)
            {
                return;
            }

            string cardId = card.CardDefinition.cardId;
            GameObject cardObj = GetCardGameObject(card);

            if (card.IsFromPlayer())
            {
                Game.CombatSystem.CombatEvents.RaisePlayerCardUse(cardId, cardObj);
            }
            else
            {
                Game.CombatSystem.CombatEvents.RaiseEnemyCardUse(cardId, cardObj);
            }
        }

        /// <summary>
        /// 카드의 GameObject를 가져옵니다
        /// </summary>
        /// <param name="card">카드 인스턴스</param>
        /// <returns>GameObject, 없으면 null</returns>
        private GameObject GetCardGameObject(ISkillCard card)
        {
            if (card is MonoBehaviour cardMono)
            {
                return cardMono.gameObject;
            }
            return null;
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

        #region 카드 히스토리 관리

        /// <summary>
        /// 이전 턴에 사용된 비-연계 스킬 카드를 조회합니다.
        /// 현재 턴에서 연계 스킬이 사용될 때, 같은 소유자의 직전 턴 카드(연계 제외)를 찾는 데 사용됩니다.
        /// </summary>
        /// <param name="currentCard">현재 실행 중인 카드 (연계 스킬)</param>
        /// <returns>이전 턴에 사용된 비-연계 카드, 없으면 null</returns>
        /// <summary>
        /// 현재 카드의 소유자가 이전 턴에 사용한 비-연계 카드를 반환합니다
        /// </summary>
        /// <param name="currentCard">현재 카드</param>
        /// <returns>이전 턴에 사용한 비-연계 카드, 없으면 null</returns>
        public ISkillCard GetPreviousNonLinkCardForOwner(ISkillCard currentCard)
        {
            if (currentCard == null || turnManager == null)
            {
                return null;
            }

            int currentTurn = turnManager.GetTurnCount();
            var history = currentCard.IsFromPlayer() ? _playerCardHistory : _enemyCardHistory;

            // 가장 최근 기록부터 거꾸로 탐색하면서,
            // - 카드가 null이 아니고
            // - 기록된 턴이 현재 턴보다 작은 경우(이전 턴들)
            // 첫 번째로 발견되는 카드를 반환합니다.
            for (int i = history.Count - 1; i >= 0; i--)
            {
                var entry = history[i];
                if (entry == null || entry.Card == null)
                {
                    continue;
                }

                if (entry.Turn < currentTurn)
                {
                    return entry.Card;
                }
            }

            return null;
        }

        /// <summary>
        /// 카드 사용 히스토리를 등록합니다.
        /// 연계 스킬 카드는 히스토리에서 제외하고, 같은 캐릭터가 나중에 연계 스킬을 사용할 때
        /// 참조할 수 있도록 이전 턴 비-연계 카드만 기록합니다.
        /// </summary>
        /// <param name="card">사용된 카드</param>
        private void RegisterCardUsage(ISkillCard card)
        {
            if (card == null || turnManager == null)
            {
                return;
            }

            // 연계 스킬 자체는 히스토리에서 제외
            if (IsLinkSkillCard(card))
            {
                return;
            }

            int currentTurn = turnManager.GetTurnCount();
            var history = card.IsFromPlayer() ? _playerCardHistory : _enemyCardHistory;

            history.Add(new CardHistoryEntry
            {
                Card = card,
                Turn = currentTurn
            });
        }

        /// <summary>
        /// 해당 카드가 연계(이전 턴 카드 재실행) 스킬 카드인지 확인합니다.
        /// 카드 정의에 ReplayPreviousTurnCardEffectSO가 포함되어 있는지 검사합니다.
        /// </summary>
        /// <param name="card">검사할 카드</param>
        /// <returns>연계 스킬 카드이면 true, 아니면 false</returns>
        private bool IsLinkSkillCard(ISkillCard card)
        {
            var definition = card?.CardDefinition;
            if (definition == null)
            {
                return false;
            }

            var config = definition.configuration;
            if (!config.hasEffects || config.effects == null)
            {
                return false;
            }

            foreach (var effectConfig in config.effects)
            {
                if (effectConfig == null || effectConfig.effectSO == null)
                {
                    continue;
                }

                if (effectConfig.effectSO is ReplayPreviousTurnCardEffectSO)
                {
                    return true;
                }
            }

            return false;
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
        /// 실행 명령을 큐에 추가합니다
        /// </summary>
        /// <param name="command">추가할 실행 명령</param>
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

        #region 슬롯 이동 (호환 API)

        /// <summary>
        /// 슬롯을 앞으로 이동시킵니다 (새로운 5슬롯 시스템)
        /// 상태 시스템으로 이관되어 직접 이동은 수행하지 않으며, TurnManager 어댑터를 통해 위임합니다.
        /// </summary>
        public void MoveSlotsForwardNew()
        {
            // 새로운 슬롯 이동은 상태/슬롯 컨트롤러가 담당. 여기서는 대기열 전진 루틴을 트리거만 시도
            if (turnManager == null)
            {
                GameLogger.LogWarning("TurnManager가 없어 슬롯 이동을 수행할 수 없습니다.", GameLogger.LogCategory.Combat);
                return;
            }

            StartCoroutine(MoveSlotsForwardRoutineInternal());
        }

        /// <summary>
        /// 슬롯을 앞으로 이동시킵니다 (레거시 4슬롯 시스템 호환)
        /// 새로운 시스템에서도 동일 루틴을 사용합니다.
        /// </summary>
        public void MoveSlotsForward()
        {
            MoveSlotsForwardNew();
        }

        private IEnumerator MoveSlotsForwardRoutineInternal()
        {
            // TurnManager는 ISlotMovementController로 위임함
            yield return turnManager?.AdvanceQueueAtTurnStartRoutine();
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

        /// <summary>
        /// 현재 카드 실행 중인지 여부
        /// </summary>
        public bool IsExecuting => isExecuting;
        
        /// <summary>
        /// 초기화 완료 여부
        /// </summary>
        public bool IsInitialized => isInitialized;
        
        /// <summary>
        /// 실행 큐에 대기 중인 명령 개수
        /// </summary>
        public int QueueCount => executionQueue.Count;

        #endregion

        #region 리셋

        /// <summary>
        /// 실행 상태를 초기화합니다
        /// 실행 큐를 비우고 실행 상태를 리셋합니다
        /// </summary>
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
