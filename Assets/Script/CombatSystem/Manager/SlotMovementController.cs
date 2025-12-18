using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.CombatSystem.Interface;
using Game.CombatSystem.Slot;
using Game.CombatSystem.Core;
using Game.CombatSystem.Data;
using Game.CharacterSystem.Data;
using Game.CharacterSystem.Manager;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.Factory;
using Game.SkillCardSystem.UI;
using Game.SkillCardSystem.Data;
using Game.CoreSystem.Utility;
using DG.Tweening;
using TMPro;
using Zenject;

namespace Game.CombatSystem.Manager
{
    /// <summary>
    /// 슬롯 이동 전담 클래스
    /// 카드 슬롯 간 이동, 초기 셋업, 카드 보충을 담당합니다.
    /// </summary>
    public class SlotMovementController : ISlotMovementController
    {
        #region 의존성

        private readonly ICardSlotRegistry _registry;
        private readonly Game.SkillCardSystem.Interface.ISkillCardFactory _cardFactory;
        private readonly PlayerManager _playerManager;
        private readonly EnemyManager _enemyManager;
        private readonly ICombatExecutionManager _executionManager;
        private readonly ITurnController _turnController;

        #endregion

        #region 내부 상태

        private bool _isAdvancingQueue = false;
        private bool _nextSpawnIsPlayer = false; // 대기4 교대 스폰 제어
        private bool _initialSlotSetupCompleted = false;
        private readonly HashSet<ISkillCard> _scheduledEnemyExec = new();
        private bool _suppressAutoRefill = false;
        private bool _suppressAutoExecution = false;
        private bool _isSummonMode = false; // 소환/복귀 모드 여부

        // 적 캐시
        private EnemyCharacterData _cachedEnemyData;
        private string _cachedEnemyName;

        // Resources 캐싱
        private SkillCardUI _cachedCardUIPrefab;

        #endregion

        #region 프로퍼티

        public bool IsAdvancingQueue => _isAdvancingQueue;
        public bool IsInitialSlotSetupCompleted => _initialSlotSetupCompleted;

        #endregion

        #region 생성자

        [Inject]
        public SlotMovementController(
            ICardSlotRegistry registry,
            Game.SkillCardSystem.Interface.ISkillCardFactory cardFactory,
            PlayerManager playerManager,
            EnemyManager enemyManager,
            ICombatExecutionManager executionManager,
            ITurnController turnController)
        {
            _registry = registry;
            _cardFactory = cardFactory;
            _playerManager = playerManager;
            _enemyManager = enemyManager;
            _executionManager = executionManager;
            _turnController = turnController;
        }

        #endregion

        #region 슬롯 이동

        public IEnumerator MoveAllSlotsForwardRoutine()
        {
            // 초기 슬롯 설정 중에는 중복 실행 방지를 우회
            if (_isAdvancingQueue && _initialSlotSetupCompleted)
            {
                yield break;
            }

            _isAdvancingQueue = true;

            yield return MoveCardToSlotRoutine(CombatSlotPosition.WAIT_SLOT_1, CombatSlotPosition.BATTLE_SLOT);
            yield return MoveCardToSlotRoutine(CombatSlotPosition.WAIT_SLOT_2, CombatSlotPosition.WAIT_SLOT_1);
            yield return MoveCardToSlotRoutine(CombatSlotPosition.WAIT_SLOT_3, CombatSlotPosition.WAIT_SLOT_2);
            yield return MoveCardToSlotRoutine(CombatSlotPosition.WAIT_SLOT_4, CombatSlotPosition.WAIT_SLOT_3);

            // 전진 후 대기4 보충
            yield return null;
            yield return RefillWaitSlot4IfNeededRoutine();

            _isAdvancingQueue = false;

            // 전진이 끝난 시점에서 배틀 슬롯의 적 카드를 자동 실행
            TryAutoExecuteEnemyAtBattleSlot();
        }

        public IEnumerator AdvanceQueueAtTurnStartRoutine()
        {
            // 한 프레임 대기
            yield return null;

            // 배틀 슬롯이 비어있으면 슬롯 이동
            if (!_registry.HasCardInSlot(CombatSlotPosition.BATTLE_SLOT))
            {
                yield return MoveAllSlotsForwardRoutine();
            }
        }

