using UnityEngine;
using System;
using System.Collections.Generic;
using Game.CombatSystem.Data;
using Game.CombatSystem.Interface;
using Game.CombatSystem.Slot;
using Game.CoreSystem.Utility;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.UI;
using Game.CharacterSystem.Manager;
using Game.CharacterSystem.Core;
using Zenject;

namespace Game.CombatSystem.Manager
{
    /// <summary>
    /// 싱글게임용 턴 관리자 (Zenject DI)
    /// 전투의 턴 순서와 상태를 관리합니다.
    /// </summary>
    public class TurnManager : MonoBehaviour, ICombatTurnManager
    {
        #region 초기화 (Zenject DI)

        private void Awake()
        {
            InitializeTurn();
        }

        #endregion

        #region 턴 관리

        /// <summary>
        /// 턴 타입 열거형
        /// </summary>
        public enum TurnType 
        { 
            /// <summary>플레이어 턴</summary>
            Player, 
            /// <summary>적 턴</summary>
            Enemy 
        }

        [System.Serializable]
        public class TurnSettings
        {
            [Header("기본 턴 설정")]
            [Tooltip("시작 턴 타입")]
            public TurnType startingTurn = TurnType.Player;

            [Tooltip("초기 턴 카운트")]
            [Range(1, 100)]
            public int initialTurnCount = 1;

        }

        [System.Serializable]
        public class TurnEvents
        {
            [Header("이벤트 설정")]
            [Tooltip("턴 시작 시 이벤트 발생")]
            public bool enableTurnStartEvents = true;

            [Tooltip("턴 종료 시 이벤트 발생")]
            public bool enableTurnEndEvents = true;

            [Tooltip("턴 변경 시 이벤트 발생")]
            public bool enableTurnChangeEvents = true;

            [Space(5)]
            [Header("애니메이션")]
            [Tooltip("턴 전환 애니메이션 시간")]
            [Range(0.1f, 3f)]
            public float transitionDuration = 1f;
        }

        [System.Serializable]
        public class DebugSettings
        {
            [Header("디버그 옵션")]
            [Tooltip("턴 정보 로깅")]
            public bool enableTurnLogging = true;

            [Tooltip("턴 상태 시각화")]
            public bool showTurnStatus = false;

            [Tooltip("턴 타이머 표시")]
            public bool showTurnTimer = false;
        }

        [Header("🔄 턴 설정")]
        [SerializeField] private TurnSettings turnSettings = new TurnSettings();
        
        [Space(10)]
        [Header("🎭 턴 이벤트")]
        [SerializeField] private TurnEvents turnEvents = new TurnEvents();
        
        [Space(10)]
        [Header("🔧 디버그 설정")]
        [SerializeField] private DebugSettings debugSettings = new DebugSettings();

        [Space(10)]
        [Header("📊 현재 상태")]
        [SerializeField] private TurnType currentTurn = TurnType.Player;
        [SerializeField] private int turnCount = 1;
        [SerializeField] private bool isGameActive = false;
        [SerializeField] private float remainingTurnTime = 0f;
        
        // ITurnManager 인터페이스 구현 - 프로퍼티
        public TurnType CurrentTurn => currentTurn;
        public int TurnCount => turnCount;
        public bool IsGameActive => isGameActive;
        public float RemainingTurnTime => remainingTurnTime;
        
        // ITurnManager 인터페이스 구현 - 이벤트
        public event Action<TurnType> OnTurnChanged;
        public event Action<int> OnTurnCountChanged;
        public event Action OnGameStarted;
        public event Action OnGameEnded;

        /// <summary>
        /// 턴을 초기화합니다.
        /// </summary>
        private void InitializeTurn()
        {
            currentTurn = turnSettings.startingTurn;
            turnCount = turnSettings.initialTurnCount;
            
            if (debugSettings.enableTurnLogging)
            {
                GameLogger.LogInfo($"턴 관리자 초기화 완료 ({currentTurn} 턴 시작)", GameLogger.LogCategory.Combat);
            }
        }

