using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;

namespace Game.CoreSystem.UI
{
    /// <summary>
    /// 씬 전환 효과를 관리하는 컨트롤러
    /// </summary>
    public class TransitionEffectController : MonoBehaviour
    {
        [Header("전환 효과")]
        [SerializeField] private CanvasGroup fadeCanvas;
        [SerializeField] private Image fadeImage;
        [SerializeField] private float fadeDuration = 1f;
        [SerializeField] private AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        
        [Header("슬라이드 효과")]
        [SerializeField] private RectTransform slidePanel;
        [SerializeField] private float slideDuration = 1f;
        [SerializeField] private AnimationCurve slideCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        
        private void Awake()
        {
            InitializeTransitionEffects();
        }
        
        /// <summary>
        /// 전환 효과 초기화
        /// </summary>
        private void InitializeTransitionEffects()
        {
            // 페이드 캔버스 설정
            if (fadeCanvas == null)
            {
                var canvasObj = new GameObject("FadeCanvas");
                canvasObj.transform.SetParent(transform);
                
                var canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 1000;
                
                fadeCanvas = canvasObj.AddComponent<CanvasGroup>();
                fadeCanvas.alpha = 0f;
                fadeCanvas.interactable = false;
                fadeCanvas.blocksRaycasts = false;
                
                // 페이드 이미지 설정
                var imageObj = new GameObject("FadeImage");
                imageObj.transform.SetParent(canvasObj.transform);
                
                fadeImage = imageObj.AddComponent<Image>();
                fadeImage.color = Color.black;
                
                var rectTransform = fadeImage.GetComponent<RectTransform>();
                rectTransform.anchorMin = Vector2.zero;
                rectTransform.anchorMax = Vector2.one;
                rectTransform.offsetMin = Vector2.zero;
                rectTransform.offsetMax = Vector2.zero;
            }
            
            // 슬라이드 패널 설정
            if (slidePanel == null)
            {
                var slideObj = new GameObject("SlidePanel");
                slideObj.transform.SetParent(transform);
                
                slidePanel = slideObj.AddComponent<RectTransform>();
                slidePanel.anchorMin = Vector2.zero;
                slidePanel.anchorMax = Vector2.one;
                slidePanel.offsetMin = Vector2.zero;
                slidePanel.offsetMax = Vector2.zero;
                
                var image = slideObj.AddComponent<Image>();
                image.color = Color.black;
            }
        }
        
        /// <summary>
        /// 페이드 인 효과
        /// </summary>
        public async Task FadeIn()
        {
            if (fadeCanvas == null) return;
            
            fadeCanvas.gameObject.SetActive(true);
            fadeCanvas.blocksRaycasts = true;
            
            await FadeTransition(true);
        }
        
        /// <summary>
        /// 페이드 아웃 효과
        /// </summary>
        public async Task FadeOut()
        {
            if (fadeCanvas == null) return;
            
            await FadeTransition(false);
            
            fadeCanvas.gameObject.SetActive(false);
            fadeCanvas.blocksRaycasts = false;
        }
        
        /// <summary>
        /// 페이드 전환 실행
        /// </summary>
        private async Task FadeTransition(bool fadeIn)
        {
            float startAlpha = fadeIn ? 0f : 1f;
            float endAlpha = fadeIn ? 1f : 0f;
            
            fadeCanvas.alpha = startAlpha;
            
            float elapsedTime = 0f;
            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.deltaTime;
                float progress = elapsedTime / fadeDuration;
                float curveValue = fadeCurve.Evaluate(progress);
                
                fadeCanvas.alpha = Mathf.Lerp(startAlpha, endAlpha, curveValue);
                
                await Task.Yield();
            }
            
            fadeCanvas.alpha = endAlpha;
        }
        
        /// <summary>
        /// 슬라이드 인 효과
        /// </summary>
        public async Task SlideIn()
        {
            if (slidePanel == null) return;
            
            slidePanel.gameObject.SetActive(true);
            await SlideTransition(true);
        }
        
        /// <summary>
        /// 슬라이드 아웃 효과
        /// </summary>
        public async Task SlideOut()
        {
            if (slidePanel == null) return;
            
            await SlideTransition(false);
            slidePanel.gameObject.SetActive(false);
        }
        
        /// <summary>
        /// 슬라이드 전환 실행
        /// </summary>
        private async Task SlideTransition(bool slideIn)
        {
            Vector2 startPos = slideIn ? new Vector2(-Screen.width, 0) : Vector2.zero;
            Vector2 endPos = slideIn ? Vector2.zero : new Vector2(Screen.width, 0);
            
            slidePanel.anchoredPosition = startPos;
            
            float elapsedTime = 0f;
            while (elapsedTime < slideDuration)
            {
                elapsedTime += Time.deltaTime;
                float progress = elapsedTime / slideDuration;
                float curveValue = slideCurve.Evaluate(progress);
                
                slidePanel.anchoredPosition = Vector2.Lerp(startPos, endPos, curveValue);
                
                await Task.Yield();
            }
            
            slidePanel.anchoredPosition = endPos;
        }
    }
}
