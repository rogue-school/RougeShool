using System;
using System.Collections.Generic;
using Game.ItemSystem.Data;

namespace Game.ItemSystem.Interface
{
    /// <summary>
    /// 아이템 시스템의 핵심 서비스 인터페이스입니다.
    /// 액티브/패시브 아이템 관리, 사용, 상태 관리를 담당합니다.
    /// </summary>
    public interface IItemService
    {
        #region 액티브 아이템 관리

        /// <summary>
        /// 액티브 아이템을 사용합니다.
        /// </summary>
        /// <param name="slotIndex">사용할 슬롯 인덱스 (0-3)</param>
        /// <returns>사용 성공 여부</returns>
        bool UseActiveItem(int slotIndex);

        /// <summary>
        /// 액티브 아이템을 인벤토리에 추가합니다.
        /// </summary>
        /// <param name="item">추가할 아이템 정의</param>
        /// <returns>추가 성공 여부</returns>
        bool AddActiveItem(ActiveItemDefinition item);

        /// <summary>
        /// 액티브 아이템을 제거합니다.
        /// </summary>
        /// <param name="slotIndex">제거할 슬롯 인덱스</param>
        /// <returns>제거 성공 여부</returns>
        bool RemoveActiveItem(int slotIndex);

        /// <summary>
        /// 액티브 아이템 슬롯 정보를 가져옵니다.
        /// </summary>
        /// <returns>슬롯 정보 배열</returns>
        ActiveItemSlotData[] GetActiveSlots();

        /// <summary>
        /// 액티브 인벤토리가 가득 찼는지 확인합니다.
        /// </summary>
        /// <returns>가득 참 여부</returns>
        bool IsActiveInventoryFull();

        #endregion

        #region 패시브 아이템 관리 (성급 시스템)

        /// <summary>
        /// 패시브 아이템을 추가합니다 (성급 시스템).
        /// </summary>
        /// <param name="item">추가할 패시브 아이템 정의</param>
        void AddPassiveItem(PassiveItemDefinition item);

        /// <summary>
        /// 특정 스킬의 성급을 가져옵니다.
        /// </summary>
        /// <param name="skillId">스킬 ID</param>
        /// <returns>성급 (0-3)</returns>
        int GetSkillStarRank(string skillId);

        /// <summary>
        /// 특정 스킬의 데미지 보너스를 가져옵니다.
        /// </summary>
        /// <param name="skillId">스킬 ID</param>
        /// <returns>데미지 보너스</returns>
        int GetSkillDamageBonus(string skillId);

        /// <summary>
        /// 모든 스킬의 성급 정보를 가져옵니다.
        /// </summary>
        /// <returns>스킬 ID → 성급 매핑</returns>
        Dictionary<string, int> GetAllSkillStarRanks();

        /// <summary>
        /// 새 게임을 위한 인벤토리 초기화
        /// </summary>
        void ResetInventoryForNewGame();

        #endregion

        #region 이벤트

        /// <summary>
        /// 액티브 아이템 사용 이벤트
        /// </summary>
        event Action<ActiveItemDefinition, int> OnActiveItemUsed;

        /// <summary>
        /// 스킬 성급 업그레이드 이벤트
        /// </summary>
        event Action<string, int> OnSkillStarUpgraded;

        /// <summary>
        /// 액티브 아이템 추가 이벤트
        /// </summary>
        event Action<ActiveItemDefinition, int> OnActiveItemAdded;

        /// <summary>
        /// 액티브 아이템 제거 이벤트
        /// </summary>
        event Action<int> OnActiveItemRemoved;

        #endregion
    }

    /// <summary>
    /// 액티브 아이템 슬롯 데이터 구조입니다.
    /// </summary>
    [System.Serializable]
    public class ActiveItemSlotData
    {
        /// <summary>
        /// 현재 아이템 정의
        /// </summary>
        public ActiveItemDefinition item;

        /// <summary>
        /// 슬롯이 비어있는지 여부
        /// </summary>
        public bool isEmpty;

        public ActiveItemSlotData()
        {
            item = null;
            isEmpty = true;
        }

        public ActiveItemSlotData(ActiveItemDefinition itemDefinition)
        {
            item = itemDefinition;
            isEmpty = itemDefinition == null;
        }
    }
}
