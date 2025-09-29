using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Threading.Tasks;
using System.Collections.Generic;
using DG.Tweening;
using Game.CoreSystem.Interface;
using Game.CoreSystem.UI;
using Game.CoreSystem.Save;
using Game.CoreSystem.Utility;
 using Game.CharacterSystem.Data;
 using Game.CharacterSystem.Core;
 using Game.SkillCardSystem.Data;
 using Game.SkillCardSystem.Deck;
using Game.SkillCardSystem.Factory;
using Game.SkillCardSystem.UI;
using Zenject;

namespace Game.UISystem
{
    /// <summary>
    /// Inspector 필드를 사용하는 모던한 메인 메뉴 컨트롤러
    /// </summary>
    public class MainMenuController : MonoBehaviour
    {
        #region 의존성 주입
        
        [Inject] private IGameStateManager gameStateManager;
        [Inject] private SettingsManager settingsManager;
        [Inject] private ISaveManager saveManager;
        [Inject] private IPlayerCharacterSelectionManager playerCharacterSelectionManager;
        
        [Header("선택 캐릭터 공유 데이터")]
        [Tooltip("메인 로비/씬 간 선택 캐릭터를 공유하는 SO")]
        [SerializeField] private SelectedCharacterSO selectedCharacterSO;
        
        #endregion
        
        #region Inspector 필드
        
        [Header("메인 메뉴")]
        [Tooltip("게임 타이틀 텍스트")]
        [SerializeField] private TextMeshProUGUI gameTitleText;
        
        [Tooltip("메인 메뉴 루트 패널 (버튼 등이 들어있는 상위 오브젝트)")]
        [SerializeField] private GameObject mainMenuPanel;
        
        [Tooltip("새 게임 버튼")]
        [SerializeField] private Button newGameButton;
        
        [Tooltip("이어하기 버튼")]
        [SerializeField] private Button continueButton;
        
        [Tooltip("크레딧 버튼")]
        [SerializeField] private Button creditsButton;
        
        [Tooltip("종료 버튼")]
        [SerializeField] private Button exitButton;
        
        [Header("캐릭터 선택")]
        [Tooltip("캐릭터 선택 패널")]
        [SerializeField] private GameObject characterSelectionPanel;
        
        [Tooltip("캐릭터 카드 컨테이너")]
        [SerializeField] private Transform characterCardContainer;
        
        [Tooltip("캐릭터 카드 프리팹")]
        [SerializeField] private GameObject characterCardPrefab;
        
        [Tooltip("선택 하이라이트")]
        [SerializeField] private GameObject selectionHighlight;
        
        [Header("게임 시작")]
        [Tooltip("게임 시작 패널")]
        [SerializeField] private GameObject gameStartPanel;
        
        [Tooltip("선택된 캐릭터 이미지")]
        [SerializeField] private Image selectedCharacterImage;
        
        [Tooltip("선택된 캐릭터 엠블럼 이미지")]
        [SerializeField] private Image selectedCharacterEmblemImage;

        [Tooltip("선택된 캐릭터의 스킬 아이콘 이미지들 (3개)")]
        [SerializeField] private Image[] selectedCharacterSkillIcons = new Image[3];

        [Tooltip("선택된 캐릭터 이름")]
        [SerializeField] private TextMeshProUGUI selectedCharacterName;
        
        [Tooltip("선택된 캐릭터 설명")]
        [SerializeField] private TextMeshProUGUI selectedCharacterDescription;
        
        [Tooltip("게임 시작 버튼")]
        [SerializeField] private Button startGameButton;
        
        [Tooltip("캐릭터 다시 선택 버튼")]
        [SerializeField] private Button reselectCharacterButton;

        [Header("미리보기 카드(실제 프리팹)")]
        [Tooltip("메뉴에서 미리보기로 사용할 스킬 카드 UI 프리팹")]
        [SerializeField] private SkillCardUI skillCardUIPrefab;
        
        [Tooltip("스킬 카드 미리보기 부모 컨테이너 (자식 정리 후 최대 3개 생성)")]
        [SerializeField] private Transform skillCardPreviewContainer;

        [Header("고정 캐릭터 매핑 (버튼 ↔ 데이터 1:1)")]
        [Tooltip("검 캐릭터 선택 버튼")]
        [SerializeField] private Button swordButton;
        [Tooltip("활 캐릭터 선택 버튼")]
        [SerializeField] private Button bowButton;
        [Tooltip("지팡이 캐릭터 선택 버튼")]
        [SerializeField] private Button staffButton;

        [Header("버튼별 프리뷰 SO (GameStartPanel 표시용)")]
        [Tooltip("검 캐릭터 프리뷰 SO (엠블럼/설명/스킬3")]
        [SerializeField] private SelectedCharacterSO swordPreviewSO;
        [Tooltip("활 캐릭터 프리뷰 SO")]
        [SerializeField] private SelectedCharacterSO bowPreviewSO;
        [Tooltip("지팡이 캐릭터 프리뷰 SO")]
        [SerializeField] private SelectedCharacterSO staffPreviewSO;

