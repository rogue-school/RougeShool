using UnityEngine;
using Game.SkillCardSystem.Interface;

namespace Game.SkillCardSystem.Effect
{
    /// <summary>
    /// 공격 스킬 사용 시 공격력 스택 버프를 관리하는 SO.
    /// 커맨드가 아닌, DamageEffectCommand에서 훅으로 관리하므로 여기서는 아이콘만 제공.
    /// 필요 시 다른 카드에서 수동으로 부여할 수도 있도록 커맨드도 제공.
    /// </summary>
    [CreateAssetMenu(fileName = "AttackPowerStackEffect", menuName = "SkillEffects/AttackPowerStackEffect")]
    public class AttackPowerStackEffectSO : SkillCardEffectSO
    {
        [Header("아이콘(선택)")]
        [SerializeField] private Sprite icon; // 현재는 사용하지 않음 (기본 아이콘 사용)

        public override ICardEffectCommand CreateEffectCommand(int power)
        {
            // 카드 자체 스택을 증가시키는 커맨드 반환
            return new GrantAttackPowerStackOnUseCommand(5);
        }

        public override void ApplyEffect(Game.CombatSystem.Interface.ICardExecutionContext context, int value, Game.CombatSystem.Interface.ICombatTurnManager controller = null) { }

        // 아이콘은 기본 SkillCardEffectSO.effectIcon 사용
    }
}


