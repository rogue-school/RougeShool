using System;
using Game.Domain.Stage.Entities;
using Game.Domain.Stage.Interfaces;

namespace Game.Application.Stage
{
    /// <summary>
    /// 스테이지를 실패 상태로 전환하는 애플리케이션 유스케이스입니다.
    /// </summary>
    public sealed class FailStageUseCase
    {
        /// <summary>
        /// 스테이지를 실패 상태로 표시합니다.
        /// </summary>
        /// <param name="stage">실패 처리할 스테이지 도메인 객체</param>
        /// <exception cref="ArgumentNullException">stage가 null인 경우</exception>
        /// <exception cref="InvalidOperationException">
        /// - 스테이지 구현이 실패 처리를 지원하지 않는 경우
        /// </exception>
        public void Execute(IStage stage)
        {
            if (stage == null)
            {
                throw new ArgumentNullException(nameof(stage), "스테이지는 null일 수 없습니다.");
            }

            if (stage is Game.Domain.Stage.Entities.Stage concreteStage)
            {
                concreteStage.MarkFailed();
            }
            else
            {
                throw new InvalidOperationException("해당 스테이지 구현은 실패 처리를 지원하지 않습니다.");
            }
        }
    }
}


