using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Game.Cards;
using Game.Interface;
using Game.Slots;

namespace Game.UI
{
    /// <summary>
    /// 카드의 UI를 담당하며, 이름, 파워, 아트 이미지를 표시합니다.
    /// 슬롯 정보는 표시하지 않는 단순화된 버전입니다.
    /// </summary>
    public class SkillCardUI : MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private TextMeshProUGUI cardNameText;
        [SerializeField] private TextMeshProUGUI powerText;
        [SerializeField] private Image cardArtImage;

        private ISkillCard card;

        /// <summary>
        /// 카드 데이터를 받아 UI 텍스트와 이미지를 설정합니다.
        /// </summary>
        public void SetCard(ISkillCard newCard)
        {
            card = newCard;

            Debug.Log($"[SkillCardUI] SetCard 호출됨: {card?.GetCardName()}");

            if (cardNameText != null)
                cardNameText.text = card.GetCardName();

            if (powerText != null)
                powerText.text = $"Power: {card.GetEffectPower(null)}";

            if (cardArtImage != null && card is ScriptableObject so && so is ICardArtProvider provider)
                cardArtImage.sprite = provider.GetArt();
        }

        /// <summary>
        /// 현재 할당된 카드 데이터를 반환합니다.
        /// </summary>
        public ISkillCard GetCard() => card;
    }
}
