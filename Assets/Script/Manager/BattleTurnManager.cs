using UnityEngine;
using Game.Managers;
using Game.Interface;
using Game.Battle;
using Game.BattleStates;

namespace Game.Managers
{
    /// <summary>
    /// 전투 턴 상태 관리 매니저입니다.
    /// </summary>
    public class BattleTurnManager : MonoBehaviour
    {
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

        public void SetState(IBattleTurnState newState)
        {
            currentState = newState;
            Debug.Log($"[BattleTurnManager] 상태 전환: {newState.GetType().Name}");
        }

        public void RegisterPlayerGuard()
        {
            Debug.Log("[BattleTurnManager] 플레이어 방어 상태 등록");
            SetState(new Game.BattleStates.PlayerGuardedState(this)); // 명시적 네임스페이스 지정
        }

        public void ReserveEnemySlot(BattleSlotPosition slot)
        {
            Debug.Log($"[BattleTurnManager] 적이 슬롯 {slot}을 예약함");
            // 슬롯 예약 처리 구현 필요
        }
    }
}
