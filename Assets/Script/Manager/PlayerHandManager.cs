using UnityEngine;
using System.Collections.Generic;
using Game.Interface;
using Game.UI.Hand;
using Game.Slots;
using Game.Combat.Turn;
using Game.Utility;
using Game.Cards;
using Game.UI;
using Game.Player;

namespace Game.Managers
{
    /// <summary>
    /// 플레이어의 핸드 슬롯과 카드 초기화를 담당하며,
    /// 카드 드래그 후 전투 슬롯 자동 배치를 지원합니다.
    /// </summary>
    public class PlayerHandManager : MonoBehaviour
    {
        [Header("카드 데이터")]
        [SerializeField] private PlayerSkillCard[] defaultCards;
        [SerializeField] private int[] defaultDamages;
        [SerializeField] private int[] defaultCoolTimes;

        [Header("UI 프리팹")]
        [SerializeField] private SkillCardUI skillCardUIPrefab;

        private Dictionary<SkillCardSlotPosition, IHandCardSlot> handSlots;

        /// <summary>
        /// 외부에서 명시적으로 슬롯 정보를 초기화합니다.
        /// </summary>
        public void Initialize()
        {
            handSlots = new Dictionary<SkillCardSlotPosition, IHandCardSlot>();

            foreach (var slot in SlotRegistry.Instance.GetAllHandSlots())
            {
                if (slot.GetOwner() == SlotOwner.PLAYER)
                {
                    handSlots[slot.GetSlotPosition()] = slot;
                }
            }

            Debug.Log($"[PlayerHandManager] 슬롯 초기화 완료: {handSlots.Count}개");
        }

        /// <summary>
        /// 전투 시작 시 초기 핸드 카드 생성 및 슬롯에 설정
        /// </summary>
        public void GenerateInitialHand()
        {
            Debug.Log("[PlayerHandManager] 초기 핸드 카드 생성 시작");

            ClearAll();

            // 슬롯 순서를 명확히 지정
            SkillCardSlotPosition[] orderedSlots = new[]
            {
                SkillCardSlotPosition.PLAYER_SLOT_1,
                SkillCardSlotPosition.PLAYER_SLOT_2,
                SkillCardSlotPosition.PLAYER_SLOT_3,
            };

            int slotCount = Mathf.Min(defaultCards.Length, defaultDamages.Length, defaultCoolTimes.Length, orderedSlots.Length);
            Debug.Log($"[PlayerHandManager] 슬롯 수: {handSlots.Count}, 카드 수: {defaultCards.Length}, 생성 슬롯 수: {slotCount}");

            for (int i = 0; i < slotCount; i++)
            {
                SkillCardSlotPosition slotPos = orderedSlots[i];

                if (!handSlots.TryGetValue(slotPos, out var slot))
                {
                    Debug.LogWarning($"[PlayerHandManager] 슬롯 {slotPos}를 찾을 수 없습니다.");
                    continue;
                }

                if (defaultCards[i] == null)
                {
                    Debug.LogError($"[PlayerHandManager] defaultCards[{i}] 가 null입니다!");
                    continue;
                }

                var runtimeCard = SkillCardFactory.CreatePlayerCard(defaultCards[i], defaultDamages[i], defaultCoolTimes[i]);
                Debug.Log($"[PlayerHandManager] 카드 생성 완료: {runtimeCard.GetCardName()} / 데미지: {defaultDamages[i]} / 쿨타임: {defaultCoolTimes[i]}");

                if (slot is PlayerHandCardSlotUI uiSlot)
                    uiSlot.InjectUIFactory(skillCardUIPrefab);

                slot.SetCard(runtimeCard);
                Debug.Log($"[PlayerHandManager] 슬롯에 카드 할당 완료: {slotPos}");
            }

            Debug.Log("[PlayerHandManager] 초기 핸드 카드 생성 완료");
        }

        public void ClearAll()
        {
            foreach (var kvp in handSlots)
            {
                kvp.Value.Clear();
            }
        }

        public ISkillCard GetSlotCard(SkillCardSlotPosition position)
        {
            return handSlots.TryGetValue(position, out var slot) ? slot.GetCard() : null;
        }

        public void SubmitCardFromHand(ISkillCard selectedCard)
        {
            if (selectedCard == null)
            {
                Debug.LogWarning("[PlayerHandManager] 선택된 카드가 null입니다.");
                return;
            }

            CombatTurnManager turnManager = FindAnyObjectByType<CombatTurnManager>();
            if (turnManager != null)
            {
                turnManager.RegisterPlayerCard(selectedCard);
                Debug.Log($"[PlayerHandManager] 카드 제출 완료: {selectedCard.GetCardName()}");
            }
            else
            {
                Debug.LogError("[PlayerHandManager] CombatTurnManager를 찾을 수 없습니다.");
            }

            foreach (var kvp in handSlots)
            {
                if (kvp.Value.GetCard() == selectedCard)
                {
                    kvp.Value.Clear();
                    break;
                }
            }
        }
    }
}
