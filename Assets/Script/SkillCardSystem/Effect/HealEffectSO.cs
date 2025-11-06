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

        [Header("비주얼 이펙트")]
        [Tooltip("치유 시 시전자 위치에 표시할 비주얼 이펙트 프리팹")]
        [SerializeField] private GameObject visualEffectPrefab;

        /// <summary>
        /// 효과 명령을 생성합니다.
        /// </summary>
        /// <param name="power">추가 파워 수치</param>
        /// <returns>치유 효과 명령</returns>
        public override ICardEffectCommand CreateEffectCommand(int power)
        {
            return new HealEffectCommand(baseHealAmount + power, maxHealAmount, visualEffectPrefab);
        }
        
        /// <summary>
        /// EffectCustomSettings를 통해 효과 명령을 생성합니다.
        /// </summary>
        /// <param name="customSettings">커스텀 설정</param>
        /// <returns>치유 효과 명령</returns>
        public ICardEffectCommand CreateEffectCommand(Game.SkillCardSystem.Data.EffectCustomSettings customSettings)
        {
            // EffectCustomSettings의 이펙트/사운드가 있으면 우선 사용, 없으면 SO의 기본 이펙트 사용
            var effectPrefab = customSettings.healEffectPrefab ?? visualEffectPrefab;
            var sfxClip = customSettings.healSfxClip;
            return new HealEffectCommand(customSettings.healAmount, maxHealAmount, effectPrefab, sfxClip);
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