        [Header("버튼별 실제 게임용 캐릭터 데이터")]
        [Tooltip("검 캐릭터 데이터(실제 게임에서 사용)")]
        [SerializeField] private PlayerCharacterData swordCharacter;
        [Tooltip("활 캐릭터 데이터(실제 게임에서 사용)")]
        [SerializeField] private PlayerCharacterData bowCharacter;
        [Tooltip("지팡이 캐릭터 데이터(실제 게임에서 사용)")]
        [SerializeField] private PlayerCharacterData staffCharacter;
        
        [Header("크레딧")]
        [Tooltip("크레딧 패널")]
        [SerializeField] private GameObject creditsPanel;
        
        [Tooltip("크레딧 제목")]
        [SerializeField] private TextMeshProUGUI creditsTitle;
        
        [Tooltip("크레딧 내용")]
        [SerializeField] private TextMeshProUGUI creditsContent;
        
        [Tooltip("메인 메뉴로 버튼")]
        [SerializeField] private Button backToMenuButton;
        
        // 설정 UI 제거됨 (요청에 따라 인스펙터 비우기)
        
        #endregion
        
        #region 상태 변수
        
        private PlayerCharacterData[] availableCharacters;
        private PlayerCharacterData selectedCharacter;
        private List<GameObject> characterCards = new List<GameObject>();
        
        #endregion
        
        #region 초기화
        
        private void Start()
        {
            GameLogger.LogInfo("[MainMenuController] 초기화 시작", GameLogger.LogCategory.UI);
            
            InitializeUI();
            ValidateInspectorBindings();
            LoadCharacterData();
            CreateCharacterCards();
            BindFixedCharacterButtons();
            // 설정 기능 제거됨
            UpdateContinueButtonState();
            
            // 초기 애니메이션
            PlayInitialAnimation();
            
            GameLogger.LogInfo("[MainMenuController] 초기화 완료", GameLogger.LogCategory.UI);
        }

        /// <summary>
        /// 인스펙터 연결 상태를 검증하고 누락 항목을 경고합니다.
        /// </summary>
        private void ValidateInspectorBindings()
        {
            if (newGameButton == null) GameLogger.LogError("[MainMenuController] 새 게임 버튼(newGameButton)이 연결되지 않았습니다", GameLogger.LogCategory.Error);
            if (startGameButton == null) GameLogger.LogError("[MainMenuController] 시작 버튼(startGameButton)이 연결되지 않았습니다", GameLogger.LogCategory.Error);
            if (reselectCharacterButton == null) GameLogger.LogWarning("[MainMenuController] 다시 선택 버튼이 연결되지 않았습니다", GameLogger.LogCategory.UI);
            if (characterSelectionPanel == null) GameLogger.LogWarning("[MainMenuController] 캐릭터 선택 패널이 연결되지 않았습니다", GameLogger.LogCategory.UI);
            if (gameStartPanel == null) GameLogger.LogWarning("[MainMenuController] 게임 시작 패널이 연결되지 않았습니다", GameLogger.LogCategory.UI);
            if (mainMenuPanel == null) GameLogger.LogWarning("[MainMenuController] 메인 메뉴 패널이 연결되지 않았습니다 (겹침 발생 가능)", GameLogger.LogCategory.UI);
            if (swordButton == null || bowButton == null || staffButton == null)
                GameLogger.LogWarning("[MainMenuController] 고정 캐릭터 버튼(검/활/지팡이) 중 일부가 비었습니다", GameLogger.LogCategory.UI);
            if (swordPreviewSO == null || bowPreviewSO == null || staffPreviewSO == null)
                GameLogger.LogWarning("[MainMenuController] 프리뷰 SO(검/활/지팡이) 중 일부가 비었습니다", GameLogger.LogCategory.UI);
            if (swordCharacter == null || bowCharacter == null || staffCharacter == null)
                GameLogger.LogWarning("[MainMenuController] 실제 게임용 캐릭터 데이터(검/활/지팡이) 중 일부가 비었습니다", GameLogger.LogCategory.UI);
        }

        /// <summary>
        /// 인스펙터에서 지정한 버튼 3개와 캐릭터 데이터 3개를 1:1로 바인딩합니다.
        /// </summary>
        private void BindFixedCharacterButtons()
        {
            BindFixed(swordButton, swordPreviewSO, swordCharacter, "검");
            BindFixed(bowButton, bowPreviewSO, bowCharacter, "활");
            BindFixed(staffButton, staffPreviewSO, staffCharacter, "지팡이");
        }

        private void BindFixed(Button button, SelectedCharacterSO previewSO, PlayerCharacterData gameCharacter, string label)
        {
            if (button == null)
            {
                return; // 선택적 기능
            }
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() =>
            {
                // 프리뷰 SO를 SelectedCharacterSO에 복사 저장하고, 시작용 캐릭터를 보관
                ApplyPreviewSOToSelected(previewSO, label);
                pendingStartCharacter = gameCharacter;
                // 실제 시작에 사용될 캐릭터를 선택 상태로도 보관하여 Start 버튼 가드 통과
                selectedCharacter = gameCharacter;
                // 패널 전환 및 UI 갱신
                if (characterSelectionPanel != null) characterSelectionPanel.SetActive(false);
                UpdateSelectedCharacterInfo();
                ShowGameStartPanel();
            });
        }

