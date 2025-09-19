using System;
using System.Collections;
using UnityEngine;
using Zenject;
using Game.CombatSystem.Interface;
using Game.CombatSystem.Data;
using Game.CombatSystem.Slot;
using Game.IManager;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.Slot;
using Game.SkillCardSystem.UI;
using Game.SkillCardSystem.Executor;
using Game.CombatSystem.Context;
using Game.CharacterSystem.Interface;
using Game.CharacterSystem.Core;
using Game.StageSystem.Interface;
using Game.UtilitySystem;
using Game.CoreSystem.Utility;
using Game.CombatSystem.State;
using System.Threading.Tasks;
using Game.CombatSystem;
using Game.AnimationSystem.Interface;
using System.Collections.Generic;

namespace Game.CombatSystem.Manager
{
    /// <summary>
    /// 전투의 전체 흐름을 조율하는 핵심 클래스.
    /// 전투 준비, 공격, 슬롯 등록, 승패 처리 등을 담당한다.
    /// </summary>
    public class CombatFlowCoordinator : MonoBehaviour, ICombatFlowCoordinator
    {
        #region 의존성 주입

        [Inject] private IStageManager stageManager;
        [Inject] private IPlayerManager playerManager;
        [Inject] private IEnemyManager enemyManager;
        [Inject] private IPlayerHandManager playerHandManager;
        // 적 핸드 시스템 제거
        [Inject] private ITurnCardRegistry turnCardRegistry;
        [Inject] private ISlotRegistry slotRegistry;
        [Inject] private ICardExecutor cardExecutor;
        [Inject] private ICoroutineRunner coroutineRunner;
        [Inject] private IAnimationFacade animationFacade;

        #endregion

        #region 내부 상태

        private ICombatTurnManager turnManager;
        // private TurnStartButtonHandler startButtonHandler; // Disabled 폴더로 이동됨
        private bool playerInputEnabled = false;
        public bool IsEnemyFirst { get; set; }
        private Action onStartButtonPressed;

        #endregion

        #region 초기화 및 구성

        [Inject]
        public void Construct(ICombatTurnManager injectedTurnManager)
        {
            turnManager = injectedTurnManager;
        }

        [Inject]
        // public void Construct(TurnStartButtonHandler startButtonHandler) // Disabled 폴더로 이동됨
        // {
        //     this.startButtonHandler = startButtonHandler;
        // }

        public void InjectTurnStateDependencies(ICombatTurnManager turnManager)
        {
            this.turnManager = turnManager;
        }

        // 인터페이스 호환성을 위한 오버로드(상태팩토리는 TurnManager 내부에서 관리)
        public void InjectTurnStateDependencies(ICombatTurnManager turnManager, ICombatStateFactory _)
        {
            this.turnManager = turnManager;
        }

        public void StartCombatFlow()
        {
            Debug.Log("[CombatFlowCoordinator] 전투 시작");
            DisableStartButton();
            
            // turnManager null 체크
            if (turnManager == null)
            {
                Debug.LogError("[CombatFlowCoordinator] turnManager가 null입니다. DI 바인딩을 확인해주세요.");
                return;
            }
            
            // StateFactory null 체크
            var stateFactory = turnManager.GetStateFactory();
            if (stateFactory == null)
            {
                Debug.LogError("[CombatFlowCoordinator] StateFactory가 null입니다. turnManager 초기화를 확인해주세요.");
                return;
            }
            
            // CombatPrepareState로 전환하여 전투 시작 시퀀스 실행
            var prepareState = stateFactory.CreatePrepareState();
            if (prepareState == null)
            {
                Debug.LogError("[CombatFlowCoordinator] CombatPrepareState 생성에 실패했습니다.");
                return;
            }
            
            turnManager.RequestStateChange(prepareState);
        }

        #endregion

        #region 이벤트 구독 및 해제

        private void OnEnable()
        {
            // 적 캐릭터 사망 이벤트 구독
            Game.CombatSystem.CombatEvents.OnEnemyCharacterDeath += OnEnemyCharacterDeath;
            Debug.Log("[CombatFlowCoordinator] 적 캐릭터 사망 이벤트 구독 완료");
        }

