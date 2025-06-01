using System.Collections;

namespace Game.CombatSystem.Interface
{
    /// <summary>
    /// 전투 초기화 단계 인터페이스입니다.
    /// 각 초기화 스텝은 Order 기준으로 자동 실행됩니다.
    /// </summary>
    public interface ICombatInitializerStep
    {
        /// <summary>
        /// 실행 순서를 결정합니다. 낮은 숫자가 먼저 실행됩니다.
        /// </summary>
        int Order { get; }

        /// <summary>
        /// 초기화 코루틴입니다.
        /// </summary>
        IEnumerator Initialize();
    }
}