        private IEnumerator MoveCardToSlotRoutine(CombatSlotPosition fromSlot, CombatSlotPosition toSlot)
        {
            var card = _registry.GetCardInSlot(fromSlot);
            if (card == null)
            {
                yield break;
            }

            // UI 이동 트윈 후 데이터 갱신
            var ui = _registry.GetCardUIInSlot(fromSlot);
            if (ui != null)
            {
                var targetName = GetSlotGameObjectName(toSlot);
                var targetGo = GameObject.Find(targetName);
                var target = targetGo != null ? targetGo.transform as RectTransform : null;
                var uiRect = ui.transform as RectTransform;

                if (uiRect != null && target != null)
                {
                    // 목적지 RectTransform이 파괴되었는지 확인
                    if (target == null || target.gameObject == null)
                    {
                        GameLogger.LogWarning(
                            $"[SlotMovementController] 목적지 슬롯이 파괴되었습니다: {toSlot}",
                            GameLogger.LogCategory.Combat);
                        yield break;
                    }

                    // 이동 중에는 최상위 캔버스 하위로 올려서 항상 슬롯 위에 보이도록 처리
                    var root = target.root as RectTransform;
                    if (root != null)
                    {
                        uiRect.SetParent(root, true);
                        uiRect.SetAsLastSibling();
                    }

                    // 목적지 월드 좌표 계산 후 월드 기준 이동 트윈 (Y=4 오프셋 적용)
                    Vector3 endWorld = target.TransformPoint(new Vector3(0f, 4f, 0f));
                    var moveTween = uiRect.DOMove(endWorld, CombatConstants.AnimationDurations.CARD_MOVE)
                        .SetEase(Ease.OutQuad);
                    var scaleTween = uiRect.DOScale(1f, CombatConstants.AnimationDurations.CARD_MOVE)
                        .SetEase(Ease.OutQuad);
                    yield return moveTween.WaitForCompletion();

                    // 최종 부모로 설정하기 전에 다시 한 번 확인
                    if (target != null && target.gameObject != null)
                    {
                        uiRect.SetParent(target, false);
                        uiRect.anchoredPosition = new Vector2(0f, 4f);
                    }
                    else
                    {
                        GameLogger.LogWarning(
                            $"[SlotMovementController] 이동 완료 후 목적지 슬롯이 파괴되었습니다: {toSlot}",
                            GameLogger.LogCategory.Combat);
                    }
                }
            }

            // 데이터 재등록
            _registry.MoveCardData(fromSlot, toSlot);
        }

        #endregion

        #region 카드 보충

