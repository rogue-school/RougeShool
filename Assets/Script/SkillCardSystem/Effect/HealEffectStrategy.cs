using Game.SkillCardSystem.Data;
using Game.SkillCardSystem.Interface;

namespace Game.SkillCardSystem.Effect
{
    /// <summary>
    /// 치유 효과 커맨드 생성 전략입니다.
    /// </summary>
    public class HealEffectStrategy : IEffectCommandStrategy
    {
        public bool CanHandle(SkillCardEffectSO effectSO)
        {
            return effectSO is HealEffectSO;
        }

        public ICardEffectCommand CreateCommand(EffectConfiguration config)
        {
            if (config.effectSO is not HealEffectSO healEffectSO)
                return null;

            if (config.useCustomSettings && config.customSettings != null)
            {
                return healEffectSO.CreateEffectCommand(config.customSettings);
            }

            return null;
        }
    }
}
