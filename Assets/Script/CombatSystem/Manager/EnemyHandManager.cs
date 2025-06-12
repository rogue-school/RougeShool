using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using Game.CharacterSystem.Interface;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.Slot;
using Game.SkillCardSystem.UI;
using Game.SkillCardSystem.Deck;
using Game.CombatSystem.Interface;
using Game.CombatSystem.Slot;
using Game.CombatSystem.Utility;
using Game.CombatSystem.Animation;
using System.Threading.Tasks;

namespace Game.CombatSystem.Manager
{
    /// <summary>
    /// 적의 손패 슬롯을 관리하며 카드 생성, 이동, 등록, 제거 등의 기능을 담당합니다.
    /// </summary>
    public class EnemyHandManager : MonoBehaviour, IEnemyHandManager
    {
        #region  인스펙터 변수

        [Header("UI 프리팹")]
        [SerializeField] private SkillCardUI cardUIPrefab;

        #endregion

        #region  내부 상태

        private readonly Dictionary<SkillCardSlotPosition, IHandCardSlot> handSlots = new();
        private readonly Dictionary<SkillCardSlotPosition, SkillCardUI> cardUIs = new();
        private readonly Dictionary<SkillCardSlotPosition, (ISkillCard, ISkillCardUI)> _cardsInSlots = new();

        private IEnemyCharacter currentEnemy;
        private EnemySkillDeck enemyDeck;

        private ISlotRegistry slotRegistry;
        private ISkillCardFactory cardFactory;
        private ITurnCardRegistry cardRegistry;

        private bool hasRegisteredThisTurn = false;

        #endregion

        #region  의존성 주입 및 초기화

        [Inject]
        public void Construct(ISlotRegistry slotRegistry, ISkillCardFactory cardFactory, ITurnCardRegistry cardRegistry)
        {
            this.slotRegistry = slotRegistry;
            this.cardFactory = cardFactory;
            this.cardRegistry = cardRegistry;
        }

        /// <summary>
        /// 적 캐릭터 정보를 기반으로 핸드 슬롯을 초기화합니다.
        /// </summary>
        public void Initialize(IEnemyCharacter enemy)
        {
            currentEnemy = enemy;
            enemyDeck = enemy.Data?.EnemyDeck;

            handSlots.Clear();
            cardUIs.Clear();
            _cardsInSlots.Clear();

            foreach (var slot in slotRegistry.GetHandSlotRegistry().GetHandSlots(SlotOwner.ENEMY))
                handSlots[slot.GetSlotPosition()] = slot;

            if (cardUIPrefab == null)
                cardUIPrefab = Resources.Load<SkillCardUI>("UI/SkillCardUI");
        }

        #endregion

        #region  카드 생성 및 슬롯 자동 채우기

        /// <summary>
        /// 현재 핸드를 비우고 슬롯을 다시 채웁니다.
        /// </summary>
        public void GenerateInitialHand()
        {
            ClearHand();
            StartCoroutine(StepwiseFillSlotsFromBack());
        }

        /// <summary>
        /// 슬롯을 순차적으로 채워나가는 루틴입니다.
        /// </summary>
        public IEnumerator StepwiseFillSlotsFromBack(float delay = 0.5f)
        {
            while (true)
            {
                bool didSomething = false;

                // 1단계: 3번 슬롯이 비었으면 생성
                if (IsSlotEmpty(SkillCardSlotPosition.ENEMY_SLOT_3))
                {
                    CreateCardInSlot(SkillCardSlotPosition.ENEMY_SLOT_3);
                    yield return new WaitForSeconds(delay);
                    didSomething = true;
                }

                // 2단계: 2번 슬롯 비었고, 3번에 있으면 이동
                if (IsSlotEmpty(SkillCardSlotPosition.ENEMY_SLOT_2) &&
                    !IsSlotEmpty(SkillCardSlotPosition.ENEMY_SLOT_3))
                {
                    ShiftSlot(SkillCardSlotPosition.ENEMY_SLOT_3, SkillCardSlotPosition.ENEMY_SLOT_2);
                    yield return new WaitForSeconds(delay);
                    didSomething = true;
                }

                // 3단계: 다시 3번 생성
                if (IsSlotEmpty(SkillCardSlotPosition.ENEMY_SLOT_3))
                {
                    CreateCardInSlot(SkillCardSlotPosition.ENEMY_SLOT_3);
                    yield return new WaitForSeconds(delay);
                    didSomething = true;
                }

                // 4단계: 1번 비었고, 2번에 있으면 이동
                if (IsSlotEmpty(SkillCardSlotPosition.ENEMY_SLOT_1) &&
                    !IsSlotEmpty(SkillCardSlotPosition.ENEMY_SLOT_2))
                {
                    ShiftSlot(SkillCardSlotPosition.ENEMY_SLOT_2, SkillCardSlotPosition.ENEMY_SLOT_1);
                    yield return new WaitForSeconds(delay);
                    didSomething = true;
                }

                // 5단계: 2번 비었고, 3번에 있으면 이동
                if (IsSlotEmpty(SkillCardSlotPosition.ENEMY_SLOT_2) &&
                    !IsSlotEmpty(SkillCardSlotPosition.ENEMY_SLOT_3))
                {
                    ShiftSlot(SkillCardSlotPosition.ENEMY_SLOT_3, SkillCardSlotPosition.ENEMY_SLOT_2);
                    yield return new WaitForSeconds(delay);
                    didSomething = true;
                }

                // 6단계: 다시 3번 생성
                if (IsSlotEmpty(SkillCardSlotPosition.ENEMY_SLOT_3))
                {
                    CreateCardInSlot(SkillCardSlotPosition.ENEMY_SLOT_3);
                    yield return new WaitForSeconds(delay);
                    didSomething = true;
                }

                if (!didSomething)
                    yield break;

                yield return null;
            }
        }

