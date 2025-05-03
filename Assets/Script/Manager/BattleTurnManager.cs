using UnityEngine;
using Game.Battle;
using Game.Interface;
using Game.UI;

namespace Game.Battle
{
    /// <summary>
    /// 전투 턴의 흐름과 상태를 제어하는 매니저 클래스입니다.
    /// 턴 시작, 종료, 가드 활성화, 슬롯 예약 등의 기능을 포함합니다.
    /// </summary>
    public class BattleTurnManager : MonoBehaviour
    {
        public static BattleTurnManager Instance { get; private set; }

        private bool playerGuardActive = false;
        private bool enemyGuardActive = false;

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
        /// 새 턴을 시작하며, 슬롯을 초기화하거나 적 AI 행동을 준비합니다.
        /// </summary>
        public void StartNewTurn()
        {
            Debug.Log("[BattleTurn] 새 턴 시작");

            // 여기서 필요한 초기화 로직이 있다면 추가
            playerGuardActive = false;
            enemyGuardActive = false;

            // 예: AI 스킬 선택, 카드 생성 등
        }

        /// <summary>
        /// 전투 턴을 종료합니다.
        /// </summary>
        public void EndTurn()
        {
            Debug.Log("[BattleTurn] 턴 종료");
            playerGuardActive = false;
            enemyGuardActive = false;
        }

        /// <summary>
        /// 플레이어의 가드 효과를 활성화합니다.
        /// </summary>
        public void ActivatePlayerGuard()
        {
            playerGuardActive = true;
            Debug.Log("[Guard] 플레이어 가드 활성화됨");
        }

        /// <summary>
        /// 적의 가드 효과를 활성화합니다.
        /// </summary>
        public void ActivateEnemyGuard()
        {
            enemyGuardActive = true;
            Debug.Log("[Guard] 적 가드 활성화됨");
        }

        /// <summary>
        /// 플레이어 가드를 소비하며 차단 여부를 반환합니다.
        /// </summary>
        public bool ConsumePlayerGuard()
        {
            if (playerGuardActive)
            {
                playerGuardActive = false;
                Debug.Log("[Guard] 플레이어 가드로 적의 공격 무효화");
                return true;
            }
            return false;
        }

        /// <summary>
        /// 적 가드를 소비하며 차단 여부를 반환합니다.
        /// </summary>
        public bool ConsumeEnemyGuard()
        {
            if (enemyGuardActive)
            {
                enemyGuardActive = false;
                Debug.Log("[Guard] 적 가드로 플레이어의 공격 무효화");
                return true;
            }
            return false;
        }

        /// <summary>
        /// 적 카드가 특정 슬롯에 예약되도록 설정합니다.
        /// </summary>
        public void ReserveEnemySlot(SlotPosition position, ISkillCard card)
        {
            BattleSlotManager.Instance.SetSlot(false, position, card);
            Debug.Log($"[BattleTurn] 적 슬롯 예약됨 - 위치: {position}, 카드: {card?.GetName()}");
        }

        /// <summary>
        /// 플레이어 카드가 특정 슬롯에 예약되도록 설정합니다.
        /// </summary>
        public void ReservePlayerSlot(SlotPosition position, ISkillCard card)
        {
            BattleSlotManager.Instance.SetSlot(true, position, card);
            Debug.Log($"[BattleTurn] 플레이어 슬롯 예약됨 - 위치: {position}, 카드: {card?.GetName()}");
        }
    }
}
