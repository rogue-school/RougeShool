using UnityEngine;
using System;
using System.Collections.Generic;
using Game.CombatSystem.Data;
using Game.CombatSystem.Interface;
using Game.CombatSystem.Slot;
using Game.CombatSystem.State;
using Game.CoreSystem.Utility;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.UI;
using Game.CharacterSystem.Manager;
using Game.CharacterSystem.Core;
using Zenject;
using DG.Tweening;
using TMPro;

namespace Game.CombatSystem.Manager
{
    /// <summary>
    /// 싱글게임용 턴 관리자 (Zenject DI)
    /// 전투의 턴 순서와 상태를 관리합니다.
    /// CombatStateMachine과 통합되어 동작합니다.
    /// </summary>
    public class TurnManager : MonoBehaviour, ICombatTurnManager
    {
        // 초기 전투/대기 슬롯 셋업 완료 신호 (내부용)
        private bool _initialSlotSetupCompleted = false;
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

        // FindObjectOfType 캐싱
        private Game.SkillCardSystem.Manager.PlayerHandManager cachedPlayerHandManager;
        private PlayerManager cachedPlayerManager;
        private EnemyManager cachedEnemyManager;
        private CombatExecutionManager cachedCombatExecutionManager;

        // Resources.Load 캐싱
        private Game.SkillCardSystem.UI.SkillCardUI cachedCardUIPrefab;

        #region 캐싱 헬퍼 메서드

        /// <summary>
        /// PlayerHandManager 캐시 가져오기 (지연 초기화)
        /// </summary>
        private Game.SkillCardSystem.Manager.PlayerHandManager GetCachedPlayerHandManager()
        {
            if (cachedPlayerHandManager == null)
            {
                cachedPlayerHandManager = FindFirstObjectByType<Game.SkillCardSystem.Manager.PlayerHandManager>();
            }
            return cachedPlayerHandManager;
        }

        /// <summary>
        /// PlayerManager 캐시 가져오기 (지연 초기화)
        /// </summary>
        private PlayerManager GetCachedPlayerManager()
        {
            if (cachedPlayerManager == null)
            {
                cachedPlayerManager = FindFirstObjectByType<PlayerManager>();
            }
            return cachedPlayerManager;
        }

        /// <summary>
        /// EnemyManager 캐시 가져오기 (지연 초기화)
        /// </summary>
        private EnemyManager GetCachedEnemyManager()
        {
            if (cachedEnemyManager == null)
            {
                cachedEnemyManager = FindFirstObjectByType<EnemyManager>();
            }
            return cachedEnemyManager;
        }

        /// <summary>
        /// CombatExecutionManager 캐시 가져오기 (지연 초기화)
        /// </summary>
        private CombatExecutionManager GetCachedCombatExecutionManager()
        {
            if (cachedCombatExecutionManager == null)
            {
                cachedCombatExecutionManager = FindFirstObjectByType<CombatExecutionManager>();
            }
            return cachedCombatExecutionManager;
        }

        /// <summary>
        /// SkillCardUI 프리팹 캐시 가져오기 (지연 초기화)
        /// </summary>
        private Game.SkillCardSystem.UI.SkillCardUI GetCachedCardUIPrefab()
        {
            if (cachedCardUIPrefab == null)
            {
                cachedCardUIPrefab = Resources.Load<Game.SkillCardSystem.UI.SkillCardUI>("Prefab/SkillCard");
                if (cachedCardUIPrefab == null)
                {
                    GameLogger.LogError("SkillCardUI 프리팹을 찾을 수 없습니다: Prefab/SkillCard", GameLogger.LogCategory.Error);
                }
            }
            return cachedCardUIPrefab;
        }

