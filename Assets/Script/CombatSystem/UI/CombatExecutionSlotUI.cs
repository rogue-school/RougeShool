using UnityEngine;
using Game.CombatSystem.Interface;
using Game.CombatSystem.Slot;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.UI;

namespace Game.CombatSystem.UI
{
    /// <summary>
    /// 전투 실행 슬롯 UI의 실제 구현입니다.
    /// 카드와 UI 참조를 저장하고 자동 실행을 지원합니다.
    /// 실행 로직은 외부 컨텍스트(ICardExecutionContext)를 통해 위임합니다.
    /// </summary>
    public class CombatExecutionSlotUI : MonoBehaviour, ICombatCardSlot
    {
        [SerializeField] private CombatSlotPosition position;

        private ISkillCard currentCard;
        private SkillCardUI currentCardUI;
        private ICardExecutionContext context;

        /// <summary>
        /// CombatTurnManager를 의존성 주입으로 설정합니다.
        /// </summary>
        public void Inject(ICardExecutionContext executionContext)
        {
            this.context = executionContext;
        }

        public CombatSlotPosition GetCombatPosition() => position;

        public SlotOwner GetOwner() =>
            position == CombatSlotPosition.FIRST ? SlotOwner.ENEMY : SlotOwner.PLAYER;

        public void SetCard(ISkillCard card)
        {
            currentCard = card;
            currentCard?.SetCombatSlot(position);
        }

        public ISkillCard GetCard() => currentCard;

        public void SetCardUI(SkillCardUI cardUI) => currentCardUI = cardUI;

        public SkillCardUI GetCardUI() => currentCardUI;

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