        private IEnumerator RefillWaitSlot4IfNeededRoutine()
        {
            if (_suppressAutoRefill)
            {
                yield break;
            }
            
            if (_registry.GetCardInSlot(CombatSlotPosition.WAIT_SLOT_4) != null)
            {
                yield break;
            }

            // 프리팹 로드 (캐시 사용)
            var cardUIPrefab = GetCachedCardUIPrefab();
            if (cardUIPrefab == null)
            {
                GameLogger.LogWarning(
                    $"{FormatLogTag()} [Refill] SkillCardUI 프리팹을 찾지 못함",
                    GameLogger.LogCategory.Combat);
                yield break;
            }

            // 현재 슬롯 상태를 기반으로 다음 생성할 카드 타입 결정
            // 1순위: 배틀 슬롯 → 2순위: WAIT_SLOT_1 → 3순위: 내부 플래그
            bool shouldSpawnPlayer = DetermineNextCardType();

            // 패턴: 플레이어 마커 1개 ↔ 적 카드 1개 (1:1 교대)
            if (shouldSpawnPlayer)
            {
                var marker = CreatePlayerMarker();
                if (marker != null)
                {
                    var ui = CreateCardUIForSlot(marker, CombatSlotPosition.WAIT_SLOT_4, cardUIPrefab);
                    var tween = PlaySpawnTween(ui);
                    _registry.RegisterCard(CombatSlotPosition.WAIT_SLOT_4, marker, ui, SlotOwner.PLAYER);
                    if (tween != null)
                    {
                        yield return tween.WaitForCompletion();
                    }
                }
            }
            else
            {
                // 적 카드 생성 (캐시된 덱 우선)
                var enemy = _enemyManager?.GetCharacter();
                var runtimeData = enemy?.CharacterData as EnemyCharacterData;
                var runtimeName = enemy?.GetCharacterName() ?? "Enemy";
                var enemyData = _cachedEnemyData ?? runtimeData;
                var enemyName = string.IsNullOrEmpty(_cachedEnemyName) ? runtimeName : _cachedEnemyName;

                Game.SkillCardSystem.Deck.EnemySkillDeck.CardEntry entry = null;
                if (enemyData?.EnemyDeck != null)
                {
                    // GetRandomEntry가 간헐적으로 null을 반환할 수 있으므로 재시도
                    for (int attempt = 0; attempt < CombatConstants.InitialSetup.ENEMY_CARD_RETRY_COUNT && entry == null; attempt++)
                    {
                        entry = enemyData.EnemyDeck.GetRandomEntry();
                    }
                }

                if (entry?.definition != null)
                {
                    // 데미지 오버라이드가 있으면 사용, 없으면 기본값 사용
                    var card = entry.HasDamageOverride()
                        ? _cardFactory.CreateEnemyCard(entry.definition, enemyName, entry.damageOverride)
                        : _cardFactory.CreateEnemyCard(entry.definition, enemyName);
                    var ui = CreateCardUIForSlot(card, CombatSlotPosition.WAIT_SLOT_4, cardUIPrefab);
                    var tween = PlaySpawnTween(ui);
                    _registry.RegisterCard(CombatSlotPosition.WAIT_SLOT_4, card, ui, SlotOwner.ENEMY);
                    if (tween != null)
                    {
                        yield return tween.WaitForCompletion();
                    }
                }
                else
                {
                    GameLogger.LogWarning(
                        $"{FormatLogTag()} [Refill] 적 덱에서 카드를 얻지 못함",
                        GameLogger.LogCategory.Combat);
                }
            }

            // 다음 생성 주체 업데이트 (1:1 교대 유지용 폴백)
            _nextSpawnIsPlayer = !shouldSpawnPlayer;
        }

        /// <summary>
        /// 슬롯 상태를 기반으로 다음 생성할 카드 타입을 결정합니다.
        /// </summary>
        private bool DetermineNextCardType()
        {
            // 1. 꼬리 쪽 카드 기준 (연속 타입 방지)
            //    WAIT_SLOT_3 → WAIT_SLOT_2 → WAIT_SLOT_1 순으로 앞에 있는 카드 확인
            var tailCard =
                _registry.GetCardInSlot(CombatSlotPosition.WAIT_SLOT_3) ??
                _registry.GetCardInSlot(CombatSlotPosition.WAIT_SLOT_2) ??
                _registry.GetCardInSlot(CombatSlotPosition.WAIT_SLOT_1);

            if (tailCard != null)
            {
                if (IsPlayerTurnMarker(tailCard))
                {
                    GameLogger.LogInfo("[자동 보충] 꼬리 슬롯: 플레이어 마커 → 다음은 적 카드", GameLogger.LogCategory.Combat);
                    return false; // 적 카드 생성
                }

                if (!tailCard.IsFromPlayer())
                {
                    GameLogger.LogInfo("[자동 보충] 꼬리 슬롯: 적 카드 → 다음은 플레이어 마커", GameLogger.LogCategory.Combat);
                    return true; // 플레이어 마커 생성
                }
            }

            // 2. 꼬리 쪽에서 판단 불가하면, 배틀 슬롯 기준으로 턴 패턴 유지
            var battleCard = _registry.GetCardInSlot(CombatSlotPosition.BATTLE_SLOT);
            if (battleCard != null)
            {
                if (IsPlayerTurnMarker(battleCard))
                {
                    GameLogger.LogInfo("[자동 보충] 배틀 슬롯: 플레이어 마커 → 다음은 적 카드", GameLogger.LogCategory.Combat);
                    return false;
                }

                if (!battleCard.IsFromPlayer())
                {
                    GameLogger.LogInfo("[자동 보충] 배틀 슬롯: 적 카드 → 다음은 플레이어 마커", GameLogger.LogCategory.Combat);
                    return true;
                }
            }

            // 3. 여전히 판단이 안 되면 현재 턴 정보를 기반으로 결정
            //    - 플레이어 턴이면 다음은 적 카드
            //    - 적 턴이면 다음은 플레이어 마커
            bool byTurn =
                _turnController != null && _turnController.IsPlayerTurn()
                    ? false  // 플레이어 턴 → 적 카드
                    : true;  // 적 턴 또는 정보 없음 → 플레이어 마커

            GameLogger.LogInfo(
                $"[자동 보충] 카드 상태로 판별 불가 → 턴 기준 사용: {(byTurn ? "플레이어 마커" : "적 카드")} (플래그: {(_nextSpawnIsPlayer ? "플레이어 마커" : "적 카드")})",
                GameLogger.LogCategory.Combat);

            return byTurn;
        }

