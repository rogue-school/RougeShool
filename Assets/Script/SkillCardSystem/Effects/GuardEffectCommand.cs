using UnityEngine;
using Game.CombatSystem.Interface;
using Game.SkillCardSystem.Interface;

namespace Game.SkillCardSystem.Effect
{
    public class GuardEffectCommand : ICardEffectCommand
    {
        public void Execute(ICardExecutionContext context, ITurnStateController controller)
        {
            if (context.Target == null)
            {
                Debug.LogWarning("[GuardEffectCommand] 대상이 null입니다.");
                return;
            }

            context.Target.SetGuarded(true);
            Debug.Log($"[GuardEffectCommand] {context.Target.GetCharacterName()} 가드 상태 적용됨");
        }
    }
}
