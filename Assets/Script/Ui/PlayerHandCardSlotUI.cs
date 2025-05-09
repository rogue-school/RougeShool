using UnityEngine;
using Game.Interface;
using Game.Slots;
using Game.Utils;
using Game.UI;

namespace Game.UI.Hand
{
    /// <summary>
    /// 플레이어 핸드에 있는 카드 슬롯 UI입니다.
    /// </summary>
    public class PlayerHandCardSlotUI : MonoBehaviour, IHandCardSlot
    {
        [SerializeField] private SkillCardSlotPosition position;
        [SerializeField] private SkillCardUI skillCardUIPrefab;

        private ISkillCard currentCard;
        private SkillCardUI currentCardUI;

        public SkillCardSlotPosition GetSlotPosition() => position;

        public SlotOwner GetOwner() => SlotOwner.PLAYER;

        /// <summary>
        /// 카드 UI 프리팹을 외부에서 주입받습니다.
        /// </summary>
        public void InjectUIFactory(SkillCardUI prefab)
        {
            skillCardUIPrefab = prefab;
        }

        public void SetCard(ISkillCard card)
        {
            Debug.Log($"[PlayerHandCardSlotUI] SetCard 호출됨: {card?.GetCardName()}");

            currentCard = card;
            currentCard.SetHandSlot(position);

            if (currentCardUI != null)
                Destroy(currentCardUI.gameObject);

            currentCardUI = SkillCardUIFactory.CreateUI(skillCardUIPrefab, transform, card);

            if (currentCardUI != null)
                Debug.Log($"[PlayerHandCardSlotUI] 카드 UI 생성 완료: {currentCardUI.name}");
            else
                Debug.LogError("[PlayerHandCardSlotUI] 카드 UI 생성 실패");
        }


        public ISkillCard GetCard() => currentCard;

        public void Clear()
        {
            currentCard = null;

            if (currentCardUI != null)
            {
                Destroy(currentCardUI.gameObject);
                currentCardUI = null;
            }
        }
    }
}
