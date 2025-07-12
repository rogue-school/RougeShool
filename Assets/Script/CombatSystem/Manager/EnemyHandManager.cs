using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using Game.CombatSystem.Interface;
using Game.CombatSystem.Slot;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.Core;
using Game.SkillCardSystem.UI;
using Game.SkillCardSystem.Data;
using Game.SkillCardSystem.Slot;
using Game.SkillCardSystem.Deck;
using Game.CharacterSystem.Interface;
using Game.CharacterSystem.Core;
using Game.IManager;
using Game.CombatSystem.Utility;
using System.Threading.Tasks;
using Game.CombatSystem.Manager;
using AnimationSystem.Animator;
using AnimationSystem.Manager;
using Game.CombatSystem.Core;
using AnimationSystem.Helper;
using AnimationSystem.Animator.SkillCardAnimation.MoveAnimation;

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
            int maxIterations = 10; // 최대 10번만 반복
            int iterationCount = 0;
            
            while (iterationCount < maxIterations)
            {
                iterationCount++;
                
                bool didSomething = false;

                // 3번 슬롯이 비어 있으면 새 카드 생성
                if (IsSlotEmpty(SkillCardSlotPosition.ENEMY_SLOT_3))
                {
                    yield return CreateCardInSlotWithAnimation(SkillCardSlotPosition.ENEMY_SLOT_3);
                    yield return new WaitForSeconds(delay);
                    didSomething = true;
                }
                // 2번 슬롯이 비어 있으면 3→2 이동
                else if (IsSlotEmpty(SkillCardSlotPosition.ENEMY_SLOT_2))
                {
                    yield return ShiftSlotWithAnimation(SkillCardSlotPosition.ENEMY_SLOT_3, SkillCardSlotPosition.ENEMY_SLOT_2);
                    yield return new WaitForSeconds(delay);
                    didSomething = true;
                }
                // 1번 슬롯이 비어 있으면 2→1 이동
                else if (IsSlotEmpty(SkillCardSlotPosition.ENEMY_SLOT_1))
                {
                    yield return ShiftSlotWithAnimation(SkillCardSlotPosition.ENEMY_SLOT_2, SkillCardSlotPosition.ENEMY_SLOT_1);
                    yield return new WaitForSeconds(delay);
                    didSomething = true;
                }

                // 더 이상 할 일이 없으면 루프 종료
                if (!didSomething)
                {
                    break;
                }
            }
            
            if (iterationCount >= maxIterations)
            {
                Debug.LogWarning("[EnemyHandManager] StepwiseFillSlotsFromBack 무한 루프 방지로 종료됨");
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

            return (card, ui, pos);
        }

        #endregion

        #region  카드 조회 및 조작

        public ISkillCard GetCardForCombat() => GetSlotCard(SkillCardSlotPosition.ENEMY_SLOT_1);
        public ISkillCard GetSlotCard(SkillCardSlotPosition pos) => handSlots.TryGetValue(pos, out var slot) ? slot.GetCard() : null;
        
        public ISkillCardUI GetCardUI(int index)
        {
            var pos = (SkillCardSlotPosition)index;
            return cardUIs.TryGetValue(pos, out var ui) ? ui : null;
        }

        public void RegisterCardToSlot(SkillCardSlotPosition pos, ISkillCard card, SkillCardUI ui)
        {
            if (handSlots.TryGetValue(pos, out var slot))
            {
                slot.SetCard(card);
                cardUIs[pos] = ui;
                _cardsInSlots[pos] = (card, ui);
            }
        }

        public (ISkillCard, ISkillCardUI) PeekCardInSlot(SkillCardSlotPosition position)
        {
            if (_cardsInSlots.TryGetValue(position, out var tuple))
            {
                return (tuple.Item1, tuple.Item2);
            }
            return (null, null);
        }

        public (ISkillCard card, SkillCardUI ui) PopFirstAvailableCard()
        {
            for (int i = 1; i <= 3; i++)
            {
                var pos = (SkillCardSlotPosition)i;
                var (card, ui) = PopCardFromSlot(pos);
                if (card != null && ui != null)
                    return (card, ui);
            }
            return (null, null);
        }

        public (ISkillCard card, SkillCardUI ui) PopCardFromSlot(SkillCardSlotPosition pos)
        {
            var (card, ui) = PeekCardInSlot(pos);
            if (card != null && ui != null)
            {
                RemoveCardFromSlot(pos);
                return (card, ui as SkillCardUI);
            }
            return (null, null);
        }

        public ISkillCard PickCardForSlot(SkillCardSlotPosition pos) => GetSlotCard(pos);

        public SkillCardUI RemoveCardFromSlot(SkillCardSlotPosition pos)
        {
            if (handSlots.TryGetValue(pos, out var slot))
            {
                slot.Clear();
                if (cardUIs.TryGetValue(pos, out var ui))
                {
                    cardUIs.Remove(pos);
                    _cardsInSlots.Remove(pos);
                    return ui;
                }
            }
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
        public void ResetTurnRegistrationFlag()
        {
            // hasRegisteredThisTurn = false; // 사용하지 않는 필드이므로 삭제
        }
        public void LogHandSlotStates()
        {
            // 디버깅용 로그 제거 - 필요시 개발자가 임시로 활성화
            // foreach (var pos in Enum.GetValues(typeof(SkillCardSlotPosition)))
            // {
            //     var slot = handSlots.TryGetValue((SkillCardSlotPosition)pos, out var s) ? s : null;
            //     var card = slot?.GetCard();
            //     var ui = cardUIs.ContainsKey((SkillCardSlotPosition)pos) ? cardUIs[(SkillCardSlotPosition)pos] : null;
            //     var dict = _cardsInSlots.ContainsKey((SkillCardSlotPosition)pos) ? _cardsInSlots[(SkillCardSlotPosition)pos].Item1 : null;
            //     Debug.Log($"[LogHandSlotStates] {pos}: card={(card != null ? card.GetCardName() : "null")}, ui={(ui != null ? "O" : "X")}, dict={(dict != null ? dict.GetCardName() : "null")}");
            // }
        }

        public bool HasInitializedEnemy(IEnemyCharacter enemy) => currentEnemy == enemy;

        #endregion

        #region  내부 헬퍼

        private bool IsSlotEmpty(SkillCardSlotPosition pos)
        {
            // 슬롯, 카드UI, 내부 딕셔너리 중 하나라도 비어있으면 슬롯이 비어있음으로 간주
            bool slotEmpty = !handSlots.TryGetValue(pos, out var slot) || slot.GetCard() == null;
            bool uiEmpty = !cardUIs.ContainsKey(pos) || cardUIs[pos] == null;
            bool dictEmpty = !_cardsInSlots.ContainsKey(pos) || _cardsInSlots[pos].Item1 == null;
            bool result = slotEmpty || uiEmpty || dictEmpty;
            
            return result;
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
            // 부모/위치/스케일을 명확히 지정
            cardUI.transform.SetParent(((MonoBehaviour)slot).transform, false);
            cardUI.transform.localPosition = Vector3.zero;
            cardUI.transform.localScale = Vector3.one;

            slot.SetCard(runtimeCard);
            if (slot is Game.CombatSystem.UI.EnemyHandCardSlotUI uiSlot)
                uiSlot.SetCardUI(cardUI);

            cardUIs[pos] = cardUI;
            _cardsInSlots[pos] = (runtimeCard, cardUI);
        }

        // 카드 생성 + 생성 애니메이션 대기용 코루틴 (슬롯 할당은 애니메이션 후에)
        private IEnumerator CreateCardInSlotWithAnimation(SkillCardSlotPosition pos)
        {
            CreateCardInSlot(pos);
            var (card, ui) = PeekCardInSlot(pos);
            if (card != null && ui != null)
            {
                var uiObj = ui as SkillCardUI;
                if (uiObj != null)
                {
                    bool animDone = false;
                    AnimationSystem.Manager.AnimationFacade.Instance.PlaySkillCardAnimation(card, "spawn", uiObj.gameObject, () => animDone = true);
                    
                    // 애니메이션 완료까지 대기 (타임아웃 추가)
                    float waitStartTime = Time.time;
                    while (!animDone)
                    {
                        if (Time.time - waitStartTime > 2f) // 2초 타임아웃
                        {
                            Debug.LogWarning($"[EnemyHandManager] Spawn 애니메이션 타임아웃: {pos}");
                            break;
                        }
                        yield return null;
                    }
                }
            }
        }

        private IEnumerator InternalCreateCardInSlotWithAnimation(SkillCardSlotPosition pos, System.Action onComplete)
        {
            if (enemyDeck == null)
            {
                Debug.LogError("[EnemyHandManager] EnemyDeck이 null입니다.");
                onComplete?.Invoke();
                yield break;
            }

            var entry = enemyDeck.GetRandomEntry();
            if (entry?.card == null)
            {
                Debug.LogWarning("[EnemyHandManager] 생성할 카드 엔트리가 유효하지 않음");
                onComplete?.Invoke();
                yield break;
            }

            var runtimeCard = cardFactory.CreateEnemyCard(entry.card.GetCardData(), entry.card.CreateEffects());
            if (runtimeCard == null)
            {
                Debug.LogError("[EnemyHandManager] 카드 생성 실패");
                onComplete?.Invoke();
                yield break;
            }

            var cardUI = Instantiate(cardUIPrefab, ((MonoBehaviour)handSlots[pos]).transform);
            if (cardUI == null)
            {
                Debug.LogError("[EnemyHandManager] 카드UI 생성 실패");
                onComplete?.Invoke();
                yield break;
            }

            // 카드UI 설정
            cardUI.SetCard(runtimeCard);
            cardUI.transform.localPosition = Vector3.zero;
            cardUI.transform.localScale = Vector3.one;

            // 슬롯에 카드/카드UI 할당 (애니메이션 실행 전)
            if (handSlots.TryGetValue(pos, out var slot))
            {
                slot.SetCard(runtimeCard);
                cardUIs[pos] = cardUI;
                _cardsInSlots[pos] = (runtimeCard, cardUI);
                
                // 애니메이션 이벤트 발행
                CombatEvents.Card.RaiseEnemyCardSpawn(runtimeCard.GetCardName(), cardUI.gameObject);
                
                // 애니메이션 완료까지 대기
                yield return new WaitForSeconds(0.5f);
            }
            else
            {
                Debug.LogError($"[EnemyHandManager] 슬롯 {pos}를 찾을 수 없습니다.");
            }
            
            // 카드 생성 후 데이터 기반 애니메이션 실행
            if (cardUI != null && runtimeCard != null)
            {
                bool animDone = false;
                AnimationSystem.Manager.AnimationFacade.Instance.PlaySkillCardAnimation(runtimeCard, "spawn", cardUI.gameObject, () => animDone = true);
                yield return new WaitUntil(() => animDone);
            }
            
            onComplete?.Invoke();
        }

        private IEnumerator ShiftSlotWithAnimation(SkillCardSlotPosition from, SkillCardSlotPosition to)
        {
            if (_cardsInSlots.TryGetValue(from, out var tuple))
            {
                var uiObj = tuple.Item2 as Game.SkillCardSystem.UI.SkillCardUI;
                if (uiObj == null)
                {
                    Debug.LogError($"[EnemyHandManager] uiObj가 null입니다. from: {from}, to: {to}");
                    yield break;
                }
                var slotObj = handSlots[to] as MonoBehaviour;
                if (slotObj == null)
                {
                    Debug.LogError($"[EnemyHandManager] slotObj가 null입니다. to: {to}");
                    yield break;
                }
                var targetSlotRect = slotObj.GetComponent<RectTransform>();
                if (targetSlotRect == null)
                {
                    Debug.LogError($"[EnemyHandManager] targetSlotRect가 null입니다. to: {to}, slotObj: {slotObj.name}");
                    yield break;
                }
                // DefaultSkillCardMoveAnimation을 안전하게 가져오거나 자동 부착
                var animator = AnimationHelper.GetOrAddAnimator<DefaultSkillCardMoveAnimation>(uiObj.gameObject);
                bool animDone = false;
                animator.PlayAnimation(targetSlotRect, () => { animDone = true; });
                
                // 애니메이션 완료까지 대기 (타임아웃 추가)
                float waitStartTime = Time.time;
                while (!animDone)
                {
                    if (Time.time - waitStartTime > 2f) // 2초 타임아웃
                    {
                        Debug.LogWarning($"[EnemyHandManager] Move 애니메이션 타임아웃: {from} -> {to}");
                        break;
                    }
                    yield return null;
                }

                // 슬롯/카드UI/딕셔너리 모두 동기화
                if (handSlots.TryGetValue(from, out var fromSlot))
                    fromSlot.Clear();
                if (handSlots.TryGetValue(to, out var toSlot))
                    toSlot.SetCard(tuple.Item1);
                cardUIs.Remove(from);
                cardUIs[to] = uiObj;
                _cardsInSlots.Remove(from);
                _cardsInSlots[to] = tuple;
            }
            else
            {
                Debug.LogWarning($"[EnemyHandManager] 이동 애니메이션 실행 실패: from 슬롯({from})에 카드가 없음");
            }
        }

        private IEnumerator InternalShiftSlotWithAnimation(SkillCardSlotPosition from, SkillCardSlotPosition to, System.Action onComplete)
        {
            if (!handSlots.TryGetValue(from, out var fromSlot) || !handSlots.TryGetValue(to, out var toSlot))
            {
                Debug.LogError($"[EnemyHandManager] 슬롯을 찾을 수 없습니다: from={from}, to={to}");
                onComplete?.Invoke();
                yield break;
            }

            var card = fromSlot.GetCard();
            var cardUI = cardUIs.ContainsKey(from) ? cardUIs[from] : null;
            var targetSlotRect = (toSlot as MonoBehaviour)?.GetComponent<RectTransform>();
            if (card == null || cardUI == null || targetSlotRect == null)
            {
                Debug.LogWarning($"[EnemyHandManager] 이동할 카드 또는 목표 슬롯 RectTransform이 없습니다: from={from}, to={to}");
                onComplete?.Invoke();
                yield break;
            }

            // 애니메이션 실행
            var animator = AnimationHelper.GetOrAddAnimator<DefaultSkillCardMoveAnimation>(cardUI.gameObject);
            bool animDone = false;
            animator.PlayAnimation(targetSlotRect, () => { animDone = true; });
            yield return new WaitUntil(() => animDone);

            // 슬롯 상태 갱신 (애니메이션 완료 후)
            fromSlot.Clear();
            toSlot.SetCard(card);
            cardUIs.Remove(from);
            cardUIs[to] = cardUI;
            _cardsInSlots.Remove(from);
            _cardsInSlots[to] = (card, cardUI);

            // 이동 이벤트 발행
            CombatEvents.Card.RaiseEnemyCardMoved(card.GetCardName(), cardUI.gameObject, Game.CombatSystem.Slot.CombatSlotPosition.FIRST);

            onComplete?.Invoke();
        }
        public IEnumerator PopCardAndRegisterToCombatSlotCoroutine(ICombatFlowCoordinator flowCoordinator)
        {
            var (card, ui) = PopCardFromSlot(SkillCardSlotPosition.ENEMY_SLOT_1);
            if (card == null || ui == null)
            {
                Debug.LogWarning("[EnemyHandManager] 카드 등록 실패: 카드 또는 UI가 null");
                yield break;
            }

            var slotPos = flowCoordinator.IsEnemyFirst ? CombatSlotPosition.FIRST : CombatSlotPosition.SECOND;

            flowCoordinator.RegisterCardToTurnRegistry(slotPos, card, ui);

            // 애니메이션이 있는 버전으로 변경
            yield return flowCoordinator.RegisterCardToCombatSlotAsync(slotPos, card, ui);
        }
        #endregion

        // 모든 카드 애니메이션이 끝난 후에만 ClearHand를 호출하는 안전 메서드
        public IEnumerator SafeClearHandAfterAllAnimations()
        {
            yield return new WaitUntil(() => !IsAnyCardAnimating());
            ClearHand();
        }

        // 현재 핸드 내 모든 카드가 애니메이션 중인지 체크 (임시: 실제 구현 필요)
        private bool IsAnyCardAnimating()
        {
            foreach (var tuple in _cardsInSlots.Values)
            {
                if (tuple.Item2 is Game.SkillCardSystem.UI.SkillCardUI uiObj && uiObj.IsAnimating)
                    return true;
            }
            return false;
        }
    }
}
