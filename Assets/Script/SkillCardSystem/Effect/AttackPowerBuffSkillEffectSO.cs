using UnityEngine;
using Game.SkillCardSystem.Interface;
using Game.CombatSystem.Interface;
using Game.SkillCardSystem.Data;

namespace Game.SkillCardSystem.Effect
{
    /// <summary>
    /// 시전자에게 공격력 증가 버프를 부여하는 스킬 카드 효과입니다.
    /// ItemSystem의 AttackPowerBuffEffect를 재사용하여 데미지 계산과 연동됩니다.
    /// </summary>
    [CreateAssetMenu(fileName = "AttackPowerBuffSkillEffect", menuName = "SkillEffects/AttackPowerBuffSkillEffect")]
    public class AttackPowerBuffSkillEffectSO : SkillCardEffectSO
    {
        #region 필드

        [Header("공격력 버프 설정")]
        [Tooltip("공격력 증가 수치")]
        [SerializeField] private int _attackPowerBonus = 3;

        [Tooltip("버프 지속 턴 수")]
        [SerializeField] private int _duration = 1;

        [Header("버프 아이콘")]
        [Tooltip("공격력 증가 버프 UI 아이콘")]
        [SerializeField] private Sprite _icon;

        #endregion

        #region 프로퍼티

        /// <summary>
        /// 기본 공격력 증가 수치를 반환합니다.
        /// </summary>
        public int DefaultAttackPowerBonus => Mathf.Max(0, _attackPowerBonus);

        /// <summary>
        /// 기본 버프 지속 턴 수를 반환합니다.
        /// </summary>
        public int DefaultDuration => Mathf.Max(1, _duration);

        #endregion

        /// <summary>
        /// 커맨드 방식 실행을 위한 이펙트 커맨드를 생성합니다.
        /// </summary>
        /// <param name="power">추가 파워 수치 (버프 수치 보정용)</param>
        /// <returns>공격력 버프 적용 커맨드</returns>
        public override ICardEffectCommand CreateEffectCommand(int power)
        {
            return CreateEffectCommand(power, null);
        }

        /// <summary>
        /// 직접 적용은 사용하지 않습니다. 실제 로직은 커맨드에서 처리합니다.
        /// </summary>
        public override void ApplyEffect(ICardExecutionContext context, int value, ICombatTurnManager controller = null)
        {
            // 실제 효과는 AttackPowerBuffSkillCommand에서 처리합니다.
        }

        /// <summary>
        /// 커스텀 설정을 포함해 커맨드를 생성합니다.
        /// </summary>
        /// <param name="power">추가 파워 수치</param>
        /// <param name="customSettings">카드별 커스텀 설정</param>
        /// <returns>공격력 버프 적용 커맨드</returns>
        public ICardEffectCommand CreateEffectCommand(int power, EffectCustomSettings customSettings)
        {
            int bonusFromSo = Mathf.Max(0, _attackPowerBonus + power);
            int durationFromSo = Mathf.Max(1, _duration);
            Sprite iconFromSo = _icon ?? effectIcon;

            if (customSettings != null)
            {
                // 공격력 증가량: damageAmount 사용 (0 이면 SO 값 사용)
                if (customSettings.damageAmount > 0)
                {
                    bonusFromSo = customSettings.damageAmount + Mathf.Max(0, power);
                }

                // 지속 턴: guardDuration 우선 사용 (0/음수면 SO 값 유지)
                if (customSettings.guardDuration > 0)
                {
                    durationFromSo = customSettings.guardDuration;
                }

                // 아이콘: guardIcon → bleedIcon → SO 기본 순으로 선택
                if (customSettings.guardIcon != null)
                {
                    iconFromSo = customSettings.guardIcon;
                }
                else if (customSettings.bleedIcon != null)
                {
                    iconFromSo = customSettings.bleedIcon;
                }
            }

            int finalBonus = Mathf.Max(0, bonusFromSo);
            int finalDuration = Mathf.Max(1, durationFromSo);

            // 효과 이름: SO의 effectName을 우선 사용, 비어 있으면 에셋 이름 사용
            string effectDisplayName = GetEffectName();
            if (string.IsNullOrWhiteSpace(effectDisplayName))
            {
                effectDisplayName = name;
            }

            return new AttackPowerBuffSkillCommand(finalBonus, finalDuration, iconFromSo, effectDisplayName);
        }
    }
}


