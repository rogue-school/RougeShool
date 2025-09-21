using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.UI;
using Game.CombatSystem.Interface;

namespace Game.CombatSystem.Interface
{
    /// <summary>
    /// 카드 등록 관련 통합 인터페이스입니다.
    /// 전투 슬롯 등록과 카드 정보 관리를 모두 담당합니다.
    /// </summary>
    public interface ICardRegistrar
    {
        /// <summary>
        /// 지정된 전투 슬롯에 스킬 카드 및 해당 UI를 등록합니다.
        /// 등록된 슬롯은 카드 실행이나 상태 갱신에 이 정보를 사용합니다.
        /// </summary>
        /// <param name="slot">
        /// <see cref="ICombatCardSlot"/> 인터페이스를 구현하는 슬롯 객체.
        /// 스킬 카드가 배치되는 대상 슬롯입니다.
        /// </param>
        /// <param name="card">
        /// <see cref="ISkillCard"/> 객체. 슬롯에 등록될 실제 카드 데이터입니다.
        /// </param>
        /// <param name="ui">
        /// <see cref="SkillCardUI"/> 컴포넌트. 카드에 대응하는 UI 오브젝트로,
        /// 시각적 효과 및 드래그 관련 처리를 담당합니다.
        /// </param>
        void RegisterCard(ICombatCardSlot slot, ISkillCard card, SkillCardUI ui);

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
