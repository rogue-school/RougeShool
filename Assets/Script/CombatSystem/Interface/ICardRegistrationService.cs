using Game.SkillCardSystem.Interface;

namespace Game.CombatSystem.Interface
{
    /// <summary>
    /// 전투 중 등록된 플레이어 및 적 카드 정보를 관리하는 서비스 인터페이스입니다.
    /// 카드 쌍 등록, 조회, 초기화를 제공합니다.
    /// </summary>
    public interface ICardRegistrationService
    {
        /// <summary>
        /// 플레이어가 선택한 카드를 등록합니다.
        /// </summary>
        /// <param name="card">플레이어 스킬 카드</param>
        void RegisterPlayerCard(ISkillCard card);

        /// <summary>
        /// 적이 선택한 카드를 등록합니다.
        /// </summary>
        /// <param name="card">적 스킬 카드</param>
        void RegisterEnemyCard(ISkillCard card);

        /// <summary>
        /// 현재 등록된 플레이어 카드와 적 카드 쌍을 반환합니다.
        /// </summary>
        /// <returns>플레이어 카드와 적 카드의 튜플</returns>
        (ISkillCard player, ISkillCard enemy) GetRegisteredCards();

        /// <summary>
        /// 등록된 카드 정보를 초기화합니다.
        /// </summary>
        void Clear();
    }
}
