using UnityEngine;

namespace Game.CombatSystem.State
{
    /// <summary>
    /// 플레이어 턴 상태
    /// - 플레이어가 카드를 드래그하여 배틀 슬롯에 배치할 수 있습니다
    /// - 카드 배치 시 CardExecutionState로 전환됩니다
    /// </summary>
    public class PlayerTurnState : BaseCombatState
    {
        public override string StateName => "PlayerTurn";

        // 플레이어 턴에서는 카드 드래그만 허용
        public override bool AllowPlayerCardDrag => true;
        public override bool AllowEnemyAutoExecution => false;
        public override bool AllowSlotMovement => false;
        public override bool AllowTurnSwitch => false;

        public override void OnEnter(CombatStateContext context)
        {
            base.OnEnter(context);

            if (context == null || !context.ValidateManagers())
            {
                LogError("컨텍스트 또는 매니저 검증 실패");
                return;
            }

            LogStateTransition("플레이어 턴 시작");

            // 플레이어 턴 설정 (TurnController 사용)
            if (context.TurnController != null)
            {
                context.TurnController.SetTurnAndIncrement(Interface.TurnType.Player);
            }

            // 턴별 효과 처리 (가드, 출혈, 반격, 기절 등)
            ProcessTurnEffects(context);

            // 플레이어 손패 생성
            if (context.HandManager != null)
            {
                context.HandManager.GenerateInitialHand();
                LogStateTransition("플레이어 손패 생성 완료");
            }
            else
            {
                LogWarning("HandManager가 null - 손패 생성 건너뜀");
            }

            LogStateTransition("플레이어 턴 시작 - 카드 드래그 대기 중");
        }

        public override void OnExit(CombatStateContext context)
        {
            base.OnExit(context);
            LogStateTransition("플레이어 턴 종료");
        }

        /// <summary>
        /// 턴별 효과를 처리합니다 (가드, 출혈, 반격, 기절 등)
        /// </summary>
        private void ProcessTurnEffects(CombatStateContext context)
        {
            if (context?.TurnController == null)
            {
                LogWarning("TurnController가 null - 턴별 효과 처리 건너뜀");
                return;
            }

            // TurnController의 ProcessAllCharacterTurnEffects 메서드 호출
            context.TurnController.ProcessAllCharacterTurnEffects();
            LogStateTransition("턴별 효과 처리 완료 (가드, 출혈, 반격, 기절 등)");
        }


        /// <summary>
        /// 플레이어가 카드를 배치했을 때 호출
        /// CardExecutionState로 전환합니다
        /// </summary>
        public void OnCardPlaced(CombatStateContext context,
            Game.SkillCardSystem.Interface.ISkillCard card,
            Game.CombatSystem.Slot.CombatSlotPosition slot)
        {
            if (context?.StateMachine == null)
            {
                LogError("StateMachine이 null입니다");
                return;
            }

            LogStateTransition($"카드 배치: {card?.GetCardName()} → {slot}");

            // 실행 컨텍스트 설정
            context.CurrentExecutingCard = card;
            context.CurrentExecutingSlot = slot;

            // 카드 실행 상태로 전환
            var executionState = new CardExecutionState();
            RequestTransition(context, executionState);
        }
    }
}
