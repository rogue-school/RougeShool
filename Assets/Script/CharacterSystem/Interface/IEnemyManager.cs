using Game.CharacterSystem.Interface;
using Game.CombatSystem.Interface;
using Game.SkillCardSystem.Interface;

namespace Game.CharacterSystem.Interface
{
    /// <summary>
    /// 전투 중 적 캐릭터 및 핸드 매니저를 관리하는 인터페이스입니다.
    /// 적 등록, 조회, 초기화, 핸드 접근 등의 기능을 제공합니다.
    /// </summary>
    public interface IEnemyManager
    {
        /// <summary>
        /// 적 캐릭터를 등록합니다.
        /// </summary>
        /// <param name="enemy">등록할 적 캐릭터</param>
        void RegisterEnemy(IEnemyCharacter enemy);

        /// <summary>
        /// 현재 등록된 적 캐릭터를 반환합니다.
        /// </summary>
        /// <returns>등록된 적 캐릭터</returns>
        IEnemyCharacter GetEnemy();

        /// <summary>
        /// 현재 전투에 사용 중인 적 캐릭터를 반환합니다.
        /// 필요 시 내부 상태에 따라 다르게 처리될 수 있습니다.
        /// </summary>
        IEnemyCharacter GetCurrentEnemy();

        /// <summary>
        /// 적 캐릭터가 등록되어 있는지 여부를 반환합니다.
        /// </summary>
        /// <returns>등록 여부</returns>
        bool HasEnemy();

        /// <summary>
        /// 등록된 적 캐릭터를 제거합니다.
        /// </summary>
        void ClearEnemy();

        /// <summary>
        /// 적 캐릭터와 관련된 모든 내부 상태를 초기화합니다.
        /// </summary>
        void Reset();

        /// <summary>
        /// 등록된 적의 핸드 매니저를 반환합니다.
        /// </summary>
        /// <returns>적 핸드 매니저</returns>
        IEnemyHandManager GetEnemyHandManager();

        /// <summary>
        /// 등록된 적 캐릭터를 명시적으로 등록 해제합니다.
        /// </summary>
        void UnregisterEnemy();
    }
}
