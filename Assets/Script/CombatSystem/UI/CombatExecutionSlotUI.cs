using UnityEngine;
using Game.CombatSystem.Interface;
using Game.CombatSystem.Slot;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.UI;

namespace Game.CombatSystem.UI
{
    /// <summary>
    /// 전투 실행 슬롯 UI 클래스 (카드 1장과 그 UI를 보관 및 실행)
    /// </summary>
    public class CombatExecutionSlotUI : MonoBehaviour, ICombatCardSlot
    {
        [SerializeField]
        private CombatSlotPosition position;

        private ISkillCard currentCard;
        private SkillCardUI currentCardUI;
        private ICardExecutionContext context;

        /// <summary>
        /// 외부에서 카드 실행 컨텍스트를 주입
        /// </summary>
        public void Inject(ICardExecutionContext executionContext)
        {
            this.context = executionContext;
        }

        /// <summary>
        /// 현재 슬롯의 전투 필드 내 위치 (왼쪽/오른쪽)
        /// </summary>
        public CombatFieldSlotPosition GetCombatPosition()
        {
            return position switch
            {
                CombatSlotPosition.FIRST => CombatFieldSlotPosition.FIELD_LEFT,
                CombatSlotPosition.SECOND => CombatFieldSlotPosition.FIELD_RIGHT,
                _ => CombatFieldSlotPosition.NONE
            };
        }

        public ISkillCard GetCard() => currentCard;

        public void SetCard(ISkillCard card)
        {
            currentCard = card;
            currentCard?.SetCombatSlot(position);
        }

        public SkillCardUI GetCardUI() => currentCardUI;

        public void SetCardUI(SkillCardUI cardUI)
        {
            currentCardUI = cardUI;
        }

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
                Debug.LogWarning("[CombatExecutionSlotUI] 실행 불가: 카드 없음");
                return;
            }

            if (context == null)
            {
                Debug.LogError("[CombatExecutionSlotUI] 실행 불가: 컨텍스트 미지정");
                return;
            }

            currentCard.ExecuteCardAutomatically(context);
        }

        public void ExecuteCardAutomatically(ICardExecutionContext ctx)
        {
            if (currentCard == null)
            {
                Debug.LogWarning("[CombatExecutionSlotUI] 실행 불가: 카드 없음");
                return;
            }

            currentCard.ExecuteCardAutomatically(ctx);
        }
    }
}
