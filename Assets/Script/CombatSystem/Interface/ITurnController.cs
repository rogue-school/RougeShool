using System;

namespace Game.CombatSystem.Interface
{
    /// <summary>
    /// 턴 관리 전담 인터페이스
    /// 턴 타입과 턴 카운트만 관리합니다.
    /// </summary>
    public interface ITurnController
    {
        /// <summary>
        /// 현재 턴 타입
        /// </summary>
        TurnType CurrentTurn { get; }

        /// <summary>
        /// 현재 턴 번호
        /// </summary>
        int TurnCount { get; }

        /// <summary>
        /// 게임 활성 상태
        /// </summary>
        bool IsGameActive { get; }

        /// <summary>
        /// 턴을 설정하고 필요시 턴 수를 증가시킵니다.
        /// </summary>
        /// <param name="turnType">설정할 턴 타입</param>
        void SetTurnAndIncrement(TurnType turnType);

        /// <summary>
        /// 특정 턴으로 설정합니다 (턴 수 증가 없음)
        /// </summary>
        /// <param name="turnType">설정할 턴 타입</param>
        void SetTurn(TurnType turnType);

        /// <summary>
        /// 턴을 초기화합니다.
        /// </summary>
        void ResetTurn();

        /// <summary>
        /// 게임을 시작합니다.
        /// </summary>
        void StartGame();

        /// <summary>
        /// 게임을 종료합니다.
        /// </summary>
        void EndGame();

        /// <summary>
        /// 모든 캐릭터의 턴 효과를 처리합니다.
        /// </summary>
        void ProcessAllCharacterTurnEffects();

        /// <summary>
        /// 플레이어 턴인지 확인합니다.
        /// </summary>
        bool IsPlayerTurn();

        /// <summary>
        /// 적 턴인지 확인합니다.
        /// </summary>
        bool IsEnemyTurn();

        /// <summary>
        /// 턴이 변경될 때 발생하는 이벤트
        /// </summary>
        event Action<TurnType> OnTurnChanged;

        /// <summary>
        /// 턴 카운트가 변경될 때 발생하는 이벤트
        /// </summary>
        event Action<int> OnTurnCountChanged;

        /// <summary>
        /// 게임이 시작될 때 발생하는 이벤트
        /// </summary>
        event Action OnGameStarted;

        /// <summary>
        /// 게임이 종료될 때 발생하는 이벤트
        /// </summary>
        event Action OnGameEnded;
    }

    /// <summary>
    /// 턴 타입
    /// </summary>
    public enum TurnType
    {
        Player,
        Enemy
    }
}
