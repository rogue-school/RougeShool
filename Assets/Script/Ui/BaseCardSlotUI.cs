using UnityEngine;
using UnityEngine.UI;

namespace Game.Cards
{
    public class BaseCardSlotUI : MonoBehaviour, ICardSlot
    {
        public Image cardImage;
        protected PlayerCardData currentCard;

        public virtual void SetCard(PlayerCardData card)
        {
            currentCard = card;
            if (cardImage != null)
            {
                cardImage.enabled = true;
            }
        }

        public virtual void Clear()
        {
            currentCard = null;
            if (cardImage != null)
            {
                cardImage.sprite = null;
                cardImage.enabled = false;
            }
        }

        public virtual PlayerCardData GetCard() => currentCard;
        public virtual bool HasCard() => currentCard != null;
    }
}