        // 시작 버튼에서 사용할 실제 게임 캐릭터 임시 저장
        private PlayerCharacterData pendingStartCharacter;
        
        /// <summary>
        /// UI 초기화
        /// </summary>
        private void InitializeUI()
        {
            // 메인 메뉴 버튼 이벤트 연결
            if (newGameButton != null)
                newGameButton.onClick.AddListener(OnNewGameButtonClicked);
            
            if (continueButton != null)
                continueButton.onClick.AddListener(OnContinueButtonClicked);
            
            if (creditsButton != null)
                creditsButton.onClick.AddListener(OnCreditsButtonClicked);
            
            if (exitButton != null)
                exitButton.onClick.AddListener(OnExitButtonClicked);
            
            // 게임 시작 패널 버튼 이벤트 연결
            if (startGameButton != null)
                startGameButton.onClick.AddListener(OnStartGameButtonClicked);
            
            if (reselectCharacterButton != null)
                reselectCharacterButton.onClick.AddListener(OnReselectCharacterButtonClicked);
            
            // 크레딧 패널 버튼 이벤트 연결
            if (backToMenuButton != null)
                backToMenuButton.onClick.AddListener(OnBackToMenuButtonClicked);
            
            // 초기 상태 설정
            if (gameStartPanel != null)
                gameStartPanel.SetActive(false);
            
            if (characterSelectionPanel != null)
                characterSelectionPanel.SetActive(false);
            
            if (creditsPanel != null)
                creditsPanel.SetActive(false);
            // 설정 패널 제거됨
        }
        
        /// <summary>
        /// 캐릭터 데이터 로드
        /// </summary>
        private void LoadCharacterData()
        {
            availableCharacters = Resources.LoadAll<PlayerCharacterData>("CharacterData");
            
            if (availableCharacters == null || availableCharacters.Length == 0)
            {
                GameLogger.LogWarning("[MainMenuController] 캐릭터 데이터를 찾을 수 없습니다!", GameLogger.LogCategory.Error);
                return;
            }
            
            GameLogger.LogInfo($"[MainMenuController] {availableCharacters.Length}개의 캐릭터 데이터 로드 완료", GameLogger.LogCategory.UI);
        }
        
        /// <summary>
        /// 캐릭터 카드들 생성
        /// </summary>
        private void CreateCharacterCards()
        {
            if (characterCardContainer == null)
            {
                GameLogger.LogWarning("[MainMenuController] 캐릭터 카드 생성에 필요한 요소가 없습니다!", GameLogger.LogCategory.Error);
                return;
            }

            if (availableCharacters == null || availableCharacters.Length == 0)
            {
                GameLogger.LogWarning("[MainMenuController] 캐릭터 데이터가 없어 카드 생성을 건너뜁니다", GameLogger.LogCategory.UI);
                return;
            }

            // 기존 캐시된 리스트 정리
            foreach (var card in characterCards)
            {
                if (card != null)
                    DestroyImmediate(card);
            }
            characterCards.Clear();

            if (characterCardPrefab != null)
            {
                // 프리팹이 연결된 경우: 프리팹 기준으로 생성
                for (int i = 0; i < availableCharacters.Length; i++)
                {
                    var characterData = availableCharacters[i];
                    var cardObject = Instantiate(characterCardPrefab, characterCardContainer);
                    characterCards.Add(cardObject);

                    // 카드 설정
                    SetupCharacterCard(cardObject, characterData, i);
                }
            }
            else
            {
                // 프리팹이 없을 때: 컨테이너의 기존 자식들을 활용하여 이벤트 바인딩 및 데이터 반영
                GameLogger.LogInfo("[MainMenuController] 카드 프리팹이 비어 있어 컨테이너 자식 오브젝트를 활용합니다", GameLogger.LogCategory.UI);

                int childCount = characterCardContainer.childCount;
                if (childCount == 0)
                {
                    GameLogger.LogError("[MainMenuController] 카드 프리팹도 없고 컨테이너에 자식 카드도 없습니다", GameLogger.LogCategory.Error);
                    return;
                }

                int setupCount = Mathf.Min(childCount, availableCharacters.Length);
                for (int i = 0; i < setupCount; i++)
                {
                    var child = characterCardContainer.GetChild(i)?.gameObject;
                    if (child == null) continue;
                    characterCards.Add(child);
                    SetupCharacterCard(child, availableCharacters[i], i);
                }

                // 남는 카드가 있다면 비활성화 처리(데이터 없는 슬롯은 클릭 방지)
                for (int i = setupCount; i < childCount; i++)
                {
                    var extra = characterCardContainer.GetChild(i)?.gameObject;
                    if (extra != null) extra.SetActive(false);
                }
            }
        }
        
