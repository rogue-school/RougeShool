using System.Collections;
using Game.CombatSystem.Slot;
using Game.SkillCardSystem.Interface;

namespace Game.CombatSystem.Interface
{
    /// <summary>
    /// 전투 중 카드 실행(공격)을 담당하는 실행기 인터페이스입니다.
    /// 카드 실행 로직을 순차적으로 처리하며, 의존 객체를 주입받습니다.
    /// </summary>
    public interface ICombatExecutor
    {
        /// <summary>
        /// 지정된 슬롯의 카드를 실행(공격)합니다.
        /// 일반적으로 코루틴으로 처리되며, 애니메이션이나 이펙트와 함께 사용됩니다.
        /// </summary>
        /// <param name="slotPosition">실행할 슬롯 위치</param>
        /// <returns>공격 처리용 코루틴</returns>
        IEnumerator PerformAttack(CombatSlotPosition slotPosition);

        /// <summary>
        /// 카드 실행에 필요한 의존성 객체들을 주입합니다.
        /// </summary>
        /// <param name="provider">카드 실행 컨텍스트 생성기</param>
        /// <param name="executor">카드 실행 처리기</param>
        void InjectExecutionDependencies(ICardExecutionContextProvider provider, ICardExecutor executor);

        /// <summary>
        /// 전투 턴 관리자를 주입합니다.
        /// (이전: ITurnStateController → 현재: ICombatTurnManager)
        /// </summary>
        /// <param name="turnManager">턴 관리 객체</param>
        void SetTurnManager(ICombatTurnManager turnManager);
    }
}
