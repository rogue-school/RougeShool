using System;
using System.Collections.Generic;
using UnityEngine;
using Game.CombatSystem.Interface;
using Game.CombatSystem.Slot;
using Game.CombatSystem.Data;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.UI;
using Game.CoreSystem.Utility;
using Game.CombatSystem.Core;

namespace Game.CombatSystem.Manager
{
    /// <summary>
    /// 카드 레지스트리 전담 클래스
    /// 슬롯별 카드 및 UI 관리만 담당하며 단일 책임 원칙을 준수합니다.
    /// </summary>
    public class CardSlotRegistry : ICardSlotRegistry
    {
        #region 내부 데이터

        private readonly Dictionary<CombatSlotPosition, ISkillCard> _cards = new();
        private readonly Dictionary<CombatSlotPosition, SkillCardUI> _cardUIs = new();
        private CombatSlotPosition? _reservedEnemySlot;

        #endregion

        #region 이벤트

        public event Action OnCardStateChanged;

        #endregion

        #region 카드 등록/조회

        public void RegisterCard(CombatSlotPosition position, ISkillCard card, SkillCardUI ui, SlotOwner owner)
        {
            if (card == null)
            {
                GameLogger.LogError(
                    $"카드 등록 실패 - null (슬롯: {position})",
                    GameLogger.LogCategory.Combat);
                return;
            }

            _cards[position] = card;
            if (ui != null)
            {
                _cardUIs[position] = ui;
            }

            if (owner == SlotOwner.ENEMY)
            {
                _reservedEnemySlot = position;
            }

            OnCardStateChanged?.Invoke();
        }

        public ISkillCard GetCardInSlot(CombatSlotPosition slot)
        {
            _cards.TryGetValue(slot, out var card);
            return card;
        }

        public SkillCardUI GetCardUIInSlot(CombatSlotPosition slot)
        {
            _cardUIs.TryGetValue(slot, out var ui);
            return ui;
        }

        public bool HasCardInSlot(CombatSlotPosition slot)
        {
            return _cards.ContainsKey(slot) && _cards[slot] != null;
        }

        #endregion

        #region 카드 이동

        public void MoveCardData(CombatSlotPosition from, CombatSlotPosition to)
        {
            if (!_cards.TryGetValue(from, out var card))
            {
                return;
            }

            // UI도 함께 이동
            _cardUIs.TryGetValue(from, out var ui);

            // 원본 슬롯에서 제거
            _cards.Remove(from);
            _cardUIs.Remove(from);

            // 목적지 슬롯에 등록
            _cards[to] = card;
            if (ui != null)
            {
                _cardUIs[to] = ui;
            }

            OnCardStateChanged?.Invoke();
        }

        #endregion

        #region 슬롯 클리어

        public void ClearSlot(CombatSlotPosition slot)
        {
            if (!_cards.ContainsKey(slot))
            {
                return;
            }

            // UI 제거
            if (_cardUIs.TryGetValue(slot, out var ui) && ui != null)
            {
                // GameObject 찾아서 UI 제거
                string slotName = GetSlotGameObjectName(slot);
                var slotGameObject = GameObject.Find(slotName);
                if (slotGameObject != null && ui != null)
                {
                    UnityEngine.Object.DestroyImmediate(ui.gameObject);
                }
            }

            // 데이터 제거
            _cards.Remove(slot);
            _cardUIs.Remove(slot);
            OnCardStateChanged?.Invoke();
        }

        public void ClearAllSlots()
        {
            var allSlots = new List<CombatSlotPosition>(_cards.Keys);

            foreach (var slot in allSlots)
            {
                // UI 제거
                if (_cardUIs.TryGetValue(slot, out var ui) && ui != null)
                {
                    UnityEngine.Object.Destroy(ui.gameObject);
                }
                _cardUIs.Remove(slot);
            }

            // 데이터 제거
            _cards.Clear();
            _reservedEnemySlot = null;
            OnCardStateChanged?.Invoke();
        }

        public void ClearEnemyCardsOnly()
        {
            var toRemove = new List<CombatSlotPosition>();

            foreach (var kvp in _cards)
            {
                if (!kvp.Value.IsFromPlayer())
                {
                    toRemove.Add(kvp.Key);
                }
            }

            foreach (var key in toRemove)
            {
                // UI 제거
                if (_cardUIs.TryGetValue(key, out var ui) && ui != null)
                {
                    UnityEngine.Object.Destroy(ui.gameObject);
                }
                _cardUIs.Remove(key);

                // 데이터 제거
                _cards.Remove(key);
            }

            _reservedEnemySlot = null;
            OnCardStateChanged?.Invoke();

            if (toRemove.Count > 0)
            {
            }
        }

        public void ClearWaitSlots()
        {
            // 모든 대기 슬롯의 카드 제거
            var waitSlots = new[]
            {
                CombatSlotPosition.WAIT_SLOT_1,
                CombatSlotPosition.WAIT_SLOT_2,
                CombatSlotPosition.WAIT_SLOT_3,
                CombatSlotPosition.WAIT_SLOT_4
            };

            foreach (var slot in waitSlots)
            {
                var card = GetCardInSlot(slot);
                if (card != null)
                {
                    // UI 제거
                    if (_cardUIs.TryGetValue(slot, out var ui) && ui != null)
                    {
                        UnityEngine.Object.Destroy(ui.gameObject);
                    }

                    // 데이터 제거
                    _cards.Remove(slot);
                    _cardUIs.Remove(slot);
                }
            }
        }

        #endregion

        #region 카드 존재 확인

        public bool HasPlayerCard()
        {
            foreach (var card in _cards.Values)
            {
                if (card.IsFromPlayer())
                {
                    return true;
                }
            }
            return false;
        }

        public bool HasEnemyCard()
        {
            foreach (var card in _cards.Values)
            {
                if (!card.IsFromPlayer())
                {
                    return true;
                }
            }
            return false;
        }

        #endregion

        #region 적 슬롯 예약

        public CombatSlotPosition? GetReservedEnemySlot()
        {
            return _reservedEnemySlot;
        }

        public void ReserveNextEnemySlot(CombatSlotPosition slot)
        {
            _reservedEnemySlot = slot;
        }

        #endregion

        #region 유틸리티

        /// <summary>
        /// CombatSlotPosition을 GameObject 이름으로 변환합니다.
        /// </summary>
        private string GetSlotGameObjectName(CombatSlotPosition position)
        {
            return position switch
            {
                CombatSlotPosition.BATTLE_SLOT => CombatConstants.SlotNames.BATTLE_SLOT,
                CombatSlotPosition.WAIT_SLOT_1 => CombatConstants.SlotNames.WAIT_SLOT_1,
                CombatSlotPosition.WAIT_SLOT_2 => CombatConstants.SlotNames.WAIT_SLOT_2,
                CombatSlotPosition.WAIT_SLOT_3 => CombatConstants.SlotNames.WAIT_SLOT_3,
                CombatSlotPosition.WAIT_SLOT_4 => CombatConstants.SlotNames.WAIT_SLOT_4,
                _ => "UnknownSlot"
            };
        }

        #endregion
    }
}
