using System;
using System.Collections;

namespace Game.CombatSystem.Interface
{
    /// <summary>
    /// 전투 시작 전 준비 과정을 수행하는 서비스 인터페이스입니다.
    /// 캐릭터 배치, 슬롯 초기화, UI 설정 등 모든 준비 단계를 포함합니다.
    /// </summary>
    public interface ICombatPreparationService
    {
        /// <summary>
        /// 전투 준비 코루틴을 실행합니다.
        /// 완료되면 onComplete 콜백을 통해 성공 여부를 반환합니다.
        /// </summary>
        /// <param name="onComplete">전투 준비 완료 시 호출될 콜백 (true = 성공, false = 실패)</param>
        IEnumerator PrepareCombat(Action<bool> onComplete);
    }
}
