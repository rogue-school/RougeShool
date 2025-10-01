using UnityEngine;
using Game.CombatSystem.Interface;
using Game.SkillCardSystem.Interface;

namespace Game.SkillCardSystem.Effect
{
    /// <summary>
    /// 대상에게 스턴 디버프를 적용하는 커맨드입니다.
    /// </summary>
    public class StunEffectCommand : ICardEffectCommand
    {
        private readonly int duration;
        private readonly Sprite icon;

        public StunEffectCommand(int duration, Sprite icon = null)
        {
            this.duration = Mathf.Max(1, duration);
            this.icon = icon;
        }

        public void Execute(ICardExecutionContext context, ICombatTurnManager turnManager)
        {
            if (context?.Target == null)
            {
                Game.CoreSystem.Utility.GameLogger.LogWarning("[StunEffectCommand] 대상이 null입니다.", Game.CoreSystem.Utility.GameLogger.LogCategory.SkillCard);
                return;
            }

            var stun = new StunDebuff(duration, icon);
            if (context.Target.RegisterStatusEffect(stun))
            {
                Game.CoreSystem.Utility.GameLogger.LogInfo($"[StunEffectCommand] {context.Target.GetCharacterName()} 스턴 적용 ({duration}턴)", Game.CoreSystem.Utility.GameLogger.LogCategory.SkillCard);
            }
            else
            {
                Game.CoreSystem.Utility.GameLogger.LogInfo($"[StunEffectCommand] 보호 상태로 스턴 차단", Game.CoreSystem.Utility.GameLogger.LogCategory.SkillCard);
            }
        }
    }
}


