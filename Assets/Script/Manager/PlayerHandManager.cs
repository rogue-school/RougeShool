using UnityEngine;
using System;
using Game.Player;
using Game.UI;
using Game.Cards;
using Game.Interface;
using Game.Utility;

namespace Game.Managers
{
    /// <summary>
    /// 플레이어의 핸드 슬롯에 스킬 카드를 배치하고 관리하는 매니저입니다.
    /// 카드 데이터(SO)와 수치를 조합하여 런타임 카드 인스턴스를 생성합니다.
    /// </summary>
    public class PlayerHandManager : MonoBehaviour
    {
        [Header("기본 스킬 카드 세트")]
        [SerializeField] private PlayerSkillCard[] defaultCards;

        [Header("핸드 슬롯들")]
        [SerializeField] private PlayerCardSlotUI[] handSlots;

        // 예시: 기본 수치 하드코딩 또는 나중에 외부 데이터 연동
        [SerializeField] private int[] defaultDamages;
        [SerializeField] private int[] defaultCoolTimes;

        private void Awake()
        {
            AutoBindSlots();
        }

        private void Start()
        {
            GenerateInitialHand();
        }

        private void AutoBindSlots()
        {
            if (handSlots == null || handSlots.Length == 0)
            {
                handSlots = FindObjectsOfType<PlayerCardSlotUI>();
                Array.Sort(handSlots, (a, b) => a.name.CompareTo(b.name));
                Debug.Log($"[PlayerHandManager] 자동 슬롯 참조 완료 - 총 {handSlots.Length}개");
            }
        }

        private void GenerateInitialHand()
        {
            ClearAll();

            for (int i = 0; i < handSlots.Length && i < defaultCards.Length; i++)
            {
                var cardData = defaultCards[i];
                if (cardData != null)
                {
                    int damage = (i < defaultDamages.Length) ? defaultDamages[i] : 5;
                    int coolTime = (i < defaultCoolTimes.Length) ? defaultCoolTimes[i] : 0;

                    ISkillCard runtimeCard = SkillCardFactory.CreatePlayerCard(cardData, damage, coolTime);
                    handSlots[i].SetCard(runtimeCard);

                    Debug.Log($"[PlayerHandManager] 슬롯 {i}에 카드 설정됨: {runtimeCard.GetCardName()}");
                }
            }
        }

        public ISkillCard GetSlotCard(int index)
        {
            if (index < 0 || index >= handSlots.Length) return null;
            return handSlots[index].GetCard();
        }

        public void ClearAll()
        {
            foreach (var slot in handSlots)
                slot.Clear();
        }
    }
}
