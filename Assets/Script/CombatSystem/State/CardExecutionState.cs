using UnityEngine;
using System.Collections;
using Game.Domain.Combat.ValueObjects;
using Game.CombatSystem.Slot;

namespace Game.CombatSystem.State
{
    /// <summary>
    /// 카드 실행 상태
    /// - 카드 효과를 실행하고 애니메이션을 재생합니다
    /// - 실행 완료 후 SlotMovingState로 전환합니다
    /// - 이 상태에서는 모든 사용자 입력이 차단됩니다
    /// </summary>
    public class CardExecutionState : BaseCombatState
    {
        public override string StateName => "CardExecution";

        // 실행 중에는 모든 액션 차단
        public override bool AllowPlayerCardDrag => false;
        public override bool AllowEnemyAutoExecution => false;
        public override bool AllowSlotMovement => false;
        public override bool AllowTurnSwitch => false;

        private bool _isExecuting = false;

        public override void OnEnter(CombatStateContext context)
        {
            base.OnEnter(context);

            if (context == null || !context.ValidateManagers())
            {
                LogError("컨텍스트 또는 매니저 검증 실패");
                return;
            }

            if (context.CurrentExecutingCard == null || !context.CurrentExecutingSlot.HasValue)
            {
                LogError("실행할 카드 또는 슬롯 정보가 없습니다");
                // 에러 시 적절한 턴으로 복귀
                ReturnToTurnState(context);
                return;
            }

            LogStateTransition($"카드 실행 시작: {context.CurrentExecutingCard.GetCardName()}");

            // 카드 실행 시작
            ExecuteCard(context);
        }

        public override void OnExit(CombatStateContext context)
        {
            base.OnExit(context);

            // 실행 컨텍스트 정리
            context.CurrentExecutingCard = null;
            context.CurrentExecutingSlot = null;
            _isExecuting = false;

            LogStateTransition("카드 실행 완료");
        }

        /// <summary>
        /// 카드를 실행합니다
        /// </summary>
        private void ExecuteCard(CombatStateContext context)
        {
            if (_isExecuting)
            {
                LogWarning("이미 카드 실행 중입니다");
                return;
            }

            _isExecuting = true;

            var card = context.CurrentExecutingCard;
            var slot = context.CurrentExecutingSlot.Value;

            // 플레이어 카드 실행 시 손패 클리어
            if (card != null && card.IsFromPlayer())
            {
                if (context.HandManager != null)
                {
                    context.HandManager.ClearAll();
                    LogStateTransition("플레이어 손패 클리어 (카드 실행)");
                }
            }

            // CombatExecutionManager를 통해 카드 실행 (도메인 유스케이스 우선 사용)
            if (context.ExecutionManager != null)
            {
                // 실행 완료 이벤트 구독 (일회성)
                System.Action<Game.CombatSystem.Interface.ExecutionResult> onCompleted = null;
                onCompleted = (result) =>
                {
                    // 이벤트 구독 해제
                    context.ExecutionManager.OnExecutionCompleted -= onCompleted;

                    // 실행 완료 후 처리
                    OnExecutionCompleted(context, result);
                };

                context.ExecutionManager.OnExecutionCompleted += onCompleted;

                // 카드 실행
                if (context.ExecuteCardUseCase != null)
                {
                    try
                    {
                        var owner = card != null && card.IsFromPlayer()
                            ? TurnType.Player
                            : TurnType.Enemy;

                        var slotPosition = ToDomainSlotPosition(slot);
                        if (slotPosition == SlotPosition.None)
                        {
                            LogError($"지원하지 않는 슬롯 위치입니다: {slot}");
                            ReturnToTurnState(context);
                            return;
                        }

                        context.ExecuteCardUseCase.Execute(slotPosition, owner);
                    }
                    catch (System.Exception ex)
                    {
                        LogError($"도메인 카드 실행 중 오류 발생: {ex.Message}");
                        ReturnToTurnState(context);
                    }
                }
                else
                {
                    context.ExecutionManager.ExecuteCardImmediately(card, slot);
                }
            }
            else
            {
                LogError("ExecutionManager가 null입니다");
                ReturnToTurnState(context);
            }
        }

        private static SlotPosition ToDomainSlotPosition(CombatSlotPosition position)
        {
            switch (position)
            {
                case CombatSlotPosition.BATTLE_SLOT:
                    return SlotPosition.BattleSlot;
                case CombatSlotPosition.WAIT_SLOT_1:
                    return SlotPosition.WaitSlot1;
                case CombatSlotPosition.WAIT_SLOT_2:
                    return SlotPosition.WaitSlot2;
                case CombatSlotPosition.WAIT_SLOT_3:
                    return SlotPosition.WaitSlot3;
                case CombatSlotPosition.WAIT_SLOT_4:
                    return SlotPosition.WaitSlot4;
                default:
                    return SlotPosition.None;
            }
        }

        /// <summary>
        /// 카드 실행이 완료되었을 때 호출
        /// </summary>
        private void OnExecutionCompleted(CombatStateContext context,
            Game.CombatSystem.Interface.ExecutionResult result)
        {
            LogStateTransition($"실행 결과: {(result.isSuccess ? "성공" : "실패")} - {result.resultMessage}");

            // 적이 죽어서 EnemyDefeatedState로 전환되거나 소환으로 SummonState로 전환된 경우에는 상태 전환을 하지 않음
            // CombatStateMachine에서 이미 적절한 상태로 전환했을 것임
            var currentState = context?.StateMachine?.GetCurrentState();
            if (currentState is EnemyDefeatedState)
            {
                LogStateTransition("적 사망으로 EnemyDefeatedState 전환됨 - 추가 상태 전환 건너뜀");
                return;
            }
            else if (currentState is SummonState)
            {
                LogStateTransition("소환으로 SummonState 전환됨 - 추가 상태 전환 건너뜀");
                return;
            }
            else if (currentState is CombatInitState)
            {
                LogStateTransition("소환으로 CombatInitState 전환됨 - 추가 상태 전환 건너뜀");
                return;
            }

            // 슬롯 이동 상태로 전환
            var slotMovingState = new SlotMovingState();
            RequestTransition(context, slotMovingState);
        }

        /// <summary>
        /// 에러 발생 시 적절한 턴 상태로 복귀
        /// </summary>
        private void ReturnToTurnState(CombatStateContext context)
        {
            if (context?.TurnController == null)
            {
                LogError("TurnController가 null이어서 복귀할 수 없습니다");
                return;
            }

            // 현재 턴 타입에 따라 적절한 상태로 복귀
            if (context.TurnController.CurrentTurn == Interface.TurnType.Player)
            {
                var playerTurnState = new PlayerTurnState();
                RequestTransition(context, playerTurnState);
            }
            else
            {
                var enemyTurnState = new EnemyTurnState();
                RequestTransition(context, enemyTurnState);
            }
        }
    }
}
