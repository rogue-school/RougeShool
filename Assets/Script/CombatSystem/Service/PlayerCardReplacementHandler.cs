using UnityEngine;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.UI;
using Game.CombatSystem.Interface;
using Game.CombatSystem.Utility;
using Game.CombatSystem.Slot;

namespace Game.CombatSystem.Service
{
    /// <summary>
    /// 플레이어 전투 슬롯의 카드를 교체할 때,
    /// 기존 카드를 핸드로 복귀시키고 새로운 카드를 슬롯에 배치하는 핸들러입니다.
    /// </summary>
    public class PlayerCardReplacementHandler : ICardReplacementHandler
    {
        #region 필드

        private readonly IPlayerHandManager handManager;
        private readonly ITurnCardRegistry cardRegistry;

        #endregion

        #region 생성자

        /// <summary>
        /// 교체 처리 핸들러 생성자.
        /// </summary>
        /// <param name="handManager">플레이어 핸드 매니저</param>
        /// <param name="cardRegistry">턴 카드 레지스트리</param>
        public PlayerCardReplacementHandler(IPlayerHandManager handManager, ITurnCardRegistry cardRegistry)
        {
            this.handManager = handManager;
            this.cardRegistry = cardRegistry;
        }

        #endregion

        #region 카드 교체 처리

        /// <summary>
        /// 슬롯의 기존 카드를 핸드로 복귀시키고 새로운 카드를 슬롯에 배치합니다.
        /// </summary>
        /// <param name="slot">대상 슬롯</param>
        /// <param name="newCard">새로 등록할 카드</param>
        /// <param name="newCardUI">새 카드 UI</param>
        public void ReplaceSlotCard(ICombatCardSlot slot, ISkillCard newCard, SkillCardUI newCardUI)
        {
            var oldCard = slot.GetCard();
            var oldUI = slot.GetCardUI() as SkillCardUI;

            if (oldCard != null && oldUI != null)
            {
                // 기존 카드 슬롯에서 제거
                CardRegistrar.ClearSlot(slot);

                // 기존 카드의 전투 슬롯 정보 제거
                var oldCombatSlot = oldCard.GetCombatSlot();
                if (oldCombatSlot.HasValue)
                {
                    cardRegistry.ClearSlot(oldCombatSlot.Value);
                }

                // 기존 카드 핸드로 복귀
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

            // 카드 UI 정렬 및 슬롯 부착
            CardSlotHelper.AttachCardToSlot(newCardUI, (MonoBehaviour)slot);

            // 전투 카드 레지스트리에 등록
            cardRegistry.RegisterCard(execSlot, newCard, newCardUI, SlotOwner.PLAYER);

            Debug.Log($"[PlayerCardReplacementHandler] 카드 교체 완료 → 슬롯: {execSlot}, 카드: {newCard.CardData?.Name}");
        }

        #endregion
    }
}
