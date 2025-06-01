using UnityEngine;
using Game.CombatSystem.Interface;
using Game.CombatSystem.Slot;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.Slot;
using Game.SkillCardSystem.UI;

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
        public SkillCardUI AttachCard(ISkillCard card)
        {
            SetCard(card); // 내부적으로 currentCard 할당
            return null;   // 적 카드의 경우 UI를 따로 생성하지 않는다면 null 반환
        }

        public void DetachCard()
        {
            Clear(); // currentCard, currentCardUI 초기화
        }
    }
}
