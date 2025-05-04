using UnityEngine;
using Game.Interface;
using Game.Characters;
using Game.Battle;
using Game.Managers;
using Game.Effect;

namespace Game.Effects
{
    /// <summary>
    /// 적이 다음 턴에 전투 슬롯을 강제로 선점하도록 예약하는 이펙트입니다.
    /// </summary>
    [CreateAssetMenu(menuName = "CardEffect/ForceNextSlot")]
    public class ForceNextSlotEffectSO : ScriptableObject, ICardEffect
    {
        public void ExecuteEffect(CharacterBase caster, CharacterBase target, int value)
        {
            // 적의 다음 선공 슬롯 예약 (기본은 FRONT)
            BattleTurnManager.Instance?.ReserveEnemySlot(SlotPosition.FRONT);
            Debug.Log("[ForceNextSlotEffectSO] 적이 다음 턴에 선공 슬롯을 예약했습니다.");
        }
    }
}
