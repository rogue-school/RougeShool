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
        private Dictionary<SkillCardSlotPosition, SkillCardUI> cardUIs;

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
            if (currentEnemy?.Data == null)
            {
                Debug.LogError("[EnemyHandManager] 적 캐릭터 또는 데이터가 null입니다.");
                return;
            }

            if (cardUIPrefab == null)
                cardUIPrefab = Resources.Load<SkillCardUI>("UI/SkillCardUI");

            handSlots = SlotRegistry.Instance.GetHandSlots(SlotOwner.ENEMY);
            cardUIs = new Dictionary<SkillCardSlotPosition, SkillCardUI>();

            Debug.Log($"[EnemyHandManager] 슬롯 초기화 완료 - 슬롯 수: {handSlots?.Count}");
        }

        /// <summary>
        /// 핸드 슬롯에 초기 카드를 생성하여 채웁니다.
        /// </summary>
        public void GenerateInitialHand()
        {
            if (currentEnemy?.Data == null)
            {
                Debug.LogError("[EnemyHandManager] 적 정보가 없어서 핸드 생성 불가");
                return;
            }

            foreach (var kvp in handSlots)
            {
                CreateCardInSlot(kvp.Key);
            }

            Debug.Log("[EnemyHandManager] 핸드 카드 생성 완료");
        }

        /// <summary>
        /// 특정 슬롯에 새 카드를 생성하고 UI를 배치합니다.
        /// </summary>
        private void CreateCardInSlot(SkillCardSlotPosition position)
        {
            var slot = handSlots[position];
            var entry = currentEnemy.Data.GetRandomEntry();

            if (entry?.card == null)
            {
                Debug.LogWarning($"[EnemyHandManager] 카드 정보가 유효하지 않음 - 슬롯: {position}");
                return;
            }

            var runtimeCard = SkillCardFactory.CreateEnemyCard(entry.card, entry.damage);
            runtimeCard.SetHandSlot(position);

            var cardUI = Instantiate(cardUIPrefab, ((MonoBehaviour)slot).transform);
            cardUI.SetCard(runtimeCard);

            slot.SetCard(runtimeCard);
            cardUIs[position] = cardUI;

            Debug.Log($"[EnemyHandManager] 카드 생성 완료 - 슬롯: {position}, 카드: {runtimeCard.GetCardName()}");
        }

        /// <summary>
        /// 전투에 사용할 고정 슬롯의 카드 반환 (ENEMY_SLOT_1)
        /// </summary>
        public ISkillCard GetCardForCombat()
        {
            return GetSlotCard(SkillCardSlotPosition.ENEMY_SLOT_1);
        }

        public ISkillCard GetSlotCard(SkillCardSlotPosition position)
        {
            return handSlots.TryGetValue(position, out var slot) ? slot.GetCard() : null;
        }

        /// <summary>
        /// 적 핸드 슬롯들을 앞으로 밀고 새 카드를 뒤에 추가합니다.
        /// </summary>
        public void AdvanceSlots()
        {
            ShiftCard(SkillCardSlotPosition.ENEMY_SLOT_2, SkillCardSlotPosition.ENEMY_SLOT_1);
            ShiftCard(SkillCardSlotPosition.ENEMY_SLOT_3, SkillCardSlotPosition.ENEMY_SLOT_2);

            CreateCardInSlot(SkillCardSlotPosition.ENEMY_SLOT_3);

            Debug.Log("[EnemyHandManager] 슬롯 전진 및 새 카드 추가 완료");
        }

        /// <summary>
        /// 한 슬롯의 카드를 다른 슬롯으로 이동합니다.
        /// </summary>
        private void ShiftCard(SkillCardSlotPosition from, SkillCardSlotPosition to)
        {
            var fromSlot = handSlots[from];
            var toSlot = handSlots[to];

            var card = fromSlot.GetCard();
            toSlot.SetCard(card);
            card?.SetHandSlot(to);

            if (cardUIs.TryGetValue(from, out var cardUI))
            {
                cardUIs[to] = cardUI;
                cardUIs.Remove(from);
                cardUI.transform.SetParent(((MonoBehaviour)toSlot).transform);
                cardUI.transform.localPosition = Vector3.zero;
                cardUI.transform.localScale = Vector3.one;
            }
        }

        /// <summary>
        /// 특정 인덱스의 카드 UI를 반환합니다. (0: ENEMY_SLOT_1)
        /// </summary>
        public SkillCardUI GetCardUI(int index)
        {
            var slotPos = index switch
            {
                0 => SkillCardSlotPosition.ENEMY_SLOT_1,
                1 => SkillCardSlotPosition.ENEMY_SLOT_2,
                2 => SkillCardSlotPosition.ENEMY_SLOT_3,
                _ => SkillCardSlotPosition.ENEMY_SLOT_1
            };

            return cardUIs.TryGetValue(slotPos, out var ui) ? ui : null;
        }
    }
}