        /// <summary>
        /// 현재 턴 타입을 반환합니다.
        /// </summary>
        /// <returns>현재 턴 타입</returns>
        public TurnType GetCurrentTurnType() => currentTurn;

        /// <summary>
        /// 현재 턴 번호를 반환합니다.
        /// </summary>
        /// <returns>턴 번호</returns>
        public int GetTurnCount() => turnCount;

        /// <summary>
        /// 플레이어 턴인지 확인합니다.
        /// </summary>
        /// <returns>플레이어 턴이면 true</returns>
        public bool IsPlayerTurn() => currentTurn == TurnType.Player;

        /// <summary>
        /// 적 턴인지 확인합니다.
        /// </summary>
        /// <returns>적 턴이면 true</returns>
        public bool IsEnemyTurn() => currentTurn == TurnType.Enemy;

        /// <summary>
        /// 턴을 전환합니다.
        /// </summary>
        public void SwitchTurn()
        {

            // 턴 전환 전 모든 캐릭터의 턴 효과 처리
            ProcessAllCharacterTurnEffects();

            currentTurn = currentTurn == TurnType.Player ? TurnType.Enemy : TurnType.Player;
            turnCount++;
            
            if (turnEvents.enableTurnChangeEvents)
            {
                OnTurnChanged?.Invoke(currentTurn);
                OnTurnCountChanged?.Invoke(turnCount);
            }
            
            if (debugSettings.enableTurnLogging)
            {
                var turnName = currentTurn == TurnType.Player ? "플레이어" : "적";
                GameLogger.LogInfo($"턴 전환: {turnName} 턴 (턴 {turnCount})", GameLogger.LogCategory.Combat);
            }
        }

        /// <summary>
        /// 다음 턴으로 진행합니다. (SwitchTurn의 별칭)
        /// </summary>
        public void NextTurn()
        {
            SwitchTurn();
        }

        /// <summary>
        /// 턴을 리셋합니다.
        /// </summary>
        public void ResetTurn()
        {
            currentTurn = turnSettings.startingTurn;
            turnCount = turnSettings.initialTurnCount;
            
            if (debugSettings.enableTurnLogging)
            {
                var turnName = currentTurn == TurnType.Player ? "플레이어" : "적";
                GameLogger.LogInfo($"턴 리셋 완료 ({turnName} 턴 시작)", GameLogger.LogCategory.Combat);
            }
        }

        /// <summary>
        /// 특정 턴으로 설정합니다.
        /// </summary>
        /// <param name="turnType">설정할 턴 타입</param>
        public void SetTurn(TurnType turnType)
        {
            currentTurn = turnType;
            OnTurnChanged?.Invoke(turnType);
            
            var turnName = turnType == TurnType.Player ? "플레이어" : "적";
            GameLogger.LogInfo($"턴 설정: {turnName} 턴 (턴 {turnCount})", GameLogger.LogCategory.Combat);
        }
        
        /// <summary>
        /// 게임을 시작합니다.
        /// </summary>
        public void StartGame()
        {
            isGameActive = true;
            OnGameStarted?.Invoke();
            
            if (debugSettings.enableTurnLogging)
            {
                GameLogger.LogInfo("게임 시작", GameLogger.LogCategory.Combat);
            }
        }
        
        /// <summary>
        /// 게임을 종료합니다.
        /// </summary>
        public void EndGame()
        {
            isGameActive = false;
            remainingTurnTime = 0f;
            OnGameEnded?.Invoke();
            
            if (debugSettings.enableTurnLogging)
            {
                GameLogger.LogInfo("게임 종료", GameLogger.LogCategory.Combat);
            }
        }
        
        /// <summary>
        /// 턴을 일시정지합니다.
        /// </summary>
        public void PauseTurn()
        {
            // 턴 일시정지 로직
            if (debugSettings.enableTurnLogging)
            {
                GameLogger.LogInfo("턴 일시정지", GameLogger.LogCategory.Combat);
            }
        }
        
