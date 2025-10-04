using UnityEngine;
using Game.SkillCardSystem.Interface;
using Game.CoreSystem.Utility;
using Game.SkillCardSystem.UI;
using Game.CombatSystem.Interface;
using Game.CombatSystem.Data;
using Game.CombatSystem.Slot;
using Game.CombatSystem.Utility;
using Game.CombatSystem.Manager;
using Game.CombatSystem.DragDrop;
using Game.CombatSystem.State;
using DG.Tweening;

namespace Game.CombatSystem.Service
{
    /// <summary>
    /// 카드 드롭 처리 서비스.
    /// 슬롯 드롭 유효성 검사, 기존 카드 교체, 전투 레지스트리 등록을 담당합니다.
    /// 상태 머신을 통해 드롭 허가 여부를 확인합니다.
    /// </summary>
    public class CardDropService
    {
        private readonly ICardValidator validator;
        private readonly CardDropRegistrar registrar;
        private readonly TurnManager turnManager;
        private readonly CombatExecutionManager executionManager;
        private CombatStateMachine stateMachine;

        /// <summary>
        /// 생성자. 필요한 의존성을 주입합니다.
        /// </summary>
        public CardDropService(
            ICardValidator validator,
            CardDropRegistrar registrar,
            TurnManager turnManager,
            CombatExecutionManager executionManager)
        {
            this.validator = validator;
            this.registrar = registrar;
            this.turnManager = turnManager;
            this.executionManager = executionManager;

            // CombatStateMachine 찾기 (지연 초기화)
            FindStateMachine();
        }

        /// <summary>
        /// CombatStateMachine을 찾습니다 (지연 초기화)
        /// </summary>
        private void FindStateMachine()
        {
            if (stateMachine == null)
            {
                stateMachine = Object.FindFirstObjectByType<CombatStateMachine>();
                if (stateMachine != null)
                {
                    GameLogger.LogInfo("[CardDropService] CombatStateMachine 연결 완료", GameLogger.LogCategory.Combat);
                }
            }
        }

