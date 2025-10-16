using Game.ItemSystem.Interface;
using Game.ItemSystem.Utility;
using Game.CoreSystem.Utility;

namespace Game.ItemSystem.Effect
{
    /// <summary>
    /// 아이템 효과 커맨드의 기본 구현
    /// 중복된 유효성 검사 로직을 통합하여 코드 중복을 제거합니다.
    /// </summary>
    public abstract class BaseItemEffectCommand : IItemEffectCommand
    {
        protected readonly string operationName;

        /// <summary>
        /// 기본 생성자
        /// </summary>
        /// <param name="operationName">작업 이름 (로깅용)</param>
        protected BaseItemEffectCommand(string operationName)
        {
            this.operationName = operationName ?? "알 수 없는 작업";
        }

        /// <summary>
        /// 효과를 실행합니다.
        /// </summary>
        /// <param name="context">사용 컨텍스트</param>
        /// <returns>실행 성공 여부</returns>
        public bool Execute(IItemUseContext context)
        {
            if (!ItemEffectValidator.ValidateUser(context, operationName))
                return false;

            try
            {
                return ExecuteInternal(context);
            }
            catch (System.Exception ex)
            {
                GameLogger.LogError($"[{operationName}] 효과 실행 중 오류 발생: {ex.Message}", GameLogger.LogCategory.Core);
                return false;
            }
        }

        /// <summary>
        /// 실제 효과 실행 로직
        /// </summary>
        /// <param name="context">사용 컨텍스트</param>
        /// <returns>실행 성공 여부</returns>
        protected abstract bool ExecuteInternal(IItemUseContext context);

        /// <summary>
        /// 효과 실행 전 추가 유효성 검사
        /// </summary>
        /// <param name="context">사용 컨텍스트</param>
        /// <returns>유효성 여부</returns>
        protected virtual bool ValidateAdditionalConditions(IItemUseContext context)
        {
            return true;
        }

        /// <summary>
        /// 효과 실행 후 처리
        /// </summary>
        /// <param name="context">사용 컨텍스트</param>
        /// <param name="success">실행 성공 여부</param>
        protected virtual void OnEffectExecuted(IItemUseContext context, bool success)
        {
            if (success)
            {
                GameLogger.LogInfo($"[{operationName}] 효과 실행 완료", GameLogger.LogCategory.Core);
            }
            else
            {
                GameLogger.LogWarning($"[{operationName}] 효과 실행 실패", GameLogger.LogCategory.Core);
            }
        }
    }
}
