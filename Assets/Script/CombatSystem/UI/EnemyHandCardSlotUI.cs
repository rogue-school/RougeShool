using UnityEngine;
using Game.CombatSystem.Interface;
using Game.CombatSystem.Slot;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.Slot;
using Game.SkillCardSystem.UI;
using Game.CombatSystem.Animation;

namespace Game.CombatSystem.UI
{
    /// <summary>
    /// 적 핸드에 있는 카드 슬롯 UI입니다.
    /// </summary>
    public class EnemyHandCardSlotUI : MonoBehaviour, IHandCardSlot
    {
        [SerializeField] private SkillCardSlotPosition position;

        private ISkillCard currentCard;
        private ISkillCardUI currentCardUI;

        public SkillCardSlotPosition GetSlotPosition() => position;
        public SlotOwner GetOwner() => SlotOwner.ENEMY;
        public ISkillCard GetCard() => currentCard;
        public ISkillCardUI GetCardUI() => currentCardUI;
        public bool HasCard() => currentCard != null;

        public void SetCard(ISkillCard card)
        {
            currentCard = card;
            currentCard?.SetHandSlot(position);
        }

        public void SetCardUI(ISkillCardUI ui)
        {
            currentCardUI = ui;

            // 적 카드에도 애니메이션 자동 실행
            if (ui is MonoBehaviour uiMb)
            {
                var animator = uiMb.GetComponent<SkillCardSpawnAnimator>();
                animator?.PlaySpawnAnimation();
            }
        }

        public void Clear()
        {
            currentCard = null;
            currentCardUI = null;
        }

        public SkillCardUI AttachCard(ISkillCard card)
        {
            SetCard(card);
            return null; // 적은 카드 UI를 생성하지 않음
        }

        public SkillCardUI AttachCard(ISkillCard card, SkillCardUI prefab)
        {
            // IHandCardSlot 인터페이스 충족을 위해 구현 필요
            SetCard(card);
            return null; // 적은 카드 UI를 생성하지 않음
        }

        public void DetachCard()
        {
            Clear();
        }
    }
}
