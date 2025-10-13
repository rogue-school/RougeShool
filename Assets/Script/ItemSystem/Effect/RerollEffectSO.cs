using UnityEngine;
using Game.ItemSystem.Interface;
using Game.CoreSystem.Utility;

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

        public override IItemEffectCommand CreateEffectCommand(int power)
        {
            return new RerollEffectCommand(rerollCount + power);
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
