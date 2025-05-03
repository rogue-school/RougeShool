using UnityEngine;
using Game.Interface;
using Game.Cards;

namespace Game.Enemy
{
    /// <summary>
    /// 적 전용 스킬 카드 데이터입니다.
    /// </summary>
    [CreateAssetMenu(menuName = "Game Assets/Skill Cards/Enemy Skill Card")]
    public class EnemySkillCard : ScriptableObject, ISkillCard
    {
        [SerializeField] private string cardName;
        [SerializeField] private string description;
        [SerializeField] private Sprite artwork;
        [SerializeField] private bool isDebuff;
        [SerializeField] private int damage = 3;

        public string GetName() => cardName;
        public string GetDescription() => description;
        public Sprite GetArtwork() => artwork;
        public bool IsDebuff() => isDebuff;

        public ICardEffect CreateEffect()
        {
            return new SlashEffect(damage);
        }
    }
}
