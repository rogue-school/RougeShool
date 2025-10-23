using UnityEngine;
using Game.ItemSystem.Interface;
using Game.CoreSystem.Utility;

namespace Game.ItemSystem.Effect
{
    /// <summary>
    /// 부활 효과 ScriptableObject입니다.
    /// </summary>
    [CreateAssetMenu(fileName = "ReviveEffect", menuName = "ItemEffects/ReviveEffect")]
    public class ReviveEffectSO : ItemEffectSO
    {
        public override IItemEffectCommand CreateEffectCommand(int power)
        {
            return new ReviveEffectCommand();
        }

        public override void ApplyEffect(IItemUseContext context, int value)
        {
            // ApplyEffect는 더 이상 사용되지 않습니다.
            // 효과는 CreateEffectCommand로 생성된 ReviveEffectCommand에서 처리됩니다.
            GameLogger.LogWarning("[ReviveEffectSO] ApplyEffect는 더 이상 사용되지 않습니다. ReviveEffectCommand를 사용하세요.", GameLogger.LogCategory.Core);
        }
    }
}
