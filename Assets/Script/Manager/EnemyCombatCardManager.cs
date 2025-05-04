using UnityEngine;
using System;
using Game.Interface;
using Game.Managers;
using Game.UI;
using Game.Enemy;

namespace Game.Battle
{
    /// <summary>
    /// 적의 전투 턴을 관리하며, 적의 스킬 카드를 슬롯에 배치 및 실행합니다.
    /// </summary>
    public class EnemyCombatCardManager : MonoBehaviour
    {
        [SerializeField] private EnemyHandManager handManager;
        [SerializeField] private EnemyCardSlotUI[] combatSlots;

        private void Awake()
        {
            AutoBindReferences();
        }

        /// <summary>
        /// 핸드 매니저 및 전투 슬롯들을 자동으로 참조합니다.
        /// </summary>
        private void AutoBindReferences()
        {
            if (handManager == null)
                handManager = FindObjectOfType<EnemyHandManager>();

            if (combatSlots == null || combatSlots.Length == 0)
            {
                combatSlots = FindObjectsOfType<EnemyCardSlotUI>();
                Array.Sort(combatSlots, (a, b) => a.name.CompareTo(b.name));
            }

            Debug.Log($"[EnemyCombatCardManager] 참조 완료 - 핸드: {(handManager != null)} / 슬롯: {combatSlots.Length}개");
        }

        /// <summary>
        /// 적의 선공 카드를 전투 슬롯에 올립니다. (보통 index 0)
        /// </summary>
        public void PrepareEnemyCard()
        {
            ISkillCard card = handManager.GetCardForCombat();
            if (card != null && combatSlots.Length > 0)
            {
                combatSlots[0].SetCard(card);
                Debug.Log($"[EnemyCombatCardManager] 적 카드 슬롯에 배치 완료: {card.GetCardName()}");
            }
        }

        /// <summary>
        /// 전투가 끝난 후 적의 핸드를 갱신합니다.
        /// </summary>
        public void EndEnemyTurn()
        {
            handManager.AdvanceSlots();
            Debug.Log("[EnemyCombatCardManager] 적 핸드 슬롯이 갱신되었습니다.");
        }

        /// <summary>
        /// 수동으로 슬롯에 카드 지정 (디버깅/테스트용)
        /// </summary>
        public void SetCardManually(int slotIndex, ISkillCard card)
        {
            if (slotIndex >= 0 && slotIndex < combatSlots.Length)
                combatSlots[slotIndex].SetCard(card);
        }

        /// <summary>
        /// 현재 전투 슬롯 수 반환
        /// </summary>
        public int GetSlotCount() => combatSlots.Length;
    }
}
