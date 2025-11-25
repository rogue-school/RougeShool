using System;
using Game.Domain.Stage.Interfaces;
using Game.Domain.Stage.ValueObjects;

namespace Game.Application.Stage
{
    /// <summary>
    /// 스테이지를 시작하고 첫 번째 적으로 진행하는 애플리케이션 유스케이스입니다.
    /// </summary>
    public sealed class StartStageUseCase
    {
        /// <summary>
        /// 스테이지를 시작하고 첫 번째 적 ID를 반환합니다.
        /// </summary>
        /// <param name="stage">시작할 스테이지 도메인 객체</param>
        /// <returns>첫 번째 적 ID</returns>
        /// <exception cref="ArgumentNullException">stage가 null인 경우</exception>
        /// <exception cref="InvalidOperationException">
        /// - 스테이지에 더 이상 적이 없는 경우
        /// - 이미 시작된 스테이지인 경우
        /// </exception>
        public string Execute(IStage stage)
        {
            if (stage == null)
            {
                throw new ArgumentNullException(nameof(stage), "스테이지는 null일 수 없습니다.");
            }

            if (stage.ProgressState != StageProgressState.NotStarted)
            {
                throw new InvalidOperationException("이미 시작된 스테이지입니다.");
            }

            if (!stage.HasNextEnemy())
            {
                throw new InvalidOperationException("스테이지에 진행할 적이 없습니다.");
            }

            string firstEnemyId = stage.PeekNextEnemyId();
            stage.AdvanceToNextEnemy();
            return firstEnemyId;
        }
    }
}


