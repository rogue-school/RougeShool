using UnityEngine;
using Game.ItemSystem.Interface;

namespace Game.ItemSystem.Effect
{
    /// <summary>
    /// 역행 모래시계 효과 ScriptableObject입니다.
    /// 카드를 다시 드로우하는 효과를 제공합니다.
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
                Debug.LogWarning("[RerollEffectSO] 사용자가 null이거나 사망 상태입니다. 리롤 실패.");
                return;
            }

            // TODO: 실제 리롤 시스템과 연동
            Debug.Log($"[RerollEffectSO] 리롤 효과: 카드 {value}장 다시 드로우");
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
