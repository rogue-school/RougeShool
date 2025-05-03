using System.Collections.Generic;
using UnityEngine;
using Game.Interface;
using Game.Battle;
using Game.Characters;

namespace Game.Battle
{
    /// <summary>
    /// 전투의 턴 흐름, 슬롯 예약, 방어 상태 등을 제어합니다.
    /// </summary>
    public class BattleTurnManager : MonoBehaviour
    {
        public static BattleTurnManager Instance { get; private set; }

        private Dictionary<SlotPosition, CharacterBase> reservedEnemySlots = new();
        private bool playerBlockActive = false;
        private bool enemyBlockActive = false;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            reservedEnemySlots = new Dictionary<SlotPosition, CharacterBase>
            {
                { SlotPosition.Front, null },
                { SlotPosition.Back, null }
            };
        }

        /// <summary>
        /// 플레이어의 방어 상태를 활성화합니다.
        /// </summary>
        public void ActivatePlayerBlock()
        {
            playerBlockActive = true;
        }

        /// <summary>
        /// 적의 방어 상태를 활성화합니다.
        /// </summary>
        public void ActivateEnemyBlock()
        {
            enemyBlockActive = true;
        }

        /// <summary>
        /// 적의 공격을 플레이어가 방어할 수 있다면 true 반환하고 상태 해제
        /// </summary>
        public bool ConsumePlayerBlock()
        {
            if (playerBlockActive)
            {
                playerBlockActive = false;
                return true;
            }
            return false;
        }

        /// <summary>
        /// 플레이어의 공격을 적이 방어할 수 있다면 true 반환하고 상태 해제
        /// </summary>
        public bool ConsumeEnemyBlock()
        {
            if (enemyBlockActive)
            {
                enemyBlockActive = false;
                return true;
            }
            return false;
        }

        /// <summary>
        /// 적이 다음 턴에 슬롯을 강제로 선점하도록 예약합니다.
        /// </summary>
        public void ReserveEnemySlot(SlotPosition slot, CharacterBase enemy)
        {
            reservedEnemySlots[slot] = enemy;
            Debug.Log($"[BattleTurnManager] {slot} 슬롯이 {enemy.name}에게 예약되었습니다.");
        }

        /// <summary>
        /// 해당 슬롯에 예약된 적을 반환합니다. (없으면 null)
        /// </summary>
        public CharacterBase GetReservedEnemy(SlotPosition slot)
        {
            return reservedEnemySlots.TryGetValue(slot, out var enemy) ? enemy : null;
        }

        /// <summary>
        /// 모든 슬롯 예약을 초기화합니다.
        /// </summary>
        public void ClearAllReservations()
        {
            foreach (var key in reservedEnemySlots.Keys)
                reservedEnemySlots[key] = null;
        }

        /// <summary>
        /// 전투의 새 턴을 시작합니다.
        /// (추후 확장: 턴 순서 정렬, 카드 활성화 등)
        /// </summary>
        public void StartNewTurn()
        {
            Debug.Log("[BattleTurnManager] 새로운 턴을 시작합니다.");

            // 슬롯 예약 초기화
            ClearAllReservations();

            // 필요 시 턴 순서 확정, 카드 효과 활성화 등 추가
        }
    }
}
