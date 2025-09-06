using UnityEngine;
using UnityEngine.UI;

namespace Game.CoreSystem.UI
{
    /// <summary>
    /// 로딩 화면을 관리하는 컨트롤러
    /// </summary>
    public class LoadingScreenController : MonoBehaviour
    {
        [Header("로딩 UI")]
        [SerializeField] private GameObject loadingPanel;
        [SerializeField] private Slider progressBar;
        [SerializeField] private Text progressText;
        [SerializeField] private Text loadingText;
        
        [Header("로딩 텍스트")]
        [SerializeField] private string[] loadingMessages = {
            "로딩 중...",
            "씬을 준비하고 있습니다...",
            "거의 완료되었습니다..."
        };
        
        private bool isLoading = false;
        private float currentProgress = 0f;
        
        private void Awake()
        {
            // 초기 상태 설정
            if (loadingPanel != null)
                loadingPanel.SetActive(false);
        }
        
        /// <summary>
        /// 로딩 화면 표시
        /// </summary>
        public void ShowLoadingScreen()
        {
            if (loadingPanel != null)
            {
                loadingPanel.SetActive(true);
                isLoading = true;
                currentProgress = 0f;
                
                UpdateProgress(0f);
                StartCoroutine(UpdateLoadingText());
            }
        }
        
        /// <summary>
        /// 로딩 화면 숨기기
        /// </summary>
        public void HideLoadingScreen()
        {
            if (loadingPanel != null)
            {
                loadingPanel.SetActive(false);
                isLoading = false;
            }
        }
        
        /// <summary>
        /// 진행률 업데이트
        /// </summary>
        public void UpdateProgress(float progress)
        {
            currentProgress = Mathf.Clamp01(progress);
            
            if (progressBar != null)
                progressBar.value = currentProgress;
            
            if (progressText != null)
                progressText.text = $"{Mathf.RoundToInt(currentProgress * 100)}%";
        }
        
        /// <summary>
        /// 로딩 텍스트 업데이트 코루틴
        /// </summary>
        private System.Collections.IEnumerator UpdateLoadingText()
        {
            int messageIndex = 0;
            
            while (isLoading)
            {
                if (loadingText != null && loadingMessages.Length > 0)
                {
                    loadingText.text = loadingMessages[messageIndex];
                    messageIndex = (messageIndex + 1) % loadingMessages.Length;
                }
                
                yield return new WaitForSeconds(1f);
            }
        }
    }
}
