using Game.SkillCardSystem.Data;
using Game.SkillCardSystem.Interface;

namespace Game.SkillCardSystem.Effect
{
    /// <summary>
    /// 반격 효과 커맨드 생성 전략입니다.
    /// </summary>
    public class CounterEffectStrategy : IEffectCommandStrategy
    {
        public bool CanHandle(SkillCardEffectSO effectSO)
        {
            return effectSO is CounterEffectSO;
        }

        public ICardEffectCommand CreateCommand(EffectConfiguration config)
        {
            if (config.effectSO is not CounterEffectSO counterEffectSO)
                return null;

            int duration = 1;
            if (config.useCustomSettings && config.customSettings != null)
            {
                duration = config.customSettings.counterDuration;
            }

            return new CounterEffectCommand(duration, counterEffectSO.GetIcon());
        }
    }
}
