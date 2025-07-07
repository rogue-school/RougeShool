using UnityEngine;
using Game.CombatSystem.Interface;
using Game.CombatSystem.Slot;
using Game.SkillCardSystem.Interface;

namespace Game.SkillCardSystem.Effect
{
    /// <summary>
    /// 적 카드의 다음 슬롯 위치를 강제로 지정하는 커맨드입니다.
    /// 플레이어 카드에는 적용되지 않습니다.
    /// </summary>
    public class ForceNextSlotEffectCommand : ICardEffectCommand
    {
        private readonly CombatSlotPosition forcedSlot;

        /// <summary>
        /// 강제로 지정할 슬롯을 설정합니다.
        /// </summary>
        /// <param name="forcedSlot">지정할 슬롯 위치</param>
        public ForceNextSlotEffectCommand(CombatSlotPosition forcedSlot)
        {
            this.forcedSlot = forcedSlot;
        }

        /// <summary>
        /// 슬롯 변경 효과를 실행합니다.
        /// 적 카드일 때만 작동하며, 다음 슬롯 위치를 강제로 지정합니다.
        /// </summary>
        /// <param name="context">카드 실행 컨텍스트</param>
        /// <param name="turnManager">전투 턴 관리자</param>
        public void Execute(ICardExecutionContext context, ICombatTurnManager turnManager)
        {
            if (context?.Card == null || context.Target == null)
            {
                Debug.LogWarning("[ForceNextSlotEffectCommand] 카드 또는 대상 정보가 누락되었습니다.");
                return;
            }

            if (context.Card.IsFromPlayer())
            {
                Debug.LogWarning("[ForceNextSlotEffectCommand] 플레이어 카드에는 슬롯 지정 효과를 사용할 수 없습니다.");
                return;
            }

            turnManager.ReserveNextEnemySlot(forcedSlot);
            Debug.Log($"[ForceNextSlotEffectCommand] 다음 적 카드 슬롯 강제 설정됨 → {forcedSlot}");
        }
    }
}
