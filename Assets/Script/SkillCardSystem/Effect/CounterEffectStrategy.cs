using UnityEngine;
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
            Sprite icon = null;
            
            if (config.useCustomSettings && config.customSettings != null)
            {
                duration = config.customSettings.counterDuration;
                icon = config.customSettings.counterIcon;
            }

            // 아이콘이 설정되지 않았으면 EffectSO에서 가져오기 (폴백)
            icon ??= counterEffectSO.GetIcon();
            
            return new CounterEffectCommand(duration, icon);
        }
    }
}
