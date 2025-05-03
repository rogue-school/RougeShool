using Game.Battle;
using Game.Characters;
using Game.Interface;

namespace Game.Effects
{
    /// <summary>
    /// 적이 다음 턴에 특정 슬롯을 강제로 선점하도록 예약하는 효과입니다.
    /// 일반적으로 선공 슬롯(SlotPosition.Front)을 예약합니다.
    /// </summary>
    public class ForceNextSlotEffect : ICardEffect
    {
        private readonly SlotPosition reservedSlot;
        private readonly ISkillCard sourceCard;

        /// <summary>
        /// 슬롯과 카드 정보를 받아 강제 슬롯 예약 효과를 구성합니다.
        /// </summary>
        public ForceNextSlotEffect(ISkillCard card, SlotPosition slot = SlotPosition.Front)
        {
            reservedSlot = slot;
            sourceCard = card;
        }

        public void ExecuteEffect(CharacterBase caster, CharacterBase target)
        {
            if (caster.CompareTag("Enemy"))
            {
                BattleTurnManager.Instance.ReserveEnemySlot(reservedSlot, sourceCard);
                UnityEngine.Debug.Log($"[ForceNextSlotEffect] {reservedSlot} 슬롯을 {caster.name}이 다음 턴에 강제로 선점합니다.");
            }
        }
    }
}
