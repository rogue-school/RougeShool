using UnityEngine;
using Game.CharacterSystem.Core;
using Game.SkillCardSystem.Interface;

namespace Game.SkillCardSystem.Effects
{
    [CreateAssetMenu(menuName = "CardEffects/SlashEffect")]
    public class SlashEffectSO : ScriptableObject, ICardEffect
    {
        public void ExecuteEffect(CharacterBase caster, CharacterBase target, int value)
        {
            target.TakeDamage(value);
        }
    }
}