        /// <summary>
        /// 턴을 재개합니다.
        /// </summary>
        public void ResumeTurn()
        {
            // 턴 재개 로직
            if (debugSettings.enableTurnLogging)
            {
                GameLogger.LogInfo("턴 재개", GameLogger.LogCategory.Combat);
            }
        }
        
        /// <summary>
        /// 턴 시간을 리셋합니다.
        /// </summary>
        public void ResetTurnTimer()
        {
            remainingTurnTime = 0f;
            
            if (debugSettings.enableTurnLogging)
            {
                GameLogger.LogInfo("턴 시간 리셋", GameLogger.LogCategory.Combat);
            }
        }

        #endregion

        #region 디버그

        /// <summary>
        /// 현재 턴 정보를 로그로 출력합니다.
        /// </summary>
        [ContextMenu("턴 정보 출력")]
        public void LogTurnInfo()
        {
            var turnName = currentTurn == TurnType.Player ? "플레이어" : "적";
            GameLogger.LogInfo($"현재 턴: {turnName} (턴 {turnCount})", GameLogger.LogCategory.Combat);
        }

        #endregion
        
        #region 카드 레지스트리 관리 (TurnCardRegistry 통합)
        
        private readonly Dictionary<CombatSlotPosition, ISkillCard> _cards = new();
        private CombatSlotPosition? _reservedEnemySlot;
        
        /// <summary>
        /// 카드 상태가 변경될 때 발생하는 이벤트
        /// </summary>
        public event Action OnCardStateChanged;
        
        /// <summary>
        /// 카드를 슬롯에 등록합니다.
        /// </summary>
        /// <param name="position">등록할 슬롯 위치</param>
        /// <param name="card">등록할 카드</param>
        /// <param name="ui">카드 UI</param>
        /// <param name="owner">카드 소유자</param>
        public void RegisterCard(CombatSlotPosition position, ISkillCard card, SkillCardUI ui, SlotOwner owner)
        {
            if (card == null)
            {
                GameLogger.LogError($"카드 등록 실패 - null (슬롯: {position})", GameLogger.LogCategory.Combat);
                return;
            }

            _cards[position] = card;

            if (owner == SlotOwner.ENEMY)
                _reservedEnemySlot = position;

            OnCardStateChanged?.Invoke();
        }
        
        /// <summary>
        /// 슬롯의 카드를 반환합니다.
        /// </summary>
        /// <param name="slot">슬롯 위치</param>
        /// <returns>해당 슬롯의 카드</returns>
        public ISkillCard GetCardInSlot(CombatSlotPosition slot)
        {
            _cards.TryGetValue(slot, out var card);
            return card;
        }
        
        /// <summary>
        /// 슬롯을 클리어합니다. (UI도 함께 제거)
        /// </summary>
        /// <param name="slot">클리어할 슬롯</param>
        public void ClearSlot(CombatSlotPosition slot)
        {
            if (_cards.ContainsKey(slot))
            {
                // UI 제거 (슬롯에 있는 모든 SkillCardUI 제거)
                string slotName = GetSlotGameObjectName(slot);
                var slotGameObject = GameObject.Find(slotName);
                if (slotGameObject != null)
                {
                    var existingUIs = slotGameObject.GetComponentsInChildren<Game.SkillCardSystem.UI.SkillCardUI>();
                    foreach (var ui in existingUIs)
                    {
                        if (ui != null)
                        {
                            DestroyImmediate(ui.gameObject);
                            GameLogger.LogInfo($"슬롯 UI 제거: {slotName}", GameLogger.LogCategory.Combat);
                        }
                    }
                }

                // 데이터 제거
                _cards.Remove(slot);
                OnCardStateChanged?.Invoke();
            }
        }
        
        /// <summary>
        /// 모든 카드를 클리어합니다.
        /// </summary>
        public void ClearAllCards()
        {
            _cards.Clear();
            _reservedEnemySlot = null;
            OnCardStateChanged?.Invoke();
        }
        
