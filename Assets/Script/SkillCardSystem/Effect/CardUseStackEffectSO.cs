using UnityEngine;
using Game.SkillCardSystem.Interface;
using Game.CombatSystem.Interface;
using Game.CoreSystem.Utility;

namespace Game.SkillCardSystem.Effect
{
    /// <summary>
    /// 카드가 사용될 때마다 해당 카드의 공격력 스택을 증가시키는 효과입니다.
    /// 데미지는 SkillCardDefinition의 hasDamage 시스템에서 자동으로 처리됩니다.
    /// </summary>
    [CreateAssetMenu(fileName = "CardUseStackEffect", menuName = "SkillEffects/CardUseStackEffect")]
    public class CardUseStackEffectSO : SkillCardEffectSO
    {
        [Header("스택 증가 설정")]
        [Tooltip("카드 사용 시 증가할 스택 수")]
        [SerializeField] private int stackIncreasePerUse = 1;
        
        [Tooltip("최대 스택 수 (0 = 무제한)")]
        [SerializeField] private int maxStacks = 5;

        /// <summary>
        /// 효과 명령을 생성합니다.
        /// </summary>
        /// <param name="power">추가 파워 수치</param>
        /// <returns>카드 사용 스택 증가 효과 명령</returns>
        public override ICardEffectCommand CreateEffectCommand(int power)
        {
            return new CardUseStackCommand(stackIncreasePerUse + power, maxStacks);
        }
        
        /// <summary>
        /// EffectCustomSettings를 통해 효과 명령을 생성합니다.
        /// </summary>
        /// <param name="customSettings">커스텀 설정</param>
        /// <returns>카드 사용 스택 증가 효과 명령</returns>
        public ICardEffectCommand CreateEffectCommand(Game.SkillCardSystem.Data.EffectCustomSettings customSettings)
        {
            return new CardUseStackCommand(customSettings);
        }

        /// <summary>
        /// 직접 효과 적용 (사용하지 않음)
        /// </summary>
        public override void ApplyEffect(ICardExecutionContext context, int value, ICombatTurnManager controller = null)
        {
            // 이 효과는 이벤트 기반으로 동작하므로 직접 적용하지 않음
        }
    }
}
