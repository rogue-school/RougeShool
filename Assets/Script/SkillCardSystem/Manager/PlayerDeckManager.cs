using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;
using Game.SkillCardSystem.Data;
using Game.SkillCardSystem.Deck;
using Game.SkillCardSystem.Interface;
using Game.CoreSystem.Save;
using Game.CoreSystem.Utility;
using Game.CharacterSystem.Manager;
using Game.CharacterSystem.Data;
using Game.CharacterSystem.Interface;

namespace Game.SkillCardSystem.Manager
{
    /// <summary>
    /// 플레이어 덱 매니저
    /// 캐릭터 데이터에서 덱 정보를 가져와서 관리하는 시스템
    /// </summary>
    public class PlayerDeckManager : MonoBehaviour, IPlayerDeckManager
    {
        #region 인스펙터 설정
        
        [Header("디버그 설정")]
        [Tooltip("디버그 로깅 활성화")]
        [SerializeField] private bool enableDebugLogging = true;
        
        #endregion
        
        #region 의존성 주입 (DI로 자동 해결)

        [Inject] private PlayerManager playerManager;
        [InjectOptional] private ISkillCardFactory cardFactory;

        #endregion
        
        #region 핵심 필드
        
        private List<PlayerSkillDeck.CardEntry> currentDeck = new();
        [Inject(Optional = true)] private IPlayerHandManager playerHandManager;
        
        #endregion
        
        #region 이벤트
        
        public System.Action<SkillCardDefinition, int> OnDeckChanged { get; set; }
        public System.Action<SkillCardDefinition, int> OnCardAdded { get; set; }
        public System.Action<SkillCardDefinition, int> OnCardRemoved { get; set; }
        
        #endregion
        
        #region 초기화
        
        private void Start()
        {
            // Zenject DI가 완료된 후 초기화
            StartCoroutine(InitializeDeckWhenReady());
            
            if (enableDebugLogging)
            {
                GameLogger.LogInfo("PlayerDeckManager 초기화", GameLogger.LogCategory.SkillCard);
            }
        }
        
        private void OnDestroy()
        {
            // 이벤트 구독 해제
            if (playerManager != null)
            {
                // PlayerManager의 이벤트 구독 해제는 PlayerManager가 파괴될 때 자동으로 해제됨
            }
        }
        
        /// <summary>
        /// PlayerManager가 준비될 때까지 대기한 후 이벤트를 구독합니다.
        /// </summary>
        private System.Collections.IEnumerator InitializeDeckWhenReady()
        {
            // PlayerManager가 준비될 때까지 대기
            int maxWaitFrames = 300; // 최대 5초 대기 (60fps 기준)
            int waitFrames = 0;
            
            while (playerManager == null && waitFrames < maxWaitFrames)
            {
                yield return new UnityEngine.WaitForSeconds(0.1f);
                waitFrames++;
            }
            
            if (playerManager == null)
            {
                GameLogger.LogError("PlayerManager를 찾을 수 없습니다. 덱 초기화를 건너뜁니다.", GameLogger.LogCategory.SkillCard);
                yield break;
            }
            
            GameLogger.LogInfo("PlayerManager 발견됨, 이벤트 구독 중...", GameLogger.LogCategory.SkillCard);
            
            // PlayerManager의 OnPlayerCharacterReady 이벤트 구독
            playerManager.OnPlayerCharacterReady += OnPlayerCharacterReady;
            
            // 이미 플레이어 캐릭터가 있다면 즉시 초기화
            if (playerManager.GetPlayer() != null)
            {
                GameLogger.LogInfo("플레이어 캐릭터가 이미 존재함, 즉시 덱 초기화", GameLogger.LogCategory.SkillCard);
                InitializeDeck();
            }
        }
        
        /// <summary>
        /// PlayerManager의 OnPlayerCharacterReady 이벤트 핸들러
        /// </summary>
        /// <param name="character">준비된 플레이어 캐릭터</param>
        private void OnPlayerCharacterReady(ICharacter character)
        {
            GameLogger.LogInfo($"플레이어 캐릭터 준비 완료: {character.GetCharacterName()}, 덱 초기화 시작", GameLogger.LogCategory.SkillCard);
            InitializeDeck();
        }
        
