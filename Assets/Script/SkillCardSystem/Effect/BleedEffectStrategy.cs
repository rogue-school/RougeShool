using Game.SkillCardSystem.Data;
using Game.SkillCardSystem.Interface;

namespace Game.SkillCardSystem.Effect
{
    /// <summary>
    /// 출혈 효과 커맨드 생성 전략입니다.
    /// </summary>
    public class BleedEffectStrategy : IEffectCommandStrategy
    {
        public bool CanHandle(SkillCardEffectSO effectSO)
        {
            return effectSO is BleedEffectSO;
        }

        public ICardEffectCommand CreateCommand(EffectConfiguration config)
        {
            if (config.effectSO is not BleedEffectSO bleedEffectSO)
                return null;

            if (config.useCustomSettings && config.customSettings != null)
            {
                return new BleedEffectCommand(
                    config.customSettings.bleedAmount,
                    config.customSettings.bleedDuration,
                    bleedEffectSO.GetIcon()
                );
            }

            return null;
        }
    }
}
