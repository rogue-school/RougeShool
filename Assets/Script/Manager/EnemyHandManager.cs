using UnityEngine;
using Game.Cards;
using Game.Interface;
using Game.UI;
using Game.Enemy;
using Game.Utility;
using Game.Slots;
using System.Collections.Generic;

namespace Game.Managers
{
    /// <summary>
    /// 적의 핸드 카드와 슬롯을 관리합니다.
    /// SkillCardSlotPosition Enum 기반으로 슬롯을 명확하게 연결합니다.
    /// </summary>
    public class EnemyHandManager : MonoBehaviour
    {
        private EnemySkillDeck enemyDeck;
        private SkillCardUI cardUIPrefab;
        private Dictionary<SkillCardSlotPosition, EnemyCardSlotUI> handSlotDict;

        private void Awake()
        {
            enemyDeck = Resources.Load<EnemySkillDeck>("EnemyDecks/DefaultEnemyDeck");
            cardUIPrefab = Resources.Load<SkillCardUI>("UI/SkillCardUI");

            BindSlotsFromScene();
        }

        private void Start()
        {
            GenerateInitialHand();
        }

        private void BindSlotsFromScene()
        {
            handSlotDict = new Dictionary<SkillCardSlotPosition, EnemyCardSlotUI>();

            var allSlots = FindObjectsOfType<EnemyCardSlotUI>();
            foreach (var slot in allSlots)
            {
                if (!handSlotDict.ContainsKey(slot.Position))
                {
                    handSlotDict.Add(slot.Position, slot);
                }
            }

            Debug.Log($"[EnemyHandManager] 슬롯 자동 등록 완료: {handSlotDict.Count}개");
        }

        public void GenerateInitialHand()
        {
            if (enemyDeck == null || enemyDeck.cards.Count == 0) return;

            foreach (var kvp in handSlotDict)
            {
                var so = enemyDeck.GetRandomCard();
                int damage = UnityEngine.Random.Range(5, 15);
                var runtimeCard = SkillCardFactory.CreateEnemyCard(so, damage);

                var cardUI = Instantiate(cardUIPrefab, kvp.Value.transform);
                cardUI.SetCard(runtimeCard);

                kvp.Value.SetCard(runtimeCard);
            }
        }

        public ISkillCard GetSlotCard(SkillCardSlotPosition pos)
        {
            return handSlotDict.TryGetValue(pos, out var slot) ? slot.GetCard() : null;
        }

        public ISkillCard GetCardForCombat()
        {
            return GetSlotCard(SkillCardSlotPosition.ENEMY_SLOT_1);
        }

        public void AdvanceSlots()
        {
            ISkillCard card2 = GetSlotCard(SkillCardSlotPosition.ENEMY_SLOT_2);
            ISkillCard card3 = GetSlotCard(SkillCardSlotPosition.ENEMY_SLOT_3);

            // 1 <- 2, 2 <- 3
            if (handSlotDict.TryGetValue(SkillCardSlotPosition.ENEMY_SLOT_1, out var slot1))
                slot1.SetCard(card2);

            if (handSlotDict.TryGetValue(SkillCardSlotPosition.ENEMY_SLOT_2, out var slot2))
                slot2.SetCard(card3);

            // 3 <- 새 카드
            if (handSlotDict.TryGetValue(SkillCardSlotPosition.ENEMY_SLOT_3, out var slot3))
            {
                var so = enemyDeck.GetRandomCard();
                var newCard = SkillCardFactory.CreateEnemyCard(so, UnityEngine.Random.Range(5, 15));
                slot3.SetCard(newCard);
            }
        }
    }
}
