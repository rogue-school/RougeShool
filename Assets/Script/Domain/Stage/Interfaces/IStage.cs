using System.Collections.Generic;
using Game.Domain.Stage.ValueObjects;

namespace Game.Domain.Stage.Interfaces
{
    /// <summary>
    /// 도메인 레벨에서의 스테이지 인터페이스입니다.
    /// </summary>
    public interface IStage
    {
        /// <summary>
        /// 스테이지 정의입니다.
        /// </summary>
        StageDefinition Definition { get; }

        /// <summary>
        /// 현재 진행 상태입니다.
        /// </summary>
        StageProgressState ProgressState { get; }

        /// <summary>
        /// 다음 적이 남아 있는지 여부입니다.
        /// </summary>
        bool HasNextEnemy();

        /// <summary>
        /// 다음 적의 ID를 미리 봅니다.
        /// </summary>
        /// <returns>다음 적 ID, 없으면 null</returns>
        string PeekNextEnemyId();

        /// <summary>
        /// 다음 적으로 진행합니다.
        /// </summary>
        void AdvanceToNextEnemy();
    }
}