        /// <summary>
        /// 카드가 플레이어 턴 마커인지 확인합니다.
        /// </summary>
        private bool IsPlayerTurnMarker(ISkillCard card)
        {
            if (card?.CardDefinition == null)
            {
                return false;
            }

            return card.CardDefinition.cardId == CombatConstants.PLAYER_MARKER_ID && card.IsFromPlayer();
        }

        #endregion


        #region 초기 셋업

        public IEnumerator SetupInitialEnemyQueueRoutine(EnemyCharacterData enemyData, string enemyName)
        {
            if (enemyData?.EnemyDeck == null)
            {
                GameLogger.LogWarning(
                    $"적 데이터 또는 적 덱이 null입니다. 적: {enemyName}",
                    GameLogger.LogCategory.Combat);
                yield break;
            }

            // SkillCardUI 프리팁 로드 (캐시 사용)
            var cardUIPrefab = GetCachedCardUIPrefab();
            if (cardUIPrefab == null)
            {
                GameLogger.LogWarning(
                    "SkillCardUI 프리팹을 찾을 수 없습니다. UI 없이 데이터만 등록합니다.",
                    GameLogger.LogCategory.Combat);
            }
            else
            {
            }

            // 초기 셋업 시작 시 상태 초기화 (이전 코루틴 중단으로 인한 상태 불일치 방지)
            _isAdvancingQueue = false;
            _initialSlotSetupCompleted = false;

            // 초기 셋업 구간에서는 자동 보충/자동 실행 억제
            _suppressAutoRefill = true;
            _suppressAutoExecution = true;

            // 적 덱/이름 캐시 저장
            _cachedEnemyData = enemyData;
            _cachedEnemyName = enemyName;

            // 초기 셋업: 플레이어 마커 ↔ 적 카드 (1:1 교대, 총 5개)
            // 게임 기본 틀: 모든 스테이지에서 플레이어가 가장 먼저 스킬카드를 사용 가능
            // 생성 순서: 플레이어 마커 > 적 스킬카드 > 플레이어 마커 > 적 스킬카드 > 플레이어 마커
            _nextSpawnIsPlayer = true;
            bool isPlayerTurn = true;

            for (int i = 0; i < CombatConstants.InitialSetup.INITIAL_CARD_COUNT; i++)
            {
                if (isPlayerTurn)
                {
                    // 플레이어 마커 생성 (플레이어의 사용 턴을 알리는 마커)
                    var marker = CreatePlayerMarker();
                    if (marker != null)
                    {
                        GameLogger.LogInfo($"[초기 셋업] {i + 1}번째: 플레이어 마커 생성", GameLogger.LogCategory.Combat);
                        yield return PlaceCardInWaitSlot4AndMoveRoutine(marker, SlotOwner.PLAYER, cardUIPrefab);
                    }
                    else
                    {
                        GameLogger.LogWarning($"[초기 셋업] {i + 1}번째: 플레이어 마커 생성 실패", GameLogger.LogCategory.Combat);
                    }
                }
                else
                {
                    // 적 스킬카드 생성 (정해진 확률대로 생성)
                    var entry = enemyData.EnemyDeck.GetRandomEntry();
                    if (entry?.definition != null)
                    {
                        // 데미지 오버라이드가 있으면 사용, 없으면 기본값 사용
                        var card = entry.HasDamageOverride()
                            ? _cardFactory.CreateEnemyCard(entry.definition, enemyName, entry.damageOverride)
                            : _cardFactory.CreateEnemyCard(entry.definition, enemyName);
                        GameLogger.LogInfo($"[초기 셋업] {i + 1}번째: 적 스킬카드 생성 - {entry.definition.displayName}", GameLogger.LogCategory.Combat);
                        yield return PlaceCardInWaitSlot4AndMoveRoutine(card, SlotOwner.ENEMY, cardUIPrefab);
                    }
                    else
                    {
                        GameLogger.LogWarning($"[초기 셋업] {i + 1}번째: 적 스킬카드 생성 실패", GameLogger.LogCategory.Combat);
                    }
                }

                // 1:1 교대
                isPlayerTurn = !isPlayerTurn;
            }

            // 초기 셋업 완료 플래그 설정
            _initialSlotSetupCompleted = true;

            // 초기 셋업 중에는 각 카드 배치 시 이미 슬롯 이동이 완료되었으므로
            // 추가 슬롯 이동은 필요 없음 (각 카드 배치 시 이미 한 칸씩 이동됨)
            GameLogger.LogInfo("[초기 셋업] 모든 카드 배치 및 슬롯 이동 완료", GameLogger.LogCategory.Combat);

            // 최종 이동/애니메이션이 모두 끝날 때까지 대기
            while (_isAdvancingQueue)
            {
                yield return null;
            }
            yield return null;

            // 초기 셋업 종료 후 자동 보충/자동 실행 활성화
            _suppressAutoRefill = false;
            
            // 소환 모드가 아닌 경우에만 자동 실행 활성화
            if (!_isSummonMode)
            {
                _suppressAutoExecution = false;
            }
            else
            {
            }
        }