        private void OnDisable()
        {
            // 적 캐릭터 사망 이벤트 구독 해제
            Game.CombatSystem.CombatEvents.OnEnemyCharacterDeath -= OnEnemyCharacterDeath;
            Debug.Log("[CombatFlowCoordinator] 적 캐릭터 사망 이벤트 구독 해제 완료");
        }

        #endregion

        #region 인터페이스 구현


        public void RegisterCardToTurnRegistry(CombatSlotPosition pos, ISkillCard card, ISkillCardUI ui)
        {
            if (ui is not SkillCardUI concreteUI)
            {
                Debug.LogError("[CombatFlowCoordinator] RegisterCardToTurnRegistry 실패: ui가 SkillCardUI 아님");
                return;
            }

            turnCardRegistry.RegisterCard(pos, card, concreteUI, SlotOwner.ENEMY);
        }

        public IEnumerator RegisterCardToCombatSlotCoroutine(CombatSlotPosition pos, ISkillCard card, ISkillCardUI ui)
        {
            yield return RegisterCardToCombatSlotCoroutine(pos, card, ui as SkillCardUI); // 다운캐스팅 유의
        }


        #endregion

        #region 카드 및 슬롯 등록

        /// <summary>
        /// 카드와 UI를 지정된 전투 슬롯에 등록합니다.
        /// </summary>
        public void RegisterCardToCombatSlot(CombatSlotPosition pos, ISkillCard card, SkillCardUI ui)
        {
            var slot = slotRegistry.GetCombatSlot(pos);
            if (slot is not ICombatCardSlot combatSlot)
            {
                Debug.LogError($"[CombatFlowCoordinator] 전투 슬롯 {pos}이 null이거나 ICombatCardSlot이 아님.");
                return;
            }

            ui.transform.SetParent(combatSlot.GetTransform());
            ui.transform.localPosition = Vector3.zero;
            ui.transform.localScale = Vector3.one;

            combatSlot.SetCard(card);
            combatSlot.SetCardUI(ui);
        }
        /// <summary>
        /// 카드와 UI를 지정된 전투 슬롯에 애니메이션을 포함하여 등록합니다.
        /// </summary>
        /// <summary>
        /// 카드와 UI를 지정된 전투 슬롯에 애니메이션을 포함하여 등록합니다.
        /// </summary>
        public IEnumerator RegisterCardToCombatSlotAsync(CombatSlotPosition pos, ISkillCard card, SkillCardUI ui)
        {
            var slot = slotRegistry.GetCombatSlot(pos);
            if (slot is not ICombatCardSlot combatSlot)
            {
                Debug.LogError($"[CombatFlowCoordinator] 전투 슬롯 {pos}이 null이거나 ICombatCardSlot이 아님.");
                yield break;
            }

            var slotRectTransform = combatSlot.GetTransform() as RectTransform;
            if (slotRectTransform == null)
            {
                Debug.LogError($"[CombatFlowCoordinator] 슬롯 {pos}의 Transform이 RectTransform이 아닙니다.");
                yield break;
            }

            bool animationDone = false;

            yield return StartCoroutine(InternalRegisterCardToCombatSlotAsync(pos, card, ui, slotRectTransform, () => {
                animationDone = true;
            }));

            // 애니메이션 완료까지 대기
            float waitStartTime = Time.time;
            while (!animationDone)
            {
                if (Time.time - waitStartTime > 35f) // 35초 타임아웃
                {
                    Debug.LogError($"[CombatFlowCoordinator] 애니메이션 대기 타임아웃: {pos}");
                    break;
                }
                yield return null; // Task.Yield() 대신 yield return null 사용
            }
        }

