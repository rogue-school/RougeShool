using UnityEngine;
using Game.Interface;
using Game.Cards;

namespace Game.UI
{
    /// <summary>
    /// 적 전용 카드 슬롯 UI.
    /// 애니메이션 외 추가 UI 없음. 기본 기능만 사용.
    /// </summary>
    public class EnemyCardSlotUI : BaseCardSlotUI
    {
        public override void SetCard(ISkillCard newCard)
        {
            base.SetCard(newCard);
        }

        public override ISkillCard GetCard()
        {
            return base.GetCard();
        }

        public override void Clear()
        {
            base.Clear();
            // 향후 효과 연출 추가 가능
        }
    }
}
