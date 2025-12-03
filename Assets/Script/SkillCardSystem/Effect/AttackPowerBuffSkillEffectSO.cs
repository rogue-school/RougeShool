using UnityEngine;
using Game.SkillCardSystem.Interface;
using Game.CombatSystem.Interface;

namespace Game.SkillCardSystem.Effect
{
    /// <summary>
    /// 시전자에게 공격력 증가 버프를 부여하는 스킬 카드 효과입니다.
    /// ItemSystem의 AttackPowerBuffEffect를 재사용하여 데미지 계산과 연동됩니다.
    /// </summary>
    [CreateAssetMenu(fileName = "AttackPowerBuffSkillEffect", menuName = "SkillEffects/AttackPowerBuffSkillEffect")]
    public class AttackPowerBuffSkillEffectSO : SkillCardEffectSO
    {
        [Header("공격력 버프 설정")]
        [Tooltip("공격력 증가 수치")]
        [SerializeField] private int _attackPowerBonus = 3;

        [Tooltip("버프 지속 턴 수")]
        [SerializeField] private int _duration = 1;

        [Header("버프 아이콘")]
        [Tooltip("공격력 증가 버프 UI 아이콘")]
        [SerializeField] private Sprite _icon;

        /// <summary>
        /// 커맨드 방식 실행을 위한 이펙트 커맨드를 생성합니다.
        /// </summary>
        /// <param name="power">추가 파워 수치 (버프 수치 보정용)</param>
        /// <returns>공격력 버프 적용 커맨드</returns>
        public override ICardEffectCommand CreateEffectCommand(int power)
        {
            int finalBonus = Mathf.Max(0, _attackPowerBonus + power);
            int finalDuration = Mathf.Max(1, _duration);
            return new AttackPowerBuffSkillCommand(finalBonus, finalDuration, _icon);
        }

        /// <summary>
        /// 직접 적용은 사용하지 않습니다. 실제 로직은 커맨드에서 처리합니다.
        /// </summary>
        public override void ApplyEffect(ICardExecutionContext context, int value, ICombatTurnManager controller = null)
        {
            // 실제 효과는 AttackPowerBuffSkillCommand에서 처리합니다.
        }
    }
}


