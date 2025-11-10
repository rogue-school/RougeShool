using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using Game.CoreSystem.UI;
using Game.CoreSystem.Utility;
using Zenject;

namespace Game.StageSystem.UI
{
    /// <summary>
    /// 스테이지 씬 UI 컨트롤러
    /// 스테이지의 설정 버튼과 기타 UI 요소들을 관리
    /// </summary>
    public class StageUIController : MonoBehaviour
    {
        [Header("UI 요소")]
        [SerializeField] private Button settingsButton;  // 스테이지의 설정 버튼
        
        [Tooltip("도감 UI 버튼 (Illustrated Manual)")]
        [SerializeField] private Button illustratedManualButton;
        
        [Header("메시지 표시")]
        [Tooltip("추후 업데이트 메시지를 표시할 TextMeshProUGUI (선택적)")]
        [SerializeField] private TextMeshProUGUI messageText;
        
        [Tooltip("메시지 표시 패널 (선택적, messageText가 없을 경우 사용)")]
        [SerializeField] private GameObject messagePanel;
        
        [Header("설정")]
        [SerializeField] private bool enableDebugLogging = true;

        // SettingsManager DI 주입
        [Inject(Optional = true)] private SettingsManager settingsManager;
        
        private Tween messageTween;
        private CanvasGroup messageCanvasGroup;

        private void Start()
        {
            InitializeUI();
            InitializeCanvasGroups();

            if (settingsManager == null && enableDebugLogging)
            {
                GameLogger.LogWarning("SettingsManager가 주입되지 않았습니다.", GameLogger.LogCategory.UI);
            }
        }
        
        /// <summary>
        /// 메시지 표시용 CanvasGroup 초기화
        /// </summary>
        private void InitializeCanvasGroups()
        {
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
        /// UI 초기화
        /// </summary>
        private void InitializeUI()
        {
            // 설정 버튼 이벤트 연결
            if (settingsButton != null)
            {
                settingsButton.onClick.AddListener(OnSettingsButtonClicked);
                
                if (enableDebugLogging)
                {
                    Debug.Log("[StageUIController] 설정 버튼 연결 완료");
                }
            }
            else
            {
                Debug.LogWarning("[StageUIController] 설정 버튼이 연결되지 않았습니다!");
            }
            
            // 도감 버튼 이벤트 연결
            if (illustratedManualButton != null)
            {
                illustratedManualButton.onClick.AddListener(OnIllustratedManualButtonClicked);
                GameLogger.LogInfo("[StageUIController] 도감 버튼 연결 완료", GameLogger.LogCategory.UI);
            }
            else
            {
                // 버튼이 Inspector에 연결되지 않았으면 이름으로 찾기 시도
                var foundButton = GameObject.Find("Illustrated Manual")?.GetComponent<Button>();
                if (foundButton != null)
                {
                    illustratedManualButton = foundButton;
                    illustratedManualButton.onClick.AddListener(OnIllustratedManualButtonClicked);
                    GameLogger.LogInfo("[StageUIController] 도감 버튼을 자동으로 찾았습니다", GameLogger.LogCategory.UI);
                }
                else
                {
                    GameLogger.LogWarning("[StageUIController] 도감 버튼을 찾을 수 없습니다", GameLogger.LogCategory.UI);
                }
            }
        }
        
        private void Update()
        {
            // ESC 키로 설정창 열기
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                OnSettingsButtonClicked();
            }
        }
        
        /// <summary>
        /// 설정 버튼 클릭 시 설정창 열기
        /// </summary>
        private void OnSettingsButtonClicked()
        {
            try
            {
                if (settingsManager == null)
                {
                    GameLogger.LogError("SettingsManager가 null입니다. CoreScene이 로드되었는지 확인하세요.", GameLogger.LogCategory.Error);
                    return;
                }
                
                if (enableDebugLogging)
                {
                    GameLogger.LogInfo("설정창 열기 요청", GameLogger.LogCategory.UI);
                }
                
                settingsManager.OpenSettings();
            }
            catch (System.Exception ex)
            {
                GameLogger.LogError($"설정창 열기 실패: {ex.Message}", GameLogger.LogCategory.Error);
            }
        }
        
        /// <summary>
        /// 도감 UI 버튼 클릭
        /// </summary>
        private void OnIllustratedManualButtonClicked()
        {
            GameLogger.LogInfo("[StageUIController] 도감 UI 버튼 클릭", GameLogger.LogCategory.UI);
            ShowUpcomingUpdateMessage();
        }
        
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
                return;
            }
            
            if (messageText != null)
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
                GameLogger.LogInfo($"[StageUIController] {message}", GameLogger.LogCategory.UI);
                GameLogger.LogWarning("[StageUIController] 메시지를 표시할 UI 요소를 찾을 수 없습니다. Inspector에서 messageText 또는 messagePanel을 설정하세요.", GameLogger.LogCategory.UI);
            }
        }
        
        private void OnDisable()
        {
            messageTween?.Kill();
            messageTween = null;
        }
        
        private void OnDestroy()
        {
            // 이벤트 해제
            if (settingsButton != null)
                settingsButton.onClick.RemoveListener(OnSettingsButtonClicked);
            
            if (illustratedManualButton != null)
                illustratedManualButton.onClick.RemoveListener(OnIllustratedManualButtonClicked);
            
            messageTween?.Kill();
            messageTween = null;
        }
    }
}
