using UnityEngine;
using Game.Battle;
using Game.Characters;
using Game.Effect;
using Game.Managers;

namespace Game.Cards
{
    [CreateAssetMenu(menuName = "CardEffects/GuardEffect")]
    public class GuardEffectSO : ScriptableObject, ICardEffect
    {
        public void ExecuteEffect(CharacterBase caster, CharacterBase target, int value)
        {
            if (BattleTurnManager.Instance is ITurnStateController controller)
            {
                controller.RegisterPlayerGuard();
            }
        }
    }
}
