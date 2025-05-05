using UnityEngine;
using Game.Characters;
using Game.Effect;

namespace Game.Effect
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
