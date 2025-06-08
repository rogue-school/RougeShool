using UnityEngine;
using Game.CombatSystem.Slot;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.Effect;
using Game.CombatSystem.Interface;
using Game.SkillCardSystem.Effects;

namespace Game.SkillCardSystem.Effect
{
    /// <summary>
    /// 다음 적 카드의 슬롯 위치를 강제로 지정하는 스킬 효과 ScriptableObject입니다.
    /// 적 카드에만 적용되며, 지정된 슬롯에 카드가 배치되도록 유도합니다.
    /// </summary>
    [CreateAssetMenu(fileName = "ForceNextSlotEffect", menuName = "SkillEffects/ForceNextSlotEffect")]
    public class ForceNextSlotEffectSO : SkillCardEffectSO
    {
        [Header("강제 슬롯 설정")]
        [Tooltip("다음 적 카드가 배치될 강제 슬롯 위치")]
        [SerializeField] private CombatSlotPosition forcedSlot = CombatSlotPosition.FIRST;

        /// <summary>
        /// 해당 효과를 위한 커맨드 객체를 생성합니다.
        /// </summary>
        /// <param name="power">이펙트 파워 값 (미사용)</param>
        /// <returns>슬롯 지정 커맨드</returns>
        public override ICardEffectCommand CreateEffectCommand(int power)
        {
            return new ForceNextSlotEffectCommand(forcedSlot);
        }

        /// <summary>
        /// 턴 매니저를 통해 다음 적 슬롯을 강제 지정합니다.
        /// </summary>
        /// <param name="context">카드 실행 컨텍스트</param>
        /// <param name="value">강제할 슬롯의 enum 인덱스 값</param>
        /// <param name="turnManager">전투 턴 관리자</param>
        public override void ApplyEffect(ICardExecutionContext context, int value, ICombatTurnManager turnManager = null)
        {
            if (turnManager == null)
            {
                Debug.LogWarning("[ForceNextSlotEffectSO] CombatTurnManager가 null입니다.");
                return;
            }

            CombatSlotPosition slot = forcedSlot;

            // value로 덮어씌우고 싶을 때만 사용
            if (System.Enum.IsDefined(typeof(CombatSlotPosition), value))
            {
                slot = (CombatSlotPosition)value;
            }

            turnManager.ReserveNextEnemySlot(slot);
            Debug.Log($"[ForceNextSlotEffectSO] 다음 적 행동 슬롯 강제 설정됨: {slot}");
        }
    }
}
