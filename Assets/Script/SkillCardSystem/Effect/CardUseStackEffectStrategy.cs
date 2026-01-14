using Game.SkillCardSystem.Data;
using Game.SkillCardSystem.Interface;

namespace Game.SkillCardSystem.Effect
{
    /// <summary>
    /// 카드 사용 스택 효과 커맨드 생성 전략입니다.
    /// </summary>
    public class CardUseStackEffectStrategy : IEffectCommandStrategy
    {
        public bool CanHandle(SkillCardEffectSO effectSO)
        {
            return effectSO is CardUseStackEffectSO;
        }

        public ICardEffectCommand CreateCommand(EffectConfiguration config)
        {
            if (config.effectSO is not CardUseStackEffectSO cardUseStackEffectSO)
                return null;

            if (config.useCustomSettings && config.customSettings != null)
            {
                return cardUseStackEffectSO.CreateEffectCommand(config.customSettings);
            }

            return null;
        }
    }
}
