using UnityEngine;
using System.Collections.Generic;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.Data;

namespace Game.SkillCardSystem.Core
{
    [CreateAssetMenu(menuName = "Game/SkillCard/Enemy Skill Card")]
    public class EnemySkillCard : ScriptableObject
    {
        public SkillCardData CardData;

        [Header("카드 실행 시 적용할 효과 목록")]
        [SerializeField] private List<ScriptableObject> effects = new();

        public List<ICardEffect> CreateEffects()
        {
            var list = new List<ICardEffect>();
            foreach (var obj in effects)
            {
                if (obj is ICardEffect effect)
                    list.Add(effect);
            }
            return list;
        }

        public string GetCardName() => CardData.Name;
        public int GetDamage() => CardData.Damage;
        public int GetCoolTime() => CardData.CoolTime;
    }
}
