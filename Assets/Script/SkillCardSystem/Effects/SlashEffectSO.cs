using UnityEngine;
using Game.CharacterSystem.Core;
using Game.SkillCardSystem.Interface;
using Game.CombatSystem.Interface;

namespace Game.SkillCardSystem.Effects
{
    [CreateAssetMenu(menuName = "Game/CardEffects/SlashEffect")]
    public class SlashEffectSO : ScriptableObject, ICardEffect
    {
        [SerializeField] private int baseDamage = 5;

        public void ExecuteEffect(CharacterBase caster, CharacterBase target, int value, ITurnStateController controller = null)
        {
            target.TakeDamage(value);
        }

        public string GetEffectName()
        {
            return "Slash";
        }

        public string GetDescription()
        {
            return $"대미지: {baseDamage} 피해";
        }
    }
}
