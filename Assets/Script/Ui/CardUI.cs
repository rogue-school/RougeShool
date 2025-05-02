using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Game.Cards;

namespace Game.UI
{
    /// <summary>
    /// 카드 데이터를 UI에 표시하고 접근할 수 있도록 하는 클래스
    /// </summary>
    public class CardUI : MonoBehaviour
    {
        [Header("UI Components")]
        public TMP_Text cardNameText;
        public TMP_Text descriptionText;
        public Image artworkImage;

        private PlayerCardData cardData;

        /// <summary>
        /// 카드 설정 및 UI 초기화
        /// </summary>
        public void SetCard(PlayerCardData data)
        {
            cardData = data;

            if (cardNameText != null)
                cardNameText.text = data.cardName;

            if (descriptionText != null)
                descriptionText.text = data.description;

            if (artworkImage != null && data.artwork != null)
                artworkImage.sprite = data.artwork;
        }

        /// <summary>
        /// 외부에서 카드 데이터를 가져올 수 있도록 Getter 제공
        /// </summary>
        public PlayerCardData CardData => cardData;
    }
}
