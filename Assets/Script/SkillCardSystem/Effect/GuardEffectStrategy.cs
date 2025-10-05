using Game.SkillCardSystem.Data;
using Game.SkillCardSystem.Interface;

namespace Game.SkillCardSystem.Effect
{
    /// <summary>
    /// 가드 효과 커맨드 생성 전략입니다.
    /// </summary>
    public class GuardEffectStrategy : IEffectCommandStrategy
    {
        public bool CanHandle(SkillCardEffectSO effectSO)
        {
            return effectSO is GuardEffectSO;
        }

        public ICardEffectCommand CreateCommand(EffectConfiguration config)
        {
            if (config.effectSO is not GuardEffectSO guardEffectSO)
                return null;

            int duration = 1;
            if (config.useCustomSettings && config.customSettings != null)
            {
                duration = config.customSettings.guardDuration;
            }

            return new GuardEffectCommand(duration, guardEffectSO.visualEffectPrefab);
        }
    }
}