        /// <summary>
        /// 캐릭터 카드 설정
        /// </summary>
        private void SetupCharacterCard(GameObject cardObject, PlayerCharacterData characterData, int index)
        {
            // 카드 컴포넌트 찾기
            var cardButton = cardObject.GetComponent<Button>();
            if (cardButton == null)
            {
                cardButton = cardObject.GetComponentInChildren<Button>(true);
            }
            var characterImage = cardObject.GetComponentInChildren<Image>();
            var characterNameText = cardObject.GetComponentInChildren<TextMeshProUGUI>();
            
            // 이벤트 연결
            if (cardButton != null)
            {
                // 중복 바인딩 방지
                cardButton.onClick.RemoveAllListeners();
                int capturedIndex = index; // 클로저를 위한 로컬 변수
                cardButton.onClick.AddListener(() => OnCharacterCardClicked(capturedIndex));
            }
            else
            {
                GameLogger.LogWarning($"[MainMenuController] 캐릭터 카드 프리팹에 Button이 없습니다. 인덱스: {index}, 오브젝트: {cardObject.name}", GameLogger.LogCategory.UI);
            }
            
            // 데이터 설정
            if (characterImage != null && characterData.Portrait != null)
                characterImage.sprite = characterData.Portrait;
            
            if (characterNameText != null)
                characterNameText.text = characterData.DisplayName;
            
            // 즉시 표시 (애니메이션 제거)
            cardObject.transform.localScale = Vector3.one;
        }
        
        // 설정 로드 기능 제거됨
        
        /// <summary>
        /// 이어하기 버튼 상태 업데이트
        /// </summary>
        private void UpdateContinueButtonState()
        {
            if (continueButton != null)
            {
                bool hasStageProgressSave = saveManager?.HasStageProgressSave() ?? false;
                continueButton.interactable = hasStageProgressSave;
                
                var buttonText = continueButton.GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                {
                    buttonText.text = "이어하기";
                    buttonText.color = hasStageProgressSave ? Color.white : Color.gray;
                }
            }
        }
        
        #endregion
        
        #region 애니메이션
        
        /// <summary>
        /// 초기 애니메이션 재생
        /// </summary>
        private void PlayInitialAnimation()
        {
            // 애니메이션 제거: 즉시 표시 상태로 설정
            if (gameTitleText != null)
            {
                gameTitleText.transform.localScale = Vector3.one;
            }
            var buttons = new Button[] { newGameButton, continueButton, creditsButton, exitButton };
            for (int i = 0; i < buttons.Length; i++)
            {
                if (buttons[i] != null)
                {
                    buttons[i].transform.localScale = Vector3.one;
                }
            }
        }
        
        /// <summary>
        /// 게임 시작 패널 표시 애니메이션
        /// </summary>
        private void ShowGameStartPanel()
        {
            if (gameStartPanel != null)
            {
                gameStartPanel.SetActive(true);
                gameStartPanel.transform.localScale = Vector3.one;
            }
            if (mainMenuPanel != null)
                mainMenuPanel.SetActive(false);
        }
        
        /// <summary>
        /// 게임 시작 패널 숨기기 애니메이션
        /// </summary>
        private void HideGameStartPanel()
        {
            if (gameStartPanel != null)
            {
                gameStartPanel.SetActive(false);
            }
            if (mainMenuPanel != null)
                mainMenuPanel.SetActive(true);
        }
        
        #endregion
        
        #region 이벤트 핸들러
        
        /// <summary>
        /// 새 게임 버튼 클릭
        /// </summary>
        private void OnNewGameButtonClicked()
        {
            GameLogger.LogInfo("[MainMenuController] 새 게임 버튼 클릭", GameLogger.LogCategory.UI);
            
            // 기존 저장 데이터 초기화
            if (saveManager != null)
            {
                saveManager.InitializeNewGame();
                GameLogger.LogInfo("[MainMenuController] 새 게임 초기화 완료", GameLogger.LogCategory.Save);
            }
            // 저장 불사용/신규 시작 플래그 설정 (StageScene에서 저장 복원 루틴 우회)
            PlayerPrefs.SetInt("RESUME_REQUESTED", 0);
            PlayerPrefs.SetInt("NEW_GAME_REQUESTED", 1);
            PlayerPrefs.SetInt("START_STAGE_NUMBER", 1);
            PlayerPrefs.SetInt("START_ENEMY_INDEX", 0);
            PlayerPrefs.Save();
            
            // 캐릭터 선택 모드로 전환
            ShowCharacterSelection();
        }
        
