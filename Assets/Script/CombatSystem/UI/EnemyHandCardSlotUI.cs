using UnityEngine;
using Game.CombatSystem.Interface;
using Game.CombatSystem.Slot;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.Slot;
using Game.SkillCardSystem.UI;
using AnimationSystem.Animator;

namespace Game.CombatSystem.UI
{
    /// <summary>
    /// 적 핸드에 배치되는 카드 슬롯 UI입니다.
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

        public async void SetCardUI(ISkillCardUI ui)
        {
            currentCardUI = ui;

            if (ui is MonoBehaviour uiMb)
            {
                var shiftAnimator = uiMb.GetComponent<SkillCardShiftAnimator>();
                var thisSlotRect = GetComponent<RectTransform>();

                // 부모 변경 (전역 위치 유지)
                uiMb.transform.SetParent(thisSlotRect.parent, true);

                // 1. 이동 애니메이션 (이동이 필요한 경우에만)
                if (shiftAnimator != null)
                    await shiftAnimator.PlayMoveAnimationAsync(thisSlotRect);

                // 2. 부모 재설정
                uiMb.transform.SetParent(thisSlotRect, false);
                uiMb.transform.localPosition = Vector3.zero;

                // 애니메이션 완료 후 정위치!
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
            return null; // 적 핸드에서는 카드 UI를 생성하지 않음
        }

        public SkillCardUI AttachCard(ISkillCard card, SkillCardUI prefab)
        {
            // IHandCardSlot 인터페이스 규약상 구현만 제공
            SetCard(card);
            return null; // 적 핸드에서는 카드 UI를 생성하지 않음
        }

        public void DetachCard()
        {
            Clear();
        }
    }
}