        /// <summary>
        /// 덱을 초기화합니다. (캐릭터 데이터에서 덱 정보를 가져옴)
        /// </summary>
        private void InitializeDeck()
        {
            var player = playerManager?.GetPlayer();
            if (player == null)
            {
                GameLogger.LogWarning("플레이어 매니저에서 플레이어를 찾을 수 없습니다.", GameLogger.LogCategory.SkillCard);
                currentDeck = new List<PlayerSkillDeck.CardEntry>();
                return;
            }

            // CharacterData가 PlayerCharacterData인지 확인
            if (player.CharacterData is PlayerCharacterData playerData)
            {
                if (playerData.SkillDeck != null)
                {
                    currentDeck = new List<PlayerSkillDeck.CardEntry>(playerData.SkillDeck.GetAllCardEntries());

                    if (enableDebugLogging)
                    {
                        GameLogger.LogInfo($"덱 초기화 완료: {playerData.DisplayName}의 덱 ({currentDeck.Count}종류, {GetTotalCardCount()}장)", GameLogger.LogCategory.SkillCard);
                    }

                    // CardCirculationSystem 초기화
                    InitializeCirculationSystem();
                }
                else
                {
                    currentDeck = new List<PlayerSkillDeck.CardEntry>();
                    GameLogger.LogWarning($"플레이어 캐릭터 '{playerData.DisplayName}'의 스킬 덱이 설정되지 않았습니다.", GameLogger.LogCategory.SkillCard);
                }
            }
            else
            {
                currentDeck = new List<PlayerSkillDeck.CardEntry>();
                GameLogger.LogWarning($"플레이어 캐릭터의 CharacterData가 PlayerCharacterData가 아닙니다. 타입: {player.CharacterData?.GetType().Name ?? "null"}", GameLogger.LogCategory.SkillCard);
            }
        }

        /// <summary>
        /// CardCirculationSystem을 초기화합니다.
        /// </summary>
        private void InitializeCirculationSystem()
        {
            // PlayerHandManager를 통해 접근
            if (cardFactory == null)
            {
                GameLogger.LogWarning("SkillCardFactory를 찾을 수 없습니다 - 순환 시스템 초기화 건너뜀", GameLogger.LogCategory.SkillCard);
                return;
            }

            // 덱의 모든 카드 정의를 ISkillCard 인스턴스로 변환
            var cardInstances = new List<ISkillCard>();
            foreach (var entry in currentDeck)
            {
                for (int i = 0; i < entry.quantity; i++)
                {
                    var cardInstance = cardFactory.CreatePlayerCard(entry.cardDefinition);
                    if (cardInstance != null)
                    {
                        cardInstances.Add(cardInstance);
                    }
                }
            }

            // PlayerHandManager를 통해 간접적으로 초기화
            if (playerHandManager != null && cardInstances.Count > 0)
            {
                // PlayerHandManager에 초기화 메서드가 있다면 호출
                StartCoroutine(DelayedInitializeCirculation(cardInstances));
            }
        }

        private System.Collections.IEnumerator DelayedInitializeCirculation(List<ISkillCard> cardInstances)
        {
            // 프레임 대기하여 모든 DI 완료 후 초기화
            yield return new WaitForEndOfFrame();

            if (playerHandManager != null)
            {
                // PlayerHandManager의 InitializeCirculationSystem 메서드 호출
                playerHandManager.InitializeDeck(cardInstances);
                GameLogger.LogInfo($"CardCirculationSystem 초기화 완료: {cardInstances.Count}장", GameLogger.LogCategory.SkillCard);
            }
        }
        
        #endregion
        
        #region 덱 관리
        
        /// <summary>
        /// 현재 덱의 모든 카드를 반환합니다.
        /// </summary>
        public List<SkillCardDefinition> GetAllCards()
        {
            var cards = new List<SkillCardDefinition>();
            foreach (var entry in currentDeck)
            {
                for (int i = 0; i < entry.quantity; i++)
                {
                    cards.Add(entry.cardDefinition);
                }
            }
            return cards;
        }
        
