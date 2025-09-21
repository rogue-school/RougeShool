using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;
using Game.SkillCardSystem.Data;
using Game.SkillCardSystem.Deck;
using Game.SkillCardSystem.Interface;
using Game.CoreSystem.Save;
using Game.CoreSystem.Utility;

namespace Game.SkillCardSystem.Manager
{
    /// <summary>
    /// 플레이어 덱을 동적으로 관리하는 매니저 클래스입니다.
    /// 게임 중 덱 구성 변경, 카드 추가/제거, 보상 지급 등을 담당합니다.
    /// </summary>
    public class PlayerDeckManager : BaseSkillCardManager<IPlayerDeckManager>, IPlayerDeckManager
    {
        #region 필드

        #region PlayerDeckManager 전용 설정
        
        [Header("덱 설정")]
        [Tooltip("기본 덱")]
        [SerializeField] private PlayerSkillDeck defaultDeck;
        [Tooltip("최대 덱 크기")]
        [SerializeField] private int maxDeckSize = 30;
        
        #endregion

        private List<PlayerSkillDeck.CardEntry> currentDeck = new();
        private SaveManager saveManager;

        #endregion

        #region 프로퍼티

        public System.Action<SkillCardDefinition, int> OnDeckChanged { get; set; }
        public System.Action<SkillCardDefinition, int> OnCardAdded { get; set; }
        public System.Action<SkillCardDefinition, int> OnCardRemoved { get; set; }

        #endregion

        #region 의존성 주입 및 초기화

        [Inject]
        public void Construct(SaveManager saveManager = null)
        {
            this.saveManager = saveManager;
        }

        #region 베이스 클래스 구현

        protected override System.Collections.IEnumerator OnInitialize()
        {
            // 덱 초기화
            InitializeDeck();

            // 카드 설정 검증
            if (!ValidateCardSettings())
            {
                yield break;
            }

            // 참조 검증
            ValidateReferences();

            // 카드 UI 연결
            ConnectCardUI();

            // 매니저 상태 로깅
            LogManagerState();

            yield return null;
        }

        public override void Reset()
        {
            // 덱 정리
            currentDeck.Clear();

            // 기본 덱으로 초기화
            InitializeDeck();

            if (managerSettings.enableDebugLogging)
            {
                GameLogger.LogInfo("PlayerDeckManager 리셋 완료", GameLogger.LogCategory.SkillCard);
            }
        }

        #endregion

        private void InitializeDeck()
        {
            if (defaultDeck != null)
            {
                // 기본 덱으로 초기화
                currentDeck = new List<PlayerSkillDeck.CardEntry>();
                foreach (var entry in defaultDeck.GetAllCardEntries())
                {
                    currentDeck.Add(new PlayerSkillDeck.CardEntry
                    {
                        cardDefinition = entry.cardDefinition,
                        quantity = entry.quantity
                    });
                }
                
                GameLogger.LogInfo($"플레이어 덱 초기화 완료: {GetTotalCardCount()}장", GameLogger.LogCategory.SkillCard);
            }
            else
            {
                GameLogger.LogWarning("기본 덱이 설정되지 않았습니다.", GameLogger.LogCategory.SkillCard);
            }
        }

        #endregion

        #region 베이스 클래스 오버라이드

        /// <summary>
        /// PlayerDeckManager는 핸드 컨테이너가 필요하지 않습니다.
        /// </summary>
        protected override bool RequiresHandContainer()
        {
            return false;
        }

        /// <summary>
        /// PlayerDeckManager는 덱 컨테이너만 필요합니다.
        /// </summary>
        protected override bool RequiresDeckContainer()
        {
            return true;
        }

        #endregion

        #region 덱 구성 관리

