using System;
using System.Collections.Generic;
using UnityEngine;
using Game.CoreSystem.Utility;
using Game.CoreSystem.Interface;
using Game.ItemSystem.Data;
using Game.ItemSystem.Effect;
using Game.ItemSystem.Interface;
using Game.ItemSystem.Runtime;
using Game.CharacterSystem.Interface;
using Zenject;

namespace Game.ItemSystem.Service
{
    /// <summary>
    /// 아이템 시스템의 핵심 서비스 구현체입니다.
    /// 액티브/패시브 아이템 관리, 사용, 상태 관리를 담당합니다.
    /// </summary>
    public class ItemService : MonoBehaviour, IItemService
    {
        #region 상수

        private const int ACTIVE_SLOT_COUNT = 4;

        #endregion

        #region 필드


        // 액티브 아이템 슬롯 관리
        private ActiveItemSlotData[] activeSlots = new ActiveItemSlotData[ACTIVE_SLOT_COUNT];

        // 패시브 아이템 관리 (성급 시스템)
        private Dictionary<string, int> skillStarRanks = new Dictionary<string, int>();
        private Dictionary<string, PassiveItemDefinition> passiveItemDefinitions = new Dictionary<string, PassiveItemDefinition>();

        // 의존성 주입
        [Inject] private ICharacter playerCharacter;
        [Inject] private IAudioManager audioManager;

        #endregion

        #region 이벤트

        public event Action<ActiveItemDefinition, int> OnActiveItemUsed;
        public event Action<string, int> OnSkillStarUpgraded;
        public event Action<ActiveItemDefinition, int> OnActiveItemAdded;
        public event Action<int> OnActiveItemRemoved;

        #endregion

        #region Unity 생명주기

        private void Awake()
        {
            InitializeActiveSlots();
        }

        #endregion

        #region 초기화

        /// <summary>
        /// 액티브 슬롯들을 초기화합니다.
        /// </summary>
        private void InitializeActiveSlots()
        {
            for (int i = 0; i < ACTIVE_SLOT_COUNT; i++)
            {
                activeSlots[i] = new ActiveItemSlotData();
            }
        }

        #endregion

        #region IItemService 구현

        /// <summary>
        /// 액티브 아이템을 사용합니다.
        /// </summary>
        /// <param name="slotIndex">슬롯 인덱스</param>
        /// <returns>사용 성공 여부</returns>
        public bool UseActiveItem(int slotIndex)
        {
            if (!IsValidSlotIndex(slotIndex))
            {
                GameLogger.LogError($"잘못된 슬롯 인덱스: {slotIndex}", GameLogger.LogCategory.Core);
                return false;
            }

            var slot = activeSlots[slotIndex];
            if (slot.isEmpty || slot.item == null)
            {
                GameLogger.LogWarning($"슬롯 {slotIndex}이 비어있습니다", GameLogger.LogCategory.Core);
                return false;
            }

            // 아이템 런타임 인스턴스 생성 및 사용
            var activeItem = new ActiveItem(slot.item, audioManager);
            bool success = activeItem.UseItem(playerCharacter, playerCharacter);

            if (success)
            {
                // 소모품인 경우 슬롯에서 제거
                if (slot.item.Type == ItemType.Active)
                {
                    RemoveActiveItem(slotIndex);
                }

                OnActiveItemUsed?.Invoke(slot.item, slotIndex);
                GameLogger.LogInfo($"아이템 사용 성공: {slot.item.DisplayName}", GameLogger.LogCategory.Core);
            }
            else
            {
                GameLogger.LogError($"아이템 사용 실패: {slot.item.DisplayName}", GameLogger.LogCategory.Core);
            }

            return success;
        }

        /// <summary>
        /// 액티브 아이템을 슬롯에 추가합니다.
        /// </summary>
        /// <param name="itemDefinition">아이템 정의</param>
        /// <returns>추가 성공 여부</returns>
        public bool AddActiveItem(ActiveItemDefinition itemDefinition)
        {
            if (itemDefinition == null)
            {
                GameLogger.LogError("아이템 정의가 null입니다", GameLogger.LogCategory.Core);
                return false;
            }

            // 빈 슬롯 찾기
            int emptySlotIndex = FindEmptySlot();
            if (emptySlotIndex == -1)
            {
                GameLogger.LogWarning("모든 슬롯이 가득 찼습니다", GameLogger.LogCategory.Core);
                return false;
            }

            // 슬롯에 아이템 추가
            activeSlots[emptySlotIndex].item = itemDefinition;
            activeSlots[emptySlotIndex].isEmpty = false;

            OnActiveItemAdded?.Invoke(itemDefinition, emptySlotIndex);
            GameLogger.LogInfo($"아이템 추가됨: {itemDefinition.DisplayName} (슬롯 {emptySlotIndex})", GameLogger.LogCategory.Core);

            return true;
        }

