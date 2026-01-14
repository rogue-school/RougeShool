using UnityEngine;
using Game.SkillCardSystem.Interface;
using Game.CombatSystem.Interface;

namespace Game.SkillCardSystem.Effect
{
    /// <summary>
    /// 시공의 폭풍 효과 SO입니다.
    /// 플레이어에게 3턴 동안 목표 데미지를 입혀야 하는 기믹을 적용합니다.
    /// </summary>
    [CreateAssetMenu(fileName = "StormOfSpaceTimeEffect", menuName = "SkillEffects/StormOfSpaceTimeEffect")]
    public class StormOfSpaceTimeEffectSO : SkillCardEffectSO
    {
        [Header("시공의 폭풍 설정")]
        [Tooltip("목표 데미지 수치")]
        [SerializeField] private int targetDamage = 30;

        [Tooltip("지속 턴 수")]
        [SerializeField] private int duration = 3;

        public override ICardEffectCommand CreateEffectCommand(int power)
        {
            return new StormOfSpaceTimeEffectCommand(targetDamage, duration, GetIcon());
        }

        public override void ApplyEffect(ICardExecutionContext context, int value, ICombatTurnManager controller = null)
        {
            Debug.LogWarning("[StormOfSpaceTimeEffectSO] ApplyEffect는 레거시 메서드입니다. StormOfSpaceTimeEffectCommand를 사용하세요.");
        }
    }
}