        /// <summary>
        /// 적 카드만 제거하고 플레이어 카드 보존
        /// </summary>
        public void ClearEnemyCardsOnly()
        {
            var toRemove = new List<CombatSlotPosition>();

            foreach (var kvp in _cards)
            {
                if (!kvp.Value.IsFromPlayer())
                    toRemove.Add(kvp.Key);
            }

            foreach (var key in toRemove)
                _cards.Remove(key);

            _reservedEnemySlot = null;
            OnCardStateChanged?.Invoke();
        }
        
        /// <summary>
        /// 플레이어 카드가 있는지 확인합니다.
        /// </summary>
        /// <returns>플레이어 카드 존재 여부</returns>
        public bool HasPlayerCard()
        {
            foreach (var card in _cards.Values)
                if (card.IsFromPlayer()) return true;

            return false;
        }
        
        /// <summary>
        /// 적 카드가 있는지 확인합니다.
        /// </summary>
        /// <returns>적 카드 존재 여부</returns>
        public bool HasEnemyCard()
        {
            foreach (var card in _cards.Values)
                if (!card.IsFromPlayer()) return true;

            return false;
        }
        
        /// <summary>
        /// 예약된 적 슬롯을 반환합니다.
        /// </summary>
        /// <returns>예약된 적 슬롯 위치</returns>
        public CombatSlotPosition? GetReservedEnemySlot() => _reservedEnemySlot;
        
        /// <summary>
        /// 다음 적 슬롯을 예약합니다.
        /// </summary>
        /// <param name="slot">예약할 슬롯</param>
        public void ReserveNextEnemySlot(CombatSlotPosition slot)
        {
            _reservedEnemySlot = slot;
        }
        
        #endregion
        
        #region ICombatTurnManager 구현
        
        /// <summary>
        /// 전투 턴 시스템을 초기화합니다.
        /// </summary>
        public void Initialize()
        {
            InitializeTurn();
        }
        
        /// <summary>
        /// 전투 턴 시스템을 재설정합니다.
        /// </summary>
        public void Reset()
        {
            ResetTurn();
        }
        
        /// <summary>
        /// 다음 턴 상태 전이를 예약합니다.
        /// </summary>
        /// <param name="nextState">전이할 다음 상태</param>
        public void RequestStateChange(object nextState)
        {
            // TODO: 상태 전이 로직 구현
            GameLogger.LogInfo($"상태 전이 요청: {nextState}", GameLogger.LogCategory.Combat);
        }
        
        /// <summary>
        /// 즉시 새로운 상태로 전이합니다.
        /// </summary>
        /// <param name="newState">전이할 상태</param>
        public void ChangeState(object newState)
        {
            // TODO: 상태 변경 로직 구현
            GameLogger.LogInfo($"상태 변경: {newState}", GameLogger.LogCategory.Combat);
        }
        
        /// <summary>
        /// 현재 턴 상태를 반환합니다.
        /// </summary>
        /// <returns>현재 턴 상태</returns>
        public object GetCurrentState()
        {
            return currentTurn;
        }
        
        /// <summary>
        /// 상태 생성 팩토리를 반환합니다.
        /// </summary>
        /// <returns>전투 상태 팩토리</returns>
        public object GetStateFactory()
        {
            // TODO: 상태 팩토리 구현
            return null;
        }
        
        /// <summary>
        /// 현재 턴이 플레이어 입력 턴인지 확인합니다.
        /// </summary>
        /// <returns>플레이어 입력 턴 여부</returns>
        public bool IsPlayerInputTurn()
        {
            return IsPlayerTurn();
        }
        
        /// <summary>
        /// 현재 턴을 설정합니다.
        /// </summary>
        /// <param name="turn">설정할 턴</param>
        public void SetCurrentTurn(int turn)
        {
            turnCount = turn;
            OnTurnCountChanged?.Invoke(turnCount);
        }
        
        /// <summary>
        /// 현재 턴을 반환합니다. (ICombatTurnManager 구현)
        /// </summary>
        /// <returns>현재 턴 번호</returns>
        public int GetCurrentTurn()
        {
            return turnCount;
        }
        
