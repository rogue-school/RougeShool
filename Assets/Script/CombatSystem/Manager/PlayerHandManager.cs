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
using System.Linq;

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
            var deck = owner?.CharacterData?.SkillDeck;
            if (deck == null)
            {
                Debug.LogError("[PlayerHandManager] 플레이어 덱이 비어 있음");
                return;
            }

            foreach (var entry in deck.Cards)
            {
                var pos = entry.Slot;
                var card = cardFactory.CreatePlayerCard(entry.Card.CardData, entry.Card.CreateEffects(), owner?.CharacterData?.name);
                card.SetCurrentCoolTime(0);
                cards[pos] = card;

                var slot = slotRegistry.GetPlayerHandSlot(pos);
                if (slot != null)
                {
                    var ui = slot.AttachCard(card, cardUIPrefab);
                    if (ui != null) cardUIs[pos] = ui;
                    // 카드 생성 이벤트 발행
                    CombatEvents.RaisePlayerCardSpawn(card.CardData.Name, ui?.gameObject);
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
            foreach (SkillCardSlotPosition pos in System.Enum.GetValues(typeof(SkillCardSlotPosition)))
            {
                if (!cards.ContainsKey(pos) || cards[pos] == null)
                {
                    cards[pos] = card;
                    var slot = slotRegistry.GetPlayerHandSlot(pos);
                    if (slot != null)
                    {
                        var ui = slot.AttachCard(card, cardUIPrefab);
                        if (ui != null) cardUIs[pos] = ui;
                        else
                        {
                            Debug.LogWarning($"[PlayerHandManager] AttachCard 실패: {pos} 슬롯에 UI 생성 불가, 강제 복구 시도");
                            if (cardUIs.TryGetValue(pos, out var oldUI) && oldUI != null)
                                Game.CombatSystem.Utility.CardSlotHelper.AttachCardToHandSlot(oldUI, pos);
                        }
                    }
                    else
                    {
                        Debug.LogError($"[PlayerHandManager] 핸드 슬롯 UI가 존재하지 않음: {pos} - 씬에 PlayerHandCardSlotUI가 반드시 존재해야 함");
                    }
                    LogHandSlotSyncState();
                    Debug.Log($"[HandRestore] 카드 {card.GetCardName()}를 슬롯 {pos}에 복귀");
                    return;
                }
            }
            Debug.LogWarning("[PlayerHandManager] 빈 슬롯을 찾을 수 없어 카드 복귀 실패");
            LogHandSlotSyncState();
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
                else
                {
                    Debug.LogWarning($"[PlayerHandManager] AttachCard 실패: {slot} 슬롯에 UI 생성 불가, 강제 복구 시도");
                    if (cardUIs.TryGetValue(slot, out var oldUI) && oldUI != null)
                        Game.CombatSystem.Utility.CardSlotHelper.AttachCardToHandSlot(oldUI, slot);
                }
            }
            else
            {
                Debug.LogWarning($"[PlayerHandManager] 지정된 슬롯이 존재하지 않습니다: {slot}");
            }
            LogHandSlotSyncState();
        }

        #endregion

        #region 카드 제거

        /// <summary>
        /// 손패에서 특정 카드를 제거합니다.
        /// 소멸 애니메이션 콜백 이외에는 카드UI/데이터를 파괴하지 말 것!
        /// </summary>
        public void RemoveCard(ISkillCard card)
        {
            Debug.LogWarning("[PlayerHandManager] RemoveCard는 반드시 AnimationFacade 소멸 애니메이션 콜백에서만 호출되어야 합니다. 직접 호출 금지!");
            foreach (var kvp in cards)
            {
                if (kvp.Value == card)
                {
                    var slot = slotRegistry.GetPlayerHandSlot(kvp.Key);
                    slot?.DetachCard();
                    if (cardUIs.TryGetValue(kvp.Key, out var ui))
                    {
                        if (ui != null && ui.gameObject != null)
                            CombatEvents.RaisePlayerCardUse(card.CardData.Name, ui.gameObject);
                    }
                    cards[kvp.Key] = null;
                    cardUIs.Remove(kvp.Key);
                    LogHandSlotSyncState();
                    return;
                }
            }
            Debug.LogWarning("[PlayerHandManager] 해당 카드를 찾을 수 없어 제거 실패");
            LogHandSlotSyncState();
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
            foreach (var pos in cardUIs.Keys.ToList())
            {
                var ui = cardUIs[pos];
                if (ui == null || ui.gameObject == null)
                {
                    cardUIs.Remove(pos);
                    if (cards.ContainsKey(pos)) cards.Remove(pos);
                    continue;
                }
                ui.SetDraggable(enable);
            }
        }

        /// <summary>
        /// 손패 카드들의 쿨타임 표시를 업데이트합니다.
        /// </summary>
        public void UpdateCoolTimeDisplay()
        {
            foreach (var pos in cardUIs.Keys.ToList())
            {
                var ui = cardUIs[pos];
                if (ui == null || ui.gameObject == null)
                {
                    cardUIs.Remove(pos);
                    if (cards.ContainsKey(pos)) cards.Remove(pos);
                    continue;
                }
                ui.UpdateCoolTimeDisplay();
            }
        }

        /// <summary>
        /// 사망 등으로 인해 소멸 애니메이션이 끝난 후, 해당 슬롯의 카드UI와 참조를 완전히 해제/파괴합니다.
        /// 반드시 AnimationFacade 소멸 애니메이션 콜백에서만 호출되어야 함! 직접 호출 금지.
        /// </summary>
        /// <param name="pos">슬롯 위치</param>
        public void RemoveCardUIAndReferences(SkillCardSlotPosition pos)
        {
            Debug.LogWarning("[PlayerHandManager] RemoveCardUIAndReferences는 반드시 AnimationFacade 소멸 애니메이션 콜백에서만 호출되어야 합니다. 직접 호출 금지!");
            var slot = slotRegistry.GetPlayerHandSlot(pos);
            slot?.Clear();
            if (cardUIs.TryGetValue(pos, out var ui))
            {
                if (ui != null && ui.gameObject != null)
                    GameObject.Destroy(ui.gameObject);
                cardUIs.Remove(pos);
            }
            if (cards.ContainsKey(pos))
                cards[pos] = null;
            LogHandSlotSyncState();
        }

        /// <summary>
        /// 모든 슬롯의 카드를 제거하고 초기화합니다.
        /// 소멸 애니메이션 콜백 이외에는 카드UI/데이터를 파괴하지 말 것!
        /// </summary>
        public void ClearAll()
        {
            Debug.LogWarning("[PlayerHandManager] ClearAll은 반드시 SafeClearHandAfterAllAnimations 등에서만 호출되어야 합니다. 직접 호출 금지!");
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

        /// <summary>
        /// 핸드 슬롯 동기화 상태를 한 번에 출력합니다.
        /// </summary>
        public void LogHandSlotSyncState()
        {
            foreach (SkillCardSlotPosition pos in System.Enum.GetValues(typeof(SkillCardSlotPosition)))
            {
                var card = cards.ContainsKey(pos) ? cards[pos] : null;
                var ui = cardUIs.ContainsKey(pos) ? cardUIs[pos] : null;
                string cardName = card != null ? card.CardData.Name : "null";
                string uiName = ui != null ? ui.name : "null";
                string sync = (card != null && ui != null) ? "정상"
                            : (card != null && ui == null) ? "⚠️ 카드만 존재"
                            : (card == null && ui != null) ? "⚠️ UI만 존재"
                            : "비어있음";
                Debug.Log($"[HandSync] 슬롯: {pos}, 카드: {cardName}, 카드UI: {uiName}, 상태: {sync}");
                if (sync != "정상" && sync != "비어있음")
                    Debug.LogWarning($"[HandSync][경고] 슬롯 {pos} 동기화 깨짐: {sync}");
            }
        }

        /// <summary>
        /// 카드UI가 슬롯에서 분리(부모 변경)될 때 동기화 보장용 함수
        /// </summary>
        public void OnCardUIDetachedFromSlot(SkillCardSlotPosition pos)
        {
            if (cardUIs.ContainsKey(pos)) cardUIs[pos] = null;
            if (cards.ContainsKey(pos)) cards[pos] = null;
            LogHandSlotSyncState();
        }

        // IPlayerHandManager.GetPlayer() 구현
        public IPlayerCharacter GetPlayer()
        {
            return owner;
        }

        // 카드 데이터와 UI 동기화 함수 추가
        public void SyncHandSlotUI()
        {
            foreach (SkillCardSlotPosition pos in System.Enum.GetValues(typeof(SkillCardSlotPosition)))
            {
                var card = GetCardInSlot(pos);
                var ui = GetCardUIInSlot(pos);
                var slot = slotRegistry.GetPlayerHandSlot(pos);

                if (card != null && (ui == null || (ui is UnityEngine.Object unityObj && unityObj == null)))
                {
                    // UI가 없으면 강제로 생성
                    if (slot != null)
                    {
                        var newUI = slot.AttachCard(card, cardUIPrefab);
                        if (newUI != null)
                            cardUIs[pos] = newUI;
                        else
                            Debug.LogWarning($"[PlayerHandManager] SyncHandSlotUI: AttachCard 실패 - {pos}");
                    }
                    else
                    {
                        Debug.LogError($"[PlayerHandManager] SyncHandSlotUI: 핸드 슬롯 UI 없음 - {pos}");
                    }
                }
            }
        }

        #endregion
    }
}
