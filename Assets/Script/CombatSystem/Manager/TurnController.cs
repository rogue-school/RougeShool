using System;
using UnityEngine;
using Game.CombatSystem.Interface;
using Game.CharacterSystem.Manager;
using Game.CoreSystem.Utility;
using Zenject;

namespace Game.CombatSystem.Manager
{
    /// <summary>
    /// 턴 관리 전담 클래스
    /// 턴 타입과 턴 카운트만 관리하며 단일 책임 원칙을 준수합니다.
    /// </summary>
    public class TurnController : ITurnController
    {
        #region 설정

        [Serializable]
        public class TurnSettings
        {
            [Tooltip("시작 턴 타입")]
            public TurnType startingTurn = TurnType.Player;

            [Tooltip("초기 턴 카운트")]
            [Range(1, 100)]
            public int initialTurnCount = 1;
        }

        private readonly TurnSettings _settings;

        #endregion

        #region 의존성

        private readonly PlayerManager _playerManager;
        private readonly EnemyManager _enemyManager;

        #endregion

        #region 상태

        private TurnType _currentTurn;
        private int _turnCount;
        private bool _isGameActive;

        #endregion

        #region 프로퍼티

        public TurnType CurrentTurn => _currentTurn;
        public int TurnCount => _turnCount;
        public bool IsGameActive => _isGameActive;

        #endregion

        #region 이벤트

        public event Action<TurnType> OnTurnChanged;
        public event Action<int> OnTurnCountChanged;
        public event Action OnGameStarted;
        public event Action OnGameEnded;

        #endregion

        #region 생성자

        [Inject]
        public TurnController(
            PlayerManager playerManager,
            EnemyManager enemyManager)
        {
            _playerManager = playerManager;
            _enemyManager = enemyManager;

            // 기본 설정 사용
            _settings = new TurnSettings();

            InitializeTurn();
        }

        /// <summary>
        /// 설정을 지정하는 생성자 (테스트용)
        /// </summary>
        public TurnController(
            PlayerManager playerManager,
            EnemyManager enemyManager,
            TurnSettings settings)
        {
            _playerManager = playerManager;
            _enemyManager = enemyManager;
            _settings = settings;

            InitializeTurn();
        }

        #endregion

        #region 초기화

        private void InitializeTurn()
        {
            _currentTurn = _settings.startingTurn;
            _turnCount = _settings.initialTurnCount;
            _isGameActive = false;
        }

        #endregion

        #region 턴 관리

        public void SetTurnAndIncrement(TurnType turnType)
        {
            // 턴 타입이 실제로 변경될 때만 턴 수 증가
            if (_currentTurn != turnType)
            {
                _turnCount++;
                GameLogger.LogInfo($"[TurnController] 턴 변경: {_currentTurn} → {turnType}, 턴 수: {_turnCount}", GameLogger.LogCategory.Combat);
                OnTurnCountChanged?.Invoke(_turnCount);
            }
            else
            {
                GameLogger.LogInfo($"[TurnController] 턴 유지: {turnType}, 턴 수: {_turnCount}", GameLogger.LogCategory.Combat);
            }

            _currentTurn = turnType;
            OnTurnChanged?.Invoke(turnType);
        }

        public void SetTurn(TurnType turnType)
        {
            GameLogger.LogInfo($"[TurnController] 턴 설정: {turnType} (턴 수: {_turnCount})", GameLogger.LogCategory.Combat);
            _currentTurn = turnType;
            OnTurnChanged?.Invoke(turnType);
        }

        public void ResetTurn()
        {
            _currentTurn = _settings.startingTurn;
            _turnCount = _settings.initialTurnCount;
        }

        public bool IsPlayerTurn()
        {
            return _currentTurn == TurnType.Player;
        }

        public bool IsEnemyTurn()
        {
            return _currentTurn == TurnType.Enemy;
        }

        #endregion

        #region 게임 상태 관리

        public void StartGame()
        {
            _isGameActive = true;
            OnGameStarted?.Invoke();
        }

        public void EndGame()
        {
            _isGameActive = false;
            OnGameEnded?.Invoke();
        }

        #endregion

        #region 턴 효과 처리

        public void ProcessAllCharacterTurnEffects()
        {
            GameLogger.LogInfo($"[TurnController] 턴 효과 처리 시작 - 현재 턴: {_currentTurn}", GameLogger.LogCategory.Combat);
            
            // 플레이어 턴 효과 처리
            var player = _playerManager?.GetCharacter();
            if (player != null)
            {
                GameLogger.LogInfo($"[TurnController] 플레이어 턴 효과 처리: {player.GetCharacterName()}", GameLogger.LogCategory.Combat);
                player.ProcessTurnEffects();
            }

            // 적 턴 효과 처리
            var enemy = _enemyManager?.GetCharacter();
            if (enemy != null)
            {
                GameLogger.LogInfo($"[TurnController] 적 턴 효과 처리: {enemy.GetCharacterName()}", GameLogger.LogCategory.Combat);
                enemy.ProcessTurnEffects();
            }
            
            GameLogger.LogInfo($"[TurnController] 턴 효과 처리 완료", GameLogger.LogCategory.Combat);
        }

        #endregion

        #region 저장/로드 지원

        /// <summary>
        /// 턴 상태를 복원합니다. (저장 시스템용)
        /// </summary>
        public void RestoreTurnState(int turnCount, TurnType turnType)
        {
            _turnCount = turnCount;
            _currentTurn = turnType;

            OnTurnChanged?.Invoke(turnType);
            OnTurnCountChanged?.Invoke(turnCount);
        }

        /// <summary>
        /// 턴 수를 설정합니다. (저장 시스템용)
        /// </summary>
        public void SetTurnCount(int count)
        {
            if (count < 1)
            {
                GameLogger.LogError($"잘못된 턴 수: {count}", GameLogger.LogCategory.Combat);
                return;
            }

            _turnCount = count;
            OnTurnCountChanged?.Invoke(_turnCount);
        }

        #endregion

        #region 디버그

        public void LogTurnInfo()
        {
        }

        #endregion
    }
}
