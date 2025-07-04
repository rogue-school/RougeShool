namespace Game.CombatSystem.Interface
{
    /// <summary>
    /// 턴 시작 조건을 검사하는 인터페이스입니다.
    /// 예: 모든 슬롯에 카드가 등록되었는지, 쿨타임이 끝났는지 등을 판단할 수 있습니다.
    /// </summary>
    public interface ITurnStartConditionChecker
    {
        /// <summary>
        /// 현재 턴을 시작할 수 있는 조건을 만족하는지 여부를 반환합니다.
        /// </summary>
        /// <returns>턴 시작 가능 여부</returns>
        bool CanStartTurn();
    }
}
