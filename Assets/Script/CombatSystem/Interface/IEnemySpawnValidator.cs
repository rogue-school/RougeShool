namespace Game.CombatSystem.Interface
{
    /// <summary>
    /// 적 캐릭터가 현재 상황에서 스폰 가능한지를 판단하는 유효성 검사 인터페이스입니다.
    /// </summary>
    public interface IEnemySpawnValidator
    {
        /// <summary>
        /// 적 캐릭터가 현재 전투 상황에서 스폰 가능한지를 반환합니다.
        /// 예: 슬롯이 비어 있는지, 이미 적이 존재하는지 등을 검사합니다.
        /// </summary>
        /// <returns>스폰 가능하면 true, 불가능하면 false</returns>
        bool CanSpawnEnemy();
    }
}
