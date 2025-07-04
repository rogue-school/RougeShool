namespace Game.CombatSystem.Interface
{
    /// <summary>
    /// 전투 페이즈를 실행하는 서비스 인터페이스입니다.
    /// 선공/후공 카드 실행, 효과 적용, 사망 처리 등을 포함한 전투 흐름 전체를 관리합니다.
    /// </summary>
    public interface ICombatExecutorService
    {
        /// <summary>
        /// 현재 턴에 등록된 플레이어와 적의 카드를 기반으로 전투 페이즈를 실행합니다.
        /// 선공 → 후공 순으로 각 슬롯의 카드를 실행하며, 카드 효과 적용, 이펙트, 
        /// 사망 판정, 상태 전이 등을 처리합니다.
        /// </summary>
        /// <remarks>
        /// 이 메서드는 일반적으로 코루틴으로 호출되어 비동기적으로 연출 및 효과 처리를 진행합니다.
        /// </remarks>
        /// <returns>전투 실행 흐름을 처리하는 코루틴</returns>
        System.Collections.IEnumerator ExecuteCombatPhase();
    }
}