        private IEnumerator InternalRegisterCardToCombatSlotAsync(CombatSlotPosition pos, ISkillCard card, SkillCardUI ui, RectTransform slotRectTransform, System.Action onComplete)
        {
            float startTime = Time.time;

            if (ui is MonoBehaviour uiMb)
            {
                var rect = uiMb.GetComponent<RectTransform>();
                if (rect != null)
                {
                    Vector3 worldPos = rect.position;
                    // 1. 부모를 슬롯(CombatCardSlot_1, 2)로 먼저 변경
                    uiMb.transform.SetParent(slotRectTransform, false);
                    rect.position = worldPos;
                    Debug.Log($"[CombatFlowCoordinator] 카드를 슬롯({slotRectTransform.name})으로 부모 변경");
                }

                // 2. 이동 애니메이션 실행 (AnimationFacade 사용)
                bool animationComplete = false;
                animationFacade.PlaySkillCardAnimation(card, uiMb.gameObject, "moveToCombatSlot", () => {
                    animationComplete = true;
                });
                
                // 애니메이션 완료까지 대기
                float waitStartTime = Time.time;
                while (!animationComplete)
                {
                    if (Time.time - waitStartTime > 5f) // 5초 타임아웃
                    {
                        Debug.LogWarning($"[CombatFlowCoordinator] 이동 애니메이션 타임아웃: {pos}");
                        break;
                    }
                    yield return null;
                }

                // 슬롯에 부착
                uiMb.transform.SetParent(slotRectTransform, false);
                uiMb.transform.localPosition = Vector3.zero;
                uiMb.transform.localScale = Vector3.one;
            }
            else
            {
                Debug.LogWarning($"[CombatFlowCoordinator] UI가 MonoBehaviour가 아님: {pos}");
                // 애니메이션 없는 기본 처리
                ui.transform.SetParent(slotRectTransform);
                ui.transform.localPosition = Vector3.zero;
                ui.transform.localScale = Vector3.one;
            }

            // 슬롯에 카드 등록 (애니메이션 완료 후)
            var combatSlot = slotRegistry.GetCombatSlot(pos) as ICombatCardSlot;
            if (combatSlot != null)
            {
                combatSlot.SetCard(card);
                combatSlot.SetCardUI(ui);
            }
            else
            {
                Debug.LogError($"[CombatFlowCoordinator] 전투 슬롯을 찾을 수 없음: {pos}");
            }

            onComplete?.Invoke();
            yield break;
        }
        public IEnumerator RegisterCardToCombatSlotCoroutine(CombatSlotPosition pos, ISkillCard card, SkillCardUI ui)
        {
            var slot = slotRegistry.GetCombatSlot(pos);
            if (slot is not ICombatCardSlot combatSlot)
            {
                Debug.LogError($"[CombatFlowCoordinator] 전투 슬롯 {pos}이 null이거나 ICombatCardSlot이 아님.");
                yield break;
            }

            var slotRect = combatSlot.GetTransform() as RectTransform;
            if (slotRect == null)
            {
                Debug.LogError($"[CombatFlowCoordinator] 슬롯 {pos}의 Transform이 RectTransform이 아님");
                yield break;
            }

            if (ui is MonoBehaviour uiMb)
            {
                // var shiftAnimator = uiMb.GetComponent<DefaultSkillCardMoveAnimation>();
                // var spawnAnimator = uiMb.GetComponent<DefaultSkillCardSpawnAnimation>();

                var rect = uiMb.GetComponent<RectTransform>();
                if (rect != null)
                {
                    Vector3 worldPos = rect.position;
                    uiMb.transform.SetParent(slotRect.parent, true);
                    rect.position = worldPos;
                }

                // if (shiftAnimator != null)
                //     yield return shiftAnimator.PlayMoveAnimationCoroutine(slotRect); // 그림자도 카드와 함께 이동

                uiMb.transform.SetParent(slotRect, false);
                uiMb.transform.localPosition = Vector3.zero;
                uiMb.transform.localScale = Vector3.one;

                // if (spawnAnimator != null)
                //     yield return spawnAnimator.PlaySpawnAnimationCoroutine(null);
            }
            else
            {
                ui.transform.SetParent(slotRect);
                ui.transform.localPosition = Vector3.zero;
                ui.transform.localScale = Vector3.one;
            }

            combatSlot.SetCard(card);
            combatSlot.SetCardUI(ui);
        }

