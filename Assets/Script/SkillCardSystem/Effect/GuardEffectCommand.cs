using UnityEngine;
using Game.CombatSystem.Interface;
using Game.SkillCardSystem.Interface;

namespace Game.SkillCardSystem.Effect
{
    /// <summary>
    /// 가드 효과를 적용하는 커맨드 클래스입니다.
    /// 다음 슬롯의 적 스킬카드를 무효화시킵니다.
    /// </summary>
    public class GuardEffectCommand : ICardEffectCommand
    {
        /// <summary>
        /// 가드 효과를 실행합니다.
        /// 다음 슬롯의 적 스킬카드를 무효화시킵니다.
        /// </summary>
        /// <param name="context">카드 실행 컨텍스트</param>
        /// <param name="turnManager">전투 턴 매니저</param>
        public void Execute(ICardExecutionContext context, ICombatTurnManager turnManager)
        {
            if (context?.Source == null)
            {
                Debug.LogWarning("[GuardEffectCommand] 소스가 null입니다.");
                return;
            }

            // 가드 효과는 턴 매니저를 통해 처리
            if (turnManager != null)
            {
                turnManager.ApplyGuardEffect();
                Debug.Log($"[GuardEffectCommand] 가드 효과 적용됨 - 다음 슬롯의 적 스킬카드 무효화");
            }
            else
            {
                Debug.LogWarning("[GuardEffectCommand] TurnManager가 null입니다.");
            }
        }

        /// <summary>
        /// 가드 효과 실행 가능 여부를 확인합니다.
        /// </summary>
        /// <param name="context">카드 실행 컨텍스트</param>
        /// <returns>실행 가능 여부</returns>
        public bool CanExecute(ICardExecutionContext context)
        {
            return context?.Source != null;
        }

        /// <summary>
        /// 가드 효과의 비용을 반환합니다.
        /// </summary>
        /// <returns>비용 (가드 효과는 비용 없음)</returns>
        public int GetCost()
        {
            return 0;
        }
    }
}
