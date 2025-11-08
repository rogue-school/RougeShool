using UnityEngine;
using Game.ItemSystem.Interface;
using Game.ItemSystem.Data;
using Game.CoreSystem.Utility;
using Game.VFXSystem.Manager;
using Game.CoreSystem.Audio;

namespace Game.ItemSystem.Effect
{
    /// <summary>
    /// 공격력 버프 효과 ScriptableObject입니다.
    /// </summary>
    [CreateAssetMenu(fileName = "AttackBuffEffect", menuName = "ItemEffects/AttackBuffEffect")]
    public class AttackBuffEffectSO : ItemEffectSO
    {
        [Header("버프 설정")]
        [Tooltip("기본 버프량")]
        [SerializeField] private int buffAmount = 3;
        
        [Tooltip("지속 시간 (턴)")]
        [SerializeField] private int duration = 1;

        [Header("사운드 설정")]
        [Tooltip("버프 적용 시 재생할 SFX 클립")]
        [SerializeField] private AudioClip sfxClip;

        [Header("비주얼 이펙트 설정")]
        [Tooltip("버프 적용 시 재생할 비주얼 이펙트 프리팹")]
        [SerializeField] private GameObject visualEffectPrefab;

        public override IItemEffectCommand CreateEffectCommand(int power)
        {
            // power 파라미터가 이미 최종 buffAmount입니다 (CustomSettings에서 가져옴)
            // SO의 buffAmount는 기본값일 뿐, 중복 더하기하면 안 됩니다!
            int finalBuffAmount = power > 0 ? power : buffAmount; // power가 있으면 사용, 없으면 기본값
            var vfxManager = UnityEngine.Object.FindFirstObjectByType<VFXManager>();
            var audioManager = UnityEngine.Object.FindFirstObjectByType<AudioManager>();
            return new AttackBuffEffectCommand(finalBuffAmount, duration, sfxClip, visualEffectPrefab, vfxManager, audioManager);
        }

        /// <summary>
        /// 커스텀 설정을 사용하여 효과 커맨드를 생성합니다.
        /// </summary>
        public IItemEffectCommand CreateEffectCommand(AttackBuffEffectCustomSettings customSettings)
        {
            var vfxManager = UnityEngine.Object.FindFirstObjectByType<VFXManager>();
            var audioManager = UnityEngine.Object.FindFirstObjectByType<AudioManager>();
            if (customSettings == null)
            {
                return new AttackBuffEffectCommand(buffAmount, duration, sfxClip, visualEffectPrefab, vfxManager, audioManager);
            }

            AudioClip finalSfxClip = customSettings.sfxClip ?? sfxClip;
            GameObject finalVisualEffectPrefab = customSettings.visualEffectPrefab ?? visualEffectPrefab;
            return new AttackBuffEffectCommand(customSettings.buffAmount, customSettings.duration, finalSfxClip, finalVisualEffectPrefab, vfxManager, audioManager);
        }

        public override void ApplyEffect(IItemUseContext context, int value)
        {
            // ApplyEffect는 더 이상 사용되지 않습니다.
            // 효과는 CreateEffectCommand로 생성된 AttackBuffEffectCommand에서 처리됩니다.
            GameLogger.LogWarning("[AttackBuffEffectSO] ApplyEffect는 더 이상 사용되지 않습니다. AttackBuffEffectCommand를 사용하세요.", GameLogger.LogCategory.Core);
        }

        protected override void OnValidate()
        {
            base.OnValidate();
            if (buffAmount < 0)
            {
                buffAmount = 0;
            }
            if (duration < 1)
            {
                duration = 1;
            }
        }
    }
}
