using System;
using Game.Domain.Stage.Interfaces;

namespace Game.Application.Stage
{
    /// <summary>
    /// 현재 스테이지에서 다음 적으로 진행하는 애플리케이션 유스케이스입니다.
    /// </summary>
    public sealed class AdvanceEnemyUseCase
    {
        /// <summary>
        /// 다음 적으로 진행하고, 진행한 적의 ID를 반환합니다.
        /// </summary>
        /// <param name="stage">진행할 스테이지 도메인 객체</param>
        /// <returns>진행된 적 ID</returns>
        /// <exception cref="ArgumentNullException">stage가 null인 경우</exception>
        /// <exception cref="InvalidOperationException">다음 적이 없는 경우</exception>
        public string Execute(IStage stage)
        {
            if (stage == null)
            {
                throw new ArgumentNullException(nameof(stage), "스테이지는 null일 수 없습니다.");
            }

            if (!stage.HasNextEnemy())
            {
                throw new InvalidOperationException("더 이상 진행할 적이 없습니다.");
            }

            string enemyId = stage.PeekNextEnemyId();
            stage.AdvanceToNextEnemy();
            return enemyId;
        }
    }
}