        #endregion

        #region  전투 슬롯 등록

        /// <summary>
        /// 적의 슬롯에서 카드를 꺼내 전투 슬롯에 등록합니다.
        /// </summary>
        public (ISkillCard card, SkillCardUI ui, CombatSlotPosition pos) PopCardAndRegisterToCombatSlot(ICombatFlowCoordinator flowCoordinator)
        {
            var (card, ui) = PopCardFromSlot(SkillCardSlotPosition.ENEMY_SLOT_1);
            if (card == null || ui == null)
            {
                Debug.LogWarning("[EnemyHandManager] 전투 슬롯 등록 실패: 카드 또는 UI가 null");
                return (null, null, CombatSlotPosition.FIRST);
            }

            var isFirst = UnityEngine.Random.value < 0.5f;
            var pos = isFirst ? CombatSlotPosition.FIRST : CombatSlotPosition.SECOND;

            flowCoordinator.RegisterCardToCombatSlot(pos, card, ui);
            cardRegistry.RegisterCard(pos, card, ui, SlotOwner.ENEMY);

            Debug.Log($"[EnemyHandManager] 전투 슬롯 등록 완료 → {card.GetCardName()} to {pos}");
            return (card, ui, pos);
        }

        #endregion

        #region  카드 조회 및 조작

        public ISkillCard GetCardForCombat() => GetSlotCard(SkillCardSlotPosition.ENEMY_SLOT_1);
        public ISkillCard GetSlotCard(SkillCardSlotPosition pos) => handSlots.TryGetValue(pos, out var slot) ? slot.GetCard() : null;
        public ISkillCardUI GetCardUI(int index)
        {
            var pos = (SkillCardSlotPosition)(index + (int)SkillCardSlotPosition.ENEMY_SLOT_1);
            return cardUIs.TryGetValue(pos, out var ui) ? ui : null;
        }

        public void RegisterCardToSlot(SkillCardSlotPosition pos, ISkillCard card, SkillCardUI ui)
        {
            if (!handSlots.TryGetValue(pos, out var slot)) return;

            slot.SetCard(card);
            cardUIs[pos] = ui;
            _cardsInSlots[pos] = (card, ui);

            if (slot is Game.CombatSystem.UI.EnemyHandCardSlotUI uiSlot)
                uiSlot.SetCardUI(ui);
        }

        public (ISkillCard, ISkillCardUI) PeekCardInSlot(SkillCardSlotPosition position)
        {
            if (_cardsInSlots.TryGetValue(position, out var value))
                return value;
            return (null, null);
        }

        public (ISkillCard card, SkillCardUI ui) PopFirstAvailableCard()
        {
            foreach (SkillCardSlotPosition pos in Enum.GetValues(typeof(SkillCardSlotPosition)))
            {
                var (card, ui) = PeekCardInSlot(pos);
                if (card != null)
                    return PopCardFromSlot(pos);
            }

            return (null, null);
        }

        public (ISkillCard card, SkillCardUI ui) PopCardFromSlot(SkillCardSlotPosition pos)
        {
            if (!handSlots.TryGetValue(pos, out var slot)) return (null, null);

            var card = slot.GetCard();
            var ui = cardUIs.TryGetValue(pos, out var foundUI) ? foundUI : null;

            slot.Clear();
            if (slot is Game.CombatSystem.UI.EnemyHandCardSlotUI uiSlot)
                uiSlot.SetCardUI(null);

            if (ui != null)
            {
                ui.transform.SetParent(null);
                cardUIs.Remove(pos);
            }

            _cardsInSlots.Remove(pos);
            return (card, ui);
        }

        /// <inheritdoc/>
        public ISkillCard PickCardForSlot(SkillCardSlotPosition pos) => GetSlotCard(pos);