        /// <summary>
        /// 가드 효과를 적용합니다.
        /// </summary>
        public void ApplyGuardEffect()
        {
            // 가드 효과는 GuardEffectCommand에서 직접 처리하므로 여기서는 로깅만
            GameLogger.LogInfo("가드 효과 적용 요청됨", GameLogger.LogCategory.Combat);
        }

        /// <summary>
        /// 모든 캐릭터의 턴 효과를 처리합니다.
        /// </summary>
        private void ProcessAllCharacterTurnEffects()
        {
            // 플레이어 캐릭터 처리
            var playerManager = FindFirstObjectByType<PlayerManager>();
            if (playerManager?.GetCharacter() != null)
            {
                playerManager.GetCharacter().ProcessTurnEffects();
                GameLogger.LogInfo($"플레이어 캐릭터 턴 효과 처리: {playerManager.GetCharacter().GetCharacterName()}", GameLogger.LogCategory.Combat);
            }

            // 적 캐릭터들 처리
            var enemyManager = FindFirstObjectByType<EnemyManager>();
            if (enemyManager?.GetCharacter() != null)
            {
                enemyManager.GetCharacter().ProcessTurnEffects();
                GameLogger.LogInfo($"적 캐릭터 턴 효과 처리: {enemyManager.GetCharacter().GetCharacterName()}", GameLogger.LogCategory.Combat);
            }

            // 추가로 씬의 모든 캐릭터 컴포넌트 처리 (안전장치)
            var allCharacters = FindObjectsByType<CharacterBase>(FindObjectsSortMode.None);
            foreach (var character in allCharacters)
            {
                if (character != null && character.gameObject.activeInHierarchy)
                {
                    character.ProcessTurnEffects();
                }
            }
        }
        
        /// <summary>
        /// 다음 턴을 진행합니다.
        /// </summary>
        public void ProceedToNextTurn()
        {
            NextTurn();
        }
        
        /// <summary>
        /// 4번 슬롯에 새로운 적 카드를 등록합니다.
        /// </summary>
        /// <param name="card">등록할 적 스킬카드</param>
        public void RegisterEnemyCardInSlot4(ISkillCard card)
        {
            if (card == null)
            {
                GameLogger.LogWarning("등록할 적 카드가 null입니다.", GameLogger.LogCategory.Combat);
                return;
            }

            RegisterCard(CombatSlotPosition.WAIT_SLOT_4, card, null, SlotOwner.ENEMY);
            GameLogger.LogInfo($"적 카드 등록 완료: {card.CardDefinition?.CardName ?? "Unknown"} → WAIT_SLOT_4", GameLogger.LogCategory.Combat);
        }