        /// <summary>
        /// 내부 전용: 슬롯 UI만 따로 다시 등록할 때 사용
        /// </summary>
        private void RegisterCardToCombatSlotUI(CombatSlotPosition pos, ISkillCard card, SkillCardUI ui)
        {
            var slot = slotRegistry.GetCombatSlot(pos);
            if (slot is ICombatCardSlot combatSlot)
            {
                ui.transform.SetParent(combatSlot.GetTransform());
                ui.transform.localPosition = Vector3.zero;
                ui.transform.localScale = Vector3.one;
                combatSlot.SetCard(card);
                combatSlot.SetCardUI(ui);
            }
        }

        #endregion

        #region 공격 처리

        public void RequestFirstAttack(Action onComplete = null)
        {
            StartCoroutine(PerformFirstAttackInternal(onComplete));
        }

        public IEnumerator PerformFirstAttack() => PerformFirstAttackInternal(null);

        private IEnumerator PerformFirstAttackInternal(Action onComplete = null)
        {
            CombatEvents.RaiseFirstAttackStarted();
            var firstCard = turnCardRegistry.GetCardInSlot(CombatSlotPosition.BATTLE_SLOT);
            if (firstCard != null)
            {
                ExecuteCard(firstCard);
            }

            yield return new WaitForSeconds(1f);
            onComplete?.Invoke();
        }


        /// <summary>
        /// 카드 실행 및 사망 여부 검사
        /// </summary>
        private void ExecuteCard(ISkillCard card)
        {
            ICharacter source = card.IsFromPlayer() ? playerManager.GetPlayer() : enemyManager.GetEnemy();
            ICharacter target = card.IsFromPlayer() ? enemyManager.GetEnemy() : playerManager.GetPlayer();

            var context = new DefaultCardExecutionContext(card, source, target);

            // ★ 카드 소유 캐릭터 생존 체크 추가
            var owner = card.GetOwner(context);
            if (owner == null || owner.IsDead())
            {
                Debug.LogWarning($"[ExecuteCard] 카드 소유 캐릭터가 이미 사망했습니다. 실행 스킵. 카드: {card.GetCardName()}");
                return;
            }

            cardExecutor.Execute(card, context, turnManager);

            card.SetCurrentCoolTime(card.GetMaxCoolTime());
            slotRegistry.GetCombatSlot(card.GetCombatSlot().Value)?.ClearCardUI();

            CombatEvents.RaiseAttackResultProcessed();

            if (IsPlayerDead())
            {
                var gameOverState = new CombatGameOverState(
                    turnManager, this,
                    slotRegistry as ICombatSlotRegistry,
                    coroutineRunner, playerManager
                );
                turnManager.RequestStateChange(gameOverState);
            }
        }

        #endregion

        #region 결과 처리 단계

        public IEnumerator PerformResultPhase()
        {
            yield return new WaitForSeconds(1f);
            ClearAllSlotUIs();
            ClearEnemyCombatSlots();
        }

        public IEnumerator PerformVictoryPhase()
        {
            yield return new WaitForSeconds(1f);
        }

        public IEnumerator PerformGameOverPhase()
        {
            yield return new WaitForSeconds(1f);
        }

        #endregion

        #region 슬롯 및 적 정리

        public void ClearAllSlotUIs()
        {
            foreach (CombatSlotPosition pos in Enum.GetValues(typeof(CombatSlotPosition)))
                slotRegistry.GetCombatSlot(pos)?.ClearCardUI();
        }

        public void ClearEnemyCombatSlots()
        {
            foreach (CombatSlotPosition pos in Enum.GetValues(typeof(CombatSlotPosition)))
            {
                var card = turnCardRegistry.GetCardInSlot(pos);
                if (card != null && !card.IsFromPlayer())
                    slotRegistry.GetCombatSlot(pos)?.ClearAll();
            }
        }

        private void ClearAllCombatSlots()
        {
            foreach (CombatSlotPosition pos in Enum.GetValues(typeof(CombatSlotPosition)))
                slotRegistry.GetCombatSlot(pos)?.ClearAll();
        }

