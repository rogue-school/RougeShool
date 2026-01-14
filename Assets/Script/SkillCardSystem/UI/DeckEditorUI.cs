using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using Zenject;
using Game.SkillCardSystem.Data;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.Deck;
using Game.CoreSystem.Utility;
using Game.VFXSystem.Pool;

namespace Game.SkillCardSystem.UI
{
    /// <summary>
    /// 덱 편집 UI 컨트롤러입니다.
    /// 게임 중에 플레이어 덱을 편집할 수 있는 인터페이스를 제공합니다.
    /// </summary>
    public class DeckEditorUI : MonoBehaviour
    {
        #region UI 컴포넌트

        [Header("덱 정보 UI")]
        [SerializeField] private TextMeshProUGUI deckInfoText;
        [SerializeField] private TextMeshProUGUI cardCountText;

        [Header("카드 목록 UI")]
        [SerializeField] private Transform cardListParent;
        [SerializeField] private GameObject cardEntryPrefab;

        [Header("카드 추가 UI")]
        [SerializeField] private Button addCardButton;
        [SerializeField] private Button removeCardButton;
        [SerializeField] private Button saveDeckButton;
        [SerializeField] private Button loadDeckButton;
        [SerializeField] private Button resetDeckButton;

        [Header("카드 선택 UI")]
        [SerializeField] private GameObject cardSelectionPanel;
        [SerializeField] private Transform availableCardsParent;
        [SerializeField] private GameObject availableCardPrefab;
        [SerializeField] private Button closeSelectionButton;

        #endregion

        #region 필드

        private IPlayerDeckManager deckManager;
        private List<SkillCardDefinition> availableCards = new();
        private SkillCardDefinition selectedCard;

        // Object Pooling
        private GenericUIPool<CardEntryUI> cardEntryPool;
        private GenericUIPool<AvailableCardUI> availableCardPool;
        private Transform poolContainer;

        #endregion

        #region 의존성 주입

        /// <summary>
        /// Zenject를 통한 의존성 주입 메서드입니다.
        /// </summary>
        /// <param name="deckManager">플레이어 덱 매니저</param>
        [Inject]
        public void Construct(IPlayerDeckManager deckManager)
        {
            this.deckManager = deckManager;
        }

        #endregion

        #region Unity 생명주기

        private void Start()
        {
            InitializePools();
            InitializeUI();
            LoadAvailableCards();
            RefreshDeckDisplay();
        }

        private void InitializePools()
        {
            // 풀 컨테이너 생성
            poolContainer = new GameObject("DeckEditorUI_PoolContainer").transform;
            poolContainer.SetParent(transform);
            poolContainer.gameObject.SetActive(false);

            // CardEntry 풀 초기화
            if (cardEntryPrefab != null)
            {
                cardEntryPool = new GenericUIPool<CardEntryUI>(
                    cardEntryPrefab,
                    poolContainer,
                    initialSize: 10,
                    maxSize: 50,
                    poolName: "CardEntryPool"
                );
            }

            // AvailableCard 풀 초기화
            if (availableCardPrefab != null)
            {
                availableCardPool = new GenericUIPool<AvailableCardUI>(
                    availableCardPrefab,
                    poolContainer,
                    initialSize: 20,
                    maxSize: 100,
                    poolName: "AvailableCardPool"
                );
            }
        }

        private void OnEnable()
        {
            RefreshDeckDisplay();
        }

        #endregion

        #region 초기화

        private void InitializeUI()
        {
            // 버튼 이벤트 연결
            if (addCardButton != null)
                addCardButton.onClick.AddListener(OnAddCardButtonClicked);
            
            if (removeCardButton != null)
                removeCardButton.onClick.AddListener(OnRemoveCardButtonClicked);
            
            if (saveDeckButton != null)
                saveDeckButton.onClick.AddListener(OnSaveDeckButtonClicked);
            
            if (loadDeckButton != null)
                loadDeckButton.onClick.AddListener(OnLoadDeckButtonClicked);
            
            if (resetDeckButton != null)
                resetDeckButton.onClick.AddListener(OnResetDeckButtonClicked);
            
            if (closeSelectionButton != null)
                closeSelectionButton.onClick.AddListener(OnCloseSelectionButtonClicked);

            // 카드 선택 패널 초기에는 비활성화
            if (cardSelectionPanel != null)
                cardSelectionPanel.SetActive(false);
        }

        private void LoadAvailableCards()
        {
            // Addressables에서 라벨 "SkillCards"로 모든 스킬카드 정의 로드
            try
            {
                var handle = UnityEngine.AddressableAssets.Addressables.LoadAssetsAsync<SkillCardDefinition>("SkillCards", null);
                var result = handle.WaitForCompletion();
                availableCards = result != null ? result.ToList() : new System.Collections.Generic.List<SkillCardDefinition>();
                
                GameLogger.LogInfo($"사용 가능한 카드 {availableCards.Count}개 로드 완료", GameLogger.LogCategory.SkillCard);
            }
            catch (System.Exception ex)
            {
                GameLogger.LogError($"[DeckEditorUI] 카드 로드 중 오류: {ex.Message}", GameLogger.LogCategory.SkillCard);
                availableCards = new System.Collections.Generic.List<SkillCardDefinition>();
            }
        }