        /// <summary>
        /// 이어하기 버튼 클릭
        /// </summary>
        private async void OnContinueButtonClicked()
        {
            GameLogger.LogInfo("[MainMenuController] 이어하기 버튼 클릭", GameLogger.LogCategory.UI);
            
            if (saveManager == null)
            {
                GameLogger.LogError("[MainMenuController] SaveManager가 없습니다!", GameLogger.LogCategory.Error);
                return;
            }
            
            try
            {
                // 오디오 설정 로드
                var (bgm, sfx) = saveManager.LoadAudioSettings();
                
                // 이어하기 플래그 설정
                PlayerPrefs.SetInt("RESUME_REQUESTED", 1);
                PlayerPrefs.Save();
                
                // 스테이지 씬으로 전환
                var sceneLoader = FindFirstObjectByType<Game.CoreSystem.Manager.SceneTransitionManager>();
                if (sceneLoader != null)
                {
                    await sceneLoader.TransitionToStageScene();
                    
                    // 스테이지 진행 상황 복원
                    bool loadSuccess = await saveManager.LoadStageProgress();
                    if (!loadSuccess)
                    {
                        GameLogger.LogWarning("[MainMenuController] 이어하기 실패, 새 게임으로 폴백", GameLogger.LogCategory.Save);
                        OnNewGameButtonClicked();
                    }
                }
            }
            catch (System.Exception ex)
            {
                GameLogger.LogError($"[MainMenuController] 이어하기 중 오류: {ex.Message}", GameLogger.LogCategory.Error);
                OnNewGameButtonClicked();
            }
        }
        
        /// <summary>
        /// 크레딧 버튼 클릭
        /// </summary>
        private void OnCreditsButtonClicked()
        {
            GameLogger.LogInfo("[MainMenuController] 크레딧 버튼 클릭", GameLogger.LogCategory.UI);
            
            if (creditsPanel != null)
            {
                creditsPanel.SetActive(true);
                creditsPanel.transform.localScale = Vector3.zero;
                creditsPanel.transform.DOScale(Vector3.one, 0.3f)
                    .SetEase(Ease.OutBack);
            }
        }
        
        /// <summary>
        /// 메인 메뉴로 버튼 클릭
        /// </summary>
        private void OnBackToMenuButtonClicked()
        {
            GameLogger.LogInfo("[MainMenuController] 메인 메뉴로 버튼 클릭", GameLogger.LogCategory.UI);
            
            if (creditsPanel != null)
            {
                creditsPanel.transform.DOScale(Vector3.zero, 0.2f)
                    .SetEase(Ease.InBack)
                    .OnComplete(() => creditsPanel.SetActive(false));
            }
        }
        
        /// <summary>
        /// 종료 버튼 클릭
        /// </summary>
        private void OnExitButtonClicked()
        {
            GameLogger.LogInfo("[MainMenuController] 게임 종료", GameLogger.LogCategory.UI);
            gameStateManager.ExitGame();
        }
        
        /// <summary>
        /// 캐릭터 카드 클릭
        /// </summary>
        private void OnCharacterCardClicked(int characterIndex)
        {
            if (characterIndex < 0 || characterIndex >= availableCharacters.Length)
            {
                GameLogger.LogWarning($"[MainMenuController] 잘못된 캐릭터 인덱스: {characterIndex}", GameLogger.LogCategory.Error);
                return;
            }
            
            selectedCharacter = availableCharacters[characterIndex];
            TryUpdateSelectedCharacterSOFrom(selectedCharacter);
            GameLogger.LogInfo($"[MainMenuController] 캐릭터 선택: {selectedCharacter.DisplayName}", GameLogger.LogCategory.UI);
            
            // SO에 요약 반영 및 시작용 캐릭터 후보 보관
            TryUpdateSelectedCharacterSOFrom(selectedCharacter);
            pendingStartCharacter = selectedCharacter;
            // 선택된 캐릭터 정보 업데이트 및 전환
            UpdateSelectedCharacterInfo();
            if (characterSelectionPanel != null) characterSelectionPanel.SetActive(false);
            ShowGameStartPanel();
            
            // 선택 하이라이트 업데이트
            UpdateSelectionHighlight(characterIndex);
        }

        /// <summary>
        /// 외부(캐릭터 셀렉터 등)에서 직접 캐릭터 데이터를 전달해 선택 처리합니다.
        /// </summary>
        /// <param name="character">선택할 캐릭터 데이터</param>
        public void SelectCharacterFromExternal(PlayerCharacterData character)
        {
            if (character == null)
            {
                GameLogger.LogWarning("[MainMenuController] 외부 선택 캐릭터가 null입니다", GameLogger.LogCategory.UI);
                return;
            }

            selectedCharacter = character;
            TryUpdateSelectedCharacterSOFrom(selectedCharacter);
            pendingStartCharacter = selectedCharacter;
            GameLogger.LogInfo($"[MainMenuController] 외부에서 캐릭터 선택: {selectedCharacter.DisplayName}", GameLogger.LogCategory.UI);

            // 캐릭터 선택 패널 숨기고 게임 시작 패널로 전환
            if (characterSelectionPanel != null)
            {
                characterSelectionPanel.SetActive(false);
            }
            UpdateSelectedCharacterInfo();
            ShowGameStartPanel();
        }

