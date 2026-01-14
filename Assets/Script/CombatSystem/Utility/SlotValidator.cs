using Game.CombatSystem.Data;
using Game.CombatSystem.Slot;
using Game.CombatSystem.Manager;
using Game.CombatSystem.Interface;
using Game.SkillCardSystem.Interface;
using Game.CoreSystem.Utility;
using Zenject;

namespace Game.CombatSystem.Utility
{
    /// <summary>
    /// 싱글게임용 슬롯 검증 유틸리티 (DI 기반)
    /// 카드 배치 가능 여부를 검증합니다.
    /// </summary>
    public class SlotValidator
    {
        // CombatSlotManager 제거됨 - 슬롯 검증 기능을 직접 구현
        
        /// <summary>
        /// 슬롯에 카드 배치 가능 여부를 검증합니다.
        /// </summary>
        /// <param name="position">슬롯 위치</param>
        /// <param name="card">배치할 카드</param>
        /// <returns>배치 가능하면 true</returns>
        public bool CanPlaceCard(CombatSlotPosition position, ISkillCard card)
        {
            // CombatSlotManager 제거로 인한 단순화된 검증
            // 실제 슬롯 검증은 다른 시스템에서 처리
            
            // 1. 기본 검증
            if (card == null)
            {
                GameLogger.LogWarning("카드가 null입니다", GameLogger.LogCategory.Combat);
                return false;
            }

            // 3. 카드 소유자 확인 (슬롯 소유자 검증은 임시로 제거)
            // TODO: 슬롯 소유자 검증 로직 추가 필요
            var cardOwner = card.GetOwner();
            GameLogger.LogInfo($"카드 소유자: {cardOwner}", GameLogger.LogCategory.Combat);

            // 4. 턴 순서 확인
            if (!IsValidTurnOrder(position, card.GetOwner()))
            {
                GameLogger.LogWarning($"잘못된 턴 순서: {card.GetOwner()}가 {position}에 카드 배치 시도", GameLogger.LogCategory.Combat);
                return false;
            }

            return true;
        }

        /// <summary>
        /// 턴 순서가 올바른지 확인합니다.
        /// </summary>
        /// <param name="position">슬롯 위치</param>
        /// <param name="cardOwner">카드 소유자</param>
        /// <returns>올바른 턴 순서면 true</returns>
        private static bool IsValidTurnOrder(CombatSlotPosition position, SlotOwner cardOwner)
        {
            // 플레이어는 특정 슬롯에만 배치 가능
            if (cardOwner == SlotOwner.PLAYER)
            {
                return position == CombatSlotPosition.BATTLE_SLOT ||
                       position == CombatSlotPosition.WAIT_SLOT_2 ||
                       position == CombatSlotPosition.WAIT_SLOT_4;
            }

            // 적은 특정 슬롯에만 배치 가능
            if (cardOwner == SlotOwner.ENEMY)
            {
                return position == CombatSlotPosition.WAIT_SLOT_1 ||
                       position == CombatSlotPosition.WAIT_SLOT_3;
            }

            return false;
        }

        /// <summary>
        /// 슬롯이 플레이어 슬롯인지 확인합니다.
        /// </summary>
        /// <param name="position">슬롯 위치</param>
        /// <returns>플레이어 슬롯이면 true</returns>
        public static bool IsPlayerSlot(CombatSlotPosition position)
        {
            return position == CombatSlotPosition.BATTLE_SLOT ||
                   position == CombatSlotPosition.WAIT_SLOT_2 ||
                   position == CombatSlotPosition.WAIT_SLOT_4;
        }

        /// <summary>
        /// 슬롯이 적 슬롯인지 확인합니다.
        /// </summary>
        /// <param name="position">슬롯 위치</param>
        /// <returns>적 슬롯이면 true</returns>
        public static bool IsEnemySlot(CombatSlotPosition position)
        {
            return position == CombatSlotPosition.WAIT_SLOT_1 ||
                   position == CombatSlotPosition.WAIT_SLOT_3;
        }

        /// <summary>
        /// 슬롯 위치에 따른 소유자를 반환합니다.
        /// </summary>
        /// <param name="position">슬롯 위치</param>
        /// <returns>슬롯 소유자</returns>
        public static SlotOwner GetSlotOwner(CombatSlotPosition position)
        {
            return position switch
            {
                CombatSlotPosition.BATTLE_SLOT => SlotOwner.PLAYER,    // 플레이어가 먼저 시작
                CombatSlotPosition.WAIT_SLOT_1 => SlotOwner.ENEMY,     // 적
                CombatSlotPosition.WAIT_SLOT_2 => SlotOwner.PLAYER,    // 플레이어
                CombatSlotPosition.WAIT_SLOT_3 => SlotOwner.ENEMY,      // 적
                CombatSlotPosition.WAIT_SLOT_4 => SlotOwner.PLAYER,     // 플레이어
                _ => SlotOwner.PLAYER
            };
        }

        /// <summary>
        /// 카드가 슬롯에 배치 가능한지 상세 검증합니다.
        /// </summary>
        /// <param name="position">슬롯 위치</param>
        /// <param name="card">배치할 카드</param>
        /// <param name="errorMessage">오류 메시지 (출력 매개변수)</param>
        /// <returns>배치 가능하면 true</returns>
        public bool ValidateCardPlacement(CombatSlotPosition position, ISkillCard card, out string errorMessage)
        {
            errorMessage = string.Empty;

            // CombatSlotManager 제거로 인한 단순화된 검증
            // 실제 슬롯 검증은 다른 시스템에서 처리

            // CombatSlotManager 제거로 인한 단순화된 검증
            // 항상 배치 가능하도록 설정

            // 3. 카드 소유자 확인 (슬롯 소유자 검증은 임시로 제거)
            // TODO: 슬롯 소유자 검증 로직 추가 필요
            var cardOwner = card.GetOwner();
            GameLogger.LogInfo($"카드 소유자: {cardOwner}", GameLogger.LogCategory.Combat);

            // 4. 턴 순서 확인
            if (!IsValidTurnOrder(position, card.GetOwner()))
            {
                errorMessage = $"잘못된 턴 순서: {card.GetOwner()}가 {position}에 카드 배치 시도";
                return false;
            }

            return true;
        }
    }
}