        public void RemoveEnemyCharacter()
        {
            var enemy = enemyManager.GetEnemy();
            if (enemy is EnemyCharacter concreteEnemy)
            {
                Destroy(concreteEnemy.gameObject);
                enemyManager.ClearEnemy();
            }
        }


        #endregion

        #region 턴 유틸리티

        public ITurnCardRegistry GetTurnCardRegistry() => turnCardRegistry;
        public ISkillCard GetCardInSlot(CombatSlotPosition pos) => turnCardRegistry.GetCardInSlot(pos);
        public IEnemyCharacter GetEnemy() => enemyManager.GetEnemy();

        /// <summary>
        /// 특정 슬롯에 카드를 설정합니다. (저장 시스템용)
        /// </summary>
        /// <param name="pos">슬롯 위치</param>
        /// <param name="card">설정할 카드</param>
        public void SetCardInSlot(CombatSlotPosition pos, ISkillCard card)
        {
            if (turnCardRegistry == null)
            {
                Debug.LogError("[CombatFlowCoordinator] TurnCardRegistry가 null입니다.");
                return;
            }

            // 기존 카드가 있다면 제거
            var existingCard = turnCardRegistry.GetCardInSlot(pos);
            if (existingCard != null)
            {
                turnCardRegistry.RemoveCardFromSlot(pos);
            }

            // 새 카드 설정
            if (card != null)
            {
                turnCardRegistry.RegisterCardToSlot(pos, card, null, SlotOwner.PLAYER);
            }

            Debug.Log($"[CombatFlowCoordinator] 슬롯 {pos}에 카드 설정: {card?.CardDefinition?.CardName ?? "null"}");
        }

        #endregion

        #region 상태 및 플래그

        public bool HasEnemy() => enemyManager.GetEnemy() != null;
        public bool CheckHasNextEnemy() => stageManager.HasNextEnemy();

        public bool IsPlayerDead()
        {
            var player = playerManager.GetPlayer();
            return player == null || player.IsDead();
        }

        public bool IsEnemyDead()
        {
            var enemy = enemyManager.GetEnemy();
            return enemy is EnemyCharacter e && e.IsMarkedDead && e.GetCurrentHP() <= 0;
        }

        public void EnablePlayerInput() 
        { 
            playerInputEnabled = true;
            CombatEvents.RaisePlayerInputEnabled();
        }
        public void DisablePlayerInput() 
        { 
            playerInputEnabled = false;
            CombatEvents.RaisePlayerInputDisabled();
        }
        public bool IsPlayerInputEnabled() => playerInputEnabled;

        #endregion

        #region UI 및 버튼

        public void EnableStartButton() 
        { 
            // startButtonHandler?.SetInteractable(true); // Disabled 폴더로 이동됨
            CombatEvents.RaiseStartButtonEnabled();
        }
        public void DisableStartButton() 
        { 
            // startButtonHandler?.SetInteractable(false); // Disabled 폴더로 이동됨
            CombatEvents.RaiseStartButtonDisabled();
        }
        public void RegisterStartButton(Action callback) => onStartButtonPressed = callback;
        public void UnregisterStartButton() => onStartButtonPressed = null;
        public void OnStartButtonClickedExternally() => onStartButtonPressed?.Invoke();

        public void ShowPlayerCardSelectionUI() { }
        public void HidePlayerCardSelectionUI() { }

        #endregion

        #region 적 소환

        public void SpawnNextEnemy() => stageManager.SpawnNextEnemy();

        #endregion

