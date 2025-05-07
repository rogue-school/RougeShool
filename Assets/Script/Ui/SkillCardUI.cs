using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Game.Cards;
using Game.Interface;
using Game.Slots;

namespace Game.UI
{
    /// <summary>
    /// 카드의 UI를 담당하며, 슬롯 정보를 통해 시각적 정보를 표시합니다.
    /// </summary>
    public class SkillCardUI : MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private TextMeshProUGUI cardNameText;
        [SerializeField] private TextMeshProUGUI powerText;
        [SerializeField] private TextMeshProUGUI slotInfoText;
        [SerializeField] private Image cardArtImage;

        private ISkillCard card;

        /// <summary>
        /// 외부에서 카드 연결 시 호출
        /// </summary>
        public void SetCard(ISkillCard newCard)
        {
            card = newCard;

            if (cardNameText != null)
                cardNameText.text = card.GetCardName();

            if (powerText != null)
                powerText.text = $"Power: {card.GetEffectPower(null)}";

            if (cardArtImage != null && card is ScriptableObject so && so is ICardArtProvider provider)
                cardArtImage.sprite = provider.GetArt();

            UpdateSlotInfoUI();
        }

        /// <summary>
        /// 슬롯 정보를 UI에 표시
        /// </summary>
        private void UpdateSlotInfoUI()
        {
            string slotInfo = "";

            if (card.GetHandSlot() != null)
                slotInfo = $"[HAND] {card.GetHandSlot().Value}";
            else if (card.GetCombatSlot() != null)
                slotInfo = $"[COMBAT] {card.GetCombatSlot().Value}";
            else
                slotInfo = "(Unassigned Slot)";

            if (slotInfoText != null)
                slotInfoText.text = slotInfo;
        }

        public ISkillCard GetCard() => card;
    }
}
