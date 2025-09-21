using UnityEngine;
using Zenject;
using System.Linq;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.Slot;
using Game.CharacterSystem.Interface;
using Game.CoreSystem.Utility;
using Game.CombatSystem.Slot;

namespace Game.SkillCardSystem.Manager
{
    /// <summary>
    /// 플레이어의 핸드(손패)를 관리하는 매니저입니다.
    /// 카드 추가/제거와 슬롯 관리를 담당합니다.
    /// </summary>
    public class PlayerHandManager : MonoBehaviour, IPlayerHandManager
    {
        #region Private Fields

        private ICharacter currentPlayer;
        private HandSlotRegistry handSlotRegistry;

        #endregion

        #region DI

        [Inject]
        public void Construct(HandSlotRegistry handSlotRegistry)
        {
            this.handSlotRegistry = handSlotRegistry;
        }

        #endregion

        #region IPlayerHandManager 구현

        /// <summary>
        /// 현재 플레이어 캐릭터를 설정합니다.
        /// </summary>
        /// <param name="player">플레이어 캐릭터</param>
        public void SetPlayer(ICharacter player)
        {
            currentPlayer = player;
            GameLogger.LogInfo($"플레이어 설정: {player?.GetCharacterName()}", GameLogger.LogCategory.SkillCard);
        }

        /// <summary>
        /// 게임 시작 시 초기 손패를 생성합니다.
        /// </summary>
        public void GenerateInitialHand()
        {
            GameLogger.LogInfo("초기 손패 생성", GameLogger.LogCategory.SkillCard);
            // TODO: 실제 초기 손패 생성 로직 구현
        }

        /// <summary>
        /// 지정한 슬롯 위치에 있는 카드를 반환합니다.
        /// </summary>
        /// <param name="pos">슬롯 위치</param>
        /// <returns>해당 슬롯의 카드</returns>
        public ISkillCard GetCardInSlot(SkillCardSlotPosition pos)
        {
            var slot = handSlotRegistry?.GetHandSlot(pos);
            return slot?.GetCard();
        }

        /// <summary>
        /// 지정한 슬롯 위치에 있는 카드 UI를 반환합니다.
        /// </summary>
        /// <param name="pos">슬롯 위치</param>
        /// <returns>해당 슬롯의 카드 UI</returns>
        public ISkillCardUI GetCardUIInSlot(SkillCardSlotPosition pos)
        {
            var slot = handSlotRegistry?.GetHandSlot(pos);
            return slot?.GetCardUI();
        }

        /// <summary>
        /// 빈 슬롯에 카드를 추가합니다.
        /// </summary>
        /// <param name="card">추가할 카드</param>
        public void AddCardToHand(ISkillCard card)
        {
            if (card == null)
            {
                GameLogger.LogWarning("추가할 카드가 null입니다.", GameLogger.LogCategory.SkillCard);
                return;
            }

            // 플레이어 핸드 슬롯에서 빈 슬롯 찾기
            var playerSlots = handSlotRegistry?.GetPlayerHandSlot();
            if (playerSlots != null)
            {
                var emptySlot = playerSlots.FirstOrDefault(slot => !slot.HasCard());
                if (emptySlot != null)
                {
                    emptySlot.SetCard(card);
                    GameLogger.LogInfo($"카드 추가: {card.GetCardName()}", GameLogger.LogCategory.SkillCard);
                }
                else
                {
                    GameLogger.LogWarning("빈 슬롯을 찾을 수 없습니다.", GameLogger.LogCategory.SkillCard);
                }
            }
            else
            {
                GameLogger.LogWarning("플레이어 핸드 슬롯을 찾을 수 없습니다.", GameLogger.LogCategory.SkillCard);
            }
        }

        /// <summary>
        /// 특정 슬롯에 카드를 추가합니다.
        /// </summary>
        /// <param name="slot">슬롯 위치</param>
        /// <param name="card">추가할 카드</param>
        public void AddCardToSlot(SkillCardSlotPosition slot, ISkillCard card)
        {
            if (card == null)
            {
                GameLogger.LogWarning("추가할 카드가 null입니다.", GameLogger.LogCategory.SkillCard);
                return;
            }

            var handSlot = handSlotRegistry?.GetHandSlot(slot);
            if (handSlot != null)
            {
                handSlot.SetCard(card);
                GameLogger.LogInfo($"카드 추가: {card.GetCardName()} to {slot}", GameLogger.LogCategory.SkillCard);
            }
            else
            {
                GameLogger.LogWarning($"슬롯을 찾을 수 없습니다: {slot}", GameLogger.LogCategory.SkillCard);
            }
        }

        /// <summary>
        /// 카드를 제거합니다.
        /// </summary>
        /// <param name="card">제거할 카드</param>
        public void RemoveCard(ISkillCard card)
        {
            if (card == null)
            {
                GameLogger.LogWarning("제거할 카드가 null입니다.", GameLogger.LogCategory.SkillCard);
                return;
            }

            // 모든 슬롯에서 해당 카드 찾기
            var slots = handSlotRegistry?.GetAllHandSlots();
            if (slots != null)
            {
                foreach (var slot in slots)
                {
                    if (slot.GetCard() == card)
                    {
                        slot.Clear();
                        GameLogger.LogInfo($"카드 제거: {card.GetCardName()}", GameLogger.LogCategory.SkillCard);
                        return;
                    }
                }
            }

            GameLogger.LogWarning($"카드를 찾을 수 없습니다: {card.GetCardName()}", GameLogger.LogCategory.SkillCard);
        }

        /// <summary>
        /// 카드 드래그 등 입력을 활성/비활성화합니다.
        /// </summary>
        /// <param name="enable">활성화 여부</param>
        public void EnableInput(bool enable)
        {
            GameLogger.LogInfo($"입력 활성화: {enable}", GameLogger.LogCategory.SkillCard);
            // TODO: 실제 입력 활성화/비활성화 로직 구현
        }

        /// <summary>
        /// 모든 핸드 슬롯과 카드 UI를 제거합니다.
        /// </summary>
        public void ClearAll()
        {
            var slots = handSlotRegistry?.GetAllHandSlots();
            if (slots != null)
            {
                foreach (var slot in slots)
                {
                    slot.Clear();
                }
            }
            GameLogger.LogInfo("모든 핸드 카드 제거", GameLogger.LogCategory.SkillCard);
        }

        /// <summary>
        /// 플레이어 캐릭터를 반환합니다.
        /// </summary>
        /// <returns>플레이어 캐릭터</returns>
        public ICharacter GetPlayer()
        {
            return currentPlayer;
        }

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            GameLogger.LogInfo("PlayerHandManager 초기화", GameLogger.LogCategory.SkillCard);
        }

        #endregion
    }
}
