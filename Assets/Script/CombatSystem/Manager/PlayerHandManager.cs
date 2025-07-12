using System.Collections.Generic;
using UnityEngine;
using Zenject;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.Slot;
using Game.SkillCardSystem.UI;
using Game.CharacterSystem.Interface;
using Game.SkillCardSystem.Factory;
using Game.CombatSystem.Interface;
using Game.CombatSystem;
using AnimationSystem.Manager;

namespace Game.SkillCardSystem.Core
{
    /// <summary>
    /// 플레이어의 손패를 관리하는 매니저 클래스입니다.
    /// 카드 생성, 제거, 복구 및 드래그 가능 여부 설정 등의 기능을 담당합니다.
    /// </summary>
    public class PlayerHandManager : MonoBehaviour, IPlayerHandManager
    {
        #region 필드

        private IPlayerCharacter owner;
        private IHandSlotRegistry slotRegistry;
        private ISkillCardFactory cardFactory;
        private SkillCardUI cardUIPrefab;

        private readonly Dictionary<SkillCardSlotPosition, ISkillCard> cards = new();
        private readonly Dictionary<SkillCardSlotPosition, SkillCardUI> cardUIs = new();

        #endregion

        #region 의존성 주입 및 초기화

        [Inject]
        public void Construct(
            ISlotRegistry slotRegistry,
            ISkillCardFactory cardFactory,
            SkillCardUI cardUIPrefab)
        {
            this.slotRegistry = slotRegistry.GetHandSlotRegistry();
            this.cardFactory = cardFactory;
            this.cardUIPrefab = cardUIPrefab;

            cards.Clear();
            cardUIs.Clear();
        }

        /// <summary>
        /// 핸드 소유자를 설정합니다.
        /// </summary>
        public void SetPlayer(IPlayerCharacter player)
        {
            this.owner = player;
        }

        #endregion

        #region 초기 핸드 구성

        /// <summary>
        /// 플레이어 덱을 기반으로 초기 핸드를 생성합니다.
        /// </summary>
        public void GenerateInitialHand()
        {
            var deck = owner?.Data?.SkillDeck;
            if (deck == null)
            {
                Debug.LogError("[PlayerHandManager] 플레이어 덱이 비어 있음");
                return;
            }

            foreach (var entry in deck.Cards)
            {
                var pos = entry.Slot;
                var card = cardFactory.CreatePlayerCard(entry.Card.CardData, entry.Card.CreateEffects());
                card.SetCurrentCoolTime(0);
                cards[pos] = card;

                var slot = slotRegistry.GetPlayerHandSlot(pos);
                if (slot != null)
                {
                    var ui = slot.AttachCard(card, cardUIPrefab);
                    if (ui != null) cardUIs[pos] = ui;
                    // 카드 생성 이벤트 발행
                    CombatEvents.Card.RaisePlayerCardSpawn(card.CardData.Name, ui?.gameObject);
                }
            }
        }

        #endregion

        #region 카드 조회

        /// <inheritdoc/>
        public ISkillCard GetCardInSlot(SkillCardSlotPosition pos) =>
            cards.TryGetValue(pos, out var c) ? c : null;

        /// <inheritdoc/>
        public ISkillCardUI GetCardUIInSlot(SkillCardSlotPosition pos) =>
            cardUIs.TryGetValue(pos, out var ui) ? ui : null;

        #endregion

        #region 카드 복구

        /// <summary>
        /// 사용 가능한 빈 슬롯에 카드를 복귀시킵니다.
        /// </summary>
        public void RestoreCardToHand(ISkillCard card)
        {
            foreach (var kvp in cards)
            {
                if (kvp.Value == null)
                {
                    cards[kvp.Key] = card;
                    var slot = slotRegistry.GetPlayerHandSlot(kvp.Key);
                    if (slot != null)
                    {
                        var ui = slot.AttachCard(card, cardUIPrefab);
                        if (ui != null) cardUIs[kvp.Key] = ui;
                    }
                    return;
                }
            }

            Debug.LogWarning("[PlayerHandManager] 빈 슬롯을 찾을 수 없어 카드 복귀 실패");
        }

        /// <summary>
        /// 특정 슬롯에 카드를 복귀시킵니다.
        /// </summary>
        public void RestoreCardToHand(ISkillCard card, SkillCardSlotPosition slot)
        {
            cards[slot] = card;

            var handSlot = slotRegistry.GetPlayerHandSlot(slot);
            if (handSlot != null)
            {
                var ui = handSlot.AttachCard(card, cardUIPrefab);
                if (ui != null)
                    cardUIs[slot] = ui;
            }
            else
            {
                Debug.LogWarning($"[PlayerHandManager] 지정된 슬롯이 존재하지 않습니다: {slot}");
            }
        }

        #endregion

        #region 카드 제거

        /// <summary>
        /// 손패에서 특정 카드를 제거합니다.
        /// </summary>
        public void RemoveCard(ISkillCard card)
        {
            foreach (var kvp in cards)
            {
                if (kvp.Value == card)
                {
                    var slot = slotRegistry.GetPlayerHandSlot(kvp.Key);
                    slot?.DetachCard();

                    // 카드 제거 이벤트 발행 (카드가 사라질 때 애니메이션)
                    if (cardUIs.TryGetValue(kvp.Key, out var ui))
                    {
                        // 이미 파괴된 오브젝트 접근 방지
                        if (ui != null && ui.gameObject != null)
                            CombatEvents.Card.RaisePlayerCardUse(card.CardData.Name, ui.gameObject);
                    }

                    cards[kvp.Key] = null;
                    cardUIs.Remove(kvp.Key);

                    return;
                }
            }

            Debug.LogWarning("[PlayerHandManager] 해당 카드를 찾을 수 없어 제거 실패");
        }

        #endregion

        #region 유틸리티

        /// <summary>
        /// 각 손패 슬롯의 상태를 로그로 출력합니다.
        /// </summary>
        public void LogPlayerHandSlotStates()
        {
            // 디버깅용 로그 제거 - 필요시 개발자가 임시로 활성화
            // foreach (SkillCardSlotPosition pos in System.Enum.GetValues(typeof(SkillCardSlotPosition)))
            // {
            //     var card = GetCardInSlot(pos);
            //     Debug.Log($"슬롯 {pos}: {(card != null ? card.CardData.Name : "비어 있음")}");
            // }
        }

        /// <summary>
        /// 손패 카드들의 드래그 가능 여부를 설정합니다.
        /// </summary>
        public void EnableInput(bool enable)
        {
            foreach (var ui in cardUIs.Values)
                ui?.SetDraggable(enable);
        }

        /// <summary>
        /// 모든 슬롯의 카드를 제거하고 초기화합니다.
        /// </summary>
        public void ClearAll()
        {
            foreach (var pos in cards.Keys)
                slotRegistry.GetPlayerHandSlot(pos)?.DetachCard();

            cards.Clear();
            cardUIs.Clear();
        }

        /// <summary>
        /// 현재 손패의 모든 카드와 UI를 반환합니다.
        /// </summary>
        public IEnumerable<(ISkillCard card, ISkillCardUI ui)> GetAllHandCards()
        {
            foreach (var kvp in cards)
            {
                var slot = kvp.Key;
                var card = kvp.Value;
                cardUIs.TryGetValue(slot, out var ui);
                yield return (card, ui);
            }
        }

        #endregion
    }
}
