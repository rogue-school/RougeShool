using UnityEngine;
using Game.ItemSystem.Interface;
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
            return new HealEffectCommand(healAmount + power);
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
