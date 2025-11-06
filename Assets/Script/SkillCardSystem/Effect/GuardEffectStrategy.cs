using UnityEngine;
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
            Sprite icon = null;
            GameObject activateEffectPrefab = null;
            AudioClip activateSfxClip = null;

            if (config.useCustomSettings && config.customSettings != null)
            {
                duration = config.customSettings.guardDuration;
                icon = config.customSettings.guardIcon;
                activateEffectPrefab = config.customSettings.guardActivateEffectPrefab;
                activateSfxClip = config.customSettings.guardActivateSfxClip;
            }

            // 아이콘이 설정되지 않았으면 EffectSO에서 가져오기 (폴백)
            icon ??= guardEffectSO.GetIcon();

            return new GuardEffectCommand(duration, icon, activateEffectPrefab, activateSfxClip);
        }
    }
}
