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

        public override IItemEffectCommand CreateEffectCommand(int power)
        {
            return new ClownPotionEffectCommand(defaultEffectAmount + power);
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
                return new ClownPotionEffectCommand();
            }

            return new ClownPotionEffectCommand(
                customSettings.healChance,
                customSettings.healAmount,
                customSettings.damageAmount
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