        public bool AddCardToDeck(SkillCardDefinition cardDefinition, int quantity = 1)
        {
            if (cardDefinition == null)
            {
                GameLogger.LogError("카드 정의가 null입니다.", GameLogger.LogCategory.SkillCard);
                return false;
            }

            if (quantity <= 0)
            {
                GameLogger.LogError($"잘못된 수량입니다: {quantity}", GameLogger.LogCategory.SkillCard);
                return false;
            }

            // 덱 크기 제한 확인
            if (GetTotalCardCount() + quantity > maxDeckSize)
            {
                GameLogger.LogWarning($"덱 크기 제한 초과: {GetTotalCardCount() + quantity}/{maxDeckSize}", GameLogger.LogCategory.SkillCard);
                return false;
            }

            // 기존 카드 엔트리 찾기
            var existingEntry = currentDeck.FirstOrDefault(entry => entry.cardDefinition == cardDefinition);
            
            if (existingEntry != null)
            {
                // 기존 카드 수량 증가
                existingEntry.quantity += quantity;
                GameLogger.LogInfo($"카드 수량 증가: {cardDefinition.displayName} +{quantity} (총 {existingEntry.quantity}장)", GameLogger.LogCategory.SkillCard);
            }
            else
            {
                // 새 카드 엔트리 추가
                var newEntry = new PlayerSkillDeck.CardEntry
                {
                    cardDefinition = cardDefinition,
                    quantity = quantity
                };
                currentDeck.Add(newEntry);
                GameLogger.LogInfo($"새 카드 추가: {cardDefinition.displayName} x{quantity}", GameLogger.LogCategory.SkillCard);
            }

            // 이벤트 발생
            OnCardAdded?.Invoke(cardDefinition, quantity);
            OnDeckChanged?.Invoke(cardDefinition, GetCardQuantity(cardDefinition));

            return true;
        }

        public bool RemoveCardFromDeck(SkillCardDefinition cardDefinition, int quantity = 1)
        {
            if (cardDefinition == null)
            {
                GameLogger.LogError("카드 정의가 null입니다.", GameLogger.LogCategory.SkillCard);
                return false;
            }

            if (quantity <= 0)
            {
                GameLogger.LogError($"잘못된 수량입니다: {quantity}", GameLogger.LogCategory.SkillCard);
                return false;
            }

            var existingEntry = currentDeck.FirstOrDefault(entry => entry.cardDefinition == cardDefinition);
            
            if (existingEntry == null)
            {
                GameLogger.LogWarning($"덱에 존재하지 않는 카드입니다: {cardDefinition.displayName}", GameLogger.LogCategory.SkillCard);
                return false;
            }

            if (existingEntry.quantity < quantity)
            {
                GameLogger.LogWarning($"제거할 수량이 부족합니다: {existingEntry.quantity}/{quantity}", GameLogger.LogCategory.SkillCard);
                return false;
            }

            // 수량 감소
            existingEntry.quantity -= quantity;
            
            // 수량이 0이 되면 엔트리 제거
            if (existingEntry.quantity <= 0)
            {
                currentDeck.Remove(existingEntry);
                GameLogger.LogInfo($"카드 완전 제거: {cardDefinition.displayName}", GameLogger.LogCategory.SkillCard);
            }
            else
            {
                GameLogger.LogInfo($"카드 수량 감소: {cardDefinition.displayName} -{quantity} (남은 {existingEntry.quantity}장)", GameLogger.LogCategory.SkillCard);
            }

            // 이벤트 발생
            OnCardRemoved?.Invoke(cardDefinition, quantity);
            OnDeckChanged?.Invoke(cardDefinition, GetCardQuantity(cardDefinition));

            return true;
        }