        /// <summary>
        /// 덱에서 카드를 드로우합니다.
        /// </summary>
        public SkillCardDefinition DrawCard()
        {
            if (currentDeck.Count == 0) return null;
            
            // 첫 번째 카드 엔트리에서 드로우
            var firstEntry = currentDeck[0];
            var card = firstEntry.cardDefinition;
            
            // 카운트 감소
            firstEntry.quantity--;
            if (firstEntry.quantity <= 0)
            {
                currentDeck.RemoveAt(0);
            }
            
            OnCardRemoved?.Invoke(card, 1);
            
            if (enableDebugLogging)
            {
                GameLogger.LogInfo($"카드 드로우: {card.displayName}", GameLogger.LogCategory.SkillCard);
            }
            
            return card;
        }
        
        /// <summary>
        /// 덱에 카드를 추가합니다.
        /// </summary>
        public bool AddCardToDeck(SkillCardDefinition cardDefinition, int quantity = 1)
        {
            if (cardDefinition == null) return false;
            
            // 기존 엔트리 찾기
            var existingEntry = currentDeck.FirstOrDefault(e => e.cardDefinition == cardDefinition);
            if (existingEntry != null)
            {
                existingEntry.quantity += quantity;
            }
            else
            {
                // 새 엔트리 추가
                currentDeck.Add(new PlayerSkillDeck.CardEntry { cardDefinition = cardDefinition, quantity = quantity });
            }
            
            OnCardAdded?.Invoke(cardDefinition, quantity);
            
            if (enableDebugLogging)
            {
                GameLogger.LogInfo($"카드 추가: {cardDefinition.displayName} x{quantity}", GameLogger.LogCategory.SkillCard);
            }
            
            return true;
        }
        
        /// <summary>
        /// 덱에 카드를 추가합니다. (기존 메서드 호환성)
        /// </summary>
        public void AddCard(SkillCardDefinition card, int count = 1)
        {
            AddCardToDeck(card, count);
        }
        
        /// <summary>
        /// 덱에서 카드를 제거합니다.
        /// </summary>
        public bool RemoveCardFromDeck(SkillCardDefinition cardDefinition, int quantity = 1)
        {
            if (cardDefinition == null) return false;
            
            var entry = currentDeck.FirstOrDefault(e => e.cardDefinition == cardDefinition);
            if (entry == null) return false;
            
            if (entry.quantity >= quantity)
            {
                entry.quantity -= quantity;
                if (entry.quantity <= 0)
                {
                    currentDeck.Remove(entry);
                }
                
                OnCardRemoved?.Invoke(cardDefinition, quantity);
                
                if (enableDebugLogging)
                {
                    GameLogger.LogInfo($"카드 제거: {cardDefinition.displayName} x{quantity}", GameLogger.LogCategory.SkillCard);
                }
                
                return true;
            }
            
            return false;
        }
        
        /// <summary>
        /// 덱에서 카드를 제거합니다. (기존 메서드 호환성)
        /// </summary>
        public bool RemoveCard(SkillCardDefinition card, int count = 1)
        {
            return RemoveCardFromDeck(card, count);
        }
        
        /// <summary>
        /// 덱에서 특정 카드를 완전히 제거합니다.
        /// </summary>
        public bool RemoveAllCardsFromDeck(SkillCardDefinition cardDefinition)
        {
            if (cardDefinition == null) return false;
            
            var entry = currentDeck.FirstOrDefault(e => e.cardDefinition == cardDefinition);
            if (entry == null) return false;
            
            int removedCount = entry.quantity;
            currentDeck.Remove(entry);
            
            OnCardRemoved?.Invoke(cardDefinition, removedCount);
            
            if (enableDebugLogging)
            {
                GameLogger.LogInfo($"카드 완전 제거: {cardDefinition.displayName} x{removedCount}", GameLogger.LogCategory.SkillCard);
            }
            
            return true;
        }
        
