using UnityEngine;
using Game.CharacterSystem.Core;
using Game.CombatSystem.Interface;
using Game.CombatSystem.Slot;
using Game.SkillCardSystem.Interface;

namespace Game.SkillCardSystem.Effects
{
    [CreateAssetMenu(menuName = "Game/CardEffects/ForceNextSlotEffect")]
    public class ForceNextSlotEffectSO : ScriptableObject, ICardEffect
    {
        public void ExecuteEffect(CharacterBase caster, CharacterBase target, int value, ITurnStateController controller = null)
        {
            if (controller != null)
            {
                controller.ReserveEnemySlot(CombatSlotPosition.FIRST);
                Debug.Log("[ForceNextSlotEffectSO] 다음 턴 적 슬롯을 FIRST로 예약");
            }
            else
            {
                Debug.LogWarning("[ForceNextSlotEffectSO] controller가 null입니다.");
            }
        }

        public string GetEffectName()
        {
            return "Force Slot";
        }

        public string GetDescription()
        {
            return "다음 턴 적 슬롯을 FIRST(선공)로 고정합니다.";
        }
    }
}