        public bool RemoveAllCardsFromDeck(SkillCardDefinition cardDefinition)
        {
            if (cardDefinition == null)
            {
                GameLogger.LogError("카드 정의가 null입니다.", GameLogger.LogCategory.SkillCard);
                return false;
            }

            var existingEntry = currentDeck.FirstOrDefault(entry => entry.cardDefinition == cardDefinition);
            
            if (existingEntry == null)
            {
                GameLogger.LogWarning($"덱에 존재하지 않는 카드입니다: {cardDefinition.displayName}", GameLogger.LogCategory.SkillCard);
                return false;
            }

            int removedQuantity = existingEntry.quantity;
            currentDeck.Remove(existingEntry);
            
            GameLogger.LogInfo($"카드 완전 제거: {cardDefinition.displayName} x{removedQuantity}", GameLogger.LogCategory.SkillCard);

            // 이벤트 발생
            OnCardRemoved?.Invoke(cardDefinition, removedQuantity);
            OnDeckChanged?.Invoke(cardDefinition, 0);

            return true;
        }

        public bool SetCardQuantity(SkillCardDefinition cardDefinition, int quantity)
        {
            if (cardDefinition == null)
            {
                GameLogger.LogError("카드 정의가 null입니다.", GameLogger.LogCategory.SkillCard);
                return false;
            }

            if (quantity < 0)
            {
                GameLogger.LogError($"잘못된 수량입니다: {quantity}", GameLogger.LogCategory.SkillCard);
                return false;
            }

            var existingEntry = currentDeck.FirstOrDefault(entry => entry.cardDefinition == cardDefinition);
            
            if (quantity == 0)
            {
                // 수량을 0으로 설정하면 카드 제거
                if (existingEntry != null)
                {
                    return RemoveAllCardsFromDeck(cardDefinition);
                }
                return true;
            }

            // 덱 크기 제한 확인
            int currentTotal = GetTotalCardCount();
            int currentQuantity = existingEntry?.quantity ?? 0;
            int newTotal = currentTotal - currentQuantity + quantity;
            
            if (newTotal > maxDeckSize)
            {
                GameLogger.LogWarning($"덱 크기 제한 초과: {newTotal}/{maxDeckSize}", GameLogger.LogCategory.SkillCard);
                return false;
            }

            if (existingEntry != null)
            {
                // 기존 카드 수량 변경
                int oldQuantity = existingEntry.quantity;
                existingEntry.quantity = quantity;
                GameLogger.LogInfo($"카드 수량 변경: {cardDefinition.displayName} {oldQuantity} → {quantity}", GameLogger.LogCategory.SkillCard);
            }
            else
            {
                // 새 카드 추가
                var newEntry = new PlayerSkillDeck.CardEntry
                {
                    cardDefinition = cardDefinition,
                    quantity = quantity
                };
                currentDeck.Add(newEntry);
                GameLogger.LogInfo($"새 카드 추가: {cardDefinition.displayName} x{quantity}", GameLogger.LogCategory.SkillCard);
            }

            // 이벤트 발생
            OnDeckChanged?.Invoke(cardDefinition, quantity);

            return true;
        }

        #endregion

        #region 덱 조회

        public List<PlayerSkillDeck.CardEntry> GetCurrentDeck()
        {
            return new List<PlayerSkillDeck.CardEntry>(currentDeck);
        }

        public int GetCardQuantity(SkillCardDefinition cardDefinition)
        {
            if (cardDefinition == null) return 0;
            
            var entry = currentDeck.FirstOrDefault(e => e.cardDefinition == cardDefinition);
            return entry?.quantity ?? 0;
        }

        public bool HasCard(SkillCardDefinition cardDefinition)
        {
            return GetCardQuantity(cardDefinition) > 0;
        }

        public int GetTotalCardCount()
        {
            return currentDeck.Sum(entry => entry.quantity);
        }

        public int GetUniqueCardCount()
        {
            return currentDeck.Count;
        }

        #endregion

        #region 덱 검증

        public bool IsValidDeck()
        {
            if (currentDeck == null || currentDeck.Count == 0)
            {
                GameLogger.LogWarning("덱이 비어있습니다.", GameLogger.LogCategory.SkillCard);
                return false;
            }

            // 각 엔트리 유효성 확인
            foreach (var entry in currentDeck)
            {
                if (!entry.IsValid())
                {
                    GameLogger.LogError($"유효하지 않은 카드 엔트리: {entry}", GameLogger.LogCategory.SkillCard);
                    return false;
                }
            }

            // 덱 크기 제한 확인
            if (!IsWithinCardLimit())
            {
                return false;
            }

            return true;
        }

