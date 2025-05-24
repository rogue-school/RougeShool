using UnityEngine;
using Game.CombatSystem.Interface;
using Game.CombatSystem.Slot;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.Slot;

namespace Game.CombatSystem.UI
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
        private ISkillCardUI currentCardUI;

        public ISkillCardUI GetCardUI()
        {
            return currentCardUI;
        }

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

        /// <summary>
        /// 현재 슬롯에 카드가 있는지 여부
        /// </summary>
        public bool HasCard()
        {
            return currentCard != null;
        }
    }
}
