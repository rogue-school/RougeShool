using UnityEngine;
using Game.SkillCardSystem.Interface;
using Game.CombatSystem.Interface;
using Game.CoreSystem.Utility;

namespace Game.SkillCardSystem.Effect
{
    /// <summary>
    /// 자신의 체력을 회복시키는 효과 ScriptableObject입니다.
    /// </summary>
    [CreateAssetMenu(fileName = "HealEffect", menuName = "SkillEffects/HealEffect")]
    public class HealEffectSO : SkillCardEffectSO
    {
        [Header("치유 설정")]
        [Tooltip("기본 치유량")]
        [SerializeField] private int baseHealAmount = 5;
        
        [Tooltip("최대 치유량 (0 = 무제한)")]
        [SerializeField] private int maxHealAmount = 0;

        /// <summary>
        /// 효과 명령을 생성합니다.
        /// </summary>
        /// <param name="power">추가 파워 수치</param>
        /// <returns>치유 효과 명령</returns>
        public override ICardEffectCommand CreateEffectCommand(int power)
        {
            return new HealEffectCommand(baseHealAmount + power, maxHealAmount);
        }
        
        /// <summary>
        /// EffectCustomSettings를 통해 효과 명령을 생성합니다.
        /// </summary>
        /// <param name="customSettings">커스텀 설정</param>
        /// <returns>치유 효과 명령</returns>
        public ICardEffectCommand CreateEffectCommand(Game.SkillCardSystem.Data.EffectCustomSettings customSettings)
        {
            return new HealEffectCommand(customSettings.healAmount, maxHealAmount);
        }

        /// <summary>
        /// 직접 효과 적용 (사용하지 않음)
        /// </summary>
        public override void ApplyEffect(ICardExecutionContext context, int value, ICombatTurnManager controller = null)
        {
            // 이 효과는 명령 패턴으로 동작하므로 직접 적용하지 않음
        }
    }
}
