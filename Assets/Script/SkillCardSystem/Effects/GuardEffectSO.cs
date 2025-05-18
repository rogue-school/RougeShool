using UnityEngine;
using Game.CharacterSystem.Core;
using Game.CombatSystem.Interface;
using Game.SkillCardSystem.Interface;

namespace Game.SkillCardSystem.Effects
{
    [CreateAssetMenu(menuName = "CardEffects/GuardEffect")]
    public class GuardEffectSO : ScriptableObject, ICardEffect
    {
        public void ExecuteEffect(CharacterBase caster, CharacterBase target, int value, ITurnStateController controller = null)
        {
            if (controller != null)
            {
                controller.RegisterPlayerGuard();
                Debug.Log("[GuardEffectSO] 플레이어 방어 상태 등록됨");
            }
            else
            {
                Debug.LogWarning("[GuardEffectSO] controller가 null이므로 Guard 등록 실패");
            }
        }

        public string GetEffectName()
        {
            return "Guard";
        }

        public string GetDescription()
        {
            return "이번 턴 동안 플레이어가 방어 상태가 됩니다.";
        }
    }
}
