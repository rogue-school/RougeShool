using UnityEngine;
using Game.SkillCardSystem.Interface;
using Game.CombatSystem.Interface;

namespace Game.SkillCardSystem.Effect
{
    /// <summary>
    /// 대상에게 기절(스턴) 디버프를 적용하는 이펙트 SO입니다.
    /// </summary>
    [CreateAssetMenu(fileName = "StunEffect", menuName = "SkillEffects/StunEffect")]
    public class StunEffectSO : SkillCardEffectSO
    {
        [Header("스턴 지속 턴 수")]
        [Tooltip("스턴이 유지될 턴 수입니다.")]
        [SerializeField] private int duration = 1;

        public override ICardEffectCommand CreateEffectCommand(int power)
        {
            // power를 추가 지속 턴으로 활용 가능
            int totalDuration = Mathf.Max(0, duration + power);
            return new StunEffectCommand(totalDuration, GetIcon());
        }

        public override void ApplyEffect(ICardExecutionContext context, int value, ICombatTurnManager turnManager = null)
        {
            if (context?.Target == null)
            {
                Debug.LogWarning("[StunEffectSO] 대상이 null입니다. 스턴 적용 실패.");
                return;
            }

            var stun = new StunDebuff(Mathf.Max(1, value > 0 ? value : duration), GetIcon());
            if (context.Target.RegisterStatusEffect(stun))
            {
                Game.CoreSystem.Utility.GameLogger.LogInfo($"[StunEffectSO] {context.Target.GetCharacterName()}에게 스턴 적용 ({stun.RemainingTurns}턴)", Game.CoreSystem.Utility.GameLogger.LogCategory.SkillCard);
            }
            else
            {
                Game.CoreSystem.Utility.GameLogger.LogInfo($"[StunEffectSO] {context.Target.GetCharacterName()}의 보호 상태로 스턴 차단", Game.CoreSystem.Utility.GameLogger.LogCategory.SkillCard);
            }
        }
    }
}


