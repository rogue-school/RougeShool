using UnityEngine;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.UI;
using Game.CombatSystem.Interface;
using Game.CombatSystem.Utility;
using Game.CombatSystem.Slot;
using Game.CombatSystem.Service;

namespace Game.CombatSystem.Service
{
    /// <summary>
    /// 플레이어의 전투 슬롯에 카드를 교체할 때 기존 카드를 핸드로 복귀시키고 새 카드를 배치하는 로직을 담당합니다.
    /// </summary>
    public class PlayerCardReplacementHandler : ICardReplacementHandler
    {
        private readonly IPlayerHandManager handManager;
        private readonly ITurnCardRegistry turnRegistry;

        public PlayerCardReplacementHandler(IPlayerHandManager handManager, ITurnCardRegistry turnRegistry)
        {
            this.handManager = handManager;
            this.turnRegistry = turnRegistry;
        }

        public void ReplaceSlotCard(ICombatCardSlot slot, ISkillCard newCard, SkillCardUI newCardUI)
        {
            var oldCard = slot.GetCard();
            var oldUI = slot.GetCardUI() as SkillCardUI;

            if (oldCard != null && oldUI != null)
            {
                // 전투 슬롯 클리어
                CardRegistrar.ClearSlot(slot);

                // 이전 슬롯 해제
                var oldCombatSlot = oldCard.GetCombatSlot();
                if (oldCombatSlot.HasValue)
                    turnRegistry.ClearPlayerCard(oldCombatSlot.Value);

                // 핸드 슬롯 복귀
                var oldHandSlot = oldCard.GetHandSlot();
                if (oldHandSlot.HasValue)
                {
                    handManager.RestoreCardToHand(oldCard, oldHandSlot.Value);
                    CardSlotHelper.AttachCardToHandSlot(oldUI, oldHandSlot.Value); // 새 헬퍼 메서드 필요
                }
                else
                {
                    Debug.LogWarning("[PlayerCardReplacementHandler] 핸드 슬롯 정보 없음 → 자동 배치 시도");
                    handManager.RestoreCardToHand(oldCard);
                    CardSlotHelper.ResetCardToOriginal(oldUI); // 위치 복원
                }
            }

            // 새 카드 전투 슬롯 등록
            slot.SetCard(newCard);
            slot.SetCardUI(newCardUI);
            var execSlot = SlotPositionUtil.ToExecutionSlot(slot.GetCombatPosition());
            newCard.SetCombatSlot(execSlot);

            CardSlotHelper.AttachCardToSlot(newCardUI, (MonoBehaviour)slot);

            Debug.Log($"[PlayerCardReplacementHandler] 새 카드 등록 완료: {newCard.CardData?.Name} → 슬롯: {execSlot}");
        }
    }
}
