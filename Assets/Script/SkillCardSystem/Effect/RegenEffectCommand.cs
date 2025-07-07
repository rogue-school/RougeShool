using UnityEngine;
using Game.CombatSystem.Interface;
using Game.SkillCardSystem.Interface;

namespace Game.SkillCardSystem.Effect
{
    /// <summary>
    /// 대상에게 일정 턴 동안 체력 재생 효과를 부여하는 커맨드 클래스입니다.
    /// </summary>
    public class RegenEffectCommand : ICardEffectCommand
    {
        private readonly int _healAmount;
        private readonly int _duration;

        /// <summary>
        /// 재생 커맨드를 초기화합니다.
        /// </summary>
        /// <param name="healAmount">턴마다 회복할 체력량</param>
        /// <param name="duration">지속 턴 수</param>
        public RegenEffectCommand(int healAmount, int duration)
        {
            _healAmount = healAmount;
            _duration = duration;
        }

        /// <summary>
        /// 카드 효과를 실행하여 대상에게 재생 효과를 부여합니다.
        /// </summary>
        /// <param name="context">카드 실행 컨텍스트 (시전자, 대상 등 포함)</param>
        /// <param name="turnManager">전투 턴 매니저 (필요 시 사용)</param>
        public void Execute(ICardExecutionContext context, ICombatTurnManager turnManager)
        {
            if (context.Target == null)
            {
                Debug.LogWarning("[RegenEffectCommand] 대상이 null입니다.");
                return;
            }

            var effect = new RegenEffect(_healAmount, _duration);
            context.Target.RegisterPerTurnEffect(effect);

            Debug.Log($"[RegenEffectCommand] {context.Target.GetCharacterName()}에게 재생 효과 적용: {_healAmount} 회복 / {_duration}턴 지속");
        }
    }
}
