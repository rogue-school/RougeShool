using UnityEngine;
using Game.Characters;
using Game.Effect;
using Game.Managers;
using Game.Interface;

namespace Game.Cards
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
