using UnityEngine;
using UnityEngine.UI;
using Game.Interface;

namespace Game.UI
{
    /// <summary>
    /// 모든 카드 슬롯 UI의 베이스 클래스입니다.
    /// </summary>
    public class BaseCardSlotUI : MonoBehaviour
    {
        [SerializeField] protected Image cardImage;
        protected ISkillCard currentCard;

        /// <summary>
        /// 슬롯에 카드 설정
        /// </summary>
        public virtual void SetCard(ISkillCard card)
        {
            currentCard = card;

            if (cardImage != null)
            {
                cardImage.enabled = card != null;
                cardImage.sprite = card?.GetArtwork();
            }
        }

        /// <summary>
        /// 슬롯 클리어 (카드 제거 및 UI 초기화)
        /// </summary>
        public virtual void ClearSlot()
        {
            currentCard = null;

            if (cardImage != null)
            {
                cardImage.sprite = null;
                cardImage.enabled = false;
            }
        }

        public ISkillCard GetCard()
        {
            return currentCard;
        }
    }
}