        /// <summary>
        /// 지정된 슬롯에 카드 드롭을 시도합니다.
        /// 조건이 충족되면 카드 UI와 데이터를 슬롯에 배치하고 레지스트리에 등록합니다.
        /// </summary>
        /// <param name="card">드롭할 카드</param>
        /// <param name="ui">해당 카드 UI</param>
        /// <param name="slot">드롭 대상 슬롯</param>
        /// <param name="message">실패 시 원인 메시지</param>
        /// <returns>드롭 성공 여부</returns>
        public bool TryDropCard(ISkillCard card, SkillCardUI ui, object slot, out string message)
        {
            message = "";

            // 상태 머신 확인 (지연 초기화)
            if (stateMachine == null)
            {
                FindStateMachine();
            }

            // 0. 상태 머신 검증 - 플레이어 카드 드래그 허용 여부 확인
            if (stateMachine != null)
            {
                if (!stateMachine.CanPlayerDragCard())
                {
                    message = "현재 상태에서 카드 배치가 허용되지 않습니다.";
                    GameLogger.LogWarning($"[CardDropService] {message} (상태: {stateMachine.GetCurrentState()?.StateName ?? "None"})", GameLogger.LogCategory.SkillCard);
                    return false;
                }
            }
            else
            {
                // 상태 머신이 없으면 기존 방식으로 폴백
                GameLogger.LogWarning("[CardDropService] CombatStateMachine을 찾을 수 없음 - 기존 방식으로 검증", GameLogger.LogCategory.SkillCard);
            }

            // 1. 플레이어 입력 턴 여부 확인 (기존 방식 - 폴백)
            if (stateMachine == null && !turnManager.IsPlayerTurn())
            {
                message = "플레이어 입력 턴이 아닙니다.";
                GameLogger.LogWarning($"[CardDropService] {message}", GameLogger.LogCategory.SkillCard);
                return false;
            }

            // 2. 카드 유효성 확인
            if (card == null)
            {
                message = "카드가 null입니다.";
                GameLogger.LogWarning($"[CardDropService] {message}", GameLogger.LogCategory.SkillCard);
                return false;
            }

            // 3. 드롭 유효성 검사 (새로운 아키텍처에서는 단순화)
            if (!CanDropCard(card, slot))
            {
                message = "카드 드롭 조건을 만족하지 않습니다.";
                GameLogger.LogWarning($"[CardDropService] 드롭 유효성 실패: {message}", GameLogger.LogCategory.SkillCard);
                return false;
            }

            // 4. 슬롯에 카드 배치 및 UI 스냅
            // GameLogger.LogInfo($"[CardDropService] 카드 드롭 성공: {card.CardDefinition?.CardName ?? "Unknown"}", GameLogger.LogCategory.SkillCard);

            var combatSlot = slot as ICombatCardSlot;
            if (combatSlot != null)
            {
                // 데이터 등록
                combatSlot.SetCard(card);
                combatSlot.SetCardUI(ui);

                // 슬롯 위치 확인 및 실행 트리거
                var slotPosition = combatSlot.Position;
                
                // UI 정중앙 스냅(부드럽게 이동 후 부모 설정)
                var target = combatSlot.GetTransform() as RectTransform;
                var uiRect = ui.transform as RectTransform;
                if (target != null && uiRect != null)
                {
                    // 월드 위치로 부드럽게 이동
                    uiRect.SetParent(target.root, true); // 우선 월드 이동을 위해 루트 유지
                    var endWorld = target.position;
                    uiRect.DOMove(endWorld, 0.15f).SetEase(Ease.OutQuad).OnComplete(() =>
                    {
                        uiRect.SetParent(target, false);
                        uiRect.anchoredPosition = Vector2.zero;
                        uiRect.localScale = Vector3.one;

                        // 상태 머신이 있으면 상태 머신에게 카드 배치 알림 (상태 머신이 실행 처리)
                        // 상태 머신이 없으면 기존 방식으로 실행
                        if (stateMachine != null)
                        {
                            stateMachine.OnPlayerCardPlaced(card, slotPosition);
                        }
                        else
                        {
                            TriggerCardExecution(card, slotPosition);
                        }
                    });
                }
                else
                {
                    // UI 이동이 불가능한 경우
                    // 상태 머신이 있으면 상태 머신에게 카드 배치 알림
                    // 상태 머신이 없으면 기존 방식으로 실행
                    if (stateMachine != null)
                    {
                        stateMachine.OnPlayerCardPlaced(card, slotPosition);
                    }
                    else
                    {
                        TriggerCardExecution(card, slotPosition);
                    }
                }
                return true;
            }

            // 다른 슬롯 타입은 추후 확장
            return false;
        }

        /// <summary>
        /// 카드 실행을 트리거합니다.
        /// </summary>
        /// <param name="card">실행할 카드</param>
        /// <param name="slotPosition">카드가 배치된 슬롯 위치</param>
        private void TriggerCardExecution(ISkillCard card, CombatSlotPosition slotPosition)
        {
            if (card == null || executionManager == null)
            {
                GameLogger.LogWarning("[CardDropService] 카드 실행 실패 - 카드 또는 실행 매니저가 null", GameLogger.LogCategory.SkillCard);
                return;
            }

            // 배틀 슬롯에 배치된 경우 즉시 실행
            if (slotPosition == CombatSlotPosition.BATTLE_SLOT)
            {
                // GameLogger.LogInfo($"[CardDropService] 배틀 슬롯 카드 즉시 실행: {card.GetCardName()}", GameLogger.LogCategory.SkillCard);
                executionManager.ExecuteCardImmediately(card, slotPosition);
            }
            else
            {
                // 대기 슬롯의 경우 턴 매니저에 알림 (향후 턴 진행 시 실행됨)
                GameLogger.LogInfo($"[CardDropService] 대기 슬롯 카드 등록: {card.GetCardName()} at {slotPosition}", GameLogger.LogCategory.SkillCard);
                // 필요시 turnManager.OnCardQueued(card, slotPosition) 같은 이벤트 추가 가능
            }
        }

        /// <summary>
        /// 카드 드롭 가능 여부를 확인합니다.
        /// </summary>
        /// <param name="card">드롭할 카드</param>
        /// <param name="slot">대상 슬롯</param>
        /// <returns>드롭 가능하면 true</returns>
        private bool CanDropCard(ISkillCard card, object slot)
        {
            // CombatSlotManager 제거로 인한 단순화된 검증
            return true; // 임시로 항상 true 반환
        }
    }
}
