using Game.CharacterSystem.Interface;

namespace Game.CombatSystem.Interface
{
    /// <summary>
    /// 캐릭터의 가드 상태를 관리하는 서비스 인터페이스
    /// </summary>
    public interface IGuardStateService
    {
        /// <summary>
        /// 모든 캐릭터의 가드 상태를 해제합니다.
        /// </summary>
        void ClearAllGuardStates();
        
        /// <summary>
        /// 특정 캐릭터의 가드 상태를 설정합니다.
        /// </summary>
        /// <param name="character">대상 캐릭터</param>
        /// <param name="isGuarded">가드 상태 여부</param>
        void SetGuardState(ICharacter character, bool isGuarded);
        
        /// <summary>
        /// 특정 캐릭터의 가드 상태를 확인합니다.
        /// </summary>
        /// <param name="character">대상 캐릭터</param>
        /// <returns>가드 상태 여부</returns>
        bool IsGuarded(ICharacter character);
        
        /// <summary>
        /// 플레이어 캐릭터의 가드 상태를 해제합니다.
        /// </summary>
        void ClearPlayerGuardState();
        
        /// <summary>
        /// 적 캐릭터의 가드 상태를 해제합니다.
        /// </summary>
        void ClearEnemyGuardState();
    }
} 