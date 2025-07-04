using UnityEngine;
using Zenject;
using Game.CombatSystem.Interface;
using Game.CombatSystem.Slot;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.Slot;
using Game.SkillCardSystem.UI;
using Game.CombatSystem.Utility;
using Game.CombatSystem.DragDrop;
using Game.CombatSystem.Animation;

namespace Game.CombatSystem.UI
{
    public class PlayerHandCardSlotUI : MonoBehaviour, IHandCardSlot
    {
        [SerializeField] private SkillCardSlotPosition position;

        private ISkillCard currentCard;
        private SkillCardUI currentCardUI;
        private SkillCardUI cardUIPrefab;
        private ICombatFlowCoordinator flowCoordinator;

        [Inject]
        public void Construct(SkillCardUI cardUIPrefab, ICombatFlowCoordinator flowCoordinator)
        {
            this.cardUIPrefab = cardUIPrefab;
            this.flowCoordinator = flowCoordinator;
        }

        public SkillCardSlotPosition GetSlotPosition() => position;
        public SlotOwner GetOwner() => SlotOwner.PLAYER;
        public ISkillCard GetCard() => currentCard;
        public ISkillCardUI GetCardUI() => currentCardUI;
        public bool HasCard() => currentCard != null;

        public void SetCard(ISkillCard card)
        {
            SetCardInternal(card, cardUIPrefab);
        }

        public SkillCardUI AttachCard(ISkillCard card)
        {
            SetCard(card);
            return currentCardUI;
        }

        public SkillCardUI AttachCard(ISkillCard card, SkillCardUI prefab)
        {
            SetCardInternal(card, prefab);
            return currentCardUI;
        }

        private void SetCardInternal(ISkillCard card, SkillCardUI prefab)
        {
            currentCard = card;
            currentCard.SetHandSlot(position);

            if (currentCardUI != null)
                Destroy(currentCardUI.gameObject);

            currentCardUI = SkillCardUIFactory.CreateUI(prefab, transform, card, flowCoordinator);

            if (currentCardUI != null)
            {
                // 정확한 부모로 강제 부착
                CardSlotHelper.AttachCardToSlot(currentCardUI, this);

                // 생성 애니메이션 실행 (존재 시)
                currentCardUI.GetComponent<SkillCardSpawnAnimator>()?.PlaySpawnAnimation();

                // 복귀 기준 위치 명시
                var dragHandler = currentCardUI.GetComponent<CardDragHandler>();
                if (dragHandler != null)
                {
                    dragHandler.OriginalParent = this.transform;
                    dragHandler.OriginalWorldPosition = this.transform.position;
                    Debug.Log($"[SetCardInternal] 강제 저장된 OriginalParent: {dragHandler.OriginalParent.name}");
                }

                Debug.Log($"[PlayerHandCardSlotUI] 카드 UI 생성 완료: {currentCardUI.name}");
            }
            else
            {
                Debug.LogError("[PlayerHandCardSlotUI] 카드 UI 생성 실패");
            }
        }


        public void DetachCard()
        {
            Clear();
        }

        public void Clear()
        {
            currentCard = null;

            if (currentCardUI != null)
            {
                Destroy(currentCardUI.gameObject);
                currentCardUI = null;
            }
        }

        public void SetInteractable(bool interactable)
        {
            if (currentCardUI != null)
                currentCardUI.SetInteractable(interactable);
        }

        public void SetCoolTimeDisplay(int coolTime, bool isOnCooldown)
        {
            currentCardUI?.ShowCoolTime(coolTime, isOnCooldown);
        }
    }
}
