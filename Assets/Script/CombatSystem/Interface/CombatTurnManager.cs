using UnityEngine;
using Game.CombatSystem.Interface;

namespace Game.CombatSystem.Interface
{
    /// <summary>
    /// 전투 턴 관리자 인터페이스
    /// </summary>
    public interface ICombatTurnManager
    {
        /// <summary>
        /// 현재 턴 타입
        /// </summary>
        TurnType CurrentTurn { get; }
        
        /// <summary>
        /// 현재 턴 번호
        /// </summary>
        int CurrentTurnNumber { get; }
        
        /// <summary>
        /// 턴 시작
        /// </summary>
        void StartTurn();
        
        /// <summary>
        /// 턴 종료
        /// </summary>
        void EndTurn();
        
        /// <summary>
        /// 다음 턴으로 진행
        /// </summary>
        void NextTurn();
        
        /// <summary>
        /// 턴 리셋
        /// </summary>
        void ResetTurn();
    }
    
    /// <summary>
    /// 턴 타입 열거형
    /// </summary>
    public enum TurnType
    {
        None,       // 없음
        Player,     // 플레이어
        Enemy       // 적
    }
}
