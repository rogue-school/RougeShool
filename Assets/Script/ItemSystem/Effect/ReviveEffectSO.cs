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
            if (context?.User == null)
            {
                GameLogger.LogWarning("[ReviveEffectSO] 사용자가 null입니다. 부활 실패.", GameLogger.LogCategory.Core);
                return;
            }

            if (!context.User.IsDead())
            {
                GameLogger.LogInfo("[ReviveEffectSO] 사용자가 이미 살아있습니다.", GameLogger.LogCategory.Core);
                return;
            }

            // 최대 체력으로 부활
            int maxHP = context.User.GetMaxHP();
            context.User.Heal(maxHP);
            
            // TODO: 디버프 제거 시스템과 연동
            GameLogger.LogInfo($"[ReviveEffectSO] 부활 완료: 체력 {maxHP}으로 회복, 모든 디버프 제거", GameLogger.LogCategory.Core);
        }
    }
}
