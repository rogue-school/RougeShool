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
        [Inject] private ISaveManager saveManager;
        [Inject] private IPlayerCharacterSelectionManager playerCharacterSelectionManager;
        [Inject(Optional = true)] private ISceneTransitionManager sceneTransitionManager;
        [Inject(Optional = true)] private Game.CoreSystem.Audio.AudioManager audioManager;

        // 선택 캐릭터 공유 SO 제거됨

        #endregion
        
        #region Inspector 필드
        
        [Header("메인 메뉴")]
        
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
        
        // 캐릭터 카드 프리팹/선택 하이라이트 제거됨
        
        [Header("게임 시작")]
        [Tooltip("게임 시작 패널")]
        [SerializeField] private GameObject gameStartPanel;
        
        [Tooltip("선택된 캐릭터 이미지")]
        [SerializeField] private Image selectedCharacterImage;
        
        // 선택된 캐릭터 엠블럼 이미지 제거됨

        // 선택된 캐릭터 스킬 아이콘 배열 제거됨

        // 선택된 캐릭터 이름 텍스트 제거됨
        
        [Tooltip("선택된 캐릭터 설명")]
        [SerializeField] private TextMeshProUGUI selectedCharacterDescription;
        
        [Tooltip("게임 시작 버튼")]
        [SerializeField] private Button startGameButton;
        
        [Tooltip("캐릭터 다시 선택 버튼")]
        [SerializeField] private Button reselectCharacterButton;

        [Header("튜토리얼")]
        [Tooltip("튜토리얼 건너뛰기 토글")]
        [SerializeField] private Toggle skipTutorialToggle;

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

        // 프리뷰 SO 제거됨

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
        
        // 크레딧 제목/내용 필드 제거됨 (패널만 토글)
        
        [Tooltip("메인 메뉴로 버튼")]
        [SerializeField] private Button backToMenuButton;
        
        [Header("메시지 표시")]
        [Tooltip("추후 업데이트 메시지를 표시할 TextMeshProUGUI (선택적)")]
        [SerializeField] private TextMeshProUGUI messageText;
        
        [Tooltip("메시지 표시 패널 (선택적, messageText가 없을 경우 사용)")]
        [SerializeField] private GameObject messagePanel;
        
        // 설정 UI 제거됨 (요청에 따라 인스펙터 비우기)
        
        #endregion
        
        #region 상태 변수
        
        private PlayerCharacterData[] availableCharacters;
        private PlayerCharacterData selectedCharacter;
        private List<GameObject> characterCards = new List<GameObject>();
        
        private Tween creditsPanelTween;
        private CanvasGroup creditsCanvasGroup;
        private CanvasGroup mainMenuCanvasGroup;
        
        private Tween messageTween;
        private CanvasGroup messageCanvasGroup;
        
        #endregion
        
        #region 초기화
        
        private void Start()
        {
            GameLogger.LogInfo("[MainMenuController] 초기화 시작", GameLogger.LogCategory.UI);
            
            InitializeUI();
            InitializeCanvasGroups();
            ValidateInspectorBindings();
            LoadCharacterData();
            CreateCharacterCards();
            BindFixedCharacterButtons();
            // 설정 기능 제거됨
            UpdateContinueButtonState();
            
            // 초기 애니메이션
            PlayInitialAnimation();
            
            // DI 주입 상태 확인
            GameLogger.LogInfo($"[MainMenuController] DI 주입 상태 - SceneTransitionManager: {(sceneTransitionManager != null ? "성공" : "실패")}", GameLogger.LogCategory.UI);
            
            GameLogger.LogInfo("[MainMenuController] 초기화 완료", GameLogger.LogCategory.UI);
        }
        
        /// <summary>
        /// 크레딧 및 메인 메뉴 CanvasGroup 초기화
        /// </summary>
        private void InitializeCanvasGroups()
        {
            if (creditsPanel != null)
            {
                creditsCanvasGroup = creditsPanel.GetComponent<CanvasGroup>();
                if (creditsCanvasGroup == null)
                {
                    creditsCanvasGroup = creditsPanel.AddComponent<CanvasGroup>();
                }
                creditsCanvasGroup.alpha = 0f;
                creditsCanvasGroup.blocksRaycasts = false;
            }
            
            if (mainMenuPanel != null)
            {
                mainMenuCanvasGroup = mainMenuPanel.GetComponent<CanvasGroup>();
                if (mainMenuCanvasGroup == null)
                {
                    mainMenuCanvasGroup = mainMenuPanel.AddComponent<CanvasGroup>();
                }
                mainMenuCanvasGroup.alpha = 1f;
                mainMenuCanvasGroup.blocksRaycasts = true;
            }
            
            if (messagePanel != null)
            {
                messageCanvasGroup = messagePanel.GetComponent<CanvasGroup>();
                if (messageCanvasGroup == null)
                {
                    messageCanvasGroup = messagePanel.AddComponent<CanvasGroup>();
                }
                messageCanvasGroup.alpha = 0f;
                messageCanvasGroup.blocksRaycasts = false;
                messagePanel.SetActive(false);
            }
            else if (messageText != null)
            {
                var parent = messageText.transform.parent;
                if (parent != null)
                {
                    messageCanvasGroup = parent.GetComponent<CanvasGroup>();
                    if (messageCanvasGroup == null)
                    {
                        messageCanvasGroup = parent.gameObject.AddComponent<CanvasGroup>();
                    }
                }
            }
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
            // 프리뷰 SO 제거됨
            if (swordCharacter == null || bowCharacter == null || staffCharacter == null)
                GameLogger.LogWarning("[MainMenuController] 실제 게임용 캐릭터 데이터(검/활/지팡이) 중 일부가 비었습니다", GameLogger.LogCategory.UI);
        }

        /// <summary>
        /// 인스펙터에서 지정한 버튼 3개와 캐릭터 데이터 3개를 1:1로 바인딩합니다.
        /// </summary>
        private void BindFixedCharacterButtons()
        {
            BindFixed(swordButton, swordCharacter, "CharacterCard_Sword");
            BindFixed(bowButton, bowCharacter, "CharacterCard_Bow");
            BindFixed(staffButton, staffCharacter, "CharacterCard_Staff");
        }

        private void BindFixed(Button button, PlayerCharacterData gameCharacter, string buttonName = null)
        {
            // 버튼이 null이면 여러 방법으로 찾기 시도
            if (button == null)
            {
                // 방법 1: characterCardContainer의 자식에서 이름으로 찾기
                if (characterCardContainer != null && !string.IsNullOrEmpty(buttonName))
                {
                    for (int i = 0; i < characterCardContainer.childCount; i++)
                    {
                        var child = characterCardContainer.GetChild(i);
                        if (child != null && child.name.Contains(buttonName.Replace("CharacterCard_", "")))
                        {
                            button = child.GetComponent<Button>();
                            if (button != null)
                            {
                                GameLogger.LogInfo($"[MainMenuController] 버튼을 컨테이너에서 찾았습니다: {child.name}", GameLogger.LogCategory.UI);
                                break;
                            }
                        }
                    }
                }
                
                // 방법 2: GameObject.Find로 찾기 (비활성화된 오브젝트도 찾기)
                if (button == null && !string.IsNullOrEmpty(buttonName))
                {
                    var foundObj = GameObject.Find(buttonName);
                    if (foundObj != null)
                    {
                        button = foundObj.GetComponent<Button>();
                        if (button != null)
                        {
                            GameLogger.LogInfo($"[MainMenuController] 버튼을 Find로 찾았습니다: {buttonName}", GameLogger.LogCategory.UI);
                        }
                    }
                }
                
                // 방법 3: characterCardContainer의 인덱스로 찾기 (지팡이는 3번째)
                if (button == null && characterCardContainer != null && buttonName != null && buttonName.Contains("Staff"))
                {
                    if (characterCardContainer.childCount >= 3)
                    {
                        var staffChild = characterCardContainer.GetChild(2);
                        if (staffChild != null)
                        {
                            button = staffChild.GetComponent<Button>();
                            if (button != null)
                            {
                                GameLogger.LogInfo($"[MainMenuController] 버튼을 인덱스로 찾았습니다: {staffChild.name}", GameLogger.LogCategory.UI);
                            }
                        }
                    }
                }
            }
            
            if (button == null)
            {
                GameLogger.LogWarning($"[MainMenuController] 버튼을 찾을 수 없습니다. 버튼 이름: {buttonName ?? "Unknown"}, 캐릭터 데이터: {(gameCharacter != null ? gameCharacter.DisplayName : "null")}", GameLogger.LogCategory.UI);
                if (gameCharacter == null)
                {
                    GameLogger.LogWarning($"[MainMenuController] 버튼과 캐릭터 데이터가 모두 null입니다. 버튼 이름: {buttonName ?? "Unknown"}", GameLogger.LogCategory.UI);
                }
                return; // 선택적 기능
            }
            
            GameLogger.LogInfo($"[MainMenuController] 버튼 바인딩 완료: {button.name}, 캐릭터: {(gameCharacter != null ? gameCharacter.DisplayName : "null")}", GameLogger.LogCategory.UI);
            
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() =>
            {
                GameLogger.LogInfo($"[MainMenuController] 버튼 클릭: {button.name}", GameLogger.LogCategory.UI);
                
                if (gameCharacter == null)
                {
                    ShowUpcomingUpdateMessage();
                    GameLogger.LogInfo("[MainMenuController] 데이터가 존재하지 않는 캐릭터를 선택하려고 시도했습니다", GameLogger.LogCategory.UI);
                    return;
                }
                
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
            
            // 크레딧 패널 버튼 이벤트 연결 (크레딧 제목/내용 필드 사용 안 함)
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

            // 메인 메뉴 버튼들에 언더라인 호버 효과 연결
            TryBindUnderlineHover(newGameButton);
            TryBindUnderlineHover(continueButton);
            TryBindUnderlineHover(creditsButton);
            TryBindUnderlineHover(exitButton);
            TryBindUnderlineHover(startGameButton);
            TryBindUnderlineHover(reselectCharacterButton);
            TryBindUnderlineHover(backToMenuButton);

            // 튜토리얼 스킵 토글 초기 상태 복원
            if (skipTutorialToggle != null)
            {
                int savedSkip = PlayerPrefs.GetInt("TUTORIAL_SKIP", 0);
                skipTutorialToggle.isOn = savedSkip == 1;
            }
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
            // 타이틀 텍스트 제거됨
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

                // 스테이지 씬으로 전환 (DI 주입)
                if (sceneTransitionManager != null)
                {
                    await sceneTransitionManager.TransitionToStageScene();
                    
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
        /// 크레딧 버튼 클릭 - 영화 스타일 페이드 전환 효과
        /// </summary>
        private void OnCreditsButtonClicked()
        {
            GameLogger.LogInfo("[MainMenuController] 크레딧 버튼 클릭", GameLogger.LogCategory.UI);
            
            if (creditsPanel == null || creditsCanvasGroup == null)
                return;
            
            creditsPanelTween?.Kill();
            
            creditsPanel.SetActive(true);
            creditsCanvasGroup.alpha = 0f;
            creditsCanvasGroup.blocksRaycasts = false;
            
            if (mainMenuCanvasGroup != null)
            {
                Sequence fadeSequence = DOTween.Sequence()
                    .Append(mainMenuCanvasGroup.DOFade(0f, 0.5f)
                        .SetEase(Ease.InQuad))
                    .AppendCallback(() =>
                    {
                        if (mainMenuPanel != null)
                            mainMenuPanel.SetActive(false);
                        
                        creditsCanvasGroup.blocksRaycasts = true;
                    })
                    .Append(creditsCanvasGroup.DOFade(1f, 0.5f)
                        .SetEase(Ease.OutQuad))
                    .SetAutoKill(true)
                    .OnComplete(() => creditsPanelTween = null);
                
                creditsPanelTween = fadeSequence;
            }
            else
            {
                if (mainMenuPanel != null)
                    mainMenuPanel.SetActive(false);
                
                creditsCanvasGroup.blocksRaycasts = true;
                creditsPanelTween = creditsCanvasGroup.DOFade(1f, 0.5f)
                    .SetEase(Ease.OutQuad)
                    .SetAutoKill(true)
                    .OnComplete(() => creditsPanelTween = null);
            }
        }
        
        /// <summary>
        /// 메인 메뉴로 버튼 클릭 - 영화 스타일 페이드 전환 효과
        /// </summary>
        private void OnBackToMenuButtonClicked()
        {
            GameLogger.LogInfo("[MainMenuController] 메인 메뉴로 버튼 클릭", GameLogger.LogCategory.UI);
            
            if (creditsPanel == null || creditsCanvasGroup == null)
                return;
            
            creditsPanelTween?.Kill();
            
            creditsCanvasGroup.blocksRaycasts = false;
            
            if (mainMenuCanvasGroup != null)
            {
                Sequence fadeSequence = DOTween.Sequence()
                    .Append(creditsCanvasGroup.DOFade(0f, 0.5f)
                        .SetEase(Ease.InQuad))
                    .AppendCallback(() =>
                    {
                        creditsPanel.SetActive(false);
                        
                        if (mainMenuPanel != null)
                            mainMenuPanel.SetActive(true);
                    })
                    .Append(mainMenuCanvasGroup.DOFade(1f, 0.5f)
                        .SetEase(Ease.OutQuad))
                    .SetAutoKill(true)
                    .OnComplete(() =>
                    {
                        if (mainMenuCanvasGroup != null)
                            mainMenuCanvasGroup.blocksRaycasts = true;
                        creditsPanelTween = null;
                    });
                
                creditsPanelTween = fadeSequence;
            }
            else
            {
                creditsPanelTween = creditsCanvasGroup.DOFade(0f, 0.5f)
                    .SetEase(Ease.InQuad)
                    .SetAutoKill(true)
                    .OnComplete(() =>
                    {
                        creditsPanel.SetActive(false);
                        creditsPanelTween = null;
                        
                        if (mainMenuPanel != null)
                            mainMenuPanel.SetActive(true);
                    });
            }
        }
        
        /// <summary>
        /// 종료 버튼 클릭
        /// </summary>
        private void OnExitButtonClicked()
        {
            GameLogger.LogInfo("[MainMenuController] 게임 종료", GameLogger.LogCategory.UI);

            if (gameStateManager != null)
            {
                gameStateManager.ExitGame();
                return;
            }

            GameLogger.LogWarning("[MainMenuController] GameStateManager가 null입니다. 직접 종료를 시도합니다.", GameLogger.LogCategory.UI);

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
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
            
            var character = availableCharacters[characterIndex];
            if (character == null)
            {
                ShowUpcomingUpdateMessage();
                GameLogger.LogInfo("[MainMenuController] 데이터가 존재하지 않는 캐릭터를 선택하려고 시도했습니다", GameLogger.LogCategory.UI);
                return;
            }
            
            selectedCharacter = character;
            GameLogger.LogInfo($"[MainMenuController] 캐릭터 선택: {selectedCharacter.DisplayName}", GameLogger.LogCategory.UI);
            
            // SO에 요약 반영 및 시작용 캐릭터 후보 보관
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

        // SelectedCharacterSO 연동 메서드 제거됨
        
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

                // 게임 시작 전 잔존할 수 있는 게임 오버 UI를 확실히 초기화
                try
                {
                    var gameOverUI = UnityEngine.Object.FindFirstObjectByType<Game.CombatSystem.UI.GameOverUI>(FindObjectsInactive.Include);
                    if (gameOverUI != null)
                    {
                        gameOverUI.HideGameOver();
                        GameLogger.LogInfo("[MainMenuController] 게임 시작 전 GameOverUI 초기화(숨김)", GameLogger.LogCategory.UI);
                    }
                }
                catch (System.Exception ex)
                {
                    GameLogger.LogWarning($"[MainMenuController] GameOverUI 초기화 중 경고: {ex.Message}", GameLogger.LogCategory.UI);
                }

                // 스테이지 씬으로 전환 (DI 주입 또는 직접 찾기)
                ISceneTransitionManager transitionManager = sceneTransitionManager;
                
                if (transitionManager == null)
                {
                    GameLogger.LogWarning("[MainMenuController] DI 주입된 SceneTransitionManager가 null입니다. 직접 찾아서 사용합니다.", GameLogger.LogCategory.UI);
                    
                    // 직접 찾아서 사용
                    var foundTransitionManager = UnityEngine.Object.FindFirstObjectByType<Game.CoreSystem.Manager.SceneTransitionManager>();
                    if (foundTransitionManager != null)
                    {
                        transitionManager = foundTransitionManager;
                        GameLogger.LogInfo($"[MainMenuController] SceneTransitionManager를 직접 찾았습니다. 이름: {foundTransitionManager.name}", GameLogger.LogCategory.UI);
                    }
                }
                
                if (transitionManager != null)
                {
                    // 튜토리얼 스킵 설정 저장 (게임 시작 시점)
                    try
                    {
                        int skip = (skipTutorialToggle != null && skipTutorialToggle.isOn) ? 1 : 0;
                        PlayerPrefs.SetInt("TUTORIAL_SKIP", skip);
                        PlayerPrefs.Save();
                        GameLogger.LogInfo($"[MainMenuController] 튜토리얼 스킵 설정 저장: {(skip == 1 ? "ON" : "OFF")}", GameLogger.LogCategory.UI);
                    }
                    catch (System.Exception ex)
                    {
                        GameLogger.LogWarning($"[MainMenuController] 튜토리얼 스킵 설정 저장 실패: {ex.Message}", GameLogger.LogCategory.UI);
                    }

                    GameLogger.LogInfo("[MainMenuController] SceneTransitionManager 발견됨, 스테이지 씬으로 전환 시작", GameLogger.LogCategory.UI);
                    await transitionManager.TransitionToStageScene();
                    GameLogger.LogInfo("[MainMenuController] 스테이지 씬으로 전환 완료", GameLogger.LogCategory.UI);
                }
                else
                {
                    GameLogger.LogError("[MainMenuController] SceneTransitionManager를 찾을 수 없습니다!", GameLogger.LogCategory.Error);
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
            // GameStartPanel은 PlayerCharacterData만 사용 (SO 미사용)
            if (selectedCharacter != null)
            {
                if (selectedCharacterDescription != null)
                    selectedCharacterDescription.text = selectedCharacter.Description ?? string.Empty; // PlayerCharacterData의 Description 사용
                if (selectedCharacterImage != null && selectedCharacter.Portrait != null)
                    selectedCharacterImage.sprite = selectedCharacter.Portrait;
            }

            // 대표 스킬 아이콘 UI 제거됨

            // 시작 버튼 라벨 "입학하기"로 설정
            if (startGameButton != null)
            {
                var label = startGameButton.GetComponentInChildren<TextMeshProUGUI>();
                if (label != null) label.text = "입학하기";
            }

            // 실제 카드 프리팹을 이용한 미리보기 생성 (PlayerCharacterData 기반)
            UpdateSkillCardPreviews();
        }

        // 스킬 아이콘 보조 메서드 제거됨

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

            // PlayerCharacterData의 덱에서 추출
            if (selectedCharacter == null || selectedCharacter.SkillDeck == null) return;
            var entries = selectedCharacter.SkillDeck.GetAllCardEntries();
            if (entries == null || entries.Count == 0) return;
            var defs = new Game.SkillCardSystem.Data.SkillCardDefinition[Mathf.Min(3, entries.Count)];
            int idx = 0;
            foreach (var e in entries)
            {
                if (e?.cardDefinition == null) continue;
                defs[idx++] = e.cardDefinition;
                if (idx >= defs.Length) break;
            }

            // AudioManager DI 주입 사용
            var factory = new SkillCardFactory(audioManager);
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

        // 프리뷰 SO 사용 제거됨

        // 버튼 하위의 'Underline'을 찾아 UnderlineHoverEffect를 보장합니다.
        private void TryBindUnderlineHover(Button button)
        {
            if (button == null) return;
            var effect = button.GetComponent<Game.UISystem.UnderlineHoverEffect>();
            if (effect == null)
            {
                effect = button.gameObject.AddComponent<Game.UISystem.UnderlineHoverEffect>();
            }
            var t = button.transform.Find("Underline");
            if (t != null)
            {
                var rect = t as RectTransform;
                effect.SetUnderline(rect);
            }
        }
        
        /// <summary>
        /// 선택 하이라이트 업데이트
        /// </summary>
        private void UpdateSelectionHighlight(int characterIndex) { }
        
        /// <summary>
        /// 캐릭터 타입에 따른 설명 반환
        /// </summary>
        // 타입별 하드코딩 설명 제거됨
        
        /// <summary>
        /// 추후 업데이트 예정 메시지 표시
        /// </summary>
        private void ShowUpcomingUpdateMessage()
        {
            ShowMessage("추후 업데이트할 예정입니다");
        }
        
        /// <summary>
        /// 메시지 표시 (공통 메서드)
        /// </summary>
        /// <param name="message">표시할 메시지</param>
        private void ShowMessage(string message)
        {
            messageTween?.Kill();
            
            if (messagePanel != null)
            {
                messagePanel.SetActive(true);
                if (messageCanvasGroup != null)
                {
                    messageCanvasGroup.alpha = 0f;
                    messageCanvasGroup.blocksRaycasts = true;
                    
                    messageTween = messageCanvasGroup.DOFade(1f, 0.3f)
                        .SetEase(Ease.OutQuad)
                        .OnComplete(() =>
                        {
                            messageTween = messageCanvasGroup.DOFade(0f, 0.3f)
                                .SetDelay(2f)
                                .SetEase(Ease.InQuad)
                                .OnComplete(() =>
                                {
                                    messagePanel.SetActive(false);
                                    messageCanvasGroup.blocksRaycasts = false;
                                    messageTween = null;
                                });
                        });
                }
            }
            else if (messageText != null)
            {
                messageText.text = message;
                messageText.gameObject.SetActive(true);
                
                if (messageCanvasGroup != null)
                {
                    messageCanvasGroup.alpha = 0f;
                    messageCanvasGroup.blocksRaycasts = true;
                    
                    messageTween = messageCanvasGroup.DOFade(1f, 0.3f)
                        .SetEase(Ease.OutQuad)
                        .OnComplete(() =>
                        {
                            messageTween = messageCanvasGroup.DOFade(0f, 0.3f)
                                .SetDelay(2f)
                                .SetEase(Ease.InQuad)
                                .OnComplete(() =>
                                {
                                    messageText.gameObject.SetActive(false);
                                    messageCanvasGroup.blocksRaycasts = false;
                                    messageTween = null;
                                });
                        });
                }
                else
                {
                    var textColor = messageText.color;
                    messageText.color = new Color(textColor.r, textColor.g, textColor.b, 0f);
                    
                    messageTween = messageText.DOFade(1f, 0.3f)
                        .SetEase(Ease.OutQuad)
                        .OnComplete(() =>
                        {
                            messageTween = messageText.DOFade(0f, 0.3f)
                                .SetDelay(2f)
                                .SetEase(Ease.InQuad)
                                .OnComplete(() =>
                                {
                                    messageText.gameObject.SetActive(false);
                                    messageTween = null;
                                });
                        });
                }
            }
            else
            {
                GameLogger.LogInfo($"[MainMenuController] {message}", GameLogger.LogCategory.UI);
            }
        }
        
        #endregion
        
        #region Cleanup
        
        private void OnDisable()
        {
            creditsPanelTween?.Kill();
            creditsPanelTween = null;
            messageTween?.Kill();
            messageTween = null;
        }
        
        private void OnDestroy()
        {
            creditsPanelTween?.Kill();
            creditsPanelTween = null;
            messageTween?.Kill();
            messageTween = null;
        }
        
        #endregion
    }
}
