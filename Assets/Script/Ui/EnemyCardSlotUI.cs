using UnityEngine;
using Game.Interface;
using Game.UI;

namespace Game.UI
{
    /// <summary>
    /// 적의 카드 슬롯 UI에서 SkillCardUI를 표시하고 카드 설정 기능을 담당합니다.
    /// </summary>
    public class EnemyCardSlotUI : MonoBehaviour, ICardSlot
    {
        [SerializeField] private SkillCardUI cardUI;
        private ISkillCard currentCard;

        public void SetCard(ISkillCard card)
        {
            currentCard = card;
            cardUI.SetCard(card);
        }

        public ISkillCard GetCard() => currentCard;

        public bool HasCard() => currentCard != null;

        public void Clear()
        {
            currentCard = null;
            cardUI.SetCard(null);
        }
    }
}
