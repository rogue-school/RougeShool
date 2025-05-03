using UnityEngine;
using Game.Cards;
using Game.Interface;

namespace Game.UI
{
    /// <summary>
    /// 적의 핸드 카드 슬롯 UI를 담당하는 컴포넌트입니다.
    /// </summary>
    public class EnemyCardSlotUI : BaseCardSlotUI
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
