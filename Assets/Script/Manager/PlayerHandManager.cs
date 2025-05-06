using UnityEngine;
using System.Collections.Generic;
using Game.Player;
using Game.UI;
using Game.Cards;
using Game.Interface;
using Game.Slots;
using Game.Managers;
using Game.Utility;

namespace Game.Managers
{
    /// <summary>
    /// 플레이어의 핸드 슬롯에 스킬 카드를 배치하고 관리하는 매니저입니다.
    /// </summary>
    public class PlayerHandManager : MonoBehaviour
    {
        [Header("기본 스킬 카드 세트")]
        [SerializeField] private PlayerSkillCard[] defaultCards;

        [SerializeField] private int[] defaultDamages;
        [SerializeField] private int[] defaultCoolTimes;

        private Dictionary<SkillCardSlotPosition, PlayerCardSlotUI> handSlotDict;

        private void Awake()
        {
            BindSlotsFromScene();
        }

        private void Start()
        {
            GenerateInitialHand();
        }

        private void BindSlotsFromScene()
        {
            handSlotDict = new Dictionary<SkillCardSlotPosition, PlayerCardSlotUI>();

            var allSlots = FindObjectsOfType<PlayerCardSlotUI>();
            foreach (var slot in allSlots)
            {
                if (!handSlotDict.ContainsKey(slot.Position))
                {
                    handSlotDict.Add(slot.Position, slot);
                }
            }

            Debug.Log($"[PlayerHandManager] 슬롯 자동 등록 완료: {handSlotDict.Count}개");
        }

        private void GenerateInitialHand()
        {
            ClearAll();

            for (int i = 0; i < defaultCards.Length; i++)
            {
                var cardData = defaultCards[i];
                if (cardData != null)
                {
                    int damage = (i < defaultDamages.Length) ? defaultDamages[i] : 5;
                    int coolTime = (i < defaultCoolTimes.Length) ? defaultCoolTimes[i] : 0;

                    ISkillCard runtimeCard = SkillCardFactory.CreatePlayerCard(cardData, damage, coolTime);

                    SkillCardSlotPosition slotPos = (SkillCardSlotPosition)System.Enum.Parse(
                        typeof(SkillCardSlotPosition), $"PLAYER_SLOT_{i + 1}"
                    );

                    if (handSlotDict.TryGetValue(slotPos, out var slot))
                    {
                        slot.SetCard(runtimeCard);
                        Debug.Log($"[PlayerHandManager] {slotPos}에 카드 설정됨: {runtimeCard.GetCardName()}");
                    }
                }
            }
        }

        public ISkillCard GetSlotCard(SkillCardSlotPosition position)
        {
            if (handSlotDict.TryGetValue(position, out var slot))
            {
                return slot.GetCard();
            }
            return null;
        }

        public void ClearAll()
        {
            foreach (var slot in handSlotDict.Values)
                slot.Clear();
        }
    }
}
