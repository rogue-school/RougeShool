using System.Collections;

namespace Game.CombatSystem.Interface
{
    /// <summary>
    /// 전투 초기화 단계에서 실행될 초기화 스텝 인터페이스입니다.
    /// 여러 초기화 스텝이 순차적으로 실행되며, Order 값이 낮을수록 먼저 호출됩니다.
    /// </summary>
    public interface ICombatInitializerStep
    {
        /// <summary>
        /// 실행 순서를 지정합니다. 숫자가 낮을수록 먼저 실행됩니다.
        /// </summary>
        int Order { get; }

        /// <summary>
        /// 초기화를 수행하는 코루틴입니다.
        /// 예: 슬롯 배치, 카드 덱 준비, 캐릭터 스폰 등
        /// </summary>
        IEnumerator Initialize();
    }
}
