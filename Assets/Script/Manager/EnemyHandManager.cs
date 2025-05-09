using UnityEngine;
using System.Collections.Generic;
using Game.Cards;
using Game.Interface;
using Game.Slots;
using Game.UI;
using Game.Utility;
using Game.Enemy;
using Game.Data;

namespace Game.Managers
{
    /// <summary>
    /// 적의 핸드 카드와 슬롯을 관리합니다.
    /// </summary>
    public class EnemyHandManager : MonoBehaviour
    {
        public static EnemyHandManager Instance { get; private set; }

        [SerializeField] private SkillCardUI cardUIPrefab;

        private Dictionary<SkillCardSlotPosition, IHandCardSlot> handSlots;
        private EnemyCharacter currentEnemy;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }
            Instance = this;
        }

        /// <summary>
        /// 적 캐릭터 정보를 기반으로 슬롯과 덱을 초기화합니다.
        /// </summary>
        public void Initialize(EnemyCharacter enemy)
        {
            currentEnemy = enemy;

            if (currentEnemy == null || currentEnemy.Data == null)
            {
                Debug.LogError("[EnemyHandManager] 적 캐릭터 또는 데이터가 null입니다.");
                return;
            }

            if (cardUIPrefab == null)
                cardUIPrefab = Resources.Load<SkillCardUI>("UI/SkillCardUI");

            handSlots = SlotRegistry.Instance.GetHandSlots(SlotOwner.ENEMY);
            Debug.Log($"[EnemyHandManager] 슬롯 초기화 완료 - 슬롯 수: {handSlots?.Count}");
        }

        public void GenerateInitialHand()
        {
            if (currentEnemy?.Data == null)
            {
                Debug.LogError("[EnemyHandManager] 적 정보가 없어서 핸드 생성 불가");
                return;
            }

            foreach (var kvp in handSlots)
            {
                var slotPosition = kvp.Key;
                var slot = kvp.Value;

                var entry = currentEnemy.Data.GetRandomEntry();
                if (entry == null || entry.card == null)
                {
                    Debug.LogWarning("[EnemyHandManager] 카드 정보가 유효하지 않습니다.");
                    continue;
                }

                var runtimeCard = SkillCardFactory.CreateEnemyCard(entry.card, entry.damage);
                runtimeCard.SetHandSlot(slotPosition);

                var cardUI = Instantiate(cardUIPrefab, ((MonoBehaviour)slot).transform);
                cardUI.SetCard(runtimeCard);

                slot.SetCard(runtimeCard);

                Debug.Log($"[EnemyHandManager] 카드 생성 완료 - 슬롯: {slotPosition}, 카드: {runtimeCard.GetCardName()}");
            }

            Debug.Log("[EnemyHandManager] 핸드 카드 생성 완료");
        }

        public ISkillCard GetSlotCard(SkillCardSlotPosition position)
        {
            return handSlots.TryGetValue(position, out var slot) ? slot.GetCard() : null;
        }

        public ISkillCard GetCardForCombat()
        {
            return GetSlotCard(SkillCardSlotPosition.ENEMY_SLOT_1);
        }

        public void AdvanceSlots()
        {
            ISkillCard card2 = GetSlotCard(SkillCardSlotPosition.ENEMY_SLOT_2);
            ISkillCard card3 = GetSlotCard(SkillCardSlotPosition.ENEMY_SLOT_3);

            handSlots[SkillCardSlotPosition.ENEMY_SLOT_1].SetCard(card2);
            card2?.SetHandSlot(SkillCardSlotPosition.ENEMY_SLOT_1);

            handSlots[SkillCardSlotPosition.ENEMY_SLOT_2].SetCard(card3);
            card3?.SetHandSlot(SkillCardSlotPosition.ENEMY_SLOT_2);

            var entry = currentEnemy.Data.GetRandomEntry();
            if (entry == null || entry.card == null)
            {
                Debug.LogWarning("[EnemyHandManager] 새 카드 생성 실패");
                return;
            }

            var newCard = SkillCardFactory.CreateEnemyCard(entry.card, entry.damage);
            newCard.SetHandSlot(SkillCardSlotPosition.ENEMY_SLOT_3);
            handSlots[SkillCardSlotPosition.ENEMY_SLOT_3].SetCard(newCard);

            Debug.Log("[EnemyHandManager] 슬롯 전진 및 새 카드 추가 완료");
        }
    }
}