        private IEnumerator PlaceCardInWaitSlot4AndMoveRoutine(ISkillCard card, SlotOwner owner, SkillCardUI cardUIPrefab)
        {
            if (card == null)
            {
                GameLogger.LogWarning("배치할 카드가 null입니다.", GameLogger.LogCategory.Combat);
                yield break;
            }

            // 초기 셋업 중: WAIT_SLOT_4에 이미 카드가 있으면 먼저 한 칸씩 이동
            // WAIT_SLOT_4 → WAIT_SLOT_3 → WAIT_SLOT_2 → WAIT_SLOT_1 → BATTLE_SLOT
            if (!_initialSlotSetupCompleted && _registry.HasCardInSlot(CombatSlotPosition.WAIT_SLOT_4))
            {
                // WAIT_SLOT_4에 이미 카드가 있으면, 모든 슬롯을 한 칸씩 이동하여 공간 확보
                yield return MoveAllSlotsForwardRoutine();
                
                // 슬롯 이동 완료 대기
                while (_isAdvancingQueue)
                {
                    yield return null;
                }
            }

            // 일반적인 경우: WAIT_SLOT_4에 이미 카드가 있으면 중복 방지
            if (_initialSlotSetupCompleted && _registry.GetCardInSlot(CombatSlotPosition.WAIT_SLOT_4) != null)
            {
                yield break;
            }

            // 1. 대기4에 카드 배치
            var cardUI = CreateCardUIForSlot(card, CombatSlotPosition.WAIT_SLOT_4, cardUIPrefab);
            var spawnTween = PlaySpawnTween(cardUI);
            _registry.RegisterCard(CombatSlotPosition.WAIT_SLOT_4, card, cardUI, owner);
            if (spawnTween != null)
            {
                yield return spawnTween.WaitForCompletion();
            }

            // 2. 슬롯 이동 처리 (일반적인 경우만)
            // 초기 셋업 중에는 이미 위에서 처리했으므로 여기서는 처리하지 않음
            if (_initialSlotSetupCompleted)
            {
                // 일반적인 경우: 배틀 슬롯이 비어있으면 슬롯 이동
                yield return null;
                if (!_registry.HasCardInSlot(CombatSlotPosition.BATTLE_SLOT))
                {
                    yield return MoveAllSlotsForwardRoutine();
                }
            }
            else
            {
                // 초기 셋업 중: 카드 배치만 하고 슬롯 이동은 다음 카드 배치 시 처리
                yield return null;
            }
        }

        #endregion

        #region 적 카드 관리

        public void RegisterEnemyCardInSlot4(ISkillCard card)
        {
            if (card == null)
            {
                GameLogger.LogWarning("등록할 적 카드가 null입니다.", GameLogger.LogCategory.Combat);
                return;
            }

            _registry.RegisterCard(CombatSlotPosition.WAIT_SLOT_4, card, null, SlotOwner.ENEMY);
        }