        /// <summary>
        /// PlayerCharacterData에서 SelectedCharacterSO 요약 정보를 구성해 저장합니다.
        /// </summary>
        private void TryUpdateSelectedCharacterSOFrom(PlayerCharacterData character)
        {
            if (selectedCharacterSO == null || character == null)
                return;

            // 설명은 기존 방식 사용
            string desc = GetCharacterDescription(character.CharacterType);

            // 스킬 3개 추출
            var skills = new List<Game.SkillCardSystem.Data.SkillCardDefinition>();
            try
            {
                if (character.SkillDeck != null)
                {
                    var entries = character.SkillDeck.GetAllCardEntries();
                    foreach (var e in entries)
                    {
                        if (e?.cardDefinition != null)
                        {
                            skills.Add(e.cardDefinition);
                            if (skills.Count >= 3) break;
                        }
                    }
                }
            }
            catch { }

            selectedCharacterSO.Set(character.Emblem, desc, skills.ToArray());
        }
        
        /// <summary>
        /// 게임 시작 버튼 클릭
        /// </summary>
        private async void OnStartGameButtonClicked()
        {
            var startCharacter = pendingStartCharacter != null ? pendingStartCharacter : selectedCharacter;
            if (startCharacter == null)
            {
                GameLogger.LogWarning("[MainMenuController] 캐릭터가 선택되지 않았습니다!", GameLogger.LogCategory.Error);
                return;
            }
            
            GameLogger.LogInfo($"[MainMenuController] 게임 시작: {startCharacter.DisplayName}", GameLogger.LogCategory.UI);
            
            try
            {
                // 코어 시스템에 선택 캐릭터 전달 (StageScene에서 사용할 수 있도록)
                if (playerCharacterSelectionManager != null)
                {
                    playerCharacterSelectionManager.SelectCharacter(startCharacter);
                }
                if (gameStateManager != null)
                {
                    gameStateManager.SelectCharacter(startCharacter);
                }
                // 선택된 캐릭터 정보 저장 (폴백용)
                PlayerPrefs.SetString("SELECTED_CHARACTER_TYPE", startCharacter.CharacterType.ToString());
                PlayerPrefs.Save();
                
                // 스테이지 씬으로 전환
                var sceneLoader = FindFirstObjectByType<Game.CoreSystem.Manager.SceneTransitionManager>();
                if (sceneLoader != null)
                {
                    await sceneLoader.TransitionToStageScene();
                    GameLogger.LogInfo("[MainMenuController] 스테이지 씬으로 전환 완료", GameLogger.LogCategory.UI);
                }
                else
                {
                    GameLogger.LogError("[MainMenuController] 씬 전환 매니저를 찾을 수 없습니다!", GameLogger.LogCategory.Error);
                }
            }
            catch (System.Exception ex)
            {
                GameLogger.LogError($"[MainMenuController] 게임 시작 실패: {ex.Message}", GameLogger.LogCategory.Error);
            }
        }
        
        /// <summary>
        /// 캐릭터 다시 선택 버튼 클릭
        /// </summary>
        private void OnReselectCharacterButtonClicked()
        {
            GameLogger.LogInfo("[MainMenuController] 캐릭터 다시 선택", GameLogger.LogCategory.UI);
            HideGameStartPanel();
            ShowCharacterSelection();
        }
        
        // 설정 관련 이벤트 제거됨
        
        #endregion
        
        #region UI 업데이트
        
        /// <summary>
        /// 캐릭터 선택 표시
        /// </summary>
        private void ShowCharacterSelection()
        {
            if (characterSelectionPanel != null)
            {
                characterSelectionPanel.SetActive(true);
                characterSelectionPanel.transform.localScale = Vector3.one;
            }
            if (mainMenuPanel != null)
                mainMenuPanel.SetActive(false);
        }
        
