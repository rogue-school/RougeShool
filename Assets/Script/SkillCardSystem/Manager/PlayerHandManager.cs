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
        [InjectOptional] private Game.SkillCardSystem.UI.SkillCardUI cardUIPrefab;
        [InjectOptional] private ICardCirculationSystem circulationSystem;

        #endregion

        #region DI

        [Inject]
        public void Construct(HandSlotRegistry handSlotRegistry)
        {
            this.handSlotRegistry = handSlotRegistry;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// PlayerDeckManager로부터 카드 인스턴스를 받아 CardCirculationSystem을 초기화합니다.
        /// </summary>
        public void InitializeDeck(System.Collections.Generic.List<ISkillCard> cardInstances)
        {
            if (circulationSystem != null)
            {
                circulationSystem.Initialize(cardInstances);
                GameLogger.LogInfo($"[PlayerHandManager] CardCirculationSystem 초기화: {cardInstances.Count}장", GameLogger.LogCategory.SkillCard);
            }
            else
            {
                GameLogger.LogWarning("[PlayerHandManager] CardCirculationSystem이 null - 초기화 실패", GameLogger.LogCategory.SkillCard);
            }
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
        /// 게임 시작 시 초기 손패를 생성합니다. (항상 3장, 덱 비파괴 샘플링)
        /// </summary>
        public void GenerateInitialHand()
        {
            // GameLogger.LogInfo("초기 손패 생성", GameLogger.LogCategory.SkillCard);

            // 의존성 상세 진단
            if (currentPlayer == null)
            {
                GameLogger.LogWarning("초기 손패 생성 실패 - Player가 null", GameLogger.LogCategory.SkillCard);
                return;
            }
            if (handSlotRegistry == null)
            {
                // SlotRegistry에서 가져오기 시도
                var slotRegistry = Object.FindFirstObjectByType<Game.CombatSystem.Slot.SlotRegistry>();
                handSlotRegistry = slotRegistry?.GetHandSlotRegistry();
                if (handSlotRegistry == null)
                {
                    GameLogger.LogWarning("초기 손패 생성 실패 - HandSlotRegistry가 null", GameLogger.LogCategory.SkillCard);
                    return;
                }
            }
            if (circulationSystem == null)
            {
                GameLogger.LogWarning("초기 손패 생성 실패 - ICardCirculationSystem이 주입되지 않음", GameLogger.LogCategory.SkillCard);
                return;
            }
            if (cardUIPrefab == null)
            {
                // Resources에서 로드 시도
                cardUIPrefab = Resources.Load<Game.SkillCardSystem.UI.SkillCardUI>("Prefab/SkillCard");
                if (cardUIPrefab != null)
                {
                    GameLogger.LogInfo("핸드용 SkillCardUI 프리팹을 Resources에서 로드했습니다.", GameLogger.LogCategory.SkillCard);
                }
                else
                {
                    GameLogger.LogWarning("초기 손패 생성 실패 - SkillCardUI 프리팹이 주입/로드되지 않음", GameLogger.LogCategory.SkillCard);
                    return;
                }
            }

            var slots = handSlotRegistry.GetPlayerHandSlot().ToList();
            if (slots == null || slots.Count == 0)
            {
                // 씬에서 직접 수집 후 등록 시도
                var found = Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None)
                    .OfType<Game.CombatSystem.Interface.IHandCardSlot>()
                    .Where(s => s.GetOwner() == Game.CombatSystem.Data.SlotOwner.PLAYER)
                    .ToList();
                if (found.Count > 0)
                {
                    handSlotRegistry.RegisterHandSlots(found);
                    slots = handSlotRegistry.GetPlayerHandSlot().ToList();
                    GameLogger.LogInfo($"플레이어 핸드 슬롯 자동 등록: {slots.Count}개", GameLogger.LogCategory.SkillCard);
                }
                if (slots == null || slots.Count == 0)
                {
                    GameLogger.LogWarning("초기 손패 생성 실패 - 플레이어 핸드 슬롯이 없음", GameLogger.LogCategory.SkillCard);
                    return;
                }
            }

            // 슬롯 초기화(기존 잔여 UI/카드 제거)
            foreach (var s in slots)
            {
                s.Clear();
            }

            // 비파괴 샘플링: 순환 시스템에서 3장 받아 손패에 부착
            var drawn = circulationSystem.DrawCardsForTurn();
            int toAttach = Mathf.Min(3, slots.Count);
            for (int i = 0; i < toAttach && i < drawn.Count; i++)
            {
                var slot = slots[i];
                var card = drawn[i];
                if (card != null && slot != null)
                {
                    slot.AttachCard(card, cardUIPrefab);
                    
                    // 전투 통계 이벤트 발생 (카드 생성)
                    if (card.CardDefinition != null)
                    {
                        string cardId = card.CardDefinition.cardId;
                        GameObject cardObj = null;
                        if (card is MonoBehaviour cardMono)
                        {
                            cardObj = cardMono.gameObject;
                        }
                        Game.CombatSystem.CombatEvents.RaisePlayerCardSpawn(cardId, cardObj);
                    }
                    
                    // GameLogger.LogInfo($"핸드에 카드 추가: {card.GetCardName()} (슬롯 {slot.GetSlotPosition()})", GameLogger.LogCategory.SkillCard);
                }
            }
        }

        /// <summary>
        /// 손패가 targetCount 미만이면 비파괴 샘플링으로 보충합니다.
        /// </summary>
        public void RefillHandTo(int targetCount)
        {
            if (handSlotRegistry == null || circulationSystem == null || cardUIPrefab == null)
            {
                GameLogger.LogWarning("손패 보충 실패 - 의존성 누락", GameLogger.LogCategory.SkillCard);
                return;
            }

            var slots = handSlotRegistry.GetPlayerHandSlot().ToList();
            int occupied = slots.Count(s => s.HasCard());
            int need = Mathf.Clamp(targetCount - occupied, 0, slots.Count - occupied);
            if (need <= 0) return;

            var drawn = circulationSystem.DrawCardsForTurn();
            int added = 0;
            foreach (var slot in slots)
            {
                if (added >= need) break;
                if (!slot.HasCard())
                {
                    var card = drawn.ElementAtOrDefault(added);
                    if (card == null) break;
                    slot.AttachCard(card, cardUIPrefab);
                    
                    // 전투 통계 이벤트 발생 (카드 생성)
                    if (card.CardDefinition != null)
                    {
                        string cardId = card.CardDefinition.cardId;
                        GameObject cardObj = null;
                        if (card is MonoBehaviour cardMono)
                        {
                            cardObj = cardMono.gameObject;
                        }
                        Game.CombatSystem.CombatEvents.RaisePlayerCardSpawn(cardId, cardObj);
                    }
                    
                    added++;
                }
            }
            GameLogger.LogInfo($"손패 보충 완료: {occupied}+{added}→{occupied + added}", GameLogger.LogCategory.SkillCard);
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
                    
                    // 전투 통계 이벤트 발생 (카드 생성)
                    if (card.CardDefinition != null)
                    {
                        string cardId = card.CardDefinition.cardId;
                        GameObject cardObj = null;
                        if (card is MonoBehaviour cardMono)
                        {
                            cardObj = cardMono.gameObject;
                        }
                        Game.CombatSystem.CombatEvents.RaisePlayerCardSpawn(cardId, cardObj);
                    }
                    
                    // 카드 추가 완료
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
                
                // 전투 통계 이벤트 발생 (카드 생성)
                if (card.CardDefinition != null)
                {
                    string cardId = card.CardDefinition.cardId;
                    GameObject cardObj = null;
                    if (card is MonoBehaviour cardMono)
                    {
                        cardObj = cardMono.gameObject;
                    }
                    Game.CombatSystem.CombatEvents.RaisePlayerCardSpawn(cardId, cardObj);
                }
                
                // 카드 추가 완료
            }
            else
            {
                GameLogger.LogWarning($"슬롯을 찾을 수 없습니다: {slot}", GameLogger.LogCategory.SkillCard);
            }
        }

        /// <summary>
        /// 카드를 제거합니다.
        /// 플레이어 카드가 전투 슬롯에서 실행된 후 핸드에서 제거할 때 사용됩니다.
        /// </summary>
        /// <param name="card">제거할 카드</param>
        public void RemoveCard(ISkillCard card)
        {
            if (card == null)
            {
                GameLogger.LogWarning("제거할 카드가 null입니다.", GameLogger.LogCategory.SkillCard);
                return;
            }

            // 플레이어 카드인지 확인
            if (!card.IsFromPlayer())
            {
                GameLogger.LogWarning($"플레이어 핸드에서 적 카드를 제거하려고 시도: {card.GetCardName()}", GameLogger.LogCategory.SkillCard);
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
                        // 플레이어 핸드에서 카드 제거 완료
                        return;
                    }
                }
            }

            GameLogger.LogWarning($"핸드에서 카드를 찾을 수 없습니다: {card.GetCardName()}", GameLogger.LogCategory.SkillCard);
        }

        /// <summary>
        /// 카드 드래그 등 입력을 활성/비활성화합니다.
        /// </summary>
        /// <param name="enable">활성화 여부</param>
        public void EnableInput(bool enable)
        {
            // GameLogger.LogInfo($"입력 활성화: {enable}", GameLogger.LogCategory.SkillCard);
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
            // GameLogger.LogInfo("모든 핸드 카드 제거", GameLogger.LogCategory.SkillCard);
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
            // GameLogger.LogInfo("PlayerHandManager 초기화", GameLogger.LogCategory.SkillCard);
        }

        #endregion
    }
}
