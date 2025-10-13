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
            if (context?.User == null || context.User.IsDead())
            {
                GameLogger.LogWarning("[ClownPotionEffectSO] 사용자가 null이거나 사망 상태입니다. 효과 적용 실패.", GameLogger.LogCategory.Core);
                return;
            }

            // 기본값 사용 (커스텀 설정이 없을 때)
            bool isHeal = UnityEngine.Random.Range(0f, 1f) < 0.5f;
            int effectAmount = defaultEffectAmount;
            
            if (isHeal)
            {
                context.User.Heal(effectAmount);
                GameLogger.LogInfo($"[ClownPotionEffectSO] 광대 물약 효과: 체력 회복 +{effectAmount}", GameLogger.LogCategory.Core);
            }
            else
            {
                context.User.TakeDamage(effectAmount);
                GameLogger.LogInfo($"[ClownPotionEffectSO] 광대 물약 효과: 데미지 -{effectAmount}", GameLogger.LogCategory.Core);
            }
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
