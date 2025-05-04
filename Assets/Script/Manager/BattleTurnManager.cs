using UnityEngine;
using Game.Battle;

namespace Game.Managers
{
    /// <summary>
    /// 전투 턴 흐름을 관리하며, 방어 상태와 적의 슬롯 선점 예약 등을 담당합니다.
    /// </summary>
    public class BattleTurnManager : MonoBehaviour
    {
        public static BattleTurnManager Instance { get; private set; }

        private bool isPlayerGuarded = false;
        private SlotPosition? reservedEnemySlot = null;

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);
        }

        /// <summary>
        /// 플레이어가 방어 상태로 진입합니다.
        /// </summary>
        public void RegisterPlayerGuard()
        {
            isPlayerGuarded = true;
            Debug.Log("[BattleTurnManager] 플레이어가 방어 상태입니다.");
        }

        /// <summary>
        /// 플레이어의 방어 상태를 확인하고 해제합니다.
        /// </summary>
        public bool ConsumePlayerGuard()
        {
            bool result = isPlayerGuarded;
            isPlayerGuarded = false;
            return result;
        }

        /// <summary>
        /// 적이 다음 턴에 특정 슬롯을 선점하도록 예약합니다.
        /// </summary>
        public void ReserveEnemySlot(SlotPosition position)
        {
            reservedEnemySlot = position;
            Debug.Log($"[BattleTurnManager] 적이 다음 턴에 {position} 슬롯을 예약했습니다.");
        }

        /// <summary>
        /// 예약된 슬롯을 가져오고 초기화합니다.
        /// </summary>
        public SlotPosition? GetAndClearReservedEnemySlot()
        {
            var slot = reservedEnemySlot;
            reservedEnemySlot = null;
            return slot;
        }
    }
}