        /// <summary>
        /// 덱의 특정 카드 수량을 설정합니다.
        /// </summary>
        public bool SetCardQuantity(SkillCardDefinition cardDefinition, int quantity)
        {
            if (cardDefinition == null) return false;
            
            var entry = currentDeck.FirstOrDefault(e => e.cardDefinition == cardDefinition);
            if (entry != null)
            {
                entry.quantity = quantity;
                if (entry.quantity <= 0)
                {
                    currentDeck.Remove(entry);
                }
            }
            else if (quantity > 0)
            {
                currentDeck.Add(new PlayerSkillDeck.CardEntry { cardDefinition = cardDefinition, quantity = quantity });
            }
            
            OnDeckChanged?.Invoke(cardDefinition, quantity);
            
            if (enableDebugLogging)
            {
                GameLogger.LogInfo($"카드 수량 설정: {cardDefinition.displayName} x{quantity}", GameLogger.LogCategory.SkillCard);
            }
            
            return true;
        }
        
        /// <summary>
        /// 덱을 셔플합니다.
        /// </summary>
        public void ShuffleDeck()
        {
            // 카드 리스트를 생성하고 셔플
            var allCards = GetAllCards();
            for (int i = 0; i < allCards.Count; i++)
            {
                var randomIndex = Random.Range(i, allCards.Count);
                (allCards[i], allCards[randomIndex]) = (allCards[randomIndex], allCards[i]);
            }
            
            // 셔플된 카드로 덱 재구성
            currentDeck.Clear();
            var cardGroups = allCards.GroupBy(c => c).Select(g => new PlayerSkillDeck.CardEntry 
            { 
                cardDefinition = g.Key, 
                quantity = g.Count() 
            });
            currentDeck.AddRange(cardGroups);
            
            if (enableDebugLogging)
            {
                GameLogger.LogInfo("덱 셔플 완료", GameLogger.LogCategory.SkillCard);
            }
        }
        
        #endregion
        
        #region 덱 정보
        
        /// <summary>
        /// 현재 덱 구성을 반환합니다.
        /// </summary>
        public List<PlayerSkillDeck.CardEntry> GetCurrentDeck()
        {
            return new List<PlayerSkillDeck.CardEntry>(currentDeck);
        }
        
        /// <summary>
        /// 특정 카드의 현재 수량을 반환합니다.
        /// </summary>
        public int GetCardQuantity(SkillCardDefinition cardDefinition)
        {
            var entry = currentDeck.FirstOrDefault(e => e.cardDefinition == cardDefinition);
            return entry?.quantity ?? 0;
        }
        
        /// <summary>
        /// 덱에 특정 카드가 있는지 확인합니다.
        /// </summary>
        public bool HasCard(SkillCardDefinition cardDefinition)
        {
            return currentDeck.Any(e => e.cardDefinition == cardDefinition && e.quantity > 0);
        }
        
        /// <summary>
        /// 덱의 총 카드 수를 반환합니다.
        /// </summary>
        public int GetTotalCardCount()
        {
            return currentDeck.Sum(e => e.quantity);
        }
        
        /// <summary>
        /// 덱의 고유 카드 종류 수를 반환합니다.
        /// </summary>
        public int GetUniqueCardCount()
        {
            return currentDeck.Count(e => e.quantity > 0);
        }
        
        /// <summary>
        /// 현재 덱이 유효한지 확인합니다.
        /// </summary>
        public bool IsValidDeck()
        {
            return GetTotalCardCount() >= 5 && GetTotalCardCount() <= 30;
        }
        
        /// <summary>
        /// 덱의 최소/최대 카드 수 제한을 확인합니다.
        /// </summary>
        public bool IsWithinCardLimit(int minCards = 5, int maxCards = 30)
        {
            int totalCount = GetTotalCardCount();
            return totalCount >= minCards && totalCount <= maxCards;
        }
        
        /// <summary>
        /// 덱 크기를 반환합니다. (기존 메서드 호환성)
        /// </summary>
        public int GetDeckSize() => GetTotalCardCount();
        
        /// <summary>
        /// 특정 카드의 개수를 반환합니다. (기존 메서드 호환성)
        /// </summary>
        public int GetCardCount(SkillCardDefinition card)
        {
            return GetCardQuantity(card);
        }
        
