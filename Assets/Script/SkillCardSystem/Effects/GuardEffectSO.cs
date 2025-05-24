using UnityEngine;
using Game.CombatSystem.Interface;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.Effects;

namespace Game.SkillCardSystem.Effect
{
    [CreateAssetMenu(fileName = "GuardEffect", menuName = "SkillEffects/GuardEffect")]
    public class GuardEffectSO : SkillCardEffectSO
    {
        public override void ApplyEffect(ICardExecutionContext context, int power, ITurnStateController controller)
        {
            if (context.Target == null)
            {
                Debug.LogWarning("[GuardEffectSO] 대상이 null입니다.");
                return;
            }

            context.Target.SetGuarded(true); // power를 사용하지 않더라도 매개변수는 필요함
            Debug.Log($"[GuardEffectSO] {context.Target.GetCharacterName()} 가드 상태 적용됨");
        }
    }
}