        /// <summary>
        /// 초기 적 카드 큐를 설정합니다. (GameStartupController에서 리플렉션으로 호출)
        /// 실제 게임 플레이처럼 대기4에서 카드 생성하고 이동하면서 순차적으로 채웁니다.
        /// </summary>
        /// <param name="enemyData">적 캐릭터 데이터</param>
        /// <param name="enemyName">적 이름</param>
        public void SetupInitialEnemyQueue(Game.CharacterSystem.Data.EnemyCharacterData enemyData, string enemyName)
        {
            if (enemyData?.EnemyDeck == null)
            {
                GameLogger.LogWarning($"적 데이터 또는 적 덱이 null입니다. 적: {enemyName}", GameLogger.LogCategory.Combat);
                return;
            }

            var factory = new Game.SkillCardSystem.Factory.SkillCardFactory();
            
            // SkillCardUI 프리팹을 Resources에서 로드
            var cardUIPrefab = Resources.Load<Game.SkillCardSystem.UI.SkillCardUI>("Prefab/SkillCard");
            
            if (cardUIPrefab == null)
            {
                GameLogger.LogWarning("SkillCardUI 프리팹을 찾을 수 없습니다. UI 없이 데이터만 등록합니다.", GameLogger.LogCategory.Combat);
            }
            else
            {
                GameLogger.LogInfo("SkillCardUI 프리팹 로드 완료", GameLogger.LogCategory.Combat);
            }
            
            GameLogger.LogInfo("동적 슬롯 셋업 시작 - 실제 게임 플레이 방식", GameLogger.LogCategory.Combat);
            
            bool isPlayerTurn = true; // 플레이어부터 시작
            
            // 5번의 카드 생성 및 이동으로 모든 슬롯 채우기
            for (int i = 0; i < 5; i++)
            {
                if (isPlayerTurn)
                {
                    // 플레이어 마커 생성 및 배치
                    var playerMarker = CreatePlayerMarker();
                    if (playerMarker != null)
                    {
                        PlaceCardInWaitSlot4AndMove(playerMarker, SlotOwner.PLAYER, cardUIPrefab);
                        GameLogger.LogInfo($"[{i+1}/5] 플레이어 마커 생성 및 배치 완료", GameLogger.LogCategory.Combat);
                    }
                }
                else
                {
                    // 적 카드 생성 및 배치
                    var enemyCardEntry = enemyData.EnemyDeck.GetRandomEntry();
                    if (enemyCardEntry?.definition != null)
                    {
                        var enemyCard = factory.CreateEnemyCard(enemyCardEntry.definition, enemyName);
                        PlaceCardInWaitSlot4AndMove(enemyCard, SlotOwner.ENEMY, cardUIPrefab);
                        GameLogger.LogInfo($"[{i+1}/5] 적 카드 생성 및 배치 완료: {enemyCard.CardDefinition?.CardName}", GameLogger.LogCategory.Combat);
                    }
                }
                
                // 플레이어와 적 교대
                isPlayerTurn = !isPlayerTurn;
            }

            GameLogger.LogInfo("동적 슬롯 셋업 완료", GameLogger.LogCategory.Combat);
        }

        /// <summary>
        /// 강제로 한 사이클을 진행합니다. (GameStartupController에서 리플렉션으로 호출)
        /// </summary>
        public void ForceOneCycle()
        {
            GameLogger.LogInfo("강제 사이클 진행 시작", GameLogger.LogCategory.Combat);
            
            // 현재는 초기 설정만 하므로 특별한 사이클 로직 없음
            // 향후 필요 시 슬롯 이동 로직 등을 추가할 수 있음
            
            GameLogger.LogInfo("강제 사이클 진행 완료", GameLogger.LogCategory.Combat);
        }

        /// <summary>
        /// 카드를 대기4에 배치하고 배틀슬롯이 비어있으면 앞으로 이동시킵니다.
        /// </summary>
        /// <param name="card">배치할 카드</param>
        /// <param name="owner">카드 소유자</param>
        /// <param name="cardUIPrefab">카드 UI 프리팹</param>
        private void PlaceCardInWaitSlot4AndMove(ISkillCard card, SlotOwner owner, Game.SkillCardSystem.UI.SkillCardUI cardUIPrefab)
        {
            if (card == null)
            {
                GameLogger.LogWarning("배치할 카드가 null입니다.", GameLogger.LogCategory.Combat);
                return;
            }

            // 1. 대기4에 카드 배치
            var cardUI = CreateCardUIForSlot(card, CombatSlotPosition.WAIT_SLOT_4, null, cardUIPrefab);
            RegisterCard(CombatSlotPosition.WAIT_SLOT_4, card, cardUI, owner);
            GameLogger.LogInfo($"대기4에 카드 배치: {card.GetCardName()}", GameLogger.LogCategory.Combat);

            // 2. 배틀슬롯이 비어있으면 모든 카드를 앞으로 이동
            if (!HasCardInSlot(CombatSlotPosition.BATTLE_SLOT))
            {
                MoveAllSlotsForward();
                GameLogger.LogInfo("배틀슬롯이 비어있어 모든 카드 앞으로 이동", GameLogger.LogCategory.Combat);
            }
        }

