using UnityEngine;
using Game.CombatSystem.Interface;
using Game.SkillCardSystem.Interface;

namespace Game.SkillCardSystem.Effect
{
    /// <summary>
    /// 대상 캐릭터에게 가드 상태를 적용하는 커맨드 클래스입니다.
    /// 카드 효과 실행 시 사용됩니다.
    /// </summary>
    public class GuardEffectCommand : ICardEffectCommand
    {
        /// <summary>
        /// 커맨드를 실행하여 대상 캐릭터에게 가드 상태를 부여합니다.
        /// </summary>
        /// <param name="context">카드 실행 컨텍스트 (대상 포함)</param>
        /// <param name="turnManager">전투 턴 매니저 (사용되지 않음)</param>
        public void Execute(ICardExecutionContext context, ICombatTurnManager turnManager)
        {
            if (context?.Target == null)
            {
                Debug.LogWarning("[GuardEffectCommand] 유효하지 않은 대상 (null)");
                return;
            }

            context.Target.SetGuarded(true);
            Debug.Log($"[GuardEffectCommand] {context.Target.GetCharacterName()}에게 가드 상태 적용됨");
        }
    }
}
