using UnityEngine;
using Game.CombatSystem.Interface;
using Game.CombatSystem.Slot;
using Game.SkillCardSystem.Interface;

namespace Game.CombatSystem.UI
{
    public class CombatExecutionSlotUI : MonoBehaviour, ICombatCardSlot
    {
        [SerializeField]
        private CombatSlotPosition position;

        private ISkillCard currentCard;
        private ISkillCardUI currentCardUI;
        private ICardExecutionContext context;

        public CombatSlotPosition Position => position;

        public void Inject(ICardExecutionContext executionContext)
        {
            this.context = executionContext;
        }

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

        public ISkillCardUI GetCardUI() => currentCardUI;

        public void SetCardUI(ISkillCardUI cardUI)
        {
            currentCardUI = cardUI;

            if (cardUI is MonoBehaviour uiMb)
            {
                uiMb.transform.SetParent(this.transform);
                uiMb.transform.localPosition = Vector3.zero;
                uiMb.transform.localScale = Vector3.one;
            }
        }

        public void Clear()
        {
            currentCard = null;

            if (currentCardUI is MonoBehaviour uiMb)
            {
                Destroy(uiMb.gameObject);
            }

            currentCardUI = null;

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
