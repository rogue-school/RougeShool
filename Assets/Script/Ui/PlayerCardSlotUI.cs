using UnityEngine;
using Game.Interface;
using Game.Cards;

namespace Game.UI
{
    /// <summary>
    /// 플레이어 전용 카드 슬롯 UI.
    /// UI 반영 및 드래그 처리가 포함됩니다.
    /// </summary>
    public class PlayerCardSlotUI : BaseCardSlotUI
    {
        [SerializeField] private SkillCardUI cardUI;

        public override void SetCard(ISkillCard newCard)
        {
            base.SetCard(newCard);
            cardUI?.SetCard(newCard);
        }

        public override ISkillCard GetCard()
        {
            return base.GetCard();
        }

        public override void Clear()
        {
            base.Clear();
            cardUI?.Clear(); // UI 초기화 포함
        }
    }
}
