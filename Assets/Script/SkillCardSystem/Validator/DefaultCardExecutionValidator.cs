using Game.SkillCardSystem.Interface;
using Game.CombatSystem.Interface;
using UnityEngine;

namespace Game.SkillCardSystem.Validator
{
    /// <summary>
    /// 스킬 카드 실행 전 유효성 검사 클래스
    /// </summary>
    public class DefaultCardExecutionValidator : ICardExecutionValidator
    {
        public bool CanExecute(ISkillCard card, ICardExecutionContext context)
        {
            if (card == null || context == null)
            {
                Debug.LogWarning("[CardValidator] 카드나 context가 null입니다.");
                return false;
            }

            if (context.Target == null || context.Target.IsDead())
            {
                Debug.LogWarning("[CardValidator] 대상이 null이거나 사망 상태입니다.");
                return false;
            }

            // 쿨타임이 1 이상이면 실행 불가
            if (card.GetCurrentCoolTime() > 0)
            {
                Debug.LogWarning($"[CardValidator] '{card.GetCardName()}' 쿨타임 미완료: 남은 쿨타임 = {card.GetCurrentCoolTime()}");
                return false;
            }

            return true;
        }
    }
}
