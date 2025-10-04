using System.Collections.Generic;
using Game.CombatSystem.Interface;
using Game.CombatSystem.Utility;
using System.Linq;
using Game.CoreSystem.Utility;

namespace Game.CombatSystem.Slot
{
    /// <summary>
    /// 전투 슬롯을 등록하고 슬롯 위치에 따라 조회할 수 있도록 관리하는 레지스트리입니다.
    /// DI 컨테이너에서 자동 생성됩니다.
    /// </summary>
    public class CombatSlotRegistry
    {
        #region 필드

        private readonly Dictionary<CombatSlotPosition, ICombatCardSlot> _slotByPosition = new();
        private readonly List<ICombatCardSlot> _allSlots = new();

        private bool _isInitialized = false;

        /// <summary>
        /// 슬롯 레지스트리가 초기화되었는지 여부
        /// </summary>
        public bool IsInitialized => _isInitialized;

        #endregion

        #region 슬롯 등록

        /// <summary>
        /// 슬롯을 등록하여 내부 데이터에 저장합니다.
        /// </summary>
        /// <param name="slots">등록할 전투 카드 슬롯 목록</param>
        public void RegisterCombatSlots(IEnumerable<ICombatCardSlot> slots)
        {
            _slotByPosition.Clear();
            
            _allSlots.Clear();

            int registeredCount = 0;

            foreach (var slot in slots)
            {
                // 슬롯 포지션은 슬롯 자체의 Position 속성을 단일 진실로 사용
                var pos = slot.Position;

                if (_slotByPosition.ContainsKey(pos))
                {
                    GameLogger.LogError($"[CombatSlotRegistry] 중복된 CombatSlotPosition: {pos}", GameLogger.LogCategory.Combat);
                    continue;
                }

                _slotByPosition.Add(pos, slot);
                _allSlots.Add(slot);
                registeredCount++;

                // 슬롯 등록 로그 추가
                if (slot is UnityEngine.MonoBehaviour mb)
                {
                    GameLogger.LogInfo($"[CombatSlotRegistry] 슬롯 등록: {pos} (GameObject: {mb.gameObject.name})", GameLogger.LogCategory.Combat);
                }
            }

            _isInitialized = true;
            GameLogger.LogInfo($"[CombatSlotRegistry] 총 {registeredCount}개 슬롯 등록 완료", GameLogger.LogCategory.Combat);

        }

        #endregion

        #region 슬롯 조회

        /// <summary>
        /// 실행 슬롯 위치를 기준으로 슬롯을 조회합니다.
        /// </summary>
        public ICombatCardSlot GetCombatSlot(CombatSlotPosition position)
        {
            _slotByPosition.TryGetValue(position, out var slot);
            return slot;
        }

        /// <summary>
        /// 필드 슬롯 위치를 기준으로 슬롯을 조회합니다.
        /// </summary>
        public ICombatCardSlot GetCombatSlot(CombatFieldSlotPosition fieldPosition)
        {
            GameLogger.LogWarning("[CombatSlotRegistry] 필드 포지션 기반 조회는 비권장입니다. SLOT_1..SLOT_4를 사용하세요.", GameLogger.LogCategory.Combat);
            return null;
        }

        /// <summary>
        /// 모든 전투 슬롯을 반환합니다.
        /// </summary>
        public IEnumerable<ICombatCardSlot> GetAllCombatSlots() => _allSlots;

        /// <summary>
        /// 기존 메서드와 호환을 위한 별칭
        /// </summary>
        public ICombatCardSlot GetSlotByPosition(CombatSlotPosition position) => GetCombatSlot(position);

        #endregion
    }
}
