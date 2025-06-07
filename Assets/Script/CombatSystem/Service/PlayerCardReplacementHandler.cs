using UnityEngine;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.UI;
using Game.CombatSystem.Interface;
using Game.CombatSystem.Utility;
using Game.CombatSystem.Slot;

namespace Game.CombatSystem.Service
{
    /// <summary>
    /// 플레이어의 전투 슬롯에 카드를 교체할 때 기존 카드를 핸드로 복귀시키고 새 카드를 배치하는 로직
    /// </summary>
    public class PlayerCardReplacementHandler : ICardReplacementHandler
    {
        private readonly IPlayerHandManager handManager;
        private readonly ITurnCardRegistry cardRegistry;

        public PlayerCardReplacementHandler(IPlayerHandManager handManager, ITurnCardRegistry cardRegistry)
        {
            this.handManager = handManager;
            this.cardRegistry = cardRegistry;
        }

        public void ReplaceSlotCard(ICombatCardSlot slot, ISkillCard newCard, SkillCardUI newCardUI)
        {
            var oldCard = slot.GetCard();
            var oldUI = slot.GetCardUI() as SkillCardUI;

            if (oldCard != null && oldUI != null)
            {
                // 슬롯 해제
                CardRegistrar.ClearSlot(slot);

                // 이전 카드 핸드 복귀
                var oldCombatSlot = oldCard.GetCombatSlot();
                if (oldCombatSlot.HasValue)
                {
                    cardRegistry.ClearSlot(oldCombatSlot.Value);
                }

                var oldHandSlot = oldCard.GetHandSlot();
                if (oldHandSlot.HasValue)
                {
                    handManager.RestoreCardToHand(oldCard, oldHandSlot.Value);
                    CardSlotHelper.AttachCardToHandSlot(oldUI, oldHandSlot.Value);
                }
                else
                {
                    Debug.LogWarning("[PlayerCardReplacementHandler] 핸드 슬롯 정보 없음 → 자동 복귀");
                    handManager.RestoreCardToHand(oldCard);
                    CardSlotHelper.ResetCardToOriginal(oldUI);
                }
            }

            // 새 카드 등록
            slot.SetCard(newCard);
            slot.SetCardUI(newCardUI);
            var execSlot = SlotPositionUtil.ToExecutionSlot(slot.GetCombatPosition());
            newCard.SetCombatSlot(execSlot);

            // 카드 UI 연동
            CardSlotHelper.AttachCardToSlot(newCardUI, (MonoBehaviour)slot);

            // 전투 카드 레지스트리에 등록
            cardRegistry.RegisterCard(execSlot, newCard, newCardUI, SlotOwner.PLAYER);

            Debug.Log($"[PlayerCardReplacementHandler] 카드 교체 완료 → 슬롯: {execSlot}, 카드: {newCard.CardData?.Name}");
        }
    }
}