        #endregion

        #region 덱 표시 업데이트

        private void RefreshDeckDisplay()
        {
            if (deckManager == null) return;

            var currentDeck = deckManager.GetCurrentDeck();
            int totalCards = deckManager.GetTotalCardCount();
            int uniqueCards = deckManager.GetUniqueCardCount();

            // 덱 정보 업데이트
            if (deckInfoText != null)
            {
                deckInfoText.text = $"덱 구성: {uniqueCards}종류, 총 {totalCards}장";
            }

            if (cardCountText != null)
            {
                cardCountText.text = $"카드 수: {totalCards}/30";
            }

            // 카드 목록 업데이트
            UpdateCardList(currentDeck);
        }

        private void UpdateCardList(List<PlayerSkillDeck.CardEntry> deckEntries)
        {
            if (cardListParent == null) return;

            // 기존 카드 엔트리 UI를 풀에 반환 (Object Pooling)
            if (cardEntryPool != null)
            {
                cardEntryPool.ReturnAll();
            }
            else
            {
                // Fallback: 풀이 없으면 Destroy
                foreach (Transform child in cardListParent)
                {
                    Destroy(child.gameObject);
                }
            }

            // 새 카드 엔트리 UI 생성
            foreach (var entry in deckEntries)
            {
                if (entry.cardDefinition != null)
                {
                    CreateCardEntryUI(entry);
                }
            }
        }

        private void CreateCardEntryUI(PlayerSkillDeck.CardEntry entry)
        {
            // Object Pooling 사용
            CardEntryUI cardEntryUI = null;
            if (cardEntryPool != null)
            {
                cardEntryUI = cardEntryPool.Get(cardListParent);
            }
            else if (cardEntryPrefab != null)
            {
                // Fallback
                var cardEntryObj = Instantiate(cardEntryPrefab, cardListParent);
                cardEntryUI = cardEntryObj.GetComponent<CardEntryUI>();
            }

            if (cardEntryUI != null)
            {
                cardEntryUI.Setup(entry, OnCardQuantityChanged, OnRemoveCardClicked);
            }
        }

        #endregion

        #region 카드 선택 UI

        private void ShowCardSelectionPanel()
        {
            if (cardSelectionPanel == null) return;

            cardSelectionPanel.SetActive(true);
            PopulateAvailableCards();
        }

        private void PopulateAvailableCards()
        {
            if (availableCardsParent == null) return;

            // 기존 카드 UI를 풀에 반환 (Object Pooling)
            if (availableCardPool != null)
            {
                availableCardPool.ReturnAll();
            }
            else
            {
                // Fallback: 풀이 없으면 Destroy
                foreach (Transform child in availableCardsParent)
                {
                    Destroy(child.gameObject);
                }
            }

            // 사용 가능한 카드 UI 생성 (Object Pooling)
            foreach (var card in availableCards)
            {
                AvailableCardUI cardUI = null;
                if (availableCardPool != null)
                {
                    cardUI = availableCardPool.Get(availableCardsParent);
                }
                else if (availableCardPrefab != null)
                {
                    // Fallback
                    var cardObj = Instantiate(availableCardPrefab, availableCardsParent);
                    cardUI = cardObj.GetComponent<AvailableCardUI>();
                }

                if (cardUI != null)
                {
                    cardUI.Setup(card, OnCardSelected);
                }
            }
        }

        #endregion

        #region 이벤트 핸들러

        private void OnAddCardButtonClicked()
        {
            ShowCardSelectionPanel();
        }

        private void OnRemoveCardButtonClicked()
        {
            // 선택된 카드가 있으면 제거
            if (selectedCard != null)
            {
                deckManager.RemoveCardFromDeck(selectedCard, 1);
                RefreshDeckDisplay();
                selectedCard = null;
            }
        }

        private void OnSaveDeckButtonClicked()
        {
            if (deckManager != null)
            {
                deckManager.SaveDeckConfiguration();
                GameLogger.LogInfo("덱 구성 저장 완료", GameLogger.LogCategory.SkillCard);
            }
        }

        private void OnLoadDeckButtonClicked()
        {
            if (deckManager != null)
            {
                deckManager.LoadDeckConfiguration();
                RefreshDeckDisplay();
                GameLogger.LogInfo("덱 구성 로드 완료", GameLogger.LogCategory.SkillCard);
            }
        }

        private void OnResetDeckButtonClicked()
        {
            if (deckManager != null)
            {
                deckManager.ResetToDefaultDeck();
                RefreshDeckDisplay();
                GameLogger.LogInfo("덱을 기본 구성으로 리셋", GameLogger.LogCategory.SkillCard);
            }
        }

        private void OnCloseSelectionButtonClicked()
        {
            if (cardSelectionPanel != null)
            {
                cardSelectionPanel.SetActive(false);
            }
        }

