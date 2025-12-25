using UnityEngine;
using Game.ItemSystem.Interface;
using Game.ItemSystem.Data;
using Game.CoreSystem.Utility;
using Game.VFXSystem.Manager;
using Game.CoreSystem.Audio;

namespace Game.ItemSystem.Effect
{
    /// <summary>
    /// 리롤 효과 ScriptableObject입니다.
    /// </summary>
    [CreateAssetMenu(fileName = "RerollEffect", menuName = "ItemEffects/RerollEffect")]
    public class RerollEffectSO : ItemEffectSO
    {
        [Header("리롤 설정")]
        [Tooltip("기본 리롤 수")]
        [SerializeField] private int rerollCount = 3;

        [Header("사운드 설정")]
        [Tooltip("리롤 효과 적용 시 재생할 SFX 클립")]
        [SerializeField] private AudioClip sfxClip;

        [Header("비주얼 이펙트 설정")]
        [Tooltip("리롤 효과 적용 시 재생할 비주얼 이펙트 프리팹")]
        [SerializeField] private GameObject visualEffectPrefab;

        public override IItemEffectCommand CreateEffectCommand(int power)
        {
            var vfxManager = UnityEngine.Object.FindFirstObjectByType<VFXManager>();
            var audioManager = UnityEngine.Object.FindFirstObjectByType<AudioManager>();
            var playerManager = UnityEngine.Object.FindFirstObjectByType<Game.CharacterSystem.Manager.PlayerManager>();
            return new RerollEffectCommand(rerollCount + power, sfxClip, visualEffectPrefab, vfxManager, audioManager, playerManager);
        }

        /// <summary>
        /// 커스텀 설정을 사용하여 효과 커맨드를 생성합니다.
        /// </summary>
        public IItemEffectCommand CreateEffectCommand(RerollEffectCustomSettings customSettings)
        {
            var vfxManager = UnityEngine.Object.FindFirstObjectByType<VFXManager>();
            var audioManager = UnityEngine.Object.FindFirstObjectByType<AudioManager>();
            var playerManager = UnityEngine.Object.FindFirstObjectByType<Game.CharacterSystem.Manager.PlayerManager>();
            if (customSettings == null)
            {
                return new RerollEffectCommand(rerollCount, sfxClip, visualEffectPrefab, vfxManager, audioManager, playerManager);
            }

            AudioClip finalSfxClip = customSettings.sfxClip ?? sfxClip;
            GameObject finalVisualEffectPrefab = customSettings.visualEffectPrefab ?? visualEffectPrefab;
            return new RerollEffectCommand(rerollCount, finalSfxClip, finalVisualEffectPrefab, vfxManager, audioManager, playerManager);
        }

        public override void ApplyEffect(IItemUseContext context, int value)
        {
            if (context?.User == null || context.User.IsDead())
            {
                GameLogger.LogWarning("[RerollEffectSO] 사용자가 null이거나 사망 상태입니다. 리롤 실패.", GameLogger.LogCategory.Core);
                return;
            }

            // TODO: 실제 리롤 시스템과 연동
            GameLogger.LogInfo($"[RerollEffectSO] 리롤 효과: 카드 {value}장 다시 드로우", GameLogger.LogCategory.Core);
        }

        protected override void OnValidate()
        {
            base.OnValidate();
            if (rerollCount < 0)
            {
                rerollCount = 0;
            }
        }
    }
}
