using UnityEngine;
using Game.ItemSystem.Interface;
using Game.ItemSystem.Data;
using Game.CoreSystem.Utility;

namespace Game.ItemSystem.Effect
{
    /// <summary>
    /// 광대 물약 랜덤 효과 ScriptableObject입니다.
    /// 50% 확률로 체력 회복 또는 데미지를 입힙니다.
    /// </summary>
    [CreateAssetMenu(fileName = "ClownPotionEffect", menuName = "ItemEffects/ClownPotionEffect")]
    public class ClownPotionEffectSO : ItemEffectSO
    {
        [Header("기본 효과 설정")]
        [Tooltip("기본 효과량 (커스텀 설정이 없을 때 사용)")]
        [SerializeField] private int defaultEffectAmount = 5;

        [Header("사운드 설정")]
        [Tooltip("체력 회복 시 재생할 SFX 클립")]
        [SerializeField] private AudioClip healSfxClip;
        
        [Tooltip("데미지 입을 시 재생할 SFX 클립")]
        [SerializeField] private AudioClip damageSfxClip;

        [Header("비주얼 이펙트 설정")]
        [Tooltip("체력 회복 시 재생할 비주얼 이펙트 프리팹")]
        [SerializeField] private GameObject healVisualEffectPrefab;
        
        [Tooltip("데미지 입을 시 재생할 비주얼 이펙트 프리팹")]
        [SerializeField] private GameObject damageVisualEffectPrefab;

        public override IItemEffectCommand CreateEffectCommand(int power)
        {
            // 기본값 사용: healChance=50, healAmount=defaultEffectAmount+power, damageAmount=defaultEffectAmount+power
            return new ClownPotionEffectCommand(
                50, 
                defaultEffectAmount + power, 
                defaultEffectAmount + power, 
                healSfxClip, 
                damageSfxClip,
                healVisualEffectPrefab,
                damageVisualEffectPrefab
            );
        }

        /// <summary>
        /// 커스텀 설정을 사용하여 효과 커맨드를 생성합니다.
        /// </summary>
        /// <param name="customSettings">커스텀 설정</param>
        /// <returns>효과 커맨드</returns>
        public IItemEffectCommand CreateEffectCommand(ClownPotionEffectCustomSettings customSettings)
        {
            if (customSettings == null)
            {
                return new ClownPotionEffectCommand(50, 5, 5, healSfxClip, damageSfxClip, healVisualEffectPrefab, damageVisualEffectPrefab);
            }

            // Custom Settings에 SFX가 있으면 우선 사용, 없으면 SO의 기본값 사용
            AudioClip finalHealSfx = customSettings.healSfxClip ?? healSfxClip;
            AudioClip finalDamageSfx = customSettings.damageSfxClip ?? damageSfxClip;
            
            // Custom Settings에 이펙트가 있으면 우선 사용, 없으면 SO의 기본값 사용
            GameObject finalHealVfx = customSettings.healVisualEffectPrefab ?? healVisualEffectPrefab;
            GameObject finalDamageVfx = customSettings.damageVisualEffectPrefab ?? damageVisualEffectPrefab;

            return new ClownPotionEffectCommand(
                customSettings.healChance,
                customSettings.healAmount,
                customSettings.damageAmount,
                finalHealSfx,
                finalDamageSfx,
                finalHealVfx,
                finalDamageVfx
            );
        }

        public override void ApplyEffect(IItemUseContext context, int value)
        {
            // ApplyEffect는 더 이상 사용되지 않습니다.
            // 효과는 CreateEffectCommand로 생성된 ClownPotionEffectCommand에서 처리됩니다.
            GameLogger.LogWarning("[ClownPotionEffectSO] ApplyEffect는 더 이상 사용되지 않습니다. ClownPotionEffectCommand를 사용하세요.", GameLogger.LogCategory.Core);
        }

        protected override void OnValidate()
        {
            base.OnValidate();
            if (defaultEffectAmount < 0)
            {
                defaultEffectAmount = 0;
            }
        }
    }
}
