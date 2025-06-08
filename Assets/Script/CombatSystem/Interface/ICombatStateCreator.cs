namespace Game.CombatSystem.Interface
{
    /// <summary>
    /// 전투 턴 상태를 생성하는 팩토리 인터페이스입니다.
    /// 각 전투 상태(예: 카드 입력, 공격 실행 등)의 구체적인 인스턴스를 생성하는 데 사용됩니다.
    /// </summary>
    public interface ICombatStateCreator
    {
        /// <summary>
        /// 전투 턴 상태 인스턴스를 생성합니다.
        /// </summary>
        /// <returns>생성된 ICombatTurnState 인스턴스</returns>
        ICombatTurnState Create();
    }
}
