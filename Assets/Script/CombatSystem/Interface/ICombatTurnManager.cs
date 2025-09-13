using Game.CombatSystem.Data;
using Game.CombatSystem.Slot;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.UI;

namespace Game.CombatSystem.Interface
{
    /// <summary>
    /// 전투 턴의 상태 관리 및 카드 등록, 턴 흐름 제어를 담당하는 인터페이스입니다.
    /// </summary>
    public interface ICombatTurnManager
    {
        /// <summary>
        /// 전투 턴 시스템을 초기화합니다.
        /// 초기 상태 설정 및 슬롯/카드 관련 초기 처리를 포함할 수 있습니다.
        /// </summary>
        void Initialize();

        /// <summary>
        /// 전투 턴 시스템을 재설정합니다.
        /// 상태 및 내부 데이터 초기화.
        /// </summary>
        void Reset();

        /// <summary>
        /// 다음 턴 상태 전이를 예약합니다.
        /// </summary>
        /// <param name="nextState">전이할 다음 상태</param>
        void RequestStateChange(ICombatTurnState nextState);

        /// <summary>
        /// 즉시 새로운 상태로 전이합니다.
        /// </summary>
        /// <param name="newState">전이할 상태</param>
        void ChangeState(ICombatTurnState newState);

        /// <summary>
        /// 현재 턴 상태를 반환합니다.
        /// </summary>
        /// <returns>현재 턴 상태</returns>
        ICombatTurnState GetCurrentState();

        /// <summary>
        /// 상태 생성 팩토리를 반환합니다.
        /// </summary>
        /// <returns>전투 상태 팩토리</returns>
        ICombatStateFactory GetStateFactory();

        /// <summary>
        /// 적이 사용할 슬롯을 예약합니다 (예: 후공 실행용).
        /// </summary>
        /// <param name="slot">예약할 슬롯</param>
        void ReserveNextEnemySlot(CombatSlotPosition slot);

        /// <summary>
        /// 예약된 적 슬롯을 가져옵니다.
        /// </summary>
        /// <returns>예약된 슬롯 위치, 없으면 null</returns>
        CombatSlotPosition? GetReservedEnemySlot();

        /// <summary>
        /// 현재 턴이 플레이어 입력 턴인지 확인합니다.
        /// </summary>
        /// <returns>플레이어 입력 턴 여부</returns>
        bool IsPlayerInputTurn();

        /// <summary>
        /// 전투 슬롯에 카드 및 UI를 등록합니다.
        /// </summary>
        /// <param name="slot">등록할 슬롯 위치</param>
        /// <param name="card">등록할 카드</param>
        /// <param name="ui">카드 UI</param>
        /// <param name="owner">카드 소유자 (플레이어 또는 적)</param>
        void RegisterCard(CombatSlotPosition slot, ISkillCard card, SkillCardUI ui, SlotOwner owner);

        /// <summary>
        /// 현재 턴을 설정합니다. (저장 시스템용)
        /// </summary>
        /// <param name="turn">설정할 턴</param>
        void SetCurrentTurn(int turn);

        /// <summary>
        /// 현재 턴을 반환합니다. (저장 시스템용)
        /// </summary>
        /// <returns>현재 턴 번호</returns>
        int GetCurrentTurn();

        /// <summary>
        /// 가드 효과를 적용합니다.
        /// 다음 슬롯의 적 스킬카드를 무효화시킵니다.
        /// </summary>
        void ApplyGuardEffect();
    }
}
