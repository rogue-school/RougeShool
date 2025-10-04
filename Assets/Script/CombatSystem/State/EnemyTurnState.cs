using UnityEngine;

namespace Game.CombatSystem.State
{
    /// <summary>
    /// 적 턴 상태
    /// - 적 카드가 자동으로 실행됩니다
    /// - 배틀 슬롯에 적 카드가 도달하면 자동 실행됩니다
    /// - 플레이어는 카드를 드래그할 수 없습니다
    /// </summary>
    public class EnemyTurnState : BaseCombatState
    {
        public override string StateName => "EnemyTurn";

        // 적 턴에서는 적 카드 자동 실행과 슬롯 이동 허용
        public override bool AllowPlayerCardDrag => false;
        public override bool AllowEnemyAutoExecution => true;
        public override bool AllowSlotMovement => true;
        public override bool AllowTurnSwitch => false;

        public override void OnEnter(CombatStateContext context)
        {
            base.OnEnter(context);

            if (context == null || !context.ValidateManagers())
            {
                LogError("컨텍스트 또는 매니저 검증 실패");
                return;
            }

            LogStateTransition("적 턴 시작");

            // 적 턴 설정
            context.TurnManager?.SetTurnAndIncrement(Manager.TurnManager.TurnType.Enemy);

            // 턴별 효과 처리 (가드, 출혈, 반격, 기절 등)
            ProcessTurnEffects(context);

            // 플레이어 손패 정리 (적 턴에는 플레이어가 카드를 낼 수 없음)
            if (context.HandManager != null)
            {
                context.HandManager.ClearAll();
                LogStateTransition("플레이어 손패 정리 완료");
            }

            // 배틀 슬롯의 적 카드를 즉시 실행
            CheckAndExecuteEnemyCard(context);
        }

        public override void OnExit(CombatStateContext context)
        {
            base.OnExit(context);
            LogStateTransition("적 턴 종료");
        }

        /// <summary>
        /// 턴별 효과를 처리합니다 (가드, 출혈, 반격, 기절 등)
        /// </summary>
        private void ProcessTurnEffects(CombatStateContext context)
        {
            if (context?.TurnManager == null)
            {
                LogWarning("TurnManager가 null - 턴별 효과 처리 건너뜀");
                return;
            }

            // TurnManager의 ProcessAllCharacterTurnEffects 메서드 호출
            context.TurnManager.ProcessAllCharacterTurnEffects();
            LogStateTransition("턴별 효과 처리 완료 (가드, 출혈, 반격, 기절 등)");
        }

        /// <summary>
        /// 배틀 슬롯의 적 카드를 확인하고 실행합니다
        /// 새로운 로직: 적 스킬카드만 실행, 플레이어 턴 마커는 무시
        /// </summary>
        private void CheckAndExecuteEnemyCard(CombatStateContext context)
        {
            if (context?.TurnManager == null)
            {
                LogError("TurnManager가 null입니다");
                return;
            }

            var battleCard = context.TurnManager.GetCardInSlot(
                Game.CombatSystem.Slot.CombatSlotPosition.BATTLE_SLOT);

            // 배틀 슬롯 상태 확인
            if (battleCard == null)
            {
                LogWarning("배틀 슬롯이 비어있음 - 적 턴인데 실행할 카드가 없음");
                // 빈 슬롯 → 플레이어 턴으로 복귀
                var playerTurnState = new PlayerTurnState();
                RequestTransition(context, playerTurnState);
                return;
            }

            // 플레이어 턴 마커인 경우 → 플레이어 턴으로 전환
            if (IsPlayerTurnMarker(battleCard))
            {
                LogStateTransition($"플레이어 턴 마커 발견: {battleCard.GetCardName()} → 플레이어 턴으로 전환");
                var playerTurnState = new PlayerTurnState();
                RequestTransition(context, playerTurnState);
                return;
            }

            // 적 스킬카드인 경우 → 실행
            if (!battleCard.IsFromPlayer())
            {
                LogStateTransition($"적 스킬카드 발견: {battleCard.GetCardName()} → 실행");
                OnEnemyCardReady(context, battleCard);
                return;
            }

            // 예상치 못한 상황 (플레이어 스킬카드 등)
            LogWarning($"예상치 못한 카드 타입: {battleCard.GetCardName()} → 플레이어 턴으로 복귀");
            var fallbackPlayerTurnState = new PlayerTurnState();
            RequestTransition(context, fallbackPlayerTurnState);
        }

        /// <summary>
        /// 카드가 플레이어 턴 마커인지 확인합니다
        /// </summary>
        private bool IsPlayerTurnMarker(Game.SkillCardSystem.Interface.ISkillCard card)
        {
            if (card?.CardDefinition == null) return false;
            
            // PLAYER_MARKER ID이면서 플레이어 소유인 경우
            return card.CardDefinition.cardId == "PLAYER_MARKER" && card.IsFromPlayer();
        }

        /// <summary>
        /// 적 카드가 배틀 슬롯에 준비되었을 때 호출
        /// CardExecutionState로 전환합니다
        /// </summary>
        public void OnEnemyCardReady(CombatStateContext context,
            Game.SkillCardSystem.Interface.ISkillCard card)
        {
            if (context?.StateMachine == null)
            {
                LogError("StateMachine이 null입니다");
                return;
            }

            if (card == null || card.IsFromPlayer())
            {
                LogWarning("유효하지 않은 적 카드입니다");
                return;
            }

            LogStateTransition($"적 카드 실행 준비: {card.GetCardName()}");

            // 실행 컨텍스트 설정
            context.CurrentExecutingCard = card;
            context.CurrentExecutingSlot = Game.CombatSystem.Slot.CombatSlotPosition.BATTLE_SLOT;

            // 카드 실행 상태로 전환
            var executionState = new CardExecutionState();
            RequestTransition(context, executionState);
        }
    }
}
