using UnityEngine;
using System.Collections.Generic;
using Game.Interface;
using Game.Effect;
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
        [SerializeField] private int damage;
        [SerializeField] private bool isDebuff;

        public string GetName() => cardName;
        public string GetDescription() => description;
        public Sprite GetArtwork() => artwork;
        public bool IsDebuff() => isDebuff;

        public List<ICardEffect> CreateEffects()
        {
            // 단일 효과지만 리스트로 감싸서 반환 (확장 가능성 고려)
            return new List<ICardEffect> { new SlashEffect(damage) };
        }
    }
}