        /// <summary>
        /// 모든 슬롯의 카드를 앞으로 한 칸씩 이동시킵니다.
        /// 대기4 → 대기3 → 대기2 → 대기1 → 배틀슬롯
        /// </summary>
        private void MoveAllSlotsForward()
        {
            // 앞에서부터 이동 (배틀슬롯이 비어있다고 가정)
            MoveCardToSlot(CombatSlotPosition.WAIT_SLOT_1, CombatSlotPosition.BATTLE_SLOT);
            MoveCardToSlot(CombatSlotPosition.WAIT_SLOT_2, CombatSlotPosition.WAIT_SLOT_1);
            MoveCardToSlot(CombatSlotPosition.WAIT_SLOT_3, CombatSlotPosition.WAIT_SLOT_2);
            MoveCardToSlot(CombatSlotPosition.WAIT_SLOT_4, CombatSlotPosition.WAIT_SLOT_3);
            
            GameLogger.LogInfo("슬롯 이동 완료: 4→3→2→1→배틀", GameLogger.LogCategory.Combat);
        }

        /// <summary>
        /// 특정 슬롯의 카드를 다른 슬롯으로 이동시킵니다.
        /// </summary>
        /// <param name="fromSlot">원본 슬롯</param>
        /// <param name="toSlot">대상 슬롯</param>
        private void MoveCardToSlot(CombatSlotPosition fromSlot, CombatSlotPosition toSlot)
        {
            var card = GetCardInSlot(fromSlot);
            if (card == null) return;

            // 원본 슬롯에서 제거
            ClearSlot(fromSlot);
            
            // 대상 슬롯에 배치 (UI도 함께 이동)
            var cardUIPrefab = Resources.Load<Game.SkillCardSystem.UI.SkillCardUI>("Prefab/SkillCard");
            var newCardUI = CreateCardUIForSlot(card, toSlot, null, cardUIPrefab);
            var owner = card.IsFromPlayer() ? SlotOwner.PLAYER : SlotOwner.ENEMY;
            RegisterCard(toSlot, card, newCardUI, owner);
            
            GameLogger.LogInfo($"카드 이동: {card.GetCardName()} ({fromSlot} → {toSlot})", GameLogger.LogCategory.Combat);
        }

        /// <summary>
        /// 특정 슬롯에 카드가 있는지 확인합니다.
        /// </summary>
        /// <param name="slot">확인할 슬롯</param>
        /// <returns>카드 존재 여부</returns>
        private bool HasCardInSlot(CombatSlotPosition slot)
        {
            return GetCardInSlot(slot) != null;
        }

        /// <summary>
        /// 플레이어 마커 카드를 생성합니다.
        /// </summary>
        /// <returns>플레이어 마커 카드</returns>
        private ISkillCard CreatePlayerMarker()
        {
            try
            {
                // 플레이어 매니저에서 플레이어 정보 가져오기
                var playerManager = FindFirstObjectByType<Game.CharacterSystem.Manager.PlayerManager>();
                if (playerManager?.GetCharacter() == null)
                {
                    GameLogger.LogWarning("플레이어 매니저 또는 플레이어 캐릭터를 찾을 수 없습니다.", GameLogger.LogCategory.Combat);
                    return null;
                }

                var playerCharacter = playerManager.GetCharacter();
                var playerData = playerCharacter.CharacterData as Game.CharacterSystem.Data.PlayerCharacterData;
                
                if (playerData?.Emblem == null)
                {
                    GameLogger.LogWarning("플레이어 데이터 또는 엠블럼을 찾을 수 없습니다.", GameLogger.LogCategory.Combat);
                    return null;
                }

                // 플레이어 마커용 SkillCardDefinition 생성
                var markerDefinition = ScriptableObject.CreateInstance<Game.SkillCardSystem.Data.SkillCardDefinition>();
                markerDefinition.cardId = "PLAYER_MARKER";
                markerDefinition.displayName = ""; // 빈 이름
                markerDefinition.displayNameKO = "";
                markerDefinition.description = ""; // 빈 설명
                markerDefinition.artwork = playerData.Emblem; // 플레이어 엠블럼 사용

                // 마커는 효과나 데미지 없음
                markerDefinition.configuration.hasDamage = false;
                markerDefinition.configuration.hasEffects = false;
                markerDefinition.configuration.ownerPolicy = Game.SkillCardSystem.Data.OwnerPolicy.Player;

                // SkillCard 런타임 인스턴스 생성
                var markerCard = new Game.SkillCardSystem.Runtime.SkillCard(markerDefinition, Game.SkillCardSystem.Data.Owner.Player, null);

                GameLogger.LogInfo("플레이어 마커 카드 생성 완료", GameLogger.LogCategory.Combat);
                return markerCard;
            }
            catch (System.Exception e)
            {
                GameLogger.LogError($"플레이어 마커 생성 실패: {e.Message}", GameLogger.LogCategory.Error);
                return null;
            }
        }