        public bool IsWithinCardLimit(int minCards = 5, int maxCards = 30)
        {
            int totalCards = GetTotalCardCount();
            
            if (totalCards < minCards)
            {
                GameLogger.LogWarning($"덱 크기가 너무 작습니다: {totalCards}/{minCards}", GameLogger.LogCategory.SkillCard);
                return false;
            }

            if (totalCards > maxCards)
            {
                GameLogger.LogWarning($"덱 크기가 너무 큽니다: {totalCards}/{maxCards}", GameLogger.LogCategory.SkillCard);
                return false;
            }

            return true;
        }

        #endregion

        #region 덱 저장/로드

        public void SaveDeckConfiguration()
        {
            if (saveManager == null)
            {
                GameLogger.LogError("SaveManager가 주입되지 않았습니다.", GameLogger.LogCategory.SkillCard);
                return;
            }

            // 덱 구성을 JSON으로 직렬화하여 저장
            var deckData = new DeckConfigurationData
            {
                cardEntries = currentDeck.Select(entry => new CardEntryData
                {
                    cardId = entry.cardDefinition?.cardId ?? "",
                    quantity = entry.quantity
                }).ToList()
            };

            string jsonData = JsonUtility.ToJson(deckData, true);
            saveManager.SavePlayerDeckConfiguration(jsonData);
            
            GameLogger.LogInfo("덱 구성 저장 완료", GameLogger.LogCategory.SkillCard);
        }

        public void LoadDeckConfiguration()
        {
            if (saveManager == null)
            {
                GameLogger.LogError("SaveManager가 주입되지 않았습니다.", GameLogger.LogCategory.SkillCard);
                return;
            }

            string jsonData = saveManager.LoadPlayerDeckConfiguration();
            if (string.IsNullOrEmpty(jsonData))
            {
                GameLogger.LogWarning("저장된 덱 구성이 없습니다. 기본 덱을 사용합니다.", GameLogger.LogCategory.SkillCard);
                ResetToDefaultDeck();
                return;
            }

            try
            {
                var deckData = JsonUtility.FromJson<DeckConfigurationData>(jsonData);
                currentDeck.Clear();

                foreach (var cardData in deckData.cardEntries)
                {
                    var cardDefinition = Resources.Load<SkillCardDefinition>($"SkillCards/{cardData.cardId}");
                    if (cardDefinition != null)
                    {
                        currentDeck.Add(new PlayerSkillDeck.CardEntry
                        {
                            cardDefinition = cardDefinition,
                            quantity = cardData.quantity
                        });
                    }
                    else
                    {
                        GameLogger.LogWarning($"카드 정의를 찾을 수 없습니다: {cardData.cardId}", GameLogger.LogCategory.SkillCard);
                    }
                }

                GameLogger.LogInfo("덱 구성 로드 완료", GameLogger.LogCategory.SkillCard);
            }
            catch (System.Exception e)
            {
                GameLogger.LogError($"덱 구성 로드 실패: {e.Message}", GameLogger.LogCategory.SkillCard);
                ResetToDefaultDeck();
            }
        }

        public void ResetToDefaultDeck()
        {
            InitializeDeck();
            GameLogger.LogInfo("기본 덱으로 리셋 완료", GameLogger.LogCategory.SkillCard);
        }

        #endregion

        #region 데이터 클래스

        [System.Serializable]
        private class DeckConfigurationData
        {
            public List<CardEntryData> cardEntries = new();
        }

        [System.Serializable]
        private class CardEntryData
        {
            public string cardId;
            public int quantity;
        }

        #endregion
    }
}
