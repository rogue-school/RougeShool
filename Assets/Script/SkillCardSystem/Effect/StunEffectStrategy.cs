using Game.SkillCardSystem.Data;
using Game.SkillCardSystem.Interface;

namespace Game.SkillCardSystem.Effect
{
    /// <summary>
    /// 스턴 효과 커맨드 생성 전략입니다.
    /// </summary>
    public class StunEffectStrategy : IEffectCommandStrategy
    {
        public bool CanHandle(SkillCardEffectSO effectSO)
        {
            return effectSO is StunEffectSO;
        }

        public ICardEffectCommand CreateCommand(EffectConfiguration config)
        {
            if (config.effectSO is not StunEffectSO stunEffectSO)
                return null;

            if (config.useCustomSettings && config.customSettings != null)
            {
                // 아이콘이 설정되지 않았으면 EffectSO에서 가져오기 (폴백)
                var icon = config.customSettings.stunIcon ?? stunEffectSO.GetIcon();
                
                return new StunEffectCommand(
                    config.customSettings.stunDuration,
                    icon
                );
            }

            return null;
        }
    }
}
