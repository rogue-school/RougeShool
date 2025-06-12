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

        public async void SetCardUI(ISkillCardUI ui)
        {
            currentCardUI = ui;

            if (ui is MonoBehaviour uiMb)
            {
                var spawnAnimator = uiMb.GetComponent<SkillCardSpawnAnimator>();
                var shiftAnimator = uiMb.GetComponent<SkillCardShiftAnimator>();
                var thisSlotRect = GetComponent<RectTransform>();

                // 현재 부모 위치를 기준으로 카드가 어디 있는지 파악
                Vector3 currentWorldPos = uiMb.transform.position;

                // 임시 부모 설정 (월드 기준 위치 유지)
                uiMb.transform.SetParent(thisSlotRect.parent, true);

                // 1. 이동 애니메이션 (현재 위치 → 이 슬롯 위치)
                if (shiftAnimator != null)
                    await shiftAnimator.PlayMoveAnimationAsync(thisSlotRect);

                // 2. 애니메이션 완료 후 이 슬롯에 부착
                uiMb.transform.SetParent(thisSlotRect, false);
                uiMb.transform.localPosition = Vector3.zero;

                // 3. 생성 애니메이션
                if (spawnAnimator != null)
                    await spawnAnimator.PlaySpawnAnimationAsync();
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
