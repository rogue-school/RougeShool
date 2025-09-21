using Game.SkillCardSystem.Interface;
using Game.CombatSystem.Interface;
using UnityEngine;
using Game.CoreSystem.Utility;

namespace Game.CombatSystem.DragDrop
{
    /// <summary>
    /// 기본 카드 드롭 유효성 검사기입니다.
    /// 플레이어 카드만 드롭 가능하며, 적 카드가 이미 있는 슬롯에는 드롭할 수 없습니다.
    /// </summary>
    public class DefaultCardDropValidator : ICardValidator
    {
        /// <summary>
        /// 지정된 카드와 슬롯의 조합이 유효한 드롭 조건을 만족하는지 검사합니다.
        /// </summary>
        /// <param name="card">드롭 시도 중인 카드</param>
        /// <param name="slot">드롭 대상 슬롯</param>
        /// <param name="reason">실패 사유 메시지 (null이면 성공)</param>
        /// <returns>유효하면 true, 그렇지 않으면 false</returns>
        public bool IsValidDrop(ISkillCard card, ICombatCardSlot slot, out string reason)
        {
            reason = null;

            // 1. Null 검사
            if (card == null || slot == null)
            {
                reason = "카드 또는 슬롯이 null입니다.";
                return false;
            }

            // 2. 플레이어 카드 여부 확인
            if (!card.IsFromPlayer())
            {
                reason = "플레이어 카드만 드롭할 수 있습니다.";
                return false;
            }

            // 3. 슬롯에 적 카드가 이미 존재하는 경우
            var existing = slot.GetCard();
            if (existing != null && !existing.IsFromPlayer())
            {
                reason = "슬롯에 적 카드가 있어 드롭할 수 없습니다.";
                return false;
            }

            // 4. 전투 슬롯만 허용
            if (slot.Position != Game.CombatSystem.Slot.CombatSlotPosition.BATTLE_SLOT)
            {
                reason = "카드는 전투 슬롯에만 드롭할 수 있습니다.";
                return false;
            }

            // 5. 기타 조건 통과
            return true;
        }
        
        /// <summary>
        /// 주어진 카드가 현재 컨텍스트에서 실행 가능한지 여부를 반환합니다.
        /// </summary>
        /// <param name="card">검사 대상 스킬 카드.</param>
        /// <param name="context">실행 컨텍스트 (시전자, 대상 등 포함).</param>
        /// <returns>true면 실행 가능, false면 제한됨.</returns>
        public bool CanExecute(ISkillCard card, ICardExecutionContext context)
        {
            // TODO: 실행 가능성 검증 로직 구현
            return true;
        }

        /// <summary>
        /// 지정된 스킬 카드와 실행 컨텍스트를 사용하여 카드의 효과를 실행합니다.
        /// </summary>
        /// <param name="card">실행할 스킬 카드</param>
        /// <param name="context">실행 컨텍스트</param>
        public void Execute(ISkillCard card, ICardExecutionContext context)
        {
            // TODO: 카드 실행 로직 구현
            GameLogger.LogInfo($"[DefaultCardDropValidator] 카드 실행: {card.CardDefinition.CardName}", GameLogger.LogCategory.Combat);
        }
    }
}
