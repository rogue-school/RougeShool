using UnityEngine;
using UnityEngine.UI;
using Game.Cards;
using Game.Units;

namespace Game.Battle
{
    public class CombatSlotUI : MonoBehaviour, ICardSlot
    {
        public Image cardImage;
        private PlayerCardData currentCard;

        public void SetCard(PlayerCardData card)
        {
            currentCard = card;
            cardImage.enabled = true;
        }

        public void Clear()
        {
            currentCard = null;
            cardImage.sprite = null;
            cardImage.enabled = false;
        }

        public bool HasCard() => currentCard != null;
        public PlayerCardData GetCard() => currentCard;

        public void ExecuteEffect(Unit caster, Unit target)
        {
            var effect = currentCard?.CreateEffect();
            effect?.ExecuteEffect(caster, target);
        }
    }
}

