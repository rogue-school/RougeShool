using UnityEngine;
using Game.Interface;
using Game.Slots;

namespace Game.UI.Hand
{
    /// <summary>
    /// 적 핸드에 있는 카드 슬롯 UI입니다.
    /// </summary>
    public class EnemyHandCardSlotUI : MonoBehaviour, IHandCardSlot
    {
        [SerializeField] private SkillCardSlotPosition position;
        private ISkillCard currentCard;

        public SkillCardSlotPosition GetSlotPosition() => position;

        public SlotOwner GetOwner() => SlotOwner.ENEMY;

        public void SetCard(ISkillCard card)
        {
            currentCard = card;
            currentCard.SetHandSlot(position);
        }

        public ISkillCard GetCard()
        {
            return currentCard;
        }

        public void Clear()
        {
            currentCard = null;
        }
    }
}
