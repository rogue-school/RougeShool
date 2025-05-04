using UnityEngine;
using Game.Characters;
using Game.Effect;

namespace Game.Effect
{
    [CreateAssetMenu(menuName = "Game Assets/Effects/Slash Effect")]
    public class SlashEffectSO : ScriptableObject, ICardEffect
    {
        public void ExecuteEffect(CharacterBase caster, CharacterBase target, int value)
        {
            target.TakeDamage(value);
        }
    }
}