        /// <summary>
        /// 액티브 아이템을 슬롯에서 제거합니다.
        /// </summary>
        /// <param name="slotIndex">슬롯 인덱스</param>
        /// <returns>제거 성공 여부</returns>
        public bool RemoveActiveItem(int slotIndex)
        {
            if (!IsValidSlotIndex(slotIndex))
            {
                GameLogger.LogError($"잘못된 슬롯 인덱스: {slotIndex}", GameLogger.LogCategory.Core);
                return false;
            }

            var slot = activeSlots[slotIndex];
            if (slot.isEmpty)
            {
                GameLogger.LogWarning($"슬롯 {slotIndex}이 이미 비어있습니다", GameLogger.LogCategory.Core);
                return false;
            }

            slot.item = null;
            slot.isEmpty = true;

            OnActiveItemRemoved?.Invoke(slotIndex);
            GameLogger.LogInfo($"아이템 제거됨: 슬롯 {slotIndex}", GameLogger.LogCategory.Core);

            return true;
        }

        /// <summary>
        /// 액티브 아이템 슬롯 정보를 가져옵니다.
        /// </summary>
        /// <returns>슬롯 정보 배열</returns>
        public ActiveItemSlotData[] GetActiveSlots()
        {
            return activeSlots;
        }

        /// <summary>
        /// 액티브 인벤토리가 가득 찼는지 확인합니다.
        /// </summary>
        /// <returns>가득 참 여부</returns>
        public bool IsActiveInventoryFull()
        {
            return FindEmptySlot() == -1;
        }

        /// <summary>
        /// 패시브 아이템을 추가합니다 (성급 시스템).
        /// </summary>
        /// <param name="passiveItemDefinition">패시브 아이템 정의</param>
        public void AddPassiveItem(PassiveItemDefinition passiveItemDefinition)
        {
            if (passiveItemDefinition == null)
            {
                GameLogger.LogError("패시브 아이템 정의가 null입니다", GameLogger.LogCategory.Core);
                return;
            }

            string skillId = passiveItemDefinition.TargetSkillId;
            if (string.IsNullOrEmpty(skillId))
            {
                GameLogger.LogError("패시브 아이템의 대상 스킬 ID가 비어있습니다", GameLogger.LogCategory.Core);
                return;
            }

            // 성급 증가 (최대 3)
            if (!skillStarRanks.ContainsKey(skillId))
            {
                skillStarRanks[skillId] = 0;
            }

            if (skillStarRanks[skillId] < 3)
            {
                skillStarRanks[skillId]++;
                OnSkillStarUpgraded?.Invoke(skillId, skillStarRanks[skillId]);
                GameLogger.LogInfo($"스킬 성급 증가: {skillId} → ★{skillStarRanks[skillId]}", GameLogger.LogCategory.Core);
            }
            else
            {
                GameLogger.LogInfo($"스킬 {skillId}이 이미 최대 성급(★3)입니다", GameLogger.LogCategory.Core);
            }
        }

        /// <summary>
        /// 스킬의 데미지 보너스를 반환합니다.
        /// </summary>
        /// <param name="skillId">스킬 ID</param>
        /// <returns>데미지 보너스</returns>
        public int GetSkillDamageBonus(string skillId)
        {
            if (string.IsNullOrEmpty(skillId))
            {
                return 0;
            }

            int totalBonus = 0;

            // 성급 시스템 보너스
            if (skillStarRanks.ContainsKey(skillId))
            {
                int starRank = skillStarRanks[skillId];
                totalBonus += starRank; // 성급당 +1 데미지 (가산)
            }

            // 스킬별 직접 보너스
            foreach (var passiveItem in passiveItemDefinitions.Values)
            {
                if (passiveItem.IsSkillDamageBonus && 
                    passiveItem.TargetSkillId == skillId)
                {
                    totalBonus += passiveItem.SkillDamageBonus;
                }
            }

            return totalBonus;
        }

        /// <summary>
        /// 스킬의 성급을 반환합니다.
        /// </summary>
        /// <param name="skillId">스킬 ID</param>
        /// <returns>성급 (0-3)</returns>
        public int GetSkillStarRank(string skillId)
        {
            if (string.IsNullOrEmpty(skillId) || !skillStarRanks.ContainsKey(skillId))
            {
                return 0;
            }

            return skillStarRanks[skillId];
        }

        /// <summary>
        /// 모든 스킬의 성급 정보를 가져옵니다.
        /// </summary>
        /// <returns>스킬 ID → 성급 매핑</returns>
        public Dictionary<string, int> GetAllSkillStarRanks()
        {
            return new Dictionary<string, int>(skillStarRanks);
        }

        #endregion

        #region 유틸리티

        /// <summary>
        /// 슬롯 인덱스가 유효한지 확인합니다.
        /// </summary>
        /// <param name="slotIndex">슬롯 인덱스</param>
        /// <returns>유효성 여부</returns>
        private bool IsValidSlotIndex(int slotIndex)
        {
            return slotIndex >= 0 && slotIndex < ACTIVE_SLOT_COUNT;
        }

        /// <summary>
        /// 빈 슬롯을 찾습니다.
        /// </summary>
        /// <returns>빈 슬롯 인덱스 (-1이면 없음)</returns>
        private int FindEmptySlot()
        {
            for (int i = 0; i < ACTIVE_SLOT_COUNT; i++)
            {
                if (activeSlots[i].isEmpty)
                {
                    return i;
                }
            }
            return -1;
        }

        #endregion
    }
}