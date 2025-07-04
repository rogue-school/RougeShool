using Game.SkillCardSystem.Interface;

namespace Game.CombatSystem.Interface
{
    /// <summary>
    /// 전투 중 플레이어와 적의 카드 정보를 등록 및 관리하는 서비스 인터페이스입니다.
    /// 카드 실행 시 사용할 등록 정보를 유지하고 상태 초기화 기능을 제공합니다.
    /// </summary>
    public interface ICardRegistrationService
    {
        /// <summary>
        /// 플레이어가 선택한 스킬 카드를 등록합니다.
        /// 이후 전투 실행 시 등록된 카드가 참조됩니다.
        /// </summary>
        /// <param name="card">
        /// 등록할 플레이어의 <see cref="ISkillCard"/> 인스턴스입니다.
        /// </param>
        void RegisterPlayerCard(ISkillCard card);

        /// <summary>
        /// 적이 선택한 스킬 카드를 등록합니다.
        /// 이후 전투 실행 시 등록된 카드가 참조됩니다.
        /// </summary>
        /// <param name="card">
        /// 등록할 적의 <see cref="ISkillCard"/> 인스턴스입니다.
        /// </param>
        void RegisterEnemyCard(ISkillCard card);

        /// <summary>
        /// 현재 등록된 플레이어 및 적 스킬 카드를 반환합니다.
        /// </summary>
        /// <returns>
        /// 등록된 플레이어 카드와 적 카드의 튜플.
        /// 등록되지 않은 경우 null을 포함할 수 있습니다.
        /// </returns>
        (ISkillCard player, ISkillCard enemy) GetRegisteredCards();

        /// <summary>
        /// 등록된 플레이어 및 적 카드 정보를 초기화합니다.
        /// 턴 종료 또는 전투 결과 이후 호출됩니다.
        /// </summary>
        void Clear();
    }
}
