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
using Game.Application.Battle;

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
        private readonly MoveSlotUseCase _moveSlotUseCase;

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
            ITurnController turnController,
            MoveSlotUseCase moveSlotUseCase)
        {
            _registry = registry;
            _cardFactory = cardFactory;
            _playerManager = playerManager;
            _enemyManager = enemyManager;
            _executionManager = executionManager;
            _turnController = turnController;
            _moveSlotUseCase = moveSlotUseCase;
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
            GameLogger.LogInfo(
                $"{FormatLogTag()} 슬롯 이동 완료: 4→3→2→1→배틀",
                GameLogger.LogCategory.Combat);

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

            GameLogger.LogInfo("슬롯 전진 완료", GameLogger.LogCategory.Combat);
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

                    // 최종 부모로 설정하고 로컬 정렬
                    uiRect.SetParent(target, false);
                    uiRect.anchoredPosition = new Vector2(0f, 4f);
                }
            }

            // 데이터 재등록
            _registry.MoveCardData(fromSlot, toSlot);

            // 도메인 슬롯 레지스트리 동기화 (가능한 경우)
            if (_moveSlotUseCase != null)
            {
                try
                {
                    var fromDomain = ToDomainSlotPosition(fromSlot);
                    var toDomain = ToDomainSlotPosition(toSlot);
                    if (fromDomain != Game.Domain.Combat.ValueObjects.SlotPosition.None &&
                        toDomain != Game.Domain.Combat.ValueObjects.SlotPosition.None)
                    {
                        _moveSlotUseCase.Execute(fromDomain, toDomain);
                    }
                }
                catch (System.Exception ex)
                {
                    GameLogger.LogError($"도메인 슬롯 이동 중 오류 발생: {ex.Message}", GameLogger.LogCategory.Error);
                }
            }

            GameLogger.LogInfo(
                $"{FormatLogTag()} 카드 이동: {card.GetCardName()} ({fromSlot} → {toSlot})",
                GameLogger.LogCategory.Combat);

            // 적 카드가 배틀 슬롯으로 이동했을 때 로그만 출력
            if (toSlot == CombatSlotPosition.BATTLE_SLOT && !card.IsFromPlayer())
            {
                GameLogger.LogInfo(
                    $"{FormatLogTag()} 적 카드 배틀 슬롯 도달: {card.GetCardName()}",
                    GameLogger.LogCategory.Combat);
            }
        }

        private static Game.Domain.Combat.ValueObjects.SlotPosition ToDomainSlotPosition(CombatSlotPosition position)
        {
            switch (position)
            {
                case CombatSlotPosition.BATTLE_SLOT:
                    return Game.Domain.Combat.ValueObjects.SlotPosition.BattleSlot;
                case CombatSlotPosition.WAIT_SLOT_1:
                    return Game.Domain.Combat.ValueObjects.SlotPosition.WaitSlot1;
                case CombatSlotPosition.WAIT_SLOT_2:
                    return Game.Domain.Combat.ValueObjects.SlotPosition.WaitSlot2;
                case CombatSlotPosition.WAIT_SLOT_3:
                    return Game.Domain.Combat.ValueObjects.SlotPosition.WaitSlot3;
                case CombatSlotPosition.WAIT_SLOT_4:
                    return Game.Domain.Combat.ValueObjects.SlotPosition.WaitSlot4;
                default:
                    return Game.Domain.Combat.ValueObjects.SlotPosition.None;
            }
        }

        #endregion

        #region 카드 보충

        private IEnumerator RefillWaitSlot4IfNeededRoutine()
        {
            if (_suppressAutoRefill)
            {
                GameLogger.LogInfo(
                    $"{FormatLogTag()} [Refill] 자동 보충 억제 중 → 스킵",
                    GameLogger.LogCategory.Combat);
                yield break;
            }

            if (_registry.GetCardInSlot(CombatSlotPosition.WAIT_SLOT_4) != null)
            {
                GameLogger.LogInfo(
                    $"{FormatLogTag()} [Refill] 대기4 이미 점유 → 스킵",
                    GameLogger.LogCategory.Combat);
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

            // 패턴: 플레이어 마커 1개 ↔ 적 카드 1개 (1:1 교대)
            if (_nextSpawnIsPlayer)
            {
                var marker = CreatePlayerMarker();
                if (marker != null)
                {
                    var ui = CreateCardUIForSlot(marker, CombatSlotPosition.WAIT_SLOT_4, cardUIPrefab);
                    var tween = PlaySpawnTween(ui);
                    _registry.RegisterCard(CombatSlotPosition.WAIT_SLOT_4, marker, ui, SlotOwner.PLAYER);
                    GameLogger.LogInfo(
                        $"{FormatLogTag()} [Refill] 대기4 보충: 플레이어 마커",
                        GameLogger.LogCategory.Combat);
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
                    GameLogger.LogInfo(
                        $"{FormatLogTag()} [Refill] 대기4 보충: 적 카드 {card.GetCardName()}",
                        GameLogger.LogCategory.Combat);
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

            // 다음 생성 주체 토글 (1:1 교대)
            _nextSpawnIsPlayer = !_nextSpawnIsPlayer;
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
                GameLogger.LogInfo("SkillCardUI 프리팹 로드 완료", GameLogger.LogCategory.Combat);
            }

            GameLogger.LogInfo("동적 슬롯 셋업 시작 - 실제 게임 플레이 방식", GameLogger.LogCategory.Combat);

            // 초기 셋업 구간에서는 자동 보충/자동 실행 억제
            _suppressAutoRefill = true;
            _suppressAutoExecution = true;

            // 적 덱/이름 캐시 저장
            _cachedEnemyData = enemyData;
            _cachedEnemyName = enemyName;

            // 초기 셋업: 플레이어 마커 ↔ 적 카드 (1:1 교대, 총 5개)
            _nextSpawnIsPlayer = true;
            bool isPlayerTurn = true;

            for (int i = 0; i < CombatConstants.InitialSetup.INITIAL_CARD_COUNT; i++)
            {
                GameLogger.LogInfo(
                    $"[초기셋업] {i + 1}/5 - {(isPlayerTurn ? "플레이어 마커" : "적 카드")}",
                    GameLogger.LogCategory.Combat);

                if (isPlayerTurn)
                {
                    var marker = CreatePlayerMarker();
                    if (marker != null)
                    {
                        yield return PlaceCardInWaitSlot4AndMoveRoutine(marker, SlotOwner.PLAYER, cardUIPrefab);
                        GameLogger.LogInfo(
                            $"[{i + 1}/5] 플레이어 마커 생성 및 배치 완료",
                            GameLogger.LogCategory.Combat);
                    }
                }
                else
                {
                    var entry = enemyData.EnemyDeck.GetRandomEntry();
                    if (entry?.definition != null)
                    {
                        // 데미지 오버라이드가 있으면 사용, 없으면 기본값 사용
                        var card = entry.HasDamageOverride()
                            ? _cardFactory.CreateEnemyCard(entry.definition, enemyName, entry.damageOverride)
                            : _cardFactory.CreateEnemyCard(entry.definition, enemyName);
                        yield return PlaceCardInWaitSlot4AndMoveRoutine(card, SlotOwner.ENEMY, cardUIPrefab);
                        GameLogger.LogInfo(
                            $"[{i + 1}/5] 적 카드 생성 및 배치 완료: {card.CardDefinition?.CardName}",
                            GameLogger.LogCategory.Combat);
                    }
                }

                // 1:1 교대
                isPlayerTurn = !isPlayerTurn;
            }

            GameLogger.LogInfo(
                "동적 슬롯 셋업 완료 - 패턴: 플레이어 → 적 → 플레이어 → 적 → 플레이어 (1:1 교대)",
                GameLogger.LogCategory.Combat);
            _initialSlotSetupCompleted = true;

            // 이동/애니메이션이 모두 끝날 때까지 대기
            while (_isAdvancingQueue)
            {
                yield return null;
            }
            yield return null;

            // 초기 셋업 완료 후 다음 생성 주체 설정
            _nextSpawnIsPlayer = false;

            // 초기 셋업 종료 후 자동 보충/자동 실행 활성화
            _suppressAutoRefill = false;
            
            // 소환 모드가 아닌 경우에만 자동 실행 활성화
            if (!_isSummonMode)
            {
                _suppressAutoExecution = false;
            }
            else
            {
                GameLogger.LogInfo("소환/복귀 모드 - 자동 실행 억제 유지", GameLogger.LogCategory.Combat);
            }
        }


        private IEnumerator PlaceCardInWaitSlot4AndMoveRoutine(ISkillCard card, SlotOwner owner, SkillCardUI cardUIPrefab)
        {
            if (card == null)
            {
                GameLogger.LogWarning("배치할 카드가 null입니다.", GameLogger.LogCategory.Combat);
                yield break;
            }

            // 1. 대기4에 카드 배치
            // 초기 셋업 중에는 중복 방지 로직을 우회하고 슬롯 이동을 통해 처리
            if (_initialSlotSetupCompleted && _registry.GetCardInSlot(CombatSlotPosition.WAIT_SLOT_4) != null)
            {
                // 일반적인 경우에만 중복 방지
                yield break;
            }

            var cardUI = CreateCardUIForSlot(card, CombatSlotPosition.WAIT_SLOT_4, cardUIPrefab);
            var spawnTween = PlaySpawnTween(cardUI);
            _registry.RegisterCard(CombatSlotPosition.WAIT_SLOT_4, card, cardUI, owner);
            GameLogger.LogInfo(
                $"대기4에 카드 배치: {card.GetCardName()}",
                GameLogger.LogCategory.Combat);
            if (spawnTween != null)
            {
                yield return spawnTween.WaitForCompletion();
            }

            // 2. 슬롯 이동 처리
            yield return null;
            if (!_registry.HasCardInSlot(CombatSlotPosition.BATTLE_SLOT))
            {
                yield return MoveAllSlotsForwardRoutine();
                GameLogger.LogInfo("배틀슬롯이 비어있어 모든 카드 앞으로 이동", GameLogger.LogCategory.Combat);
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
            GameLogger.LogInfo(
                $"적 카드 등록 완료: {card.CardDefinition?.CardName ?? "Unknown"} → WAIT_SLOT_4",
                GameLogger.LogCategory.Combat);
        }

        public void ClearEnemyCache()
        {
            _cachedEnemyData = null;
            _cachedEnemyName = null;
            GameLogger.LogInfo("적 캐시 초기화 완료", GameLogger.LogCategory.Combat);
        }

        public void ResetSlotStates()
        {
            GameLogger.LogInfo("슬롯 상태 리셋 시작", GameLogger.LogCategory.Combat);

            // 슬롯 이동 상태 리셋
            _isAdvancingQueue = false;
            _initialSlotSetupCompleted = false;

            // 자동 실행/보충 상태 리셋
            _suppressAutoRefill = false;
            _suppressAutoExecution = false;

            // 다음 생성 주체 리셋
            _nextSpawnIsPlayer = true;

            GameLogger.LogInfo("슬롯 상태 리셋 완료", GameLogger.LogCategory.Combat);
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
                GameLogger.LogInfo("소환 모드 해제 - 일반 전투로 전환", GameLogger.LogCategory.Combat);
            }
        }

        /// <summary>
        /// 소환/복귀 모드를 설정합니다
        /// </summary>
        public void SetSummonMode(bool isSummonMode)
        {
            _isSummonMode = isSummonMode;
            GameLogger.LogInfo($"소환 모드 설정: {isSummonMode}", GameLogger.LogCategory.Combat);
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
