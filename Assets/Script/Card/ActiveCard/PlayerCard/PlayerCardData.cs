using UnityEngine;
using Game.Effects;

namespace Game.Cards
{
    public enum CardType { Attack, Defense }

    [CreateAssetMenu(menuName = "Card System/Player Card")]
    public class PlayerCardData : ScriptableObject
    {
        public string cardName;
        public string description;
        public int damage;
        public CardType type;
        public int cooldown;

        public Sprite artwork;


        public ICardEffect CreateEffect()
        {
            return type switch
            {
                CardType.Attack => new SlashEffect(damage),
                CardType.Defense => new BlockEffect(damage),
                _ => null
            };
        }
    }
}
