using UnityEngine;
using Game.SkillCardSystem.Interface;

namespace Game.SkillCardSystem.Effect
{
    /// <summary>
    /// 반격 버프를 적용하는 SO. 커맨드를 통해 소스에게 CounterBuff를 1턴 부여합니다.
    /// </summary>
    [CreateAssetMenu(fileName = "CounterEffect", menuName = "SkillEffects/CounterEffect")]
    public class CounterEffectSO : SkillCardEffectSO
    {
        [Header("반격 설정")]
        [Tooltip("반격 지속 턴 수 (SO 기본값, 카드 커스텀으로 재정의 가능)")]
        [SerializeField] private int duration = 1;

        public override ICardEffectCommand CreateEffectCommand(int power)
        {
            return new CounterEffectCommand(duration, GetIcon());
        }

        public override void ApplyEffect(Game.CombatSystem.Interface.ICardExecutionContext context, int value, Game.CombatSystem.Interface.ICombatTurnManager controller = null)
        {
            // 즉시 적용 경로는 사용하지 않음. 커맨드 경로만 사용.
        }
    }
}


