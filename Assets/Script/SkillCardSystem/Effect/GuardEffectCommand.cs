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
        /// 커맨드를 실행하여 카드를 사용한 플레이어 자신에게 가드 상태를 부여합니다.
        /// </summary>
        /// <param name="context">카드 실행 컨텍스트 (소스 포함)</param>
        /// <param name="turnManager">전투 턴 매니저 (사용되지 않음)</param>
        public void Execute(ICardExecutionContext context, ICombatTurnManager turnManager)
        {
            if (context?.Source == null)
            {
                Debug.LogWarning("[GuardEffectCommand] 유효하지 않은 소스(카드 사용자) (null)");
                return;
            }

            // 방어 카드는 카드를 사용한 플레이어 자신에게 가드 상태를 부여
            context.Source.SetGuarded(true);
            Debug.Log($"[GuardEffectCommand] {context.Source.GetCharacterName()}에게 가드 상태 적용됨");
        }
    }
}
