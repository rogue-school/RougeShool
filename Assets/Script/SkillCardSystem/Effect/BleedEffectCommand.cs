using UnityEngine;
using Game.CombatSystem.Interface;
using Game.CoreSystem.Utility;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.Effect;

namespace Game.SkillCardSystem.Effect
{
    /// <summary>
    /// 출혈 효과를 캐릭터에 적용하는 커맨드 클래스입니다.
    /// 지정된 수치와 지속 시간으로 <see cref="BleedEffect"/>를 생성하여 타겟에 등록합니다.
    /// </summary>
    public class BleedEffectCommand : ICardEffectCommand
    {
        private readonly int amount;
        private readonly int duration;
        private readonly Sprite icon;

        /// <summary>
        /// 출혈 커맨드를 초기화합니다.
        /// </summary>
        /// <param name="amount">매 턴 입힐 피해량</param>
        /// <param name="duration">지속 턴 수</param>
        public BleedEffectCommand(int amount, int duration, Sprite icon = null)
        {
            this.amount = amount;
            this.duration = duration;
            this.icon = icon;
        }

        /// <summary>
        /// 출혈 효과를 대상 캐릭터에 적용합니다.
        /// </summary>
        /// <param name="context">카드 실행 컨텍스트 (시전자, 대상 등 포함)</param>
        /// <param name="turnManager">전투 턴 관리자 (옵션)</param>
        public void Execute(ICardExecutionContext context, ICombatTurnManager turnManager)
        {
            if (context?.Target == null)
            {
                GameLogger.LogWarning("[BleedEffectCommand] 대상이 null이므로 출혈 효과를 적용할 수 없습니다.", GameLogger.LogCategory.SkillCard);
                return;
            }

            if (icon == null)
            {
                GameLogger.LogWarning("[BleedEffectCommand] 아이콘이 null입니다. BleedEffectSO의 Icon이 비어있지 않은지 확인하세요.", GameLogger.LogCategory.SkillCard);
            }

            var bleedEffect = new BleedEffect(amount, duration, icon);
            
            // 가드 상태 확인하여 상태이상 효과 등록
            if (context.Target.RegisterStatusEffect(bleedEffect))
            {
                GameLogger.LogInfo($"[BleedEffectCommand] {context.Target.GetCharacterName()}에게 출혈 {amount} 적용 (지속 {duration}턴)", GameLogger.LogCategory.SkillCard);
            }
            else
            {
                GameLogger.LogInfo($"[BleedEffectCommand] {context.Target.GetCharacterName()}의 가드로 출혈 효과 차단됨", GameLogger.LogCategory.SkillCard);
            }
        }
    }
}
