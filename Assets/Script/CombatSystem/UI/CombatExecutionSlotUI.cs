using UnityEngine;
using Game.CombatSystem.Interface;
using Game.CombatSystem.Slot;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.UI;

namespace Game.CombatSystem.UI
{
    public class CombatExecutionSlotUI : MonoBehaviour, ICombatCardSlot
    {
        [SerializeField] private CombatSlotPosition position;

        private ISkillCard currentCard;
        private SkillCardUI currentCardUI;
        private ICardExecutionContext context;

        public void Inject(ICardExecutionContext executionContext) => this.context = executionContext;

        public CombatSlotPosition GetCombatPosition() => position;

        public SlotOwner GetOwner() =>
            position == CombatSlotPosition.FIRST ? SlotOwner.ENEMY : SlotOwner.PLAYER;

        public ISkillCard GetCard() => currentCard;

        public void SetCard(ISkillCard card)
        {
            currentCard = card;
            currentCard?.SetCombatSlot(position);
        }

        public SkillCardUI GetCardUI() => currentCardUI;

        public void SetCardUI(SkillCardUI cardUI) => currentCardUI = cardUI;

        public void Clear()
        {
            currentCard = null;
            if (currentCardUI != null)
            {
                Destroy(currentCardUI.gameObject);
                currentCardUI = null;
            }

            Debug.Log($"[CombatExecutionSlotUI] 슬롯 클리어 완료: {gameObject.name}");
        }

        public bool HasCard() => currentCard != null;

        public bool IsEmpty() => !HasCard();

        public void ExecuteCardAutomatically()
        {
            if (currentCard == null)
            {
                Debug.LogWarning("[CombatExecutionSlotUI] currentCard가 null입니다.");
                return;
            }

            if (context == null)
            {
                Debug.LogError("[CombatExecutionSlotUI] ICardExecutionContext가 주입되지 않았습니다.");
                return;
            }

            currentCard.ExecuteCardAutomatically(context);
        }

        public void ExecuteCardAutomatically(ICardExecutionContext ctx)
        {
            if (currentCard == null)
            {
                Debug.LogWarning("[CombatExecutionSlotUI] currentCard가 null입니다.");
                return;
            }

            currentCard.ExecuteCardAutomatically(ctx);
        }
    }
}
