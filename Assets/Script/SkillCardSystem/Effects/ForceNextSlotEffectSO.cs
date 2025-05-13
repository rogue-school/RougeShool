using UnityEngine;
using Game.CharacterSystem.Core;
using Game.CombatSystem.Core;
using Game.CombatSystem.Interface;
using Game.CombatSystem.Slot;
using Game.SkillCardSystem.Interface;

namespace Game.SkillCardSystem.Effects
{
    [CreateAssetMenu(menuName = "CardEffects/ForceNextSlotEffect")]
    public class ForceNextSlotEffectSO : ScriptableObject, ICardEffect
    {
        public void ExecuteEffect(CharacterBase caster, CharacterBase target, int value)
        {
            if (CombatTurnManager.Instance is ITurnStateController controller)
            {
                controller.ReserveEnemySlot(CombatSlotPosition.FIRST);
            }
        }
    }
}
