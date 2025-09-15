using UnityEngine;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.UI;
using Game.CombatSystem.Interface;
using Game.CombatSystem.Data;
using Game.CombatSystem.Slot;
using Game.CombatSystem.Utility;

namespace Game.CombatSystem.Service
{
    /// <summary>
    /// 카드 드롭 처리 서비스.
    /// 슬롯 드롭 유효성 검사, 기존 카드 교체, 전투 레지스트리 등록을 담당합니다.
    /// </summary>
    public class CardDropService
    {
        private readonly ICardDropValidator validator;
        private readonly ICardRegistrar registrar;
        private readonly ICombatTurnManager turnManager;
        private readonly ICardReplacementHandler replacementHandler;
        private readonly IPlayerHandManager playerHandManager;

        /// <summary>
        /// 생성자. 필요한 의존성을 주입합니다.
        /// </summary>
        public CardDropService(
            ICardDropValidator validator,
            ICardRegistrar registrar,
            ICombatTurnManager turnManager,
            ICardReplacementHandler replacementHandler,
            IPlayerHandManager playerHandManager)
        {
            this.validator = validator;
            this.registrar = registrar;
            this.turnManager = turnManager;
            this.replacementHandler = replacementHandler;
            this.playerHandManager = playerHandManager;
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
        public bool TryDropCard(ISkillCard card, SkillCardUI ui, ICombatCardSlot slot, out string message)
        {
            message = "";

            // 1. 플레이어 입력 턴 여부 확인
            if (!turnManager.IsPlayerInputTurn())
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

            // 4. 드롭 유효성 검사
            if (!validator.IsValidDrop(card, slot, out message))
            {
                Debug.LogWarning($"[CardDropService] 드롭 유효성 실패: {message}");
                return false;
            }

            // 5. 슬롯 카드 교체 처리
            replacementHandler.ReplaceSlotCard(slot, card, ui);

            // 6. 레지스트리에 등록 (슬롯의 논리 포지션을 직접 사용)
            var execSlot = slot.Position;
            turnManager.RegisterCard(execSlot, card, ui, SlotOwner.PLAYER);

            return true;
        }
    }
}
