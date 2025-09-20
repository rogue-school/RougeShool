using UnityEngine;

namespace Game.CombatSystem.Interface
{
    /// <summary>
    /// 전투 페이즈를 나타내는 열거형
    /// </summary>
    public enum CombatPhase
    {
        None,           // 없음
        Preparation,    // 준비
        PlayerTurn,     // 플레이어 턴
        EnemyTurn,      // 적 턴
        Resolution,     // 해결
        Victory,        // 승리
        Defeat,         // 패배
        Paused          // 일시정지
    }
    
    /// <summary>
    /// 전투 페이즈 확장 메서드
    /// </summary>
    public static class CombatPhaseExtensions
    {
        /// <summary>
        /// 페이즈의 한국어 이름을 반환
        /// </summary>
        public static string GetDisplayName(this CombatPhase phase)
        {
            return phase switch
            {
                CombatPhase.None => "없음",
                CombatPhase.Preparation => "준비",
                CombatPhase.PlayerTurn => "플레이어 턴",
                CombatPhase.EnemyTurn => "적 턴",
                CombatPhase.Resolution => "해결",
                CombatPhase.Victory => "승리",
                CombatPhase.Defeat => "패배",
                CombatPhase.Paused => "일시정지",
                _ => "알 수 없음"
            };
        }
        
        /// <summary>
        /// 다음 페이즈로 진행 가능한지 확인
        /// </summary>
        public static bool CanTransitionTo(this CombatPhase currentPhase, CombatPhase nextPhase)
        {
            return currentPhase switch
            {
                CombatPhase.None => nextPhase == CombatPhase.Preparation,
                CombatPhase.Preparation => nextPhase == CombatPhase.PlayerTurn || nextPhase == CombatPhase.Paused,
                CombatPhase.PlayerTurn => nextPhase == CombatPhase.EnemyTurn || nextPhase == CombatPhase.Resolution || nextPhase == CombatPhase.Paused,
                CombatPhase.EnemyTurn => nextPhase == CombatPhase.PlayerTurn || nextPhase == CombatPhase.Resolution || nextPhase == CombatPhase.Paused,
                CombatPhase.Resolution => nextPhase == CombatPhase.PlayerTurn || nextPhase == CombatPhase.Victory || nextPhase == CombatPhase.Defeat,
                CombatPhase.Victory => nextPhase == CombatPhase.None,
                CombatPhase.Defeat => nextPhase == CombatPhase.None,
                CombatPhase.Paused => currentPhase != CombatPhase.Victory && currentPhase != CombatPhase.Defeat,
                _ => false
            };
        }
    }
}
