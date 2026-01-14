using Game.SkillCardSystem.Data;
using Game.SkillCardSystem.Interface;

namespace Game.SkillCardSystem.Effect
{
    /// <summary>
    /// 데미지 효과 커맨드 생성 전략입니다.
    /// </summary>
    public class DamageEffectStrategy : IEffectCommandStrategy
    {
        public bool CanHandle(SkillCardEffectSO effectSO)
        {
            return effectSO is DamageEffectSO;
        }

        public ICardEffectCommand CreateCommand(EffectConfiguration config)
        {
            if (config.effectSO is not DamageEffectSO)
                return null;

            if (config.useCustomSettings && config.customSettings != null)
            {
                // 의존성 찾기
                var audioManager = UnityEngine.Object.FindFirstObjectByType<Game.CoreSystem.Audio.AudioManager>();
                var itemService = UnityEngine.Object.FindFirstObjectByType<Game.ItemSystem.Service.ItemService>();
                
                return new DamageEffectCommand(
                    config.customSettings.damageAmount,
                    config.customSettings.damageHits,
                    config.customSettings.ignoreGuard,
                    config.customSettings.ignoreCounter,
                    audioManager as Game.CoreSystem.Interface.IAudioManager,
                    itemService as Game.ItemSystem.Interface.IItemService
                );
            }

            return null;
        }
    }
}
