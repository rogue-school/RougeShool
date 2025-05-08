using UnityEngine;
using System.Collections.Generic;
using Game.Interface;
using Game.UI.Hand;
using Game.Player;
using Game.Slots;
using Game.Combat.Turn;

namespace Game.Managers
{
    /// <summary>
    /// 플레이어의 핸드 슬롯과 카드 초기화를 담당하며,
    /// 카드 드래그 후 전투 슬롯 자동 배치를 지원합니다.
    /// </summary>
    public class PlayerHandManager : MonoBehaviour
    {
        [SerializeField] private PlayerSkillCard[] defaultCards;
        [SerializeField] private int[] defaultDamages;
        [SerializeField] private int[] defaultCoolTimes;

        private Dictionary<SkillCardSlotPosition, IHandCardSlot> handSlots;

        private void Awake()
        {
            handSlots = new Dictionary<SkillCardSlotPosition, IHandCardSlot>();

            foreach (var slot in SlotRegistry.Instance.GetAllHandSlots())
            {
                if (slot.GetOwner() == SlotOwner.PLAYER)
                {
                    handSlots[slot.GetSlotPosition()] = slot;
                }
            }
        }

        /// <summary>
        /// 전투 시작 시 초기 핸드 카드 생성 및 슬롯에 설정
        /// </summary>
        public void GenerateInitialHand()
        {
            ClearAll();

            int i = 0;
            foreach (var kvp in handSlots)
            {
                var card = ScriptableObject.Instantiate(defaultCards[i]);
                card.SetPower(defaultDamages[i]);
                card.SetCoolTime(defaultCoolTimes[i]);

                kvp.Value.SetCard(card);
                i++;
            }
        }

        /// <summary>
        /// 핸드 슬롯 전부 비움
        /// </summary>
        public void ClearAll()
        {
            foreach (var kvp in handSlots)
            {
                kvp.Value.Clear();
            }
        }

        /// <summary>
        /// 특정 슬롯 위치의 카드를 가져옴
        /// </summary>
        public ISkillCard GetSlotCard(SkillCardSlotPosition position)
        {
            return handSlots.TryGetValue(position, out var slot) ? slot.GetCard() : null;
        }

        /// <summary>
        /// 플레이어가 핸드에서 선택한 카드 등록 요청 (외부에서 호출)
        /// → 전투 슬롯에 자동 배치됨
        /// </summary>
        public void SubmitCardFromHand(ISkillCard selectedCard)
        {
            if (selectedCard == null)
            {
                Debug.LogWarning("[PlayerHandManager] 선택된 카드가 null입니다.");
                return;
            }

            CombatTurnManager turnManager = FindObjectOfType<CombatTurnManager>();
            if (turnManager != null)
            {
                turnManager.RegisterPlayerCard(selectedCard);
                Debug.Log($"[PlayerHandManager] 카드 제출 완료: {selectedCard.GetCardName()}");
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