        private void OnPlayerCharacterSpawned(string characterId, GameObject characterObject)
        {
            animationFacade.PlayCharacterAnimation(characterId, "spawn", characterObject);
        }
        private void OnEnemyCharacterSpawned(string enemyId, GameObject enemyObject)
        {
            animationFacade.PlayCharacterAnimation(enemyId, "spawn", enemyObject, null, true);
        }
        private void OnPlayerSkillCardUsed(string cardId, GameObject cardObject)
        {
            // AnimationFacade.Instance.PlaySkillCardAnimation(cardId, "use", cardObject); // 제거
        }
        private void OnEnemySkillCardUsed(string cardId, GameObject cardObject)
        {
            // AnimationFacade.Instance.PlaySkillCardAnimation(cardId, "use", cardObject); // 제거
        }
        private void OnPlayerCharacterDeath(string characterId, GameObject characterObject)
        {
            // AnimationFacade.Instance.PlayCharacterDeathAnimation(characterId, characterObject); // 제거
        }
        private void OnEnemyCharacterDeath(Game.CharacterSystem.Data.EnemyCharacterData enemyData, GameObject enemyObject)
        {
            Debug.Log($"[CombatFlowCoordinator] 적 캐릭터 사망: {enemyData?.name ?? "Unknown"}");
            // 핸드카드 소멸 애니메이션 트리거는 상태 패턴에서만 실행
            // VanishEnemySkillCardsOnDeath(enemyData?.name ?? "Unknown"); // 제거
            // 기존 사망 애니메이션 실행
            // AnimationFacade.Instance.PlayCharacterDeathAnimation(enemyData?.name ?? "Unknown", enemyObject, null, true); // 제거
        }
        
        /// <summary>
        /// 적 캐릭터 사망 시 해당 캐릭터의 스킬카드들을 소멸시킵니다.
        /// </summary>
        /// <param name="enemyId">사망한 적 캐릭터 ID</param>
        private void VanishEnemySkillCardsOnDeath(string enemyId)
        {
            Debug.Log($"[CombatFlowCoordinator] 적 캐릭터 스킬카드 소멸 시작: {enemyId}");
            // AnimationFacade를 통한 일괄 소멸 애니메이션 실행
            // AnimationFacade.Instance.VanishAllHandCardsOnCharacterDeath(false); // 제거
        }
        
        /// <summary>
        /// 적 스킬카드들을 찾습니다.
        /// </summary>
        /// <returns>적 스킬카드 GameObject 리스트</returns>
        private List<GameObject> FindEnemySkillCards()
        {
            var skillCards = new List<GameObject>();
            
            // 적 스킬카드 슬롯들에서 카드들을 찾기
            var cardSlots = FindObjectsByType<Game.SkillCardSystem.UI.SkillCardUI>(FindObjectsSortMode.None);
            
            foreach (var cardSlot in cardSlots)
            {
                if (cardSlot == null || cardSlot.GetCard() == null) continue;
                
                // 적 스킬카드인지 확인 (플레이어가 아닌 카드)
                if (!cardSlot.GetCard().IsFromPlayer())
                {
                    skillCards.Add(cardSlot.gameObject);
                }
            }
            
            return skillCards;
        }

        // ICombatFlowCoordinator 인터페이스 구현: 전투 승리 후 상태 정리
        public IEnumerator CleanupAfterVictory()
        {
            // 적 핸드 시스템이 제거되어 더 이상 정리할 것이 없음
            yield return null;
        }

        /// <summary>
        /// 적이 선공인지 설정합니다. (저장 시스템용)
        /// </summary>
        /// <param name="isEnemyFirst">적이 선공인지 여부</param>
        public void SetEnemyFirst(bool isEnemyFirst)
        {
            IsEnemyFirst = isEnemyFirst;
            Debug.Log($"[CombatFlowCoordinator] 적 선공 설정: {IsEnemyFirst}");
        }
        
        /// <summary>
        /// 저장된 전투 상태를 복원합니다.
        /// 이어하기 기능을 위해 사용됩니다.
        /// </summary>
        public System.Collections.IEnumerator ResumeCombatFromSave()
        {
            Debug.Log("[CombatFlowCoordinator] 저장된 전투 상태 복원 시작");
            
            // TODO: 실제 저장된 데이터 로드 및 복원 로직 구현
            // 1. 저장된 캐릭터 상태 복원
            // 2. 저장된 슬롯 상태 복원
            // 3. 저장된 턴 상태 복원
            // 4. 저장된 카드 상태 복원
            
            // 임시로 새 전투 시작 (실제 구현 시 저장된 상태로 복원)
            Debug.Log("[CombatFlowCoordinator] 임시로 새 전투 시작 (실제 구현 필요)");
            StartCombatFlow();
            
            yield return null;
        }
    }
}
