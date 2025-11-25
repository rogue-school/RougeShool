using UnityEngine;
using Game.ItemSystem.Interface;
using Game.ItemSystem.Data;
using Game.CoreSystem.Utility;
using Game.VFXSystem.Manager;
using Game.CoreSystem.Audio;

namespace Game.ItemSystem.Effect
{
    /// <summary>
    /// 실드 브레이커 효과 ScriptableObject입니다.
    /// </summary>
    [CreateAssetMenu(fileName = "ShieldBreakerEffect", menuName = "ItemEffects/ShieldBreakerEffect")]
    public class ShieldBreakerEffectSO : ItemEffectSO
    {
        [Header("실드 브레이커 설정")]
        [Tooltip("지속 시간 (턴)")]
        [SerializeField] private int duration = 2;

        [Header("사운드 설정")]
        [Tooltip("실드 브레이커 효과 적용 시 재생할 SFX 클립")]
        [SerializeField] private AudioClip sfxClip;

        [Header("비주얼 이펙트 설정")]
        [Tooltip("실드 브레이커 효과 적용 시 재생할 비주얼 이펙트 프리팹")]
        [SerializeField] private GameObject visualEffectPrefab;

        public override IItemEffectCommand CreateEffectCommand(int power)
        {
            var vfxManager = Game.VFXSystem.Manager.VFXManager.Instance;
            var audioManager = Game.CoreSystem.Audio.AudioManager.Instance;
            return new ShieldBreakerEffectCommand(duration, sfxClip, visualEffectPrefab, vfxManager, audioManager);
        }

        /// <summary>
        /// 커스텀 설정을 사용하여 효과 커맨드를 생성합니다.
        /// </summary>
        public IItemEffectCommand CreateEffectCommand(ShieldBreakerEffectCustomSettings customSettings)
        {
            var vfxManager = Game.VFXSystem.Manager.VFXManager.Instance;
            var audioManager = Game.CoreSystem.Audio.AudioManager.Instance;
            if (customSettings == null)
            {
                return new ShieldBreakerEffectCommand(duration, sfxClip, visualEffectPrefab, vfxManager, audioManager);
            }

            AudioClip finalSfxClip = customSettings.sfxClip ?? sfxClip;
            GameObject finalVisualEffectPrefab = customSettings.visualEffectPrefab ?? visualEffectPrefab;
            return new ShieldBreakerEffectCommand(customSettings.duration, finalSfxClip, finalVisualEffectPrefab, vfxManager, audioManager);
        }

        public override void ApplyEffect(IItemUseContext context, int value)
        {
            if (context?.User == null || context.User.IsDead())
            {
                GameLogger.LogWarning("[ShieldBreakerEffectSO] 사용자가 null이거나 사망 상태입니다. 실드 브레이커 실패.", GameLogger.LogCategory.Core);
                return;
            }

            // TODO: 실제 실드 브레이커 시스템과 연동
            GameLogger.LogInfo($"[ShieldBreakerEffectSO] 실드 브레이커 효과: 방어/반격 무시 ({duration}턴)", GameLogger.LogCategory.Core);
        }

        protected override void OnValidate()
        {
            base.OnValidate();
            if (duration < 1)
            {
                duration = 1;
            }
        }
    }
}