        #endregion

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
        /// 특정 턴으로 설정하고 턴 수를 증가시킵니다.
        /// 상태 패턴에서 턴 전환 시 사용합니다.
        /// </summary>
        /// <param name="turnType">설정할 턴 타입</param>
        public void SetTurnAndIncrement(TurnType turnType)
        {
            // 턴 타입이 실제로 변경될 때만 턴 수 증가
            if (currentTurn != turnType)
            {
                turnCount++;
                
                // 턴 변경 이벤트 통지
                if (turnEvents.enableTurnChangeEvents)
                {
                    OnTurnCountChanged?.Invoke(turnCount);
                }
            }
            
            currentTurn = turnType;
            OnTurnChanged?.Invoke(turnType);
            
            var turnName = turnType == TurnType.Player ? "플레이어" : "적";
            GameLogger.LogInfo($"턴 설정 및 증가: {turnName} 턴 (턴 {turnCount})", GameLogger.LogCategory.Combat);
        }
        
        /// <summary>
        /// 턴 상태를 복원합니다. (저장 시스템용)
        /// </summary>
        /// <param name="turnCount">복원할 턴 수</param>
        /// <param name="turnType">복원할 턴 타입</param>
        public void RestoreTurnState(int turnCount, TurnType turnType)
        {
            this.turnCount = turnCount;
            this.currentTurn = turnType;
            
            OnTurnChanged?.Invoke(turnType);
            OnTurnCountChanged?.Invoke(turnCount);
            
            var turnName = turnType == TurnType.Player ? "플레이어" : "적";
            GameLogger.LogInfo($"턴 상태 복원: {turnName} 턴 (턴 {turnCount})", GameLogger.LogCategory.Combat);
        }
        
        /// <summary>
        /// 턴 수를 설정합니다. (저장 시스템용)
        /// </summary>
        /// <param name="count">설정할 턴 수</param>
        public void SetTurnCount(int count)
        {
            if (count < 1)
            {
                GameLogger.LogError($"잘못된 턴 수: {count}", GameLogger.LogCategory.Combat);
                return;
            }
            
            turnCount = count;
            OnTurnCountChanged?.Invoke(turnCount);
            GameLogger.LogInfo($"턴 수 설정: {count}", GameLogger.LogCategory.Combat);
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

        // 내부 전진/생성 제어 플래그
        private bool _isAdvancingQueue = false;
        private bool _nextSpawnIsPlayer = false; // 대기4 교대 스폰 제어 (false=적, true=플레이어 마커) - 1:1 교대
        private readonly System.Collections.Generic.HashSet<Game.SkillCardSystem.Interface.ISkillCard> _scheduledEnemyExec = new();
        private bool _suppressAutoRefill = false; // 초기 셋업 등 특정 구간에서 자동 보충 억제
        private bool _suppressAutoExecution = false; // 초기 셋업 중 자동 실행 억제
        // 초기 셋업 시 사용한 적 덱/이름 캐시 (보충 시 동일 소스 사용 보장)
        private Game.CharacterSystem.Data.EnemyCharacterData _cachedEnemyData;
        private string _cachedEnemyName;

        /// <summary>
        /// 적 캐시를 초기화합니다. 적이 교체될 때 호출되어야 합니다.
        /// </summary>
        public void ClearEnemyCache()
        {
            _cachedEnemyData = null;
            _cachedEnemyName = null;
            GameLogger.LogInfo("[TurnManager] 적 캐시 초기화 완료", GameLogger.LogCategory.Combat);
        }

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

        /// <summary>
        /// 로그 태그(턴/프레임)를 생성합니다. 예: [T2-Enemy-F12345]
        /// </summary>
        private string FormatLogTag()
        {
            var turnName = currentTurn == TurnType.Player ? "Player" : "Enemy";
            return $"[T{turnCount}-{turnName}-F{Time.frameCount}]";
        }

        #endregion
        
        #region 카드 레지스트리 관리 (TurnCardRegistry 통합)
        
        private readonly Dictionary<CombatSlotPosition, ISkillCard> _cards = new();
        private readonly Dictionary<CombatSlotPosition, Game.SkillCardSystem.UI.SkillCardUI> _cardUIs = new();
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
            if (ui != null)
                _cardUIs[position] = ui;

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
                    if (_cardUIs.TryGetValue(slot, out var ui) && ui != null)
                    {
                        DestroyImmediate(ui.gameObject);
                        // GameLogger.LogInfo($"슬롯 UI 제거: {slotName}", GameLogger.LogCategory.Combat);
                    }
                }

