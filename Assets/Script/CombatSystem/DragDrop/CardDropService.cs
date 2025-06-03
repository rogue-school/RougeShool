using UnityEngine;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.UI;
using Game.CombatSystem.Interface;
using Game.CombatSystem.Slot;
using Game.CombatSystem.Utility;

namespace Game.CombatSystem.Service
{
    /// <summary>
    /// 카드 드롭 처리 서비스.
    /// 슬롯 드롭 유효성 검사 및 교체 처리, 전투 슬롯 및 레지스트리 등록을 담당.
    /// </summary>
    public class CardDropService
    {
        private readonly ICardDropValidator validator;
        private readonly ICardRegistrar registrar;
        private readonly ICombatTurnManager turnManager;
        private readonly ICardReplacementHandler replacementHandler;

        public CardDropService(
            ICardDropValidator validator,
            ICardRegistrar registrar,
            ICombatTurnManager turnManager,
            ICardReplacementHandler replacementHandler)
        {
            this.validator = validator;
            this.registrar = registrar;
            this.turnManager = turnManager;
            this.replacementHandler = replacementHandler;
        }

        /// <summary>
        /// 드롭 가능한지 판단 후 카드 등록 로직 실행
        /// </summary>
        public bool TryDropCard(ISkillCard card, SkillCardUI ui, ICombatCardSlot slot, out string message)
        {
            message = "";

            if (!turnManager.IsPlayerInputTurn())
            {
                message = "플레이어 입력 턴이 아닙니다.";
                Debug.LogWarning("[CardDropService] 입력 턴 아님");
                return false;
            }

            if (!validator.IsValidDrop(card, slot, out message))
            {
                Debug.LogWarning($"[CardDropService] 드롭 유효성 실패: {message}");
                return false;
            }

            // 슬롯 교체 처리
            replacementHandler.ReplaceSlotCard(slot, card, ui);

            // 슬롯 위치를 실행 슬롯으로 변환
            var execSlot = SlotPositionUtil.ToExecutionSlot(slot.GetCombatPosition());

            // 전투 턴 매니저에 카드 등록
            turnManager.RegisterCard(execSlot, card, ui, SlotOwner.PLAYER);

            Debug.Log("[CardDropService] 카드 드롭 처리 완료");
            return true;
        }
    }
}
