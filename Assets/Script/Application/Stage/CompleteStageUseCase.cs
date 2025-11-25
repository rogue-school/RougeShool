using System;
using Game.Domain.Stage.Interfaces;
using Game.Domain.Stage.ValueObjects;

namespace Game.Application.Stage
{
    /// <summary>
    /// 스테이지를 완료 상태로 검증하는 애플리케이션 유스케이스입니다.
    /// 도메인 스테이지가 내부적으로 Completed 상태로 전환되었는지 확인하는 용도로 사용합니다.
    /// </summary>
    public sealed class CompleteStageUseCase
    {
        /// <summary>
        /// 스테이지가 완료 상태인지 확인합니다.
        /// </summary>
        /// <param name="stage">검증할 스테이지 도메인 객체</param>
        /// <exception cref="ArgumentNullException">stage가 null인 경우</exception>
        /// <exception cref="InvalidOperationException">스테이지가 Completed 상태가 아닌 경우</exception>
        public void Execute(IStage stage)
        {
            if (stage == null)
            {
                throw new ArgumentNullException(nameof(stage), "스테이지는 null일 수 없습니다.");
            }

            if (stage.ProgressState != StageProgressState.Completed)
            {
                throw new InvalidOperationException("스테이지가 완료 상태가 아닙니다.");
            }
        }
    }
}


