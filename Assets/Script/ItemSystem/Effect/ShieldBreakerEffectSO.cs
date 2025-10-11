using UnityEngine;
using Game.ItemSystem.Interface;

namespace Game.ItemSystem.Effect
{
    /// <summary>
    /// 실드 브레이커 효과 ScriptableObject입니다.
    /// </summary>
    [CreateAssetMenu(fileName = "ShieldBreakerEffect", menuName = "ItemEffects/ShieldBreakerEffect")]
    public class ShieldBreakerEffectSO : ItemEffectSO
    {
        [Header("실드 브레이커 설정")]
        [Tooltip("지속 시간 (턴)")]
        [SerializeField] private int duration = 1;

        public override IItemEffectCommand CreateEffectCommand(int power)
        {
            return new ShieldBreakerEffectCommand(duration);
        }

        public override void ApplyEffect(IItemUseContext context, int value)
        {
            if (context?.User == null || context.User.IsDead())
            {
                Debug.LogWarning("[ShieldBreakerEffectSO] 사용자가 null이거나 사망 상태입니다. 실드 브레이커 실패.");
                return;
            }

            // TODO: 실제 실드 브레이커 시스템과 연동
            Debug.Log($"[ShieldBreakerEffectSO] 실드 브레이커 효과: 방어/반격 무시 ({duration}턴)");
        }

        protected override void OnValidate()
        {
            base.OnValidate();
            if (duration < 1)
            {
                duration = 1;
            }
        }
    }
}
