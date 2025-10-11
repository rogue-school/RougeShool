using UnityEngine;
using Game.ItemSystem.Interface;
using Game.ItemSystem.Data;

namespace Game.ItemSystem.Effect
{
    /// <summary>
    /// 운명의 주사위 효과 ScriptableObject입니다.
    /// 사용 시 다음 턴 적이 사용 예정인 스킬을, 해당 적이 지닌 다른 스킬 중 하나로 무작위 변경시킵니다.
    /// </summary>
    [CreateAssetMenu(fileName = "DiceOfFateEffect", menuName = "ItemEffects/DiceOfFateEffect")]
    public class DiceOfFateEffectSO : ItemEffectSO
    {
        [Header("운명의 주사위 설정")]
        [Tooltip("변경할 스킬 수 (기본값: 1)")]
        [SerializeField] private int changeCount = 1;

        public override IItemEffectCommand CreateEffectCommand(int power)
        {
            return new DiceOfFateEffectCommand(changeCount + power);
        }

        /// <summary>
        /// 커스텀 설정을 사용하여 효과 커맨드를 생성합니다.
        /// </summary>
        /// <param name="customSettings">커스텀 설정</param>
        /// <returns>효과 커맨드</returns>
        public IItemEffectCommand CreateEffectCommand(DiceOfFateEffectCustomSettings customSettings)
        {
            if (customSettings == null)
            {
                return new DiceOfFateEffectCommand(changeCount);
            }

            return new DiceOfFateEffectCommand(customSettings.changeCount);
        }

        public override void ApplyEffect(IItemUseContext context, int value)
        {
            if (context?.User == null || context.User.IsDead())
            {
                Debug.LogWarning("[DiceOfFateEffectSO] 사용자가 null이거나 사망 상태입니다. 운명의 주사위 실패.");
                return;
            }

            // TODO: 실제 운명의 주사위 시스템과 연동
            Debug.Log($"[DiceOfFateEffectSO] 운명의 주사위 효과: 다음 적 스킬 {value}개를 무작위로 변경");
        }

        protected override void OnValidate()
        {
            base.OnValidate();
            if (changeCount < 1)
            {
                changeCount = 1;
            }
        }
    }
}
