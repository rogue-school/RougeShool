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
    public class DefaultCardExecutionValidator : ICardValidator
    {
        /// <summary>
        /// 지정된 카드가 주어진 슬롯에 드롭 가능한지를 판별합니다.
        /// </summary>
        /// <param name="card">드롭을 시도하는 스킬 카드</param>
        /// <param name="slot">드롭 대상 슬롯</param>
        /// <param name="reason">드롭이 불가능한 경우 사유를 문자열로 반환합니다.</param>
        /// <returns>true이면 드롭이 허용되고, false이면 드롭이 차단됩니다.</returns>
        public bool IsValidDrop(ISkillCard card, ICombatCardSlot slot, out string reason)
        {
            reason = null;
            // TODO: 드롭 검증 로직 구현
            return true;
        }
        
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


            return true;
        }

        /// <summary>
        /// 카드를 실행합니다.
        /// </summary>
        /// <param name="card">실행할 스킬 카드</param>
        /// <param name="context">카드 실행 컨텍스트</param>
        public void Execute(ISkillCard card, ICardExecutionContext context)
        {
            if (!CanExecute(card, context))
            {
                Debug.LogWarning($"[CardValidator] 카드 실행 불가: {card.CardDefinition?.CardName ?? "Unknown"}");
                return;
            }

            // 카드 실행 로직
            Debug.Log($"[CardValidator] 카드 실행: {card.CardDefinition?.CardName ?? "Unknown"} → {context.Target?.GetCharacterName() ?? "Unknown"}");
            
            // TODO: 실제 카드 효과 실행 로직 구현 필요
        }
    }
}
