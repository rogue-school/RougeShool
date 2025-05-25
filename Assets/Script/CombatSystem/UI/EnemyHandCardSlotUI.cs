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
        private ISkillCardUI currentCardUI; // 할당 전용 필드

        public SkillCardSlotPosition GetSlotPosition() => position;

        public SlotOwner GetOwner() => SlotOwner.ENEMY;

        public ISkillCard GetCard() => currentCard;

        public ISkillCardUI GetCardUI() => currentCardUI;

        public void SetCard(ISkillCard card)
        {
            currentCard = card;
            currentCard?.SetHandSlot(position);
        }

        /// <summary>
        /// 외부에서 카드 UI를 설정합니다.
        /// </summary>
        public void SetCardUI(ISkillCardUI ui)
        {
            currentCardUI = ui;
        }

        public void Clear()
        {
            currentCard = null;
            currentCardUI = null;
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
