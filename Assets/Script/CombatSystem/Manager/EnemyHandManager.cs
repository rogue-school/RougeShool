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

            var registry = SlotRegistry.Instance;
            handSlots.Clear();
            foreach (var slot in registry.GetHandSlots(SlotOwner.ENEMY))
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
            {
                cardUIPrefab = Resources.Load<SkillCardUI>("Prefab/EnemySkillCard");
                if (cardUIPrefab == null)
                    Debug.LogError("[EnemyHandManager] cardUIPrefab 로드 실패");
            }
        }

        public void GenerateInitialHand()
        {
            if (handSlots.Count == 0) return;
            if (GetSlotCard(SkillCardSlotPosition.ENEMY_SLOT_1) != null) return;

            CreateCardInSlot(SkillCardSlotPosition.ENEMY_SLOT_3);
            FillEmptySlots();
            LogHandSlotStates();
        }

        public void FillEmptySlots()
        {
            for (int i = 0; i < 3; i++)
            {
                bool moved = false;

                moved |= TryShift(SkillCardSlotPosition.ENEMY_SLOT_2, SkillCardSlotPosition.ENEMY_SLOT_1);
                moved |= TryShift(SkillCardSlotPosition.ENEMY_SLOT_3, SkillCardSlotPosition.ENEMY_SLOT_2);

                if (IsSlotEmpty(SkillCardSlotPosition.ENEMY_SLOT_3))
                {
                    CreateCardInSlot(SkillCardSlotPosition.ENEMY_SLOT_3);
                    moved = true;
                }

                if (!moved) break;
            }

            LogHandSlotStates();
        }

        public void AdvanceSlots() => FillEmptySlots();

        public ISkillCard GetCardForCombat() => GetSlotCard(SkillCardSlotPosition.ENEMY_SLOT_1);

        public ISkillCard GetSlotCard(SkillCardSlotPosition pos)
            => handSlots.TryGetValue(pos, out var slot) ? slot.GetCard() : null;

        public ISkillCardUI GetCardUI(int index)
        {
            SkillCardSlotPosition pos = (SkillCardSlotPosition)(index + 3);
            return cardUIs.TryGetValue(pos, out var ui) ? ui : null;
        }

        public void ClearHand()
        {
            ClearAllSlots();
            ClearAllUI();
        }

        public void ClearAllSlots()
        {
            foreach (var slot in handSlots.Values) slot?.Clear();
            foreach (var ui in cardUIs.Values) if (ui != null) Destroy(ui.gameObject);
            cardUIs.Clear();
        }

        public void ClearAllUI()
        {
            var registry = SlotRegistry.Instance;
            foreach (var slot in registry.GetHandSlots(SlotOwner.ENEMY)) slot?.Clear();
            foreach (var ui in cardUIs.Values) if (ui != null) Destroy(ui.gameObject);
            cardUIs.Clear();
        }

        public SkillCardUI RemoveCardFromSlot(SkillCardSlotPosition pos)
        {
            if (handSlots.TryGetValue(pos, out var slot))
                slot.Clear();  // 카드 정보만 제거

            if (cardUIs.TryGetValue(pos, out var ui))
            {
                cardUIs.Remove(pos);  // UI는 제거만 하지 않고 반환
                return ui;
            }

            return null;  // UI 없으면 null 반환
        }

        public void LogHandSlotStates()
        {
            Debug.Log("[EnemyHandManager] 슬롯 상태 확인:");
            for (int i = 0; i < 3; i++)
            {
                SkillCardSlotPosition pos = (SkillCardSlotPosition)(i + 3);
                var card = GetSlotCard(pos);
                var ui = GetCardUI(i);

                Debug.Log($" → {pos}: 카드 = {card?.CardData.Name ?? "없음"}, UI = {(ui != null ? "있음" : "없음")}");
            }
        }
        public void LogPlayerHandSlotStates()
        {
            Debug.Log("[PlayerHandManager] 플레이어 슬롯 상태 확인:");

            // 플레이어의 슬롯에 대한 정보를 순회합니다.
            for (int i = 0; i < 3; i++)
            {
                // 플레이어 핸드 슬롯의 위치
                SkillCardSlotPosition pos = (SkillCardSlotPosition)(i);  // 플레이어 슬롯 1, 2, 3 (index 0, 1, 2)

                // 슬롯에서 카드 가져오기
                var card = GetSlotCard(pos);

                // 카드 UI 가져오기
                var ui = GetCardUI(i);

                // 카드와 UI 상태를 로그로 출력
                Debug.Log($" → {pos}: 카드 = {card?.CardData.Name ?? "없음"}, UI = {(ui != null ? "있음" : "없음")}");
            }
        }

        private bool IsSlotEmpty(SkillCardSlotPosition pos)
        {
            return handSlots.TryGetValue(pos, out var slot) && slot.GetCard() == null;
        }

        private void CreateCardInSlot(SkillCardSlotPosition pos)
        {
            if (!ValidateSlotForCreation(pos, out var slot)) return;

            var entry = currentEnemy.Data.GetRandomEntry();
            if (entry?.card == null) return;

            var runtimeCard = SkillCardFactory.CreateEnemyCard(entry.card, entry.card.CardData.Damage);
            if (runtimeCard == null) return;

            runtimeCard.SetHandSlot(pos);

            var cardUI = Instantiate(cardUIPrefab, ((MonoBehaviour)slot).transform);
            cardUI.SetCard(runtimeCard);
            cardUI.transform.localPosition = Vector3.zero;
            cardUI.transform.localScale = Vector3.one;

            slot.SetCard(runtimeCard);
            cardUIs[pos] = cardUI;

            Debug.Log($"[EnemyHandManager] {pos}에 카드 및 UI 생성 완료 → {entry.card.CardData.Name}");
        }

        private bool TryShift(SkillCardSlotPosition from, SkillCardSlotPosition to)
        {
            if (!handSlots.TryGetValue(from, out var fromSlot) || !handSlots.TryGetValue(to, out var toSlot))
                return false;

            if (fromSlot.GetCard() == null || toSlot.GetCard() != null)
                return false;

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

        private bool ValidateSlotForCreation(SkillCardSlotPosition pos, out IHandCardSlot slot)
        {
            slot = null;

            if (cardUIPrefab == null)
            {
                Debug.LogError("[EnemyHandManager] 카드 프리팹이 null입니다.");
                return false;
            }

            if (!handSlots.TryGetValue(pos, out slot) || slot.GetCard() != null || currentEnemy?.Data == null)
                return false;

            return true;
        }
    }
}
