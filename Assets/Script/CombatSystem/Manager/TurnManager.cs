using UnityEngine;
using System;
using System.Collections.Generic;
using Game.CombatSystem.Data;
using Game.CombatSystem.Interface;
using Game.CombatSystem.Slot;
using Game.CoreSystem.Utility;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.UI;
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

            [Space(5)]
            [Header("턴 제한")]
            [Tooltip("최대 턴 수 (0 = 무제한)")]
            [Range(0, 1000)]
            public int maxTurns = 0;

            [Tooltip("턴 시간 제한 (초, 0 = 무제한)")]
            [Range(0f, 300f)]
            public float turnTimeLimit = 0f;
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
        public float TurnTimeLimit => turnSettings.turnTimeLimit;
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
            // 최대 턴 수 확인
            if (turnSettings.maxTurns > 0 && turnCount >= turnSettings.maxTurns)
            {
                if (debugSettings.enableTurnLogging)
                {
                    GameLogger.LogWarning($"최대 턴 수({turnSettings.maxTurns})에 도달했습니다.", GameLogger.LogCategory.Combat);
                }
                return;
            }

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
            remainingTurnTime = turnSettings.turnTimeLimit;
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
            remainingTurnTime = turnSettings.turnTimeLimit;
            
            if (debugSettings.enableTurnLogging)
            {
                GameLogger.LogInfo($"턴 시간 리셋: {remainingTurnTime}초", GameLogger.LogCategory.Combat);
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
        /// 슬롯을 클리어합니다.
        /// </summary>
        /// <param name="slot">클리어할 슬롯</param>
        public void ClearSlot(CombatSlotPosition slot)
        {
            if (_cards.Remove(slot))
                OnCardStateChanged?.Invoke();
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
            // TODO: 가드 효과 구현
            GameLogger.LogInfo("가드 효과 적용", GameLogger.LogCategory.Combat);
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
            // TODO: 적 카드 등록 로직 구현
            GameLogger.LogInfo($"적 카드 등록: {card?.CardDefinition?.CardName ?? "Unknown"}", GameLogger.LogCategory.Combat);
        }
        
        #endregion
    }
}
