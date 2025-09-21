using UnityEngine;
using System;
using Game.CombatSystem.Data;
using Game.CoreSystem.Utility;

namespace Game.CombatSystem.Manager
{
    /// <summary>
    /// 싱글게임용 턴 관리자 (싱글톤)
    /// 전투의 턴 순서와 상태를 관리합니다.
    /// </summary>
    public class TurnManager : MonoBehaviour
    {
        #region 싱글톤

        public static TurnManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeTurn();
            }
            else
            {
                Destroy(gameObject);
            }
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

        /// <summary>
        /// 턴이 변경될 때 발생하는 이벤트
        /// </summary>
        public event Action<TurnType> OnTurnChanged;

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
        /// 현재 턴을 반환합니다.
        /// </summary>
        /// <returns>현재 턴 타입</returns>
        public TurnType GetCurrentTurn() => currentTurn;

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
        /// <param name="turnNumber">설정할 턴 번호</param>
        public void SetTurn(TurnType turnType, int turnNumber = 1)
        {
            currentTurn = turnType;
            turnCount = turnNumber;
            
            var turnName = turnType == TurnType.Player ? "플레이어" : "적";
            GameLogger.LogInfo($"턴 설정: {turnName} 턴 (턴 {turnNumber})", GameLogger.LogCategory.Combat);
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
    }
}
