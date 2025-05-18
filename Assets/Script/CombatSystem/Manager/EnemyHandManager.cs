using UnityEngine;
using System.Collections.Generic;
using Game.CharacterSystem.Interface;
using Game.CombatSystem.Interface;
using Game.SkillCardSystem.Factory;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.Slot;
using Game.SkillCardSystem.UI;
using Game.CombatSystem.Slot;
using Game.IManager;

namespace Game.CombatSystem.Manager
{
    public class EnemyHandManager : MonoBehaviour, IEnemyHandManager
    {
        [Header("UI 프리팹")]
        [SerializeField] private SkillCardUI cardUIPrefab;

        private readonly Dictionary<SkillCardSlotPosition, IHandCardSlot> handSlots = new();
        private readonly Dictionary<SkillCardSlotPosition, SkillCardUI> cardUIs = new();

        private IEnemyCharacter currentEnemy;

        public void Initialize(IEnemyCharacter enemy)
        {
            currentEnemy = enemy;

            if (enemy == null)
            {
                Debug.LogError("[EnemyHandManager] 초기화 실패 - Enemy가 null입니다.");
                return;
            }

            if (SlotRegistry.Instance == null)
            {
                Debug.LogError("[EnemyHandManager] 초기화 실패 - SlotRegistry.Instance가 null입니다.");
                return;
            }

            handSlots.Clear();
            foreach (var slot in SlotRegistry.Instance.GetHandSlots(SlotOwner.ENEMY))
            {
                if (slot != null)
                    handSlots[slot.GetSlotPosition()] = slot;
            }

            cardUIs.Clear();

            if (handSlots.Count == 0)
                Debug.LogError("[EnemyHandManager] 적 핸드 슬롯이 등록되지 않았습니다.");
            else
                Debug.Log($"[EnemyHandManager] 적 슬롯 {handSlots.Count}개 초기화 완료");

            if (cardUIPrefab == null)
                cardUIPrefab = Resources.Load<SkillCardUI>("UI/SkillCardUI");
        }

        public void GenerateInitialHand()
        {
            if (handSlots.Count == 0)
            {
                Debug.LogWarning("[EnemyHandManager] 핸드 슬롯이 비어있습니다. 초기 핸드 생성 불가.");
                return;
            }

            CreateCardInSlot(SkillCardSlotPosition.ENEMY_SLOT_3);
            FillEmptySlots();
        }

        public void AdvanceSlots()
        {
            FillEmptySlots();
        }

        public void FillEmptySlots()
        {
            for (int i = 0; i < 3; i++)
            {
                bool moved = false;

                // 오른쪽 -> 왼쪽 순서로 이동 시도
                if (TryShift(SkillCardSlotPosition.ENEMY_SLOT_2, SkillCardSlotPosition.ENEMY_SLOT_1))
                    moved = true;

                if (TryShift(SkillCardSlotPosition.ENEMY_SLOT_3, SkillCardSlotPosition.ENEMY_SLOT_2))
                    moved = true;

                // ENEMY_SLOT_3이 비어 있으면 새 카드 생성
                if (IsSlotEmpty(SkillCardSlotPosition.ENEMY_SLOT_3))
                {
                    CreateCardInSlot(SkillCardSlotPosition.ENEMY_SLOT_3);
                    moved = true;
                }

                if (!moved)
                    break;
            }
        }

        public ISkillCard GetCardForCombat() => GetSlotCard(SkillCardSlotPosition.ENEMY_SLOT_1);

        public ISkillCard GetSlotCard(SkillCardSlotPosition position)
        {
            return handSlots.TryGetValue(position, out var slot) ? slot.GetCard() : null;
        }

        public ISkillCardUI GetCardUI(int index)
        {
            var pos = index switch
            {
                0 => SkillCardSlotPosition.ENEMY_SLOT_1,
                1 => SkillCardSlotPosition.ENEMY_SLOT_2,
                2 => SkillCardSlotPosition.ENEMY_SLOT_3,
                _ => SkillCardSlotPosition.ENEMY_SLOT_1
            };

            return cardUIs.TryGetValue(pos, out var ui) ? ui : null;
        }

