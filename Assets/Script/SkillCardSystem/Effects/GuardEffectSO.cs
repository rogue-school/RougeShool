using UnityEngine;
using Game.CharacterSystem.Core;
using Game.CombatSystem.Core;
using Game.CombatSystem.Interface;
using Game.SkillCardSystem.Interface;

namespace Game.SkillCardSystem.Effects
{
    [CreateAssetMenu(menuName = "CardEffects/GuardEffect")]
    public class GuardEffectSO : ScriptableObject, ICardEffect
    {
        public void ExecuteEffect(CharacterBase caster, CharacterBase target, int value)
        {
            if (CombatTurnManager.Instance is ITurnStateController controller)
            {
                controller.RegisterPlayerGuard();
            }
        }
    }
}
