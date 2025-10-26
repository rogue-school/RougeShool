using UnityEngine;
using Game.ItemSystem.Interface;
using Game.ItemSystem.Data;
using Game.CoreSystem.Utility;

namespace Game.ItemSystem.Effect
{
    /// <summary>
    /// 체력 회복 효과 ScriptableObject입니다.
    /// </summary>
    [CreateAssetMenu(fileName = "HealEffect", menuName = "ItemEffects/HealEffect")]
    public class HealEffectSO : ItemEffectSO
    {
        [Header("회복 설정")]
        [Tooltip("기본 회복량")]
        [SerializeField] private int healAmount = 3;

        public override IItemEffectCommand CreateEffectCommand(int power)
        {
            // power가 0이 아니면 power 값을 사용 (커스텀 설정값)
            // power가 0이면 기본 healAmount 사용
            int finalHealAmount = power > 0 ? power : healAmount;
            return new HealEffectCommand(finalHealAmount);
        }

        /// <summary>
        /// 커스텀 설정을 사용하여 효과 커맨드를 생성합니다.
        /// </summary>
        public IItemEffectCommand CreateEffectCommand(HealEffectCustomSettings customSettings)
        {
            if (customSettings == null)
            {
                return new HealEffectCommand(healAmount);
            }

            return new HealEffectCommand(customSettings.healAmount);
        }

        public override void ApplyEffect(IItemUseContext context, int value)
        {
            // ApplyEffect는 더 이상 사용되지 않습니다.
            // 효과는 CreateEffectCommand로 생성된 HealEffectCommand에서 처리됩니다.
            GameLogger.LogWarning("[HealEffectSO] ApplyEffect는 더 이상 사용되지 않습니다. HealEffectCommand를 사용하세요.", GameLogger.LogCategory.Core);
        }

        protected override void OnValidate()
        {
            base.OnValidate();
            if (healAmount < 0)
            {
                healAmount = 0;
            }
        }
    }
}