        /// <summary>
        /// 선택된 캐릭터 정보 업데이트
        /// </summary>
        private void UpdateSelectedCharacterInfo()
        {
            // SO 기반으로 GameStartPanel을 채움 (없으면 PlayerCharacterData로 폴백)
            bool usedSO = false;
            if (selectedCharacterSO != null)
            {
                // 이름/설명/이미지 세팅
                if (selectedCharacterName != null)
                    selectedCharacterName.text = selectedCharacter != null ? selectedCharacter.DisplayName : selectedCharacterName.text;

                if (selectedCharacterDescription != null)
                    selectedCharacterDescription.text = !string.IsNullOrEmpty(selectedCharacterSO.Description)
                        ? selectedCharacterSO.Description
                        : (selectedCharacter != null ? GetCharacterDescription(selectedCharacter.CharacterType) : selectedCharacterDescription.text);

                if (selectedCharacterEmblemImage != null)
                {
                    if (selectedCharacterSO.Emblem != null)
                    {
                        selectedCharacterEmblemImage.sprite = selectedCharacterSO.Emblem;
                        selectedCharacterEmblemImage.enabled = true;
                    }
                    else
                    {
                        selectedCharacterEmblemImage.enabled = false;
                    }
                }

                if (selectedCharacterImage != null && selectedCharacter?.Portrait != null)
                    selectedCharacterImage.sprite = selectedCharacter.Portrait;

                // 아이콘 3개: SO.SkillCards 우선
                try
                {
                    if (selectedCharacterSkillIcons != null && selectedCharacterSkillIcons.Length > 0)
                    {
                        int filled = 0;
                        for (int i = 0; i < selectedCharacterSkillIcons.Length; i++)
                        {
                            if (selectedCharacterSkillIcons[i] != null)
                                selectedCharacterSkillIcons[i].enabled = false;
                        }
                        if (selectedCharacterSO.SkillCards != null)
                        {
                            foreach (var def in selectedCharacterSO.SkillCards)
                            {
                                if (def == null) continue;
                                if (filled >= selectedCharacterSkillIcons.Length) break;
                                var icon = def.artwork;
                                if (icon != null)
                                {
                                    selectedCharacterSkillIcons[filled].sprite = icon;
                                    selectedCharacterSkillIcons[filled].enabled = true;
                                    filled++;
                                }
                            }
                        }
                    }
                }
                catch { }

                usedSO = true;
            }

            if (!usedSO && selectedCharacter != null)
            {
                if (selectedCharacterName != null)
                    selectedCharacterName.text = selectedCharacter.DisplayName;
                if (selectedCharacterDescription != null)
                    selectedCharacterDescription.text = GetCharacterDescription(selectedCharacter.CharacterType);
                if (selectedCharacterImage != null && selectedCharacter.Portrait != null)
                    selectedCharacterImage.sprite = selectedCharacter.Portrait;
                if (selectedCharacterEmblemImage != null)
                {
                    if (selectedCharacter.Emblem != null)
                    {
                        selectedCharacterEmblemImage.sprite = selectedCharacter.Emblem;
                        selectedCharacterEmblemImage.enabled = true;
                    }
                    else selectedCharacterEmblemImage.enabled = false;
                }
            }

            // 대표 스킬 3개 아이콘 설정
            try
            {
                if (!usedSO && selectedCharacter != null && selectedCharacter.SkillDeck != null && selectedCharacterSkillIcons != null && selectedCharacterSkillIcons.Length > 0)
                {
                    var entries = selectedCharacter.SkillDeck.GetAllCardEntries();
                    int filled = 0;
                    for (int i = 0; i < selectedCharacterSkillIcons.Length; i++)
                    {
                        if (selectedCharacterSkillIcons[i] == null) continue;
                        selectedCharacterSkillIcons[i].enabled = false;
                    }

                    foreach (var entry in entries)
                    {
                        if (entry == null || entry.cardDefinition == null) continue;
                        if (filled >= selectedCharacterSkillIcons.Length) break;

                        var icon = entry.cardDefinition.artwork;
                        if (icon != null)
                        {
                            selectedCharacterSkillIcons[filled].sprite = icon;
                            selectedCharacterSkillIcons[filled].enabled = true;
                            filled++;
                        }
                    }

                    // 부족하면 비활성화 유지
                    if (filled < selectedCharacterSkillIcons.Length)
                    {
                        GameLogger.LogWarning($"[MainMenuController] 대표 스킬 아이콘이 {filled}개만 설정되었습니다", GameLogger.LogCategory.UI);
                    }
                }
            }
            catch (System.Exception ex)
            {
                GameLogger.LogError($"[MainMenuController] 스킬 아이콘 설정 실패: {ex.Message}", GameLogger.LogCategory.Error);
            }

            // 시작 버튼 라벨 "입학하기"로 설정
            if (startGameButton != null)
            {
                var label = startGameButton.GetComponentInChildren<TextMeshProUGUI>();
                if (label != null) label.text = "입학하기";
            }

            // 실제 카드 프리팹을 이용한 미리보기 생성 (SO 우선)
            UpdateSkillCardPreviews();
        }

