using UnityEngine;
using Game.Battle;
using Game.Characters;
using Game.Effect;
using Game.Managers;
using Game.Interface;

namespace Game.Cards
{
    [CreateAssetMenu(menuName = "CardEffects/ForceNextSlotEffect")]
    public class ForceNextSlotEffectSO : ScriptableObject, ICardEffect
    {
        public void ExecuteEffect(CharacterBase caster, CharacterBase target, int value)
        {
            if (BattleTurnManager.Instance is ITurnStateController controller)
            {
                controller.ReserveEnemySlot(BattleSlotPosition.FIRST);
            }
        }
    }
}
