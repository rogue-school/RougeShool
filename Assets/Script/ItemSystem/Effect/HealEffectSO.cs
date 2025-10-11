using UnityEngine;
using Game.ItemSystem.Interface;

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
            if (context?.User == null || context.User.IsDead())
            {
                Debug.LogWarning("[HealEffectSO] 사용자가 null이거나 사망 상태입니다. 회복 적용 실패.");
                return;
            }

            int currentHP = context.User.GetCurrentHP();
            int maxHP = context.User.GetMaxHP();
            int actualHeal = Mathf.Min(value, maxHP - currentHP);

            if (actualHeal > 0)
            {
                context.User.Heal(actualHeal);
                Debug.Log($"[HealEffectSO] 체력 회복: {actualHeal} (현재: {currentHP + actualHeal}/{maxHP})");
            }
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
