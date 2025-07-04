using System.Collections;
using Game.CombatSystem.Slot;
using Game.SkillCardSystem.Interface;

namespace Game.CombatSystem.Interface
{
    /// <summary>
    /// 전투 중 카드 실행(공격)을 담당하는 실행기 인터페이스입니다.
    /// 슬롯에 등록된 카드를 순차적으로 실행하며, 컨텍스트 생성 및 실행기를 주입받아 처리합니다.
    /// </summary>
    public interface ICombatExecutor
    {
        /// <summary>
        /// 주어진 전투 슬롯 위치에 등록된 카드를 실행합니다.
        /// 카드 실행에는 애니메이션, 이펙트, 데미지 처리 등이 포함될 수 있으며,
        /// 일반적으로 코루틴으로 비동기 처리됩니다.
        /// </summary>
        /// <param name="slotPosition">
        /// 실행할 카드가 등록된 전투 슬롯 위치입니다.
        /// 이 위치는 선공/후공 등의 공용 전투 흐름 기준이며, 소속(플레이어/적)을 포함하지 않습니다.
        /// </param>
        /// <returns>카드 실행을 위한 코루틴</returns>
        IEnumerator PerformAttack(CombatSlotPosition slotPosition);

        /// <summary>
        /// 카드 실행에 필요한 의존성 객체를 주입합니다.
        /// 컨텍스트 제공자와 카드 실행 로직 구현체가 포함됩니다.
        /// </summary>
        /// <param name="provider">카드 실행 컨텍스트 제공자</param>
        /// <param name="executor">카드 실행 처리기</param>
        void InjectExecutionDependencies(ICardExecutionContextProvider provider, ICardExecutor executor);

        /// <summary>
        /// 전투 턴을 관리하는 객체를 주입합니다.
        /// 카드 실행 도중 상태 전이, 턴 종료 처리 등에 활용됩니다.
        /// </summary>
        /// <param name="turnManager">전투 턴 관리 객체</param>
        void SetTurnManager(ICombatTurnManager turnManager);
    }
}
