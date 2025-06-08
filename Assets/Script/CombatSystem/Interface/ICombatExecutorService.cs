namespace Game.CombatSystem.Interface
{
    /// <summary>
    /// 전투 페이즈를 실행하는 서비스를 정의하는 인터페이스입니다.
    /// 플레이어/적 카드 실행을 포함한 전투 흐름을 처리합니다.
    /// </summary>
    public interface ICombatExecutorService
    {
        /// <summary>
        /// 등록된 플레이어/적 카드에 따라 전투 페이즈를 실행합니다.
        /// 이 코루틴은 카드 효과 적용, 애니메이션 재생, 사망 처리 등을 포함할 수 있습니다.
        /// </summary>
        /// <returns>전투 실행 처리 코루틴</returns>
        System.Collections.IEnumerator ExecuteCombatPhase();
    }
}