        /// <summary>
        /// 덱이 비어있는지 확인합니다. (기존 메서드 호환성)
        /// </summary>
        public bool IsDeckEmpty() => currentDeck.Count == 0;
        
        #endregion
        
        #region 저장/로드
        
        /// <summary>
        /// 현재 덱 구성을 저장합니다.
        /// </summary>
        public void SaveDeckConfiguration()
        {
            if (saveManager != null)
            {
                // 저장 로직 구현 (필요시)
                if (enableDebugLogging)
                {
                    GameLogger.LogInfo("덱 구성 저장 완료", GameLogger.LogCategory.SkillCard);
                }
            }
        }
        
        /// <summary>
        /// 저장된 덱 구성을 로드합니다.
        /// </summary>
        public void LoadDeckConfiguration()
        {
            if (saveManager != null)
            {
                // 로드 로직 구현 (필요시)
                if (enableDebugLogging)
                {
                    GameLogger.LogInfo("덱 구성 로드 완료", GameLogger.LogCategory.SkillCard);
                }
            }
        }
        
        /// <summary>
        /// 캐릭터의 기본 덱으로 리셋합니다.
        /// </summary>
        public void ResetToCharacterDeck()
        {
            InitializeDeck();
            
            if (enableDebugLogging)
            {
                GameLogger.LogInfo("캐릭터 기본 덱으로 리셋 완료", GameLogger.LogCategory.SkillCard);
            }
        }
        
        /// <summary>
        /// 기본 덱으로 리셋합니다. (기존 메서드 호환성)
        /// </summary>
        public void ResetToDefaultDeck()
        {
            ResetToCharacterDeck();
        }
        
        /// <summary>
        /// 덱을 저장합니다. (기존 메서드 호환성)
        /// </summary>
        public void SaveDeck()
        {
            SaveDeckConfiguration();
        }
        
        /// <summary>
        /// 덱을 로드합니다. (기존 메서드 호환성)
        /// </summary>
        public void LoadDeck()
        {
            LoadDeckConfiguration();
        }
        
        #endregion
        
        #region 캐릭터 연동
        
        /// <summary>
        /// 현재 플레이어 캐릭터의 덱 정보를 가져옵니다.
        /// </summary>
        /// <returns>플레이어 캐릭터의 스킬 덱</returns>
        public PlayerSkillDeck GetCharacterDeck()
        {
            var player = playerManager?.GetPlayer();
            if (player?.CharacterData is PlayerCharacterData playerData)
            {
                return playerData.SkillDeck;
            }
            return null;
        }
        
        /// <summary>
        /// 현재 플레이어 캐릭터의 이름을 가져옵니다.
        /// </summary>
        /// <returns>플레이어 캐릭터 이름</returns>
        public string GetCharacterName()
        {
            var player = playerManager?.GetPlayer();
            if (player?.CharacterData is PlayerCharacterData playerData)
            {
                return playerData.DisplayName;
            }
            return "Unknown";
        }
        
        /// <summary>
        /// 캐릭터가 변경되었을 때 덱을 다시 초기화합니다.
        /// </summary>
        public void RefreshDeckFromCharacter()
        {
            InitializeDeck();
            
            if (enableDebugLogging)
            {
                GameLogger.LogInfo($"캐릭터 변경으로 인한 덱 새로고침: {GetCharacterName()}", GameLogger.LogCategory.SkillCard);
            }
        }
        
        /// <summary>
        /// 현재 덱이 캐릭터의 기본 덱과 다른지 확인합니다.
        /// </summary>
        /// <returns>덱이 변경되었는지 여부</returns>
        public bool IsDeckModified()
        {
            var characterDeck = GetCharacterDeck();
            if (characterDeck == null) return false;
            
            var characterEntries = characterDeck.GetAllCardEntries();
            if (characterEntries.Count != currentDeck.Count) return true;
            
            foreach (var characterEntry in characterEntries)
            {
                var currentEntry = currentDeck.FirstOrDefault(e => e.cardDefinition == characterEntry.cardDefinition);
                if (currentEntry == null || currentEntry.quantity != characterEntry.quantity)
                {
                    return true;
                }
            }
            
            return false;
        }
        
        #endregion
    }
}