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
    /// �� �ڵ忡 �ִ� ī�� ���� UI�Դϴ�.
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

                // 임시 부모 변경 (월드 위치 유지)
                uiMb.transform.SetParent(thisSlotRect.parent, true);

                // 1. 이동 애니메이션 (이동이 필요한 경우에만)
                if (shiftAnimator != null)
                    await shiftAnimator.PlayMoveAnimationAsync(thisSlotRect);

                // 2. 애니메이션 후 슬롯에 정착
                uiMb.transform.SetParent(thisSlotRect, false);
                uiMb.transform.localPosition = Vector3.zero;

                // 생성 애니메이션 실행 부분 제거!
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
            return null; // ���� ī�� UI�� �������� ����
        }

        public SkillCardUI AttachCard(ISkillCard card, SkillCardUI prefab)
        {
            // IHandCardSlot �������̽� ������ ���� ���� �ʿ�
            SetCard(card);
            return null; // ���� ī�� UI�� �������� ����
        }

        public void DetachCard()
        {
            Clear();
        }
    }
}
