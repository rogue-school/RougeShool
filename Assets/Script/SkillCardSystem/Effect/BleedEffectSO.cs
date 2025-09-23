using UnityEngine;
using Game.SkillCardSystem.Interface;
using UnityEngine.Serialization;
using Game.SkillCardSystem.Effect;
using Game.CombatSystem.Interface;
using Game.CoreSystem.Utility;

namespace Game.SkillCardSystem.Effect
{
    /// <summary>
    /// 출혈 효과를 정의하는 스킬 카드 이펙트 ScriptableObject입니다.
    /// 대상에게 매 턴 피해를 주는 <see cref="BleedEffect"/>를 적용합니다.
    /// </summary>
    [CreateAssetMenu(fileName = "BleedEffect", menuName = "SkillEffects/BleedEffect")]
    public class BleedEffectSO : SkillCardEffectSO
    {
        [Header("출혈 수치 설정")]
        [Tooltip("기본 출혈 피해량")]
        [SerializeField] private int bleedAmount;

        [Tooltip("출혈 지속 턴 수")]
        [SerializeField] private int duration;

        // 레거시 마이그레이션: 과거 자식 필드명(icon)에서 부모(effectIcon)로 이전
        [FormerlySerializedAs("icon")]
        [SerializeField, HideInInspector] private Sprite legacyIcon;

        // 아이콘은 상위 SkillCardEffectSO.icon을 사용

        /// <summary>
        /// 출혈 이펙트 커맨드를 생성합니다. 파워 수치를 기반으로 피해량이 증가합니다.
        /// </summary>
        /// <param name="power">외부에서 전달된 파워 수치</param>
        /// <returns>출혈 커맨드 객체</returns>
        public override ICardEffectCommand CreateEffectCommand(int power)
        {
            var soIcon = GetIcon();
            if (soIcon == null)
            {
                Game.CoreSystem.Utility.GameLogger.LogWarning($"[BleedEffectSO] Icon이 비어 있습니다. SO 이름='{name}'", Game.CoreSystem.Utility.GameLogger.LogCategory.SkillCard);
            }
            return new BleedEffectCommand(bleedAmount + power, duration, soIcon);
        }

        /// <summary>
        /// 출혈 효과를 즉시 적용합니다. 커맨드가 아닌 직접 실행 방식입니다.
        /// </summary>
        /// <param name="context">카드 실행 컨텍스트</param>
        /// <param name="value">적용할 피해량</param>
        /// <param name="controller">전투 턴 관리자 (필요 시)</param>
        public override void ApplyEffect(ICardExecutionContext context, int value, ICombatTurnManager controller = null)
        {
            if (context?.Target == null)
            {
                GameLogger.LogWarning("[BleedEffectSO] 대상이 null이므로 출혈 적용 불가", GameLogger.LogCategory.SkillCard);
                return;
            }

            var bleed = new BleedEffect(value, duration, GetIcon());
            
            // 가드 상태 확인하여 상태이상 효과 등록
            if (context.Target.RegisterStatusEffect(bleed))
            {
                GameLogger.LogInfo($"[BleedEffectSO] {context.Target.GetCharacterName()}에게 출혈 {value} 적용 (지속 {duration}턴)", GameLogger.LogCategory.SkillCard);
            }
            else
            {
                GameLogger.LogInfo($"[BleedEffectSO] {context.Target.GetCharacterName()}의 가드로 출혈 효과 차단됨", GameLogger.LogCategory.SkillCard);
            }
        }

        private void OnValidate()
        {
            // 에셋을 열었을 때 레거시 아이콘이 남아 있으면 부모 필드로 이관
            if (effectIcon == null && legacyIcon != null)
            {
                effectIcon = legacyIcon;
                legacyIcon = null;
                #if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(this);
                #endif
            }
        }
    }
}