        public void ClearEnemyCache()
        {
            _cachedEnemyData = null;
            _cachedEnemyName = null;
        }

        public void ResetSlotStates()
        {
            // 슬롯 이동 상태 리셋
            _isAdvancingQueue = false;
            _initialSlotSetupCompleted = false;

            // 자동 실행/보충 상태 리셋
            _suppressAutoRefill = false;
            _suppressAutoExecution = false;

            // 다음 생성 주체 리셋
            _nextSpawnIsPlayer = true;
        }

        /// <summary>
        /// 소환 모드를 해제합니다 (PlayerTurnState 진입 시 호출)
        /// </summary>
        public void ClearSummonMode()
        {
            if (_isSummonMode)
            {
                _isSummonMode = false;
                _suppressAutoExecution = false;
            }
        }

        /// <summary>
        /// 소환/복귀 모드를 설정합니다
        /// </summary>
        public void SetSummonMode(bool isSummonMode)
        {
            _isSummonMode = isSummonMode;
        }

        /// <summary>
        /// 현재 소환/복귀 모드 상태를 반환합니다
        /// </summary>
        public bool IsSummonMode => _isSummonMode;

        #endregion

        #region 자동 실행

        private void TryAutoExecuteEnemyAtBattleSlot()
        {
            if (_suppressAutoExecution || !_initialSlotSetupCompleted || _isAdvancingQueue || !_turnController.IsEnemyTurn())
            {
                return;
            }

            var card = _registry.GetCardInSlot(CombatSlotPosition.BATTLE_SLOT);
            if (card != null && !card.IsFromPlayer())
            {
                if (_scheduledEnemyExec.Contains(card))
                {
                    return;
                }

                _scheduledEnemyExec.Add(card);
                if (_executionManager != null)
                {
                    GameLogger.LogInfo(
                        $"{FormatLogTag()} 배틀 슬롯 적 카드 자동 실행 트리거: {card.GetCardName()}",
                        GameLogger.LogCategory.Combat);
                    _executionManager.ExecuteCardImmediately(card, CombatSlotPosition.BATTLE_SLOT);
                }
            }
        }

        #endregion

        #region UI 생성

        private SkillCardUI CreateCardUIForSlot(ISkillCard card, CombatSlotPosition slotPosition, SkillCardUI cardUIPrefab)
        {
            if (card == null || cardUIPrefab == null)
            {
                GameLogger.LogWarning(
                    $"카드 UI 생성 실패 - 카드 또는 프리팹이 null (슬롯: {slotPosition})",
                    GameLogger.LogCategory.Combat);
                return null;
            }

            try
            {
                // 씬에서 직접 슬롯 GameObject 찾기
                string slotName = GetSlotGameObjectName(slotPosition);
                var slotGameObject = GameObject.Find(slotName);

                if (slotGameObject == null)
                {
                    GameLogger.LogWarning(
                        $"슬롯 GameObject를 찾을 수 없습니다: {slotName} (위치: {slotPosition})",
                        GameLogger.LogCategory.Combat);
                    return null;
                }

                Transform slotTransform = slotGameObject.transform;

                // SkillCardUIFactory를 통해 UI 생성
                var cardUI = SkillCardUIFactory.CreateUI(cardUIPrefab, slotTransform, card, null);

                // 플레이어 마커 UI 간소화
                if (card?.CardDefinition?.cardId == CombatConstants.PLAYER_MARKER_ID && cardUI != null)
                {
                    SimplifyPlayerMarkerUI(cardUI);
                }

                return cardUI;
            }
            catch (System.Exception e)
            {
                GameLogger.LogError(
                    $"카드 UI 생성 중 오류 발생 ({slotPosition}): {e.Message}",
                    GameLogger.LogCategory.Error);
                return null;
            }
        }

