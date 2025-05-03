using UnityEngine;
using Game.Interface;
using Game.Cards;
using Game.Effects;

namespace Game.Player
{
    /// <summary>
    /// 플레이어 전용 스킬 카드 데이터입니다.
    /// </summary>
    [CreateAssetMenu(menuName = "Card/PlayerSkillCard")]
    public class PlayerSkillCard : ScriptableObject, ISkillCard
    {
        [Header("기본 정보")]
        [SerializeField] private string cardName;
        [SerializeField] private string description;
        [SerializeField] private Sprite artwork;
        [SerializeField] private CardType type;
        [SerializeField] private int coolTime = 0;

        [Header("카드 효과 수치")]
        [SerializeField] private int attackPower = 5;   // SlashEffect용
        [SerializeField] private int blockValue = 3;    // BlockEffect용

        public string GetName() => cardName;
        public string GetDescription() => description;
        public Sprite GetArtwork() => artwork;
        public int GetCoolTime() => coolTime;
        public CardType GetCardType() => type;

        public ICardEffect CreateEffect()
        {
            return type switch
            {
                CardType.Attack => new SlashEffect(attackPower),
                CardType.Defense => new BlockEffect(blockValue),
                _ => null
            };
        }
    }
}