        private void OnCardSelected(SkillCardDefinition cardDefinition)
        {
            selectedCard = cardDefinition;
            
            // 카드 추가
            if (deckManager != null)
            {
                bool success = deckManager.AddCardToDeck(cardDefinition, 1);
                if (success)
                {
                    RefreshDeckDisplay();
                    OnCloseSelectionButtonClicked();
                }
            }
        }

        private void OnCardQuantityChanged(SkillCardDefinition cardDefinition, int newQuantity)
        {
            if (deckManager != null)
            {
                deckManager.SetCardQuantity(cardDefinition, newQuantity);
                RefreshDeckDisplay();
            }
        }

        private void OnRemoveCardClicked(SkillCardDefinition cardDefinition)
        {
            if (deckManager != null)
            {
                deckManager.RemoveAllCardsFromDeck(cardDefinition);
                RefreshDeckDisplay();
            }
        }

        #endregion
    }

    /// <summary>
    /// 카드 엔트리 UI 컴포넌트
    /// </summary>
    public class CardEntryUI : MonoBehaviour
    {
        [Header("UI 컴포넌트")]
        [SerializeField] private TextMeshProUGUI cardNameText;
        [SerializeField] private TextMeshProUGUI quantityText;
        [SerializeField] private Button increaseButton;
        [SerializeField] private Button decreaseButton;
        [SerializeField] private Button removeButton;

        private SkillCardDefinition cardDefinition;
        private int currentQuantity;
        private System.Action<SkillCardDefinition, int> onQuantityChanged;
        private System.Action<SkillCardDefinition> onRemoveClicked;

        /// <summary>
        /// 카드 엔트리 UI를 설정합니다.
        /// </summary>
        /// <param name="entry">카드 엔트리 데이터</param>
        /// <param name="onQuantityChanged">수량 변경 시 호출될 콜백</param>
        /// <param name="onRemoveClicked">제거 버튼 클릭 시 호출될 콜백</param>
        public void Setup(PlayerSkillDeck.CardEntry entry, 
                         System.Action<SkillCardDefinition, int> onQuantityChanged,
                         System.Action<SkillCardDefinition> onRemoveClicked)
        {
            this.cardDefinition = entry.cardDefinition;
            this.currentQuantity = entry.quantity;
            this.onQuantityChanged = onQuantityChanged;
            this.onRemoveClicked = onRemoveClicked;

            UpdateDisplay();

            // 버튼 이벤트 연결
            if (increaseButton != null)
                increaseButton.onClick.AddListener(OnIncreaseClicked);
            
            if (decreaseButton != null)
                decreaseButton.onClick.AddListener(OnDecreaseClicked);
            
            if (removeButton != null)
                removeButton.onClick.AddListener(OnRemoveClicked);
        }

        private void UpdateDisplay()
        {
            if (cardNameText != null)
                cardNameText.text = cardDefinition?.displayName ?? "Unknown Card";
            
            if (quantityText != null)
                quantityText.text = currentQuantity.ToString();
        }

        private void OnIncreaseClicked()
        {
            currentQuantity++;
            onQuantityChanged?.Invoke(cardDefinition, currentQuantity);
            UpdateDisplay();
        }

        private void OnDecreaseClicked()
        {
            if (currentQuantity > 1)
            {
                currentQuantity--;
                onQuantityChanged?.Invoke(cardDefinition, currentQuantity);
                UpdateDisplay();
            }
        }

        private void OnRemoveClicked()
        {
            onRemoveClicked?.Invoke(cardDefinition);
        }
    }

    /// <summary>
    /// 사용 가능한 카드 UI 컴포넌트
    /// </summary>
    public class AvailableCardUI : MonoBehaviour
    {
        [Header("UI 컴포넌트")]
        [SerializeField] private TextMeshProUGUI cardNameText;
        [SerializeField] private TextMeshProUGUI cardDescriptionText;
        [SerializeField] private Button selectButton;

        private SkillCardDefinition cardDefinition;
        private System.Action<SkillCardDefinition> onCardSelected;

        /// <summary>
        /// 사용 가능한 카드 UI를 설정합니다.
        /// </summary>
        /// <param name="cardDefinition">카드 정의</param>
        /// <param name="onCardSelected">카드 선택 시 호출될 콜백</param>
        public void Setup(SkillCardDefinition cardDefinition, System.Action<SkillCardDefinition> onCardSelected)
        {
            this.cardDefinition = cardDefinition;
            this.onCardSelected = onCardSelected;

            UpdateDisplay();

            if (selectButton != null)
                selectButton.onClick.AddListener(OnSelectClicked);
        }

        private void UpdateDisplay()
        {
            if (cardNameText != null)
                cardNameText.text = cardDefinition?.displayName ?? "Unknown Card";
            
            if (cardDescriptionText != null)
                cardDescriptionText.text = cardDefinition?.description ?? "No description";
        }

        private void OnSelectClicked()
        {
            onCardSelected?.Invoke(cardDefinition);
        }
    }
}