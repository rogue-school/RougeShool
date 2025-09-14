using Game.CombatSystem.Slot;

namespace Game.CombatSystem.Interface
{
    /// <summary>
    /// 전투 페이즈를 실행하는 서비스 인터페이스입니다.
    /// 4개 슬롯 시스템과 즉시 실행을 지원합니다.
    /// </summary>
    public interface ICombatExecutorService
    {
        /// <summary>
        /// 1번 슬롯에서만 카드를 실행합니다.
        /// 카드 사용 후 슬롯을 한 칸씩 이동시킵니다.
        /// </summary>
        /// <returns>전투 실행 흐름을 처리하는 코루틴</returns>
        System.Collections.IEnumerator ExecuteCardInSlot1();

        /// <summary>
        /// 즉시 실행 모드: 1번 슬롯에 카드 배치 시 즉시 실행
        /// </summary>
        void ExecuteImmediately();
    }
}
