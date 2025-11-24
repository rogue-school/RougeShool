using System.Collections.Generic;
using Game.Domain.Item.ValueObjects;

namespace Game.Domain.Item.Interfaces
{
    /// <summary>
    /// 아이템 사용 및 강화 정보를 제공하는 도메인 서비스 인터페이스입니다.
    /// </summary>
    public interface IItemService
    {
        /// <summary>
        /// 액티브 아이템을 사용할 수 있는지 여부를 검사합니다.
        /// </summary>
        /// <param name="slotIndex">슬롯 인덱스 (0 이상)</param>
        /// <returns>사용 가능 여부</returns>
        bool CanUseActiveItem(int slotIndex);

        /// <summary>
        /// 특정 스킬 ID에 대한 강화 단계를 반환합니다.
        /// </summary>
        /// <param name="skillId">스킬 ID</param>
        /// <returns>강화 단계</returns>
        int GetEnhancementLevel(string skillId);

        /// <summary>
        /// 특정 스킬 ID에 대한 데미지 보너스를 반환합니다.
        /// </summary>
        /// <param name="skillId">스킬 ID</param>
        /// <returns>데미지 보너스</returns>
        int GetDamageBonus(string skillId);

        /// <summary>
        /// 모든 스킬의 강화 단계 정보를 반환합니다.
        /// </summary>
        /// <returns>스킬 ID → 강화 단계 매핑</returns>
        IReadOnlyDictionary<string, int> GetAllEnhancementLevels();
    }
}


