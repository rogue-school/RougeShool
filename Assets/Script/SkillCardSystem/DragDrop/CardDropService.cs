using UnityEngine;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.UI;
using Game.CombatSystem.Interface;
using Game.CombatSystem.Data;
using Game.CombatSystem.Slot;
using Game.CombatSystem.Utility;
using Game.CombatSystem.Manager;
using Game.CombatSystem.DragDrop;

namespace Game.CombatSystem.Service
{
    /// <summary>
    /// 카드 드롭 처리 서비스.
    /// 슬롯 드롭 유효성 검사, 기존 카드 교체, 전투 레지스트리 등록을 담당합니다.
    /// </summary>
    public class CardDropService
    {
        private readonly ICardValidator validator;
        private readonly CardDropRegistrar registrar;
        private readonly TurnManager turnManager;

        /// <summary>
        /// 생성자. 필요한 의존성을 주입합니다.
        /// </summary>
        public CardDropService(
            ICardValidator validator,
            CardDropRegistrar registrar,
            TurnManager turnManager)
        {
            this.validator = validator;
            this.registrar = registrar;
            this.turnManager = turnManager;
        }

        /// <summary>
        /// 지정된 슬롯에 카드 드롭을 시도합니다.
        /// 조건이 충족되면 카드 UI와 데이터를 슬롯에 배치하고 레지스트리에 등록합니다.
        /// </summary>
        /// <param name="card">드롭할 카드</param>
        /// <param name="ui">해당 카드 UI</param>
        /// <param name="slot">드롭 대상 슬롯</param>
        /// <param name="message">실패 시 원인 메시지</param>
        /// <returns>드롭 성공 여부</returns>
        public bool TryDropCard(ISkillCard card, SkillCardUI ui, CombatSlot slot, out string message)
        {
            message = "";

            // 1. 플레이어 입력 턴 여부 확인
            if (!turnManager.IsPlayerTurn())
            {
                message = "플레이어 입력 턴이 아닙니다.";
                Debug.LogWarning($"[CardDropService] {message}");
                return false;
            }

            // 2. 카드 유효성 확인
            if (card == null)
            {
                message = "카드가 null입니다.";
                Debug.LogWarning($"[CardDropService] {message}");
                return false;
            }

            // 3. 쿨타임 검사는 기획상 사용하지 않음 (항상 통과)

            // 4. 드롭 유효성 검사 (새로운 아키텍처에서는 단순화)
            if (!CanDropCard(card, slot))
            {
                message = "카드 드롭 조건을 만족하지 않습니다.";
                Debug.LogWarning($"[CardDropService] 드롭 유효성 실패: {message}");
                return false;
            }

            // 5. 슬롯에 카드 배치
            if (!slot.TryPlaceCard(card))
            {
                message = "슬롯에 카드 배치 실패";
                Debug.LogWarning($"[CardDropService] {message}");
                return false;
            }

            // 6. 새로운 아키텍처에서는 별도의 레지스트리 등록 불필요
            // CombatSlotManager가 슬롯 상태를 직접 관리

            return true;
        }

        /// <summary>
        /// 카드 드롭 가능 여부를 확인합니다.
        /// </summary>
        /// <param name="card">드롭할 카드</param>
        /// <param name="slot">대상 슬롯</param>
        /// <returns>드롭 가능하면 true</returns>
        private bool CanDropCard(ISkillCard card, CombatSlot slot)
        {
            // 슬롯이 비어있고, 카드 소유자가 슬롯 소유자와 일치하는지 확인
            return slot.IsEmpty() && card.GetOwner() == slot.Owner;
        }
    }
}