        /// <summary>
        /// 특정 슬롯에 카드 UI를 생성합니다.
        /// </summary>
        /// <param name="card">카드 데이터</param>
        /// <param name="slotPosition">슬롯 위치</param>
        /// <param name="combatSlotRegistry">전투 슬롯 레지스트리</param>
        /// <param name="cardUIPrefab">카드 UI 프리팹</param>
        /// <returns>생성된 카드 UI</returns>
        private Game.SkillCardSystem.UI.SkillCardUI CreateCardUIForSlot(
            ISkillCard card, 
            CombatSlotPosition slotPosition, 
            Game.CombatSystem.Slot.CombatSlotRegistry combatSlotRegistry, 
            Game.SkillCardSystem.UI.SkillCardUI cardUIPrefab)
        {
            if (card == null || cardUIPrefab == null)
            {
                GameLogger.LogWarning($"카드 UI 생성 실패 - 카드 또는 프리팹이 null (슬롯: {slotPosition})", GameLogger.LogCategory.Combat);
                return null;
            }

            try
            {
                // 씬에서 직접 슬롯 GameObject 찾기 (슬롯 이름 기반)
                string slotName = GetSlotGameObjectName(slotPosition);
                var slotGameObject = GameObject.Find(slotName);
                
                if (slotGameObject == null)
                {
                    GameLogger.LogWarning($"슬롯 GameObject를 찾을 수 없습니다: {slotName} (위치: {slotPosition})", GameLogger.LogCategory.Combat);
                    return null;
                }

                Transform slotTransform = slotGameObject.transform;

                // SkillCardUIFactory를 통해 UI 생성
                var cardUI = Game.SkillCardSystem.UI.SkillCardUIFactory.CreateUI(cardUIPrefab, slotTransform, card, null);
                
                if (cardUI != null)
                {
                    GameLogger.LogInfo($"카드 UI 생성 완료: {card.GetCardName()} → {slotPosition} ({slotName})", GameLogger.LogCategory.Combat);
                }
                else
                {
                    GameLogger.LogWarning($"카드 UI 생성 실패: {slotPosition}", GameLogger.LogCategory.Combat);
                }

                return cardUI;
            }
            catch (System.Exception e)
            {
                GameLogger.LogError($"카드 UI 생성 중 오류 발생 ({slotPosition}): {e.Message}", GameLogger.LogCategory.Error);
                return null;
            }
        }

        /// <summary>
        /// CombatSlotPosition을 GameObject 이름으로 변환합니다.
        /// </summary>
        /// <param name="position">슬롯 위치</param>
        /// <returns>GameObject 이름</returns>
        private string GetSlotGameObjectName(CombatSlotPosition position)
        {
            return position switch
            {
                CombatSlotPosition.BATTLE_SLOT => "BattleSlot",
                CombatSlotPosition.WAIT_SLOT_1 => "WaitSlot1", 
                CombatSlotPosition.WAIT_SLOT_2 => "WaitSlot2",
                CombatSlotPosition.WAIT_SLOT_3 => "WaitSlot3",
                CombatSlotPosition.WAIT_SLOT_4 => "WaitSlot4",
                _ => "UnknownSlot"
            };
        }
        
        #endregion
    }
}