        /// <summary>
        /// 선택된 캐릭터의 대표 스킬 3개를 실제 카드 UI 프리팹으로 미리보기 생성
        /// </summary>
        private void UpdateSkillCardPreviews()
        {
            if (skillCardPreviewContainer == null || skillCardUIPrefab == null)
            {
                return;
            }

            // 기존 미리보기 정리
            for (int i = skillCardPreviewContainer.childCount - 1; i >= 0; i--)
            {
                var child = skillCardPreviewContainer.GetChild(i);
                if (child != null)
                {
                    DestroyImmediate(child.gameObject);
                }
            }

            // 대표 스킬 3개까지 생성
            var defs = selectedCharacterSO != null && selectedCharacterSO.SkillCards != null && selectedCharacterSO.SkillCards.Length > 0
                ? selectedCharacterSO.SkillCards
                : null;

            if (defs == null)
            {
                // 폴백: PlayerCharacterData의 덱에서 추출
                if (selectedCharacter == null || selectedCharacter.SkillDeck == null) return;
                var entries = selectedCharacter.SkillDeck.GetAllCardEntries();
                if (entries == null || entries.Count == 0) return;
                defs = new Game.SkillCardSystem.Data.SkillCardDefinition[Mathf.Min(3, entries.Count)];
                int idx = 0;
                foreach (var e in entries)
                {
                    if (e?.cardDefinition == null) continue;
                    defs[idx++] = e.cardDefinition;
                    if (idx >= defs.Length) break;
                }
            }

            var factory = new SkillCardFactory();
            int created = 0;
            foreach (var def in defs)
            {
                if (def == null) continue;
                if (created >= 3) break;
                try
                {
                    var ownerName = selectedCharacter != null ? selectedCharacter.DisplayName : "";
                    var card = factory.CreatePlayerCard(def, ownerName);
                    var ui = SkillCardUIFactory.CreateUI(skillCardUIPrefab, skillCardPreviewContainer, card, null);
                    if (ui != null)
                    {
                        // 미리보기는 상호작용/드래그 비활성화
                        ui.SetInteractable(false);
                        ui.SetDraggable(false);
                        var cg = ui.GetComponent<CanvasGroup>() ?? ui.gameObject.AddComponent<CanvasGroup>();
                        cg.interactable = false;
                        cg.blocksRaycasts = false;

                        // 프리뷰 크기를 200x300으로 고정 유지하면서 내부 레이아웃은 스케일로 보존
                        const float targetWidth = 200f;
                        const float targetHeight = 300f;

                        var instRect = ui.GetComponent<RectTransform>();
                        var prefabRect = skillCardUIPrefab != null ? skillCardUIPrefab.GetComponent<RectTransform>() : null;
                        if (instRect != null)
                        {
                            // 레이아웃 그룹 간섭 최소화: 선호 크기를 명시
                            var layout = ui.GetComponent<UnityEngine.UI.LayoutElement>();
                            if (layout == null) layout = ui.gameObject.AddComponent<UnityEngine.UI.LayoutElement>();
                            layout.preferredWidth = targetWidth;
                            layout.preferredHeight = targetHeight;

                            // 원본 크기를 기준으로 균등 스케일 계산
                            float baseW = prefabRect != null && prefabRect.rect.width > 0 ? prefabRect.rect.width : Mathf.Max(1f, instRect.rect.width);
                            float baseH = prefabRect != null && prefabRect.rect.height > 0 ? prefabRect.rect.height : Mathf.Max(1f, instRect.rect.height);
                            float scaleX = targetWidth / baseW;
                            float scaleY = targetHeight / baseH;
                            float uniform = Mathf.Min(scaleX, scaleY);
                            instRect.localScale = new Vector3(uniform, uniform, 1f);

                            // 중앙 정렬을 위한 피벗/앵커 보정(필요 시)
                            instRect.pivot = new Vector2(0.5f, 0.5f);
                            instRect.anchorMin = instRect.anchorMax = new Vector2(0.5f, 0.5f);
                        }
                    }
                    created++;
                }
                catch (System.Exception ex)
                {
                    GameLogger.LogWarning($"[MainMenuController] 카드 미리보기 생성 실패: {ex.Message}", GameLogger.LogCategory.UI);
                }
            }
        }

        private void ApplyPreviewSOToSelected(SelectedCharacterSO previewSO, string label)
        {
            if (selectedCharacterSO == null)
            {
                GameLogger.LogWarning("[MainMenuController] SelectedCharacterSO가 할당되지 않았습니다", GameLogger.LogCategory.UI);
                return;
            }
            if (previewSO == null)
            {
                GameLogger.LogWarning($"[MainMenuController] {label} 프리뷰 SO가 비어 있어 기본값으로 표시합니다", GameLogger.LogCategory.UI);
                // 비어 있으면 초기화만
                selectedCharacterSO.Clear();
                return;
            }
            selectedCharacterSO.Set(previewSO.Emblem, previewSO.Description, previewSO.SkillCards);
        }
        
        /// <summary>
        /// 선택 하이라이트 업데이트
        /// </summary>
        private void UpdateSelectionHighlight(int characterIndex)
        {
            if (selectionHighlight != null && characterIndex < characterCards.Count)
            {
                var selectedCard = characterCards[characterIndex];
                if (selectedCard != null)
                {
                    selectionHighlight.transform.position = selectedCard.transform.position;
                    selectionHighlight.SetActive(true);
                }
            }
        }
        
        /// <summary>
        /// 캐릭터 타입에 따른 설명 반환
        /// </summary>
        private string GetCharacterDescription(PlayerCharacterType characterType)
        {
            switch (characterType)
            {
                case PlayerCharacterType.Sword:
                    return "근접 전투의 달인\n강력한 공격력과 방어력을 가진 검사";
                case PlayerCharacterType.Bow:
                    return "원거리 공격의 전문가\n화살을 이용한 정밀한 공격";
                case PlayerCharacterType.Staff:
                    return "마법의 지배자\n마나를 이용한 강력한 마법 공격";
                default:
                    return "알 수 없는 캐릭터 타입";
            }
        }
        
        #endregion
    }
}
