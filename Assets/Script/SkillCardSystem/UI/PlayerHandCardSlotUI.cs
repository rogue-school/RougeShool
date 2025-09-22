using UnityEngine;
using Zenject;
using Game.CombatSystem.Manager;
using Game.CombatSystem.Data;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.Slot;
using Game.SkillCardSystem.UI;
using Game.CombatSystem.Utility;
using Game.CombatSystem.DragDrop;
using Game.CoreSystem.Utility;

namespace Game.CombatSystem.UI
{
    /// <summary>
    /// 플레이어 핸드 슬롯 UI.
    /// 이전 시스템의 불필요한 의존성을 제거하고, 플레이어 카드 전용으로 동작합니다.
    /// </summary>
    public class PlayerHandCardSlotUI : MonoBehaviour, Game.CombatSystem.Interface.IHandCardSlot
    {
        [SerializeField] private SkillCardSlotPosition position;

        private ISkillCard currentCard;
        private SkillCardUI currentCardUI;
        private SkillCardUI cardUIPrefab;
        // CombatSlotManager 제거됨 - 슬롯 관리 기능을 CombatFlowManager로 통합

        [Inject]
        public void Construct(SkillCardUI cardUIPrefab)
        {
            this.cardUIPrefab = cardUIPrefab;
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

        /// <summary>
        /// 내부 전용: 카드와 UI를 이 슬롯에 부착합니다.
        /// </summary>
        private void SetCardInternal(ISkillCard card, SkillCardUI prefab)
        {
            if (card == null)
            {
                GameLogger.LogWarning("[PlayerHandCardSlotUI] null 카드는 등록할 수 없습니다.", GameLogger.LogCategory.SkillCard);
                return;
            }

            if (!card.IsFromPlayer())
            {
                GameLogger.LogWarning("[PlayerHandCardSlotUI] 플레이어 슬롯에는 플레이어 카드만 배치할 수 있습니다.", GameLogger.LogCategory.SkillCard);
                return;
            }
            currentCard = card;
            currentCard.SetHandSlot(position);

            if (currentCardUI != null)
                Destroy(currentCardUI.gameObject);

            currentCardUI = SkillCardUIFactory.CreateUI(prefab, transform, card, null);

            if (currentCardUI != null)
            {
                // 정확한 부모로 강제 부착(레이아웃/정렬 용이)
                CardSlotHelper.AttachCardToSlot(currentCardUI, this);

                // 생성 애니메이션 건너뛰기 (AnimationSystem 제거로 인해 임시 비활성화)
                GameLogger.LogInfo($"카드 생성 애니메이션을 건너뜁니다: {card.GetCardName()}", GameLogger.LogCategory.SkillCard);

                // 복귀 기준 위치 명시
                var dragHandler = currentCardUI.GetComponent<CardDragHandler>();
                if (dragHandler != null)
                {
                    dragHandler.OriginalParent = this.transform;
                    dragHandler.OriginalWorldPosition = this.transform.position;
                }
            }
            else
            {
                GameLogger.LogError("[PlayerHandCardSlotUI] 카드 UI 생성 실패", GameLogger.LogCategory.SkillCard);
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


        private void OnValidate()
        {
            // 플레이어 슬롯 전용 유효성 강제: 잘못된 값이 설정되면 PLAYER_SLOT_1로 보정
            if (position != SkillCardSlotPosition.PLAYER_SLOT_1 &&
                position != SkillCardSlotPosition.PLAYER_SLOT_2 &&
                position != SkillCardSlotPosition.PLAYER_SLOT_3)
            {
                position = SkillCardSlotPosition.PLAYER_SLOT_1;
            }
        }
    }
}