        private void SimplifyPlayerMarkerUI(SkillCardUI cardUI)
        {
            try
            {
                var t = cardUI.transform;
                var nameGo = t.Find("CardName")?.gameObject;
                if (nameGo != null) nameGo.SetActive(false);
                var deGo = t.Find("DE")?.gameObject;
                if (deGo != null) deGo.SetActive(false);

                // 모든 TMP 텍스트 숨김
                var tmps = cardUI.GetComponentsInChildren<TMP_Text>(true);
                foreach (var tmp in tmps)
                {
                    tmp.gameObject.SetActive(false);
                }

                // 드래그 비활성화 및 레이캐스트 최소화
                cardUI.SetDraggable(false);
                if (cardUI.TryGetComponent<CanvasGroup>(out var cg))
                {
                    cg.interactable = false;
                    cg.blocksRaycasts = false;
                }
            }
            catch { }
        }

        private Tween PlaySpawnTween(SkillCardUI cardUI)
        {
            if (cardUI == null) return null;

            if (cardUI.TryGetComponent<CanvasGroup>(out var cg))
            {
                cg.alpha = 0f;
                cg.DOFade(1f, CombatConstants.AnimationDurations.CARD_SPAWN).SetEase(Ease.OutQuad);
            }

            var rt = cardUI.transform as RectTransform;
            if (rt != null)
            {
                rt.localScale = Vector3.one * 0.7f;
                return rt.DOScale(1f, CombatConstants.AnimationDurations.CARD_SPAWN).SetEase(Ease.OutBack);
            }

            return null;
        }

        #endregion

        #region 플레이어 마커 생성

        private ISkillCard CreatePlayerMarker()
        {
            try
            {
                var playerCharacter = _playerManager?.GetCharacter();
                if (playerCharacter == null)
                {
                    GameLogger.LogWarning(
                        "플레이어 캐릭터를 찾을 수 없습니다.",
                        GameLogger.LogCategory.Combat);
                    return null;
                }

                var playerData = playerCharacter.CharacterData as PlayerCharacterData;

                if (playerData?.Emblem == null)
                {
                    GameLogger.LogWarning(
                        "플레이어 데이터 또는 엠블럼을 찾을 수 없습니다.",
                        GameLogger.LogCategory.Combat);
                    return null;
                }

                // 플레이어 마커용 SkillCardDefinition 생성
                var markerDefinition = ScriptableObject.CreateInstance<Game.SkillCardSystem.Data.SkillCardDefinition>();
                markerDefinition.cardId = CombatConstants.PLAYER_MARKER_ID;
                markerDefinition.displayName = "";
                markerDefinition.displayNameKO = "";
                markerDefinition.description = "";
                markerDefinition.artwork = playerData.Emblem;

                // 마커는 효과나 데미지 없음
                markerDefinition.configuration.hasDamage = false;
                markerDefinition.configuration.hasEffects = false;
                markerDefinition.configuration.ownerPolicy = Game.SkillCardSystem.Data.OwnerPolicy.Player;

                // SkillCard 런타임 인스턴스 생성
                var markerCard = new Game.SkillCardSystem.Runtime.SkillCard(
                    markerDefinition,
                    Game.SkillCardSystem.Data.Owner.Player,
                    null);

                return markerCard;
            }
            catch (System.Exception e)
            {
                GameLogger.LogError(
                    $"플레이어 마커 생성 실패: {e.Message}",
                    GameLogger.LogCategory.Error);
                return null;
            }
        }

        #endregion

        #region 유틸리티

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

        private SkillCardUI GetCachedCardUIPrefab()
        {
            if (_cachedCardUIPrefab == null)
            {
                _cachedCardUIPrefab = Resources.Load<SkillCardUI>("Prefab/SkillCard");
                if (_cachedCardUIPrefab == null)
                {
                    GameLogger.LogError(
                        "SkillCardUI 프리팹을 찾을 수 없습니다: Prefab/SkillCard",
                        GameLogger.LogCategory.Error);
                }
            }
            return _cachedCardUIPrefab;
        }

        private string FormatLogTag()
        {
            var turnName = _turnController.IsPlayerTurn() ? "Player" : "Enemy";
            return $"[T{_turnController.TurnCount}-{turnName}-F{Time.frameCount}]";
        }

        /// <summary>
        /// CardSlotRegistry 인스턴스를 반환합니다
        /// </summary>
        /// <returns>CardSlotRegistry 인스턴스</returns>
        public ICardSlotRegistry GetCardSlotRegistry()
        {
            return _registry;
        }

        #endregion
    }
}
