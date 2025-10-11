using UnityEngine;
using Game.ItemSystem.Interface;
using Game.ItemSystem.Data;

namespace Game.ItemSystem.Effect
{
    /// <summary>
    /// 타임 스톱 스크롤 효과 ScriptableObject입니다.
    /// 사용 시 다음 턴에 사용될 적 카드를 봉인시킵니다.
    /// </summary>
    [CreateAssetMenu(fileName = "TimeStopEffect", menuName = "ItemEffects/TimeStopEffect")]
    public class TimeStopEffectSO : ItemEffectSO
    {
        [Header("시간 정지 설정")]
        [Tooltip("봉인할 적 카드 수 (기본값: 1)")]
        [SerializeField] private int sealCount = 1;

        public override IItemEffectCommand CreateEffectCommand(int power)
        {
            return new TimeStopEffectCommand(sealCount + power);
        }

        /// <summary>
        /// 커스텀 설정을 사용하여 효과 커맨드를 생성합니다.
        /// </summary>
        /// <param name="customSettings">커스텀 설정</param>
        /// <returns>효과 커맨드</returns>
        public IItemEffectCommand CreateEffectCommand(TimeStopEffectCustomSettings customSettings)
        {
            if (customSettings == null)
            {
                return new TimeStopEffectCommand(sealCount);
            }

            return new TimeStopEffectCommand(customSettings.sealCount);
        }

        public override void ApplyEffect(IItemUseContext context, int value)
        {
            if (context?.User == null || context.User.IsDead())
            {
                Debug.LogWarning("[TimeStopEffectSO] 사용자가 null이거나 사망 상태입니다. 시간 정지 실패.");
                return;
            }

            // TODO: 실제 시간 정지 시스템과 연동
            Debug.Log($"[TimeStopEffectSO] 시간 정지 효과: 다음 적 카드 {value}장 봉인");
        }

        protected override void OnValidate()
        {
            base.OnValidate();
            if (sealCount < 1)
            {
                sealCount = 1;
            }
        }
    }
}
