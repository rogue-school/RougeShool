using UnityEngine;
using Game.Cards;
using Game.Interface;

namespace Game.UI
{
    /// <summary>
    /// 플레이어 핸드 슬롯 UI에 사용되는 카드 슬롯 컴포넌트입니다.
    /// </summary>
    public class PlayerCardSlotUI : BaseCardSlotUI
    {
        [SerializeField] private SkillCardUI cardUI;

        public override void SetCard(ISkillCard card)
        {
            base.SetCard(card);
            if (cardUI != null)
                cardUI.SetCard(card);
        }

        public override void Clear()
        {
            base.Clear();
            if (cardUI != null)
                cardUI.SetCard(null);
        }
    }
}
