using UnityEngine;
using Game.ItemSystem.Interface;
using Game.CoreSystem.Utility;

namespace Game.ItemSystem.Effect
{
    /// <summary>
    /// 공격력 버프 효과 ScriptableObject입니다.
    /// </summary>
    [CreateAssetMenu(fileName = "AttackBuffEffect", menuName = "ItemEffects/AttackBuffEffect")]
    public class AttackBuffEffectSO : ItemEffectSO
    {
        [Header("버프 설정")]
        [Tooltip("기본 버프량")]
        [SerializeField] private int buffAmount = 3;
        
        [Tooltip("지속 시간 (턴)")]
        [SerializeField] private int duration = 1;

        public override IItemEffectCommand CreateEffectCommand(int power)
        {
            return new AttackBuffEffectCommand(buffAmount + power, duration);
        }

        public override void ApplyEffect(IItemUseContext context, int value)
        {
            if (context?.User == null || context.User.IsDead())
            {
                GameLogger.LogWarning("[AttackBuffEffectSO] 사용자가 null이거나 사망 상태입니다. 버프 적용 실패.", GameLogger.LogCategory.Core);
                return;
            }

            // TODO: 실제 버프 시스템과 연동
            GameLogger.LogInfo($"[AttackBuffEffectSO] 공격력 버프 적용: +{value} ({duration}턴)", GameLogger.LogCategory.Core);
        }

        protected override void OnValidate()
        {
            base.OnValidate();
            if (buffAmount < 0)
            {
                buffAmount = 0;
            }
            if (duration < 1)
            {
                duration = 1;
            }
        }
    }
}
