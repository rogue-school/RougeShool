using Game.Battle;
using Game.Characters;
using Game.Interface;
using Game.Managers;

namespace Game.Effects
{
    /// <summary>
    /// 적이 다음 턴에 특정 슬롯을 강제로 선점하도록 예약하는 효과입니다.
    /// 일반적으로 선공 슬롯(SlotPosition.Front)을 예약합니다.
    /// </summary>
    public class ForceNextSlotEffect : ICardEffect
    {
        private readonly SlotPosition reservedSlot;

        public ForceNextSlotEffect(SlotPosition slot = SlotPosition.Front)
        {
            reservedSlot = slot;
        }

        public void ExecuteEffect(CharacterBase caster, CharacterBase target)
        {
            if (caster.CompareTag("Enemy"))
            {
                BattleTurnManager.Instance.ReserveEnemySlot(reservedSlot, caster);
                UnityEngine.Debug.Log($"[ForceNextSlotEffect] {reservedSlot} 슬롯을 {caster.name}이 다음 턴에 강제로 선점합니다.");
            }
        }
    }
}
