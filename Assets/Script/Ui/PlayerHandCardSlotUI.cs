using UnityEngine;
using Game.Cards;
using Game.Interface;
using Game.Slots;

namespace Game.UI.Hand
{
    /// <summary>
    /// 플레이어 핸드에 있는 카드 슬롯 UI입니다.
    /// </summary>
    public class PlayerHandCardSlotUI : MonoBehaviour, IHandCardSlot
    {
        [SerializeField] private SkillCardSlotPosition position;
        private ISkillCard currentCard;

        public SkillCardSlotPosition GetSlotPosition() => position;

        public SlotOwner GetOwner() => SlotOwner.PLAYER;

        public void SetCard(ISkillCard card)
        {
            currentCard = card;
            currentCard.SetOwnerSlot(position);
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