        public void ClearAllSlots()
        {
            foreach (var slot in handSlots.Values)
                slot?.Clear();

            foreach (var ui in cardUIs.Values)
                if (ui != null) Destroy(ui.gameObject);

            cardUIs.Clear();

            Debug.Log("[EnemyHandManager] 모든 슬롯과 카드 UI 제거 완료");
        }

        public void ClearAllUI()
        {
            if (SlotRegistry.Instance == null)
            {
                Debug.LogError("[EnemyHandManager] ClearAllUI 실패 - SlotRegistry가 null입니다.");
                return;
            }

            foreach (var slot in SlotRegistry.Instance.GetHandSlots(SlotOwner.ENEMY))
                slot?.Clear();

            foreach (var ui in cardUIs.Values)
                if (ui != null) Destroy(ui.gameObject);

            cardUIs.Clear();

            Debug.Log("[EnemyHandManager] UI 완전 초기화 완료");
        }

        // 내부 유틸리티 ----------------------

        private bool IsSlotEmpty(SkillCardSlotPosition pos)
        {
            return handSlots.TryGetValue(pos, out var slot) && slot.GetCard() == null;
        }

        private void CreateCardInSlot(SkillCardSlotPosition pos)
        {
            if (!handSlots.TryGetValue(pos, out var slot))
            {
                Debug.LogWarning($"[EnemyHandManager] 카드 생성 실패 - 슬롯 {pos} 없음");
                return;
            }

            if (slot.GetCard() != null)
            {
                Debug.LogWarning($"[EnemyHandManager] 카드 생성 스킵 - 슬롯 {pos}에 이미 카드 존재");
                return;
            }

            if (currentEnemy?.Data == null)
            {
                Debug.LogWarning("[EnemyHandManager] 카드 생성 실패 - Enemy 데이터 없음");
                return;
            }

            var entry = currentEnemy.Data.GetRandomEntry();
            if (entry?.card == null)
            {
                Debug.LogWarning("[EnemyHandManager] 랜덤 카드 entry가 null입니다.");
                return;
            }

            var runtimeCard = SkillCardFactory.CreateEnemyCard(entry.card, entry.damage);
            runtimeCard.SetHandSlot(pos);

            var cardUI = Instantiate(cardUIPrefab, ((MonoBehaviour)slot).transform);
            cardUI.SetCard(runtimeCard);

            slot.SetCard(runtimeCard);
            cardUIs[pos] = cardUI;

            Debug.Log($"[EnemyHandManager] {pos}에 카드 생성 완료");
        }

        private bool TryShift(SkillCardSlotPosition from, SkillCardSlotPosition to)
        {
            if (!handSlots.TryGetValue(from, out var fromSlot))
            {
                Debug.LogWarning($"[TryShift] 이동 실패 - from 슬롯({from}) 존재하지 않음");
                return false;
            }

            if (!handSlots.TryGetValue(to, out var toSlot))
            {
                Debug.LogWarning($"[TryShift] 이동 실패 - to 슬롯({to}) 존재하지 않음");
                return false;
            }

            if (fromSlot.GetCard() == null)
            {
                Debug.Log($"[TryShift] 이동 실패 - from 슬롯({from})에 카드 없음");
                return false;
            }

            if (toSlot.GetCard() != null)
            {
                Debug.Log($"[TryShift] 이동 실패 - to 슬롯({to})에 이미 카드 존재");
                return false;
            }

            var card = fromSlot.GetCard();
            fromSlot.Clear();
            toSlot.SetCard(card);
            card?.SetHandSlot(to);

            if (cardUIs.TryGetValue(from, out var ui))
            {
                cardUIs[to] = ui;
                cardUIs.Remove(from);
                ui.transform.SetParent(((MonoBehaviour)toSlot).transform);
                ui.transform.localPosition = Vector3.zero;
                ui.transform.localScale = Vector3.one;
            }

            Debug.Log($"[TryShift] 카드 이동 성공: {from} → {to}");
            return true;

        }
    }
}