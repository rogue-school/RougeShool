using Game.SkillCardSystem.Interface;
using Game.CombatSystem.Interface;
using UnityEngine;

namespace Game.SkillCardSystem.Validator
{
    /// <summary>
    /// 기본 스킬 카드 실행 유효성 검사기입니다.
    /// - 카드가 존재하는지
    /// - 실행 컨텍스트가 유효한지
    /// - 대상이 생존해 있는지
    /// - 쿨타임이 완료되었는지 확인합니다.
    /// </summary>
    public class DefaultCardExecutionValidator : ICardExecutionValidator
    {
        /// <summary>
        /// 카드가 현재 실행 가능한 상태인지 검사합니다.
        /// </summary>
        /// <param name="card">검사할 스킬 카드</param>
        /// <param name="context">카드 실행 컨텍스트 (소스 및 타겟 포함)</param>
        /// <returns>실행 가능 여부</returns>
        public bool CanExecute(ISkillCard card, ICardExecutionContext context)
        {
            // === Null 체크 ===
            if (card == null)
            {
                Debug.LogWarning("[CardValidator] 카드가 null입니다.");
                return false;
            }

            if (context == null)
            {
                Debug.LogWarning("[CardValidator] 컨텍스트가 null입니다.");
                return false;
            }

            // === 대상 유효성 확인 ===
            if (context.Target == null)
            {
                Debug.LogWarning("[CardValidator] 대상이 null입니다.");
                return false;
            }

            if (context.Target.IsDead())
            {
                Debug.LogWarning($"[CardValidator] 대상 {context.Target.GetCharacterName()}은 사망 상태입니다.");
                return false;
            }

            // === 쿨타임 검사 ===
            int currentCoolTime = card.GetCurrentCoolTime();
            if (currentCoolTime > 0)
            {
                Debug.LogWarning($"[CardValidator] '{card.GetCardName()}'는 쿨타임 중입니다. (남은 턴: {currentCoolTime})");
                return false;
            }

            return true;
        }
    }
}
