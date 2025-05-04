using UnityEngine;
using UnityEngine.UI;
using Game.Cards;
using Game.Interface;
using Game.Characters;

namespace Game.Battle
{
    public class BattleCardSlotUI : MonoBehaviour, ICardSlot
    {
        public Image cardImage;
        private ISkillCard currentCard;

        public void SetCard(ISkillCard card)
        {
            currentCard = card;
            cardImage.enabled = true;
            // cardImage.sprite = card.GetArtwork();
        }

        public ISkillCard GetCard() => currentCard;
        public bool HasCard() => currentCard != null;

        public void Clear()
        {
            currentCard = null;
            cardImage.sprite = null;
            cardImage.enabled = false;
        }

        public void ExecuteEffect(CharacterBase caster, CharacterBase target)
        {
            if (currentCard == null) return;

            foreach (var effect in currentCard.CreateEffects())
            {
                effect?.ExecuteEffect(caster, target);
            }
        }
    }
}
