using Game.ItemSystem.Interface;
using Game.CoreSystem.Utility;

namespace Game.ItemSystem.Utility
{
    /// <summary>
    /// 아이템 효과 유효성 검사 유틸리티 클래스
    /// 중복된 유효성 검사 로직을 통합하여 코드 중복을 제거합니다.
    /// </summary>
    public static class ItemEffectValidator
    {
        /// <summary>
        /// 사용자 유효성을 검사합니다.
        /// </summary>
        /// <param name="context">아이템 사용 컨텍스트</param>
        /// <param name="operationName">작업 이름 (로깅용)</param>
        /// <returns>유효성 여부</returns>
        public static bool ValidateUser(IItemUseContext context, string operationName)
        {
            if (context?.User == null || context.User.IsDead())
            {
                GameLogger.LogError($"{operationName} 실패: 사용자가 null이거나 사망 상태입니다", GameLogger.LogCategory.Core);
                return false;
            }
            return true;
        }

        /// <summary>
        /// 아이템 정의 유효성을 검사합니다.
        /// </summary>
        /// <param name="itemDefinition">아이템 정의</param>
        /// <param name="operationName">작업 이름</param>
        /// <returns>유효성 여부</returns>
        public static bool ValidateItemDefinition(Data.ItemDefinition itemDefinition, string operationName)
        {
            if (itemDefinition == null)
            {
                GameLogger.LogError($"{operationName} 실패: 아이템 정의가 null입니다", GameLogger.LogCategory.Core);
                return false;
            }
            return true;
        }

        /// <summary>
        /// 슬롯 인덱스 유효성을 검사합니다.
        /// </summary>
        /// <param name="slotIndex">슬롯 인덱스</param>
        /// <param name="maxSlots">최대 슬롯 수</param>
        /// <param name="operationName">작업 이름</param>
        /// <returns>유효성 여부</returns>
        public static bool ValidateSlotIndex(int slotIndex, int maxSlots, string operationName)
        {
            if (slotIndex < 0 || slotIndex >= maxSlots)
            {
                GameLogger.LogError($"{operationName} 실패: 잘못된 슬롯 인덱스 {slotIndex} (범위: 0-{maxSlots - 1})", GameLogger.LogCategory.Core);
                return false;
            }
            return true;
        }

        /// <summary>
        /// 컬렉션 유효성을 검사합니다.
        /// </summary>
        /// <param name="collection">검사할 컬렉션</param>
        /// <param name="operationName">작업 이름</param>
        /// <returns>유효성 여부</returns>
        public static bool ValidateCollection<T>(System.Collections.Generic.ICollection<T> collection, string operationName)
        {
            if (collection == null)
            {
                GameLogger.LogError($"{operationName} 실패: 컬렉션이 null입니다", GameLogger.LogCategory.Core);
                return false;
            }

            if (collection.Count == 0)
            {
                GameLogger.LogWarning($"{operationName}: 컬렉션이 비어있습니다", GameLogger.LogCategory.Core);
                return false;
            }

            return true;
        }
    }
}
