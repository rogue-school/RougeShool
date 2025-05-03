using UnityEngine;
using UnityEngine.UI;
using Game.Cards;
using Game.Interface;

namespace Game.UI
{
    /// <summary>
    /// 플레이어 카드 슬롯 UI를 제어합니다.
    /// </summary>
    public class PlayerCardSlotUI : MonoBehaviour
    {
        [SerializeField] private Image cardImage;
        private ISkillCard currentCard;

        public void SetCard(ISkillCard card)
        {
            currentCard = card;

            if (cardImage != null)
            {
                cardImage.enabled = true;
                // cardImage.sprite = card.GetArtwork(); ← 실제 이미지 설정이 필요하다면 사용
            }
        }

        /// <summary>
        /// 카드 슬롯을 비웁니다.
        /// </summary>
        public void ClearSlot()
        {
            currentCard = null;

            if (cardImage != null)
            {
                cardImage.sprite = null;
                cardImage.enabled = false;
            }
        }
    }
}
