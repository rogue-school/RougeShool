namespace Game.CombatSystem.Interface
{
    /// <summary>
    /// 턴을 시작할 수 있는 조건이 충족되었는지 판단하는 인터페이스입니다.
    /// 예: 플레이어와 적 카드가 모두 준비되었는지 여부 등
    /// </summary>
    public interface ITurnStartConditionChecker
    {
        bool CanStartTurn();
    }
}