        /// <inheritdoc/>
        public SkillCardUI RemoveCardFromSlot(SkillCardSlotPosition pos)
        {
            if (!handSlots.TryGetValue(pos, out var slot)) return null;

            slot.Clear();

            if (cardUIs.TryGetValue(pos, out var ui))
            {
                Destroy(ui.gameObject);
                cardUIs.Remove(pos);
                _cardsInSlots.Remove(pos);
                return ui;
            }

            _cardsInSlots.Remove(pos);
            return null;
        }

        #endregion

        #region  정리 및 상태

        public void ClearHand()
        {
            foreach (var slot in handSlots.Values)
                slot?.Clear();

            foreach (var ui in cardUIs.Values)
                if (ui != null) Destroy(ui.gameObject);

            cardUIs.Clear();
            _cardsInSlots.Clear();
        }

        public void ClearAllCards() => ClearHand();
        public void ResetTurnRegistrationFlag() => hasRegisteredThisTurn = false;
        public void LogHandSlotStates()
        {
            foreach (var kvp in handSlots)
                Debug.Log($"[Slot {kvp.Key}] 카드 있음: {kvp.Value?.GetCard() != null}");
        }

        public bool HasInitializedEnemy(IEnemyCharacter enemy) => currentEnemy == enemy;

        #endregion

        #region  내부 헬퍼

        private bool IsSlotEmpty(SkillCardSlotPosition pos)
        {
            return handSlots.TryGetValue(pos, out var slot) && slot.GetCard() == null;
        }

        private void CreateCardInSlot(SkillCardSlotPosition pos)
        {
            if (enemyDeck == null)
            {
                Debug.LogError("[EnemyHandManager] EnemyDeck이 null입니다.");
                return;
            }

            if (!handSlots.TryGetValue(pos, out var slot))
            {
                Debug.LogError($"[EnemyHandManager] 슬롯 {pos}를 찾을 수 없습니다.");
                return;
            }

            var entry = enemyDeck.GetRandomEntry();
            if (entry?.card == null)
            {
                Debug.LogWarning("[EnemyHandManager] 생성할 카드 엔트리가 유효하지 않음");
                return;
            }

            var runtimeCard = cardFactory.CreateEnemyCard(entry.card.GetCardData(), entry.card.CreateEffects());
            runtimeCard.SetHandSlot(pos);

            var cardUI = Instantiate(cardUIPrefab, ((MonoBehaviour)slot).transform);
            cardUI.SetCard(runtimeCard);
            cardUI.transform.localPosition = Vector3.zero;
            cardUI.transform.localScale = Vector3.one;

            slot.SetCard(runtimeCard);
            if (slot is Game.CombatSystem.UI.EnemyHandCardSlotUI uiSlot)
                uiSlot.SetCardUI(cardUI);

            cardUIs[pos] = cardUI;
            _cardsInSlots[pos] = (runtimeCard, cardUI);

            Debug.Log($"[EnemyHandManager] 카드 생성 완료: {runtimeCard.GetCardName()} → {pos}");
        }

        private bool ShiftSlot(SkillCardSlotPosition from, SkillCardSlotPosition to)
        {
            if (!handSlots.TryGetValue(from, out var fromSlot) || !handSlots.TryGetValue(to, out var toSlot))
                return false;

            var card = fromSlot.GetCard();
            if (card == null || toSlot.GetCard() != null)
                return false;

            var ui = cardUIs.TryGetValue(from, out var oldUI) ? oldUI : null;

            // 1. UI 애니메이션 실행을 위한 준비
            if (ui != null && ui.TryGetComponent<SkillCardShiftAnimator>(out var animator))
            {
                var toSlotRect = ((MonoBehaviour)toSlot).GetComponent<RectTransform>();

                // 월드 좌표 유지한 채 부모 임시 변경
                ui.transform.SetParent(toSlotRect.parent, true);

                // 비동기 애니메이션 실행을 따로 처리 (동기 흐름 유지)
                _ = animator.PlayMoveAnimationAsync(toSlotRect).ContinueWith(_ =>
                {
                    // 애니메이션이 끝난 후 카드 위치 갱신
                    UnityMainThreadDispatcher.Enqueue(() =>
                    {
                        ui.transform.SetParent(((MonoBehaviour)toSlot).transform, false);
                        ui.transform.localPosition = Vector3.zero;
                        ui.transform.localScale = Vector3.one;
                    });
                });
            }

            // 2. 슬롯 및 카드 데이터 갱신
            fromSlot.Clear();
            toSlot.SetCard(card);
            card.SetHandSlot(to);

            if (ui != null)
            {
                cardUIs[to] = ui;
                cardUIs.Remove(from);
            }

            _cardsInSlots[to] = (card, ui);
            _cardsInSlots.Remove(from);

            Debug.Log($"[EnemyHandManager] 카드 이동: {from} → {to}");
            return true;
        }


        #endregion
    }
}