                // 데이터 제거
                _cards.Remove(slot);
                _cardUIs.Remove(slot);
                OnCardStateChanged?.Invoke();
            }
        }
        
        /// <summary>
        /// 모든 슬롯을 완전히 정리합니다 (데이터 + UI)
        /// 적 처치 시 플레이어 핸드와 모든 슬롯을 정리할 때 사용됩니다.
        /// </summary>
        public void ClearAllSlots()
        {
            var allSlots = new List<CombatSlotPosition>(_cards.Keys);
            
            foreach (var slot in allSlots)
            {
                // UI 제거
                if (_cardUIs.TryGetValue(slot, out var ui) && ui != null)
                {
                    if (ui is MonoBehaviour uiMb)
                    {
                        Destroy(uiMb.gameObject);
                        GameLogger.LogInfo($"[TurnManager] 슬롯 UI 제거: {slot}", GameLogger.LogCategory.Combat);
                    }
                }
                _cardUIs.Remove(slot);
            }

            // 데이터 제거
            _cards.Clear();
            _reservedEnemySlot = null;
            OnCardStateChanged?.Invoke();

            GameLogger.LogInfo($"[TurnManager] 모든 슬롯 정리 완료: {allSlots.Count}개 슬롯", GameLogger.LogCategory.Combat);
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
        /// 적 카드만 제거하고 플레이어 카드 보존 (UI 포함)
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
            {
                // UI 제거
                if (_cardUIs.TryGetValue(key, out var ui) && ui != null)
                {
                    if (ui is MonoBehaviour uiMb)
                    {
                        Destroy(uiMb.gameObject);
                        GameLogger.LogInfo($"[TurnManager] 적 카드 UI 제거: 슬롯 {key}", GameLogger.LogCategory.Combat);
                    }
                }
                _cardUIs.Remove(key);

                // 데이터 제거
                _cards.Remove(key);
            }

            _reservedEnemySlot = null;
            OnCardStateChanged?.Invoke();

            if (toRemove.Count > 0)
            {
                GameLogger.LogInfo($"[TurnManager] 적 카드 {toRemove.Count}개 제거 완료 (UI 포함)", GameLogger.LogCategory.Combat);
            }
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
        /// 턴 효과는 모든 캐릭터에게 동시에 적용되어야 합니다.
        /// 상태 패턴에서 호출할 수 있도록 public으로 변경
        /// </summary>
        public void ProcessAllCharacterTurnEffects()
        {
            // 모든 캐릭터의 턴 효과를 동시에 처리
            var playerManager = GetCachedPlayerManager();
            var enemyManager = GetCachedEnemyManager();

            var player = playerManager?.GetCharacter();
            var enemy = enemyManager?.GetCharacter();

            // 플레이어 턴 효과 처리
            if (player != null)
            {
                player.ProcessTurnEffects();
                GameLogger.LogInfo($"플레이어 캐릭터 턴 효과 처리: {player.GetCharacterName()}", GameLogger.LogCategory.Combat);
            }

            // 적 턴 효과 처리
            if (enemy != null)
            {
                enemy.ProcessTurnEffects();
                // GameLogger.LogInfo($"적 캐릭터 턴 효과 처리: {enemy.GetCharacterName()}", GameLogger.LogCategory.Combat);
            }
        }
        
        /// <summary>
        /// 슬롯 전진 루틴 (상태 패턴에서 호출)
        /// 배틀 슬롯이 비어있으면 대기 슬롯을 앞으로 이동시킵니다.
        /// </summary>
		public System.Collections.IEnumerator AdvanceQueueAtTurnStartRoutine()
		{
			// 한 프레임 대기
			yield return null;

			// 배틀 슬롯이 비어있으면 슬롯 이동
			if (!HasCardInSlot(CombatSlotPosition.BATTLE_SLOT))
			{
                yield return MoveAllSlotsForwardRoutine();
			}

			GameLogger.LogInfo("슬롯 전진 완료", GameLogger.LogCategory.Combat);
        }

        /// <summary>
        /// 대기 슬롯 4가 비어있으면 교대 규칙에 따라 카드를 보충합니다.
        /// </summary>
        private System.Collections.IEnumerator RefillWaitSlot4IfNeededRoutine()
        {
            if (_suppressAutoRefill)
            {
                GameLogger.LogInfo($"{FormatLogTag()} [Refill] 자동 보충 억제 중 → 스킵", GameLogger.LogCategory.Combat);
                yield break;
            }
            if (GetCardInSlot(CombatSlotPosition.WAIT_SLOT_4) != null)
            {
                GameLogger.LogInfo($"{FormatLogTag()} [Refill] 대기4 이미 점유 → 스킵", GameLogger.LogCategory.Combat);
                yield break;
            }

            // 프리팹 로드 (캐시 사용)
            var cardUIPrefab = GetCachedCardUIPrefab();
            if (cardUIPrefab == null)
            {
                GameLogger.LogWarning($"{FormatLogTag()} [Refill] SkillCardUI 프리팹을 찾지 못함", GameLogger.LogCategory.Combat);
                yield break;
            }

            // 패턴: 플레이어 마커 1개 ↔ 적 카드 1개 (1:1 교대)
            if (_nextSpawnIsPlayer)
            {
                var marker = CreatePlayerMarker();
                if (marker != null)
                {
                    // Wait4에 고정 배치(전진 트리거하지 않음)
                    var ui = CreateCardUIForSlot(marker, CombatSlotPosition.WAIT_SLOT_4, null, cardUIPrefab);
                    var tween = PlaySpawnTween(ui);
                    RegisterCard(CombatSlotPosition.WAIT_SLOT_4, marker, ui, SlotOwner.PLAYER);
                    GameLogger.LogInfo($"{FormatLogTag()} [Refill] 대기4 보충: 플레이어 마커", GameLogger.LogCategory.Combat);
                    if (tween != null) yield return tween.WaitForCompletion();
                }
            }
            else
            {
                // 적 카드 생성 (캐시된 덱 우선)
                var enemyManager = GetCachedEnemyManager();
                var enemy = enemyManager?.GetCharacter();
                var runtimeData = enemy?.CharacterData as Game.CharacterSystem.Data.EnemyCharacterData;
                var runtimeName = enemy?.GetCharacterName() ?? "Enemy";
                var enemyData = _cachedEnemyData ?? runtimeData;
                var enemyName = string.IsNullOrEmpty(_cachedEnemyName) ? runtimeName : _cachedEnemyName;
                var audioMgr = UnityEngine.Object.FindFirstObjectByType<Game.CoreSystem.Audio.AudioManager>();
                var factory = new Game.SkillCardSystem.Factory.SkillCardFactory(audioMgr);

                Game.SkillCardSystem.Deck.EnemySkillDeck.CardEntry entry = null;
                if (enemyData?.EnemyDeck != null)
                {
                    // GetRandomEntry가 간헐적으로 null을 반환할 수 있으므로 소량 재시도
                    for (int attempt = 0; attempt < 5 && entry == null; attempt++)
                    {
                        entry = enemyData.EnemyDeck.GetRandomEntry();
                    }
                }

                if (entry?.definition != null)
                {
                    var card = factory.CreateEnemyCard(entry.definition, enemyName);
                    var ui = CreateCardUIForSlot(card, CombatSlotPosition.WAIT_SLOT_4, null, cardUIPrefab);
                    var tween = PlaySpawnTween(ui);
                    RegisterCard(CombatSlotPosition.WAIT_SLOT_4, card, ui, SlotOwner.ENEMY);
                    GameLogger.LogInfo($"{FormatLogTag()} [Refill] 대기4 보충: 적 카드 {card.GetCardName()}", GameLogger.LogCategory.Combat);
                    if (tween != null) yield return tween.WaitForCompletion();
                }
                else
                {
                    GameLogger.LogWarning($"{FormatLogTag()} [Refill] 적 덱에서 카드를 얻지 못함", GameLogger.LogCategory.Combat);
                }
            }

            // 다음 생성 주체 토글 (1:1 교대)
            _nextSpawnIsPlayer = !_nextSpawnIsPlayer;
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
        /// 코루틴: 동적 셋업을 단계별로 순차 진행
        /// CombatInitState에서 호출합니다.
        /// </summary>
        public System.Collections.IEnumerator SetupInitialEnemyQueueRoutine(Game.CharacterSystem.Data.EnemyCharacterData enemyData, string enemyName)
        {
            if (enemyData?.EnemyDeck == null)
            {
                GameLogger.LogWarning($"적 데이터 또는 적 덱이 null입니다. 적: {enemyName}", GameLogger.LogCategory.Combat);
                yield break;
            }

            var audioMgr = UnityEngine.Object.FindFirstObjectByType<Game.CoreSystem.Audio.AudioManager>();
            var factory = new Game.SkillCardSystem.Factory.SkillCardFactory(audioMgr);

            // SkillCardUI 프리팁 로드 (캐시 사용)
            var cardUIPrefab = GetCachedCardUIPrefab();
            if (cardUIPrefab == null)
            {
                GameLogger.LogWarning("SkillCardUI 프리팹을 찾을 수 없습니다. UI 없이 데이터만 등록합니다.", GameLogger.LogCategory.Combat);
            }
            else
            {
                GameLogger.LogInfo("SkillCardUI 프리팹 로드 완료", GameLogger.LogCategory.Combat);
            }

            GameLogger.LogInfo("동적 슬롯 셋업 시작 - 실제 게임 플레이 방식", GameLogger.LogCategory.Combat);

            // 초기 셋업 구간에서는 자동 보충/자동 실행 억제 (중복 생성/조기 실행 방지)
            _suppressAutoRefill = true;
            _suppressAutoExecution = true;
            // 적 덱/이름 캐시 저장
            _cachedEnemyData = enemyData;
            _cachedEnemyName = enemyName;

            // 초기 셋업: 플레이어 마커 ↔ 적 카드 (1:1 교대, 총 5개)
            // 패턴: 플레이어 → 적 → 플레이어 → 적 → 플레이어
            _nextSpawnIsPlayer = true;
            bool isPlayerTurn = true;

            for (int i = 0; i < 5; i++)
            {
                GameLogger.LogInfo($"[초기셋업] {i+1}/5 - {(isPlayerTurn ? "플레이어 마커" : "적 카드")}", GameLogger.LogCategory.Combat);

                if (isPlayerTurn)
                {
                    var marker = CreatePlayerMarker();
                    if (marker != null)
                    {
                        yield return PlaceCardInWaitSlot4AndMoveRoutine(marker, SlotOwner.PLAYER, cardUIPrefab);
                        GameLogger.LogInfo($"[{i+1}/5] 플레이어 마커 생성 및 배치 완료", GameLogger.LogCategory.Combat);
                    }
                }
                else
                {
                    var entry = enemyData.EnemyDeck.GetRandomEntry();
                    if (entry?.definition != null)
                    {
                        var card = factory.CreateEnemyCard(entry.definition, enemyName);
                        yield return PlaceCardInWaitSlot4AndMoveRoutine(card, SlotOwner.ENEMY, cardUIPrefab);
                        GameLogger.LogInfo($"[{i+1}/5] 적 카드 생성 및 배치 완료: {card.CardDefinition?.CardName}", GameLogger.LogCategory.Combat);
                    }
                }

                // 1:1 교대
                isPlayerTurn = !isPlayerTurn;
            }

            GameLogger.LogInfo("동적 슬롯 셋업 완료 - 패턴: 플레이어 → 적 → 플레이어 → 적 → 플레이어 (1:1 교대)", GameLogger.LogCategory.Combat);
            _initialSlotSetupCompleted = true;

            // 이동/애니메이션이 모두 끝날 때까지 대기
            while (_isAdvancingQueue)
            {
                yield return null;
            }
            yield return null;

            // 초기 셋업 완료 후 다음 생성 주체 설정
            // 마지막이 플레이어 마커였으므로 다음은 적 카드
            _nextSpawnIsPlayer = false;
            
            // 초기 셋업 종료 후 자동 보충/자동 실행 활성화
            _suppressAutoRefill = false;
            _suppressAutoExecution = false;
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
        private System.Collections.IEnumerator PlaceCardInWaitSlot4AndMoveRoutine(ISkillCard card, SlotOwner owner, Game.SkillCardSystem.UI.SkillCardUI cardUIPrefab)
        {
            if (card == null)
            {
                GameLogger.LogWarning("배치할 카드가 null입니다.", GameLogger.LogCategory.Combat);
                yield break;
            }

            // 1. 대기4에 카드 배치 (중복 방지: 이미 있으면 스킵)
            if (GetCardInSlot(CombatSlotPosition.WAIT_SLOT_4) != null)
            {
                yield break;
            }
            var cardUI = CreateCardUIForSlot(card, CombatSlotPosition.WAIT_SLOT_4, null, cardUIPrefab);
            var spawnTween = PlaySpawnTween(cardUI);
            RegisterCard(CombatSlotPosition.WAIT_SLOT_4, card, cardUI, owner);
            GameLogger.LogInfo($"대기4에 카드 배치: {card.GetCardName()}", GameLogger.LogCategory.Combat);
            if (spawnTween != null) yield return spawnTween.WaitForCompletion();

            // 2. 배틀슬롯이 비어있으면 모든 카드를 앞으로 이동 (현재 프레임의 레이아웃/애니메이션 반영을 위해 1프레임 대기)
            yield return null;
            if (!HasCardInSlot(CombatSlotPosition.BATTLE_SLOT))
            {
                yield return MoveAllSlotsForwardRoutine();
                GameLogger.LogInfo("배틀슬롯이 비어있어 모든 카드 앞으로 이동", GameLogger.LogCategory.Combat);
            }
        }

        // 기존 즉시 실행 버전은 내부적으로 코루틴 호출로 대체(호환용)
        private void PlaceCardInWaitSlot4AndMove(ISkillCard card, SlotOwner owner, Game.SkillCardSystem.UI.SkillCardUI cardUIPrefab)
        {
            StartCoroutine(PlaceCardInWaitSlot4AndMoveRoutine(card, owner, cardUIPrefab));
        }

        /// <summary>
        /// 모든 슬롯의 카드를 앞으로 한 칸씩 이동시킵니다. (코루틴)
        /// 대기4 → 대기3 → 대기2 → 대기1 → 배틀슬롯
        /// </summary>
        private System.Collections.IEnumerator MoveAllSlotsForwardRoutine()
        {
            if (_isAdvancingQueue) yield break;
            _isAdvancingQueue = true;

            yield return MoveCardToSlotRoutine(CombatSlotPosition.WAIT_SLOT_1, CombatSlotPosition.BATTLE_SLOT);
            yield return MoveCardToSlotRoutine(CombatSlotPosition.WAIT_SLOT_2, CombatSlotPosition.WAIT_SLOT_1);
            yield return MoveCardToSlotRoutine(CombatSlotPosition.WAIT_SLOT_3, CombatSlotPosition.WAIT_SLOT_2);
            yield return MoveCardToSlotRoutine(CombatSlotPosition.WAIT_SLOT_4, CombatSlotPosition.WAIT_SLOT_3);

            // 전진 후 대기4 보충 (모든 이동 트윈이 끝난 다음 1프레임 대기 후 보충)
            yield return null;
            yield return RefillWaitSlot4IfNeededRoutine();

            _isAdvancingQueue = false;
            GameLogger.LogInfo($"{FormatLogTag()} 슬롯 이동 완료: 4→3→2→1→배틀", GameLogger.LogCategory.Combat);

            // 전진이 끝난 시점에서 배틀 슬롯의 적 카드를 자동 실행 (Enemy 턴, 억제 해제 상태)
            TryAutoExecuteEnemyAtBattleSlot();
        }

        /// <summary>
        /// 특정 슬롯의 카드를 다른 슬롯으로 이동시킵니다.
        /// </summary>
        /// <param name="fromSlot">원본 슬롯</param>
        /// <param name="toSlot">대상 슬롯</param>
        private System.Collections.IEnumerator MoveCardToSlotRoutine(CombatSlotPosition fromSlot, CombatSlotPosition toSlot)
        {
            var card = GetCardInSlot(fromSlot);
            if (card == null) yield break;

            // UI 이동 트윈 후 데이터 갱신
            if (_cardUIs.TryGetValue(fromSlot, out var ui) && ui != null)
            {
                var targetName = GetSlotGameObjectName(toSlot);
                var targetGo = GameObject.Find(targetName);
                var target = targetGo != null ? targetGo.transform as RectTransform : null;
                var uiRect = ui.transform as RectTransform;
                if (uiRect != null && target != null)
                {
                    // 이동 중에는 최상위 캔버스 하위로 올려서 항상 슬롯 위에 보이도록 처리
                    var originalParent = uiRect.parent as RectTransform;
                    var root = target.root as RectTransform;
                    if (root != null)
                    {
                        uiRect.SetParent(root, true);
                        uiRect.SetAsLastSibling();
                    }

                    // 목적지 월드 좌표 계산 후 월드 기준 이동 트윈 → 완료 시 부모 재설정
                    Vector3 endWorld = (target as RectTransform) != null
                        ? (target as RectTransform).TransformPoint(Vector3.zero)
                        : target.position;
                    var moveTween = uiRect.DOMove(endWorld, 0.25f).SetEase(Ease.OutQuad);
                    var scaleTween = uiRect.DOScale(1f, 0.25f).SetEase(Ease.OutQuad);
                    yield return moveTween.WaitForCompletion();
                    // 최종 부모로 설정하고 로컬 정렬
                    uiRect.SetParent(target, false);
                    uiRect.anchoredPosition = Vector2.zero;
                }
            }

            // 데이터 재등록
            _cards.Remove(fromSlot);
            if (_cardUIs.ContainsKey(fromSlot)) _cardUIs.Remove(fromSlot);
            var owner = card.IsFromPlayer() ? SlotOwner.PLAYER : SlotOwner.ENEMY;
            _cards[toSlot] = card;
            if (ui != null) _cardUIs[toSlot] = ui;
            OnCardStateChanged?.Invoke();
            
            GameLogger.LogInfo($"{FormatLogTag()} 카드 이동: {card.GetCardName()} ({fromSlot} → {toSlot})", GameLogger.LogCategory.Combat);

            // 적 카드가 배틀 슬롯으로 이동했을 때 로그만 출력
            // 실제 실행은 SlotMovingState에서 처리
            if (toSlot == CombatSlotPosition.BATTLE_SLOT && !card.IsFromPlayer())
            {
                GameLogger.LogInfo($"{FormatLogTag()} 적 카드 배틀 슬롯 도달: {card.GetCardName()} (SlotMovingState에서 자동 실행됨)", GameLogger.LogCategory.Combat);
            }
        }

        private System.Collections.IEnumerator ExecuteEnemyCardNextFrame(CombatExecutionManager exec, ISkillCard card, CombatSlotPosition toSlot)
        {
            yield return null;
            // 최종 게이트 재검증 후 실행
            bool canAutoExecute = !_suppressAutoExecution && _initialSlotSetupCompleted && !_isAdvancingQueue && currentTurn == TurnType.Enemy;
            if (canAutoExecute && GetCardInSlot(CombatSlotPosition.BATTLE_SLOT) == card)
            {
                GameLogger.LogInfo($"{FormatLogTag()} 적 카드 배틀 슬롯 도달, 자동 실행: {card.GetCardName()}", GameLogger.LogCategory.Combat);
                exec.ExecuteCardImmediately(card, toSlot);
            }
            else
            {
                GameLogger.LogInfo($"{FormatLogTag()} [AutoExec-Skip@NextFrame] 게이트 조건 불충족 또는 카드 변경", GameLogger.LogCategory.Combat);
            }
        }

        /// <summary>
        /// 전진이 모두 끝난 후 배틀 슬롯에 적 카드가 대기 중이면 자동 실행합니다.
        /// </summary>
        private void TryAutoExecuteEnemyAtBattleSlot()
        {
            if (_suppressAutoExecution || !_initialSlotSetupCompleted || _isAdvancingQueue || currentTurn != TurnType.Enemy)
                return;

            var card = GetCardInSlot(CombatSlotPosition.BATTLE_SLOT);
            if (card != null && !card.IsFromPlayer())
            {
                if (_scheduledEnemyExec.Contains(card)) return;
                _scheduledEnemyExec.Add(card);
                var exec = GetCachedCombatExecutionManager();
                if (exec != null)
                {
                    GameLogger.LogInfo($"{FormatLogTag()} 배틀 슬롯 적 카드 자동 실행 트리거: {card.GetCardName()}", GameLogger.LogCategory.Combat);
                    exec.ExecuteCardImmediately(card, CombatSlotPosition.BATTLE_SLOT);
                }
            }
        }

        // 기존 즉시 실행 버전(호환용)
        private void MoveCardToSlot(CombatSlotPosition fromSlot, CombatSlotPosition toSlot)
        {
            StartCoroutine(MoveCardToSlotRoutine(fromSlot, toSlot));
        }

        /// <summary>
        /// 카드 스폰 트윈(등장 연출)을 재생합니다.
        /// </summary>
        private Tween PlaySpawnTween(Game.SkillCardSystem.UI.SkillCardUI cardUI)
        {
            if (cardUI == null) return null;
            if (cardUI.TryGetComponent<CanvasGroup>(out var cg))
            {
                cg.alpha = 0f;
                cg.DOFade(1f, 0.2f).SetEase(Ease.OutQuad);
            }
            var rt = cardUI.transform as RectTransform;
            if (rt != null)
            {
                rt.localScale = Vector3.one * 0.7f;
                return rt.DOScale(1f, 0.2f).SetEase(Ease.OutBack);
            }
            return null;
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
                var playerManager = GetCachedPlayerManager();
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

                // GameLogger.LogInfo("플레이어 마커 카드 생성 완료", GameLogger.LogCategory.Combat);
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

                // 플레이어 마커 UI 간소화: 텍스트 숨김, 드래그 비활성화
                try
                {
                    if (card?.CardDefinition?.cardId == "PLAYER_MARKER" && cardUI != null)
                    {
                        var t = cardUI.transform;
                        var nameGo = t.Find("CardName")?.gameObject;
                        if (nameGo != null) nameGo.SetActive(false);
                        var deGo = t.Find("DE")?.gameObject;
                        if (deGo != null) deGo.SetActive(false);
                        // 모든 TMP 텍스트 숨김 보강
                        var tmps = cardUI.GetComponentsInChildren<TMP_Text>(true);
                        foreach (var tmp in tmps)
                        {
                            tmp.gameObject.SetActive(false);
                        }
                        // 드래그 비활성화 및 레이캐스트 최소화
                        cardUI.SetDraggable(false);
                        if (cardUI.TryGetComponent<UnityEngine.CanvasGroup>(out var cg))
                        {
                            cg.interactable = false;
                            cg.blocksRaycasts = false;
                        }
                    }
                }
                catch { }
                
                if (cardUI != null)
                {
                    // GameLogger.LogInfo($"카드 UI 생성 완료: {card.GetCardName()} → {slotPosition} ({slotName})", GameLogger.LogCategory.Combat);
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
