using UnityEngine;
using Game.Battle;
using Game.Managers;
using Game.Interface;
using Game.BattleStates;

namespace Game.Managers
{
    /// <summary>
    /// 전투 턴 상태 관리 매니저입니다.
    /// </summary>
    public class BattleTurnManager : MonoBehaviour
    {
        // 싱글톤 인스턴스 추가
        public static BattleTurnManager Instance { get; private set; }

        private IBattleTurnState currentState;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        /// <summary>
        /// 턴 상태를 전환합니다.
        /// </summary>
        public void SetState(IBattleTurnState newState)
        {
            currentState = newState;
            Debug.Log($"[BattleTurnManager] 상태 전환: {newState.GetType().Name}");
        }

        /// <summary>
        /// 플레이어가 방어 상태일 때 상태 등록합니다.
        /// </summary>
        public void RegisterPlayerGuard()
        {
            Debug.Log("[BattleTurnManager] 플레이어 방어 상태 등록");
            SetState(new Game.BattleStates.PlayerGuardedState(this));
        }

        /// <summary>
        /// 적의 다음 턴 슬롯을 예약합니다.
        /// </summary>
        public void ReserveEnemySlot(SlotPosition slot)
        {
            Debug.Log($"[BattleTurnManager] 적이 슬롯 {slot}을 예약함");
            // 예약 처리는 BattleSlotManager 등에서 연동
        }
    }
}
