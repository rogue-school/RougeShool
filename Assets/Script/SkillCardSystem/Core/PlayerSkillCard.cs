using UnityEngine;
using Game.SkillCardSystem.Interface;
using System.Collections.Generic;

namespace Game.SkillCardSystem.Data
{
    [CreateAssetMenu(menuName = "Game/SkillCard/Player Skill Card")]
    public class PlayerSkillCard : ScriptableObject
    {
        [Header("카드 데이터")]
        public SkillCardData CardData;
        public int GetCoolTime() => CardData.CoolTime;

        [Header("카드 효과 목록")]
        [SerializeField] private List<ScriptableObject> effects;

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
    }
}
