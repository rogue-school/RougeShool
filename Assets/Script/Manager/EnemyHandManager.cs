using UnityEngine;
using System.Collections.Generic;
using Game.Cards;
using Game.Interface;
using Game.Managers;
using Game.Slots;
using Game.UI;
using Game.Utility;

namespace Game.Managers
{
    /// <summary>
    /// 적의 핸드 카드와 슬롯을 관리합니다.
    /// SkillCardSlotPosition Enum 기반으로 슬롯을 명확하게 연결합니다.
    /// </summary>
    public class EnemyHandManager : MonoBehaviour
    {
        public static EnemyHandManager Instance { get; private set; }

        [SerializeField] private EnemySkillDeck enemyDeck;
        [SerializeField] private SkillCardUI cardUIPrefab;

        private Dictionary<SkillCardSlotPosition, IHandCardSlot> handSlots;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }
            Instance = this;

            if (enemyDeck == null)
                enemyDeck = Resources.Load<EnemySkillDeck>("EnemyDecks/DefaultEnemyDeck");

            if (cardUIPrefab == null)
                cardUIPrefab = Resources.Load<SkillCardUI>("UI/SkillCardUI");

            handSlots = SlotRegistry.Instance.GetHandSlots(SlotOwner.ENEMY);
        }

        private void Start()
        {
            GenerateInitialHand();
        }

        public void GenerateInitialHand()
        {
            foreach (var kvp in handSlots)
            {
                var so = enemyDeck.GetRandomCard();
                var runtimeCard = SkillCardFactory.CreateEnemyCard(so, Random.Range(5, 15));

                var cardUI = Instantiate(cardUIPrefab, ((MonoBehaviour)kvp.Value).transform);
                cardUI.SetCard(runtimeCard);

                kvp.Value.SetCard(runtimeCard);
            }
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
            handSlots[SkillCardSlotPosition.ENEMY_SLOT_2].SetCard(card3);

            var so = enemyDeck.GetRandomCard();
            var newCard = SkillCardFactory.CreateEnemyCard(so, Random.Range(5, 15));
            handSlots[SkillCardSlotPosition.ENEMY_SLOT_3].SetCard(newCard);
        }
    }
}
