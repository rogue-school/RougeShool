using UnityEngine;
using Game.ItemSystem.Interface;
using Game.ItemSystem.Data;
using Game.CoreSystem.Utility;
using Game.VFXSystem.Manager;
using Game.CoreSystem.Audio;

namespace Game.ItemSystem.Effect
{
    /// <summary>
    /// 부활 효과 ScriptableObject입니다.
    /// </summary>
    [CreateAssetMenu(fileName = "ReviveEffect", menuName = "ItemEffects/ReviveEffect")]
    public class ReviveEffectSO : ItemEffectSO
    {
        [Header("사운드 설정")]
        [Tooltip("부활 효과 적용 시 재생할 SFX 클립")]
        [SerializeField] private AudioClip sfxClip;

        [Header("비주얼 이펙트 설정")]
        [Tooltip("부활 효과 적용 시 재생할 비주얼 이펙트 프리팹")]
        [SerializeField] private GameObject visualEffectPrefab;

        public override IItemEffectCommand CreateEffectCommand(int power)
        {
            var vfxManager = Game.VFXSystem.Manager.VFXManager.Instance;
            var audioManager = Game.CoreSystem.Audio.AudioManager.Instance;
            return new ReviveEffectCommand(sfxClip, visualEffectPrefab, vfxManager, audioManager);
        }

        /// <summary>
        /// 커스텀 설정을 사용하여 효과 커맨드를 생성합니다.
        /// </summary>
        public IItemEffectCommand CreateEffectCommand(ReviveEffectCustomSettings customSettings)
        {
            var vfxManager = Game.VFXSystem.Manager.VFXManager.Instance;
            var audioManager = Game.CoreSystem.Audio.AudioManager.Instance;
            if (customSettings == null)
            {
                return new ReviveEffectCommand(sfxClip, visualEffectPrefab, vfxManager, audioManager);
            }

            AudioClip finalSfxClip = customSettings.sfxClip ?? sfxClip;
            GameObject finalVisualEffectPrefab = customSettings.visualEffectPrefab ?? visualEffectPrefab;
            return new ReviveEffectCommand(finalSfxClip, finalVisualEffectPrefab, vfxManager, audioManager);
        }

        public override void ApplyEffect(IItemUseContext context, int value)
        {
            // ApplyEffect는 더 이상 사용되지 않습니다.
            // 효과는 CreateEffectCommand로 생성된 ReviveEffectCommand에서 처리됩니다.
            GameLogger.LogWarning("[ReviveEffectSO] ApplyEffect는 더 이상 사용되지 않습니다. ReviveEffectCommand를 사용하세요.", GameLogger.LogCategory.Core);
        }
    }
}
