using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

namespace Game.CharacterSystem.UI
{
    /// <summary>
    /// 버프/디버프 아이콘을 관리하는 컴포넌트입니다.
    /// 지속 시간 표시, 애니메이션, 툴팁 등을 제공합니다.
    /// </summary>
    public class BuffDebuffIcon : MonoBehaviour
    {
        #region Serialized Fields

        [Header("UI Components")]
        [Tooltip("아이콘 이미지")]
        [SerializeField] private Image iconImage;
        
        [Tooltip("지속 시간 텍스트")]
        [SerializeField] private TextMeshProUGUI durationText;
        
        [Tooltip("아이콘 배경")]
        [SerializeField] private Image backgroundImage;
        
        [Tooltip("아이콘 테두리")]
        [SerializeField] private Image borderImage;

        [Header("색상 설정")]
        [Tooltip("버프 아이콘 색상")]
        [SerializeField] private Color buffColor = Color.blue;
        
        [Tooltip("디버프 아이콘 색상")]
        [SerializeField] private Color debuffColor = Color.red;
        
        [Tooltip("기본 아이콘 색상")]
        [SerializeField] private Color defaultColor = Color.white;

        [Header("애니메이션 설정")]
        [Tooltip("페이드 인/아웃 속도")]
        [SerializeField] private float fadeSpeed = 2f;
        
        [Tooltip("호버 시 스케일")]
        [SerializeField] private float hoverScale = 1.2f;

        #endregion

        #region Private Fields

        private string effectId;
        private bool isBuff;
        private float duration = -1f; // -1이면 영구
        private float remainingTime;
        private bool isPermanent;

        private Tween fadeTween;
        private Tween scaleTween;
        private Tween pulseTween;
        private Tween durationTween;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            InitializeIcon();
        }

        private void OnDisable()
        {
            fadeTween?.Kill();
            scaleTween?.Kill();
            pulseTween?.Kill();
            durationTween?.Kill();
            fadeTween = null;
            scaleTween = null;
            pulseTween = null;
            durationTween = null;
        }

        private void OnDestroy()
        {
            fadeTween?.Kill();
            scaleTween?.Kill();
            pulseTween?.Kill();
            durationTween?.Kill();
            fadeTween = null;
            scaleTween = null;
            pulseTween = null;
            durationTween = null;
        }

        #endregion

        #region 초기화

        /// <summary>
        /// 아이콘 초기화
        /// </summary>
        private void InitializeIcon()
        {
            // 초기 상태 설정
            if (iconImage != null)
                iconImage.color = defaultColor;
            
            if (durationText != null)
                durationText.text = "";
            
            // 초기 알파값 설정
            SetAlpha(0f);
        }

        /// <summary>
        /// 버프/디버프 아이콘을 설정합니다.
        /// </summary>
        /// <param name="effectId">효과 ID</param>
        /// <param name="iconSprite">아이콘 스프라이트</param>
        /// <param name="isBuff">버프 여부</param>
        /// <param name="duration">지속 시간 (초, -1이면 영구)</param>
        public void SetupIcon(string effectId, Sprite iconSprite, bool isBuff, float duration = -1f)
        {
            this.effectId = effectId;
            this.isBuff = isBuff;
            this.duration = duration;
            this.remainingTime = duration;
            this.isPermanent = duration < 0;

            // 아이콘 이미지 설정
            if (iconImage != null && iconSprite != null)
                iconImage.sprite = iconSprite;

            // 색상 설정
            SetIconColor();

            // 지속 시간 텍스트 설정
            UpdateDurationText();

            FadeIn();

            if (!isPermanent)
            {
                StartDurationTimer();
            }

            Debug.Log($"[BuffDebuffIcon] {(isBuff ? "버프" : "디버프")} 아이콘 설정: {effectId}, 지속시간: {(isPermanent ? "영구" : duration.ToString("F1") + "초")}");
        }

        /// <summary>
        /// 아이콘 색상을 설정합니다.
        /// </summary>
        private void SetIconColor()
        {
            if (iconImage == null) return;

            Color targetColor = isBuff ? buffColor : debuffColor;
            iconImage.color = targetColor;

            // 배경 색상도 설정
            if (backgroundImage != null)
            {
                Color bgColor = targetColor;
                bgColor.a = 0.3f; // 반투명
                backgroundImage.color = bgColor;
            }

            // 테두리 색상 설정
            if (borderImage != null)
                borderImage.color = targetColor;
        }

        #endregion

        #region 지속 시간 관리

        /// <summary>
        /// DOTween을 사용하여 지속 시간 타이머를 시작합니다.
        /// </summary>
        private void StartDurationTimer()
        {
            durationTween?.Kill();

            durationTween = DOTween.To(() => remainingTime, x => {
                remainingTime = x;
                UpdateDurationText();

                if (remainingTime <= 5f && remainingTime > 0)
                {
                    ShowExpirationWarning();
                }
            }, 0f, duration)
            .SetEase(Ease.Linear)
            .SetAutoKill(true)
            .OnComplete(() => {
                durationTween = null;
                OnDurationExpired();
            });
        }

        /// <summary>
        /// 지속 시간 텍스트를 업데이트합니다.
        /// </summary>
        private void UpdateDurationText()
        {
            if (durationText == null || isPermanent) return;

            if (remainingTime > 60f)
            {
                // 1분 이상이면 분:초 형식
                int minutes = Mathf.FloorToInt(remainingTime / 60f);
                int seconds = Mathf.FloorToInt(remainingTime % 60f);
                durationText.text = $"{minutes}:{seconds:D2}";
            }
            else
            {
                // 1분 미만이면 초만 표시
                durationText.text = remainingTime.ToString("F0");
            }
        }

        /// <summary>
        /// 지속 시간 만료 시 호출됩니다.
        /// </summary>
        private void OnDurationExpired()
        {
            Debug.Log($"[BuffDebuffIcon] 지속 시간 만료: {effectId}");
            
            // 페이드 아웃 후 제거
            FadeOut(() => {
                Destroy(gameObject);
            });
        }

        /// <summary>
        /// 만료 임박 경고를 표시합니다.
        /// </summary>
        private void ShowExpirationWarning()
        {
            // 깜빡이는 효과
            if (pulseTween == null || !pulseTween.IsActive())
            {
                pulseTween = iconImage.DOColor(Color.red, 0.5f)
                    .SetLoops(-1, LoopType.Yoyo)
                    .SetEase(Ease.InOutSine);
            }
        }

        #endregion

        #region 애니메이션

        /// <summary>
        /// 페이드 인 애니메이션을 실행합니다.
        /// </summary>
        private void FadeIn()
        {
            fadeTween?.Kill();
            fadeTween = DOTween.To(() => GetAlpha(), x => SetAlpha(x), 1f, 1f / fadeSpeed)
                .SetEase(Ease.OutQuad)
                .SetAutoKill(true);
        }

        /// <summary>
        /// 페이드 아웃 애니메이션을 실행합니다.
        /// </summary>
        /// <param name="onComplete">완료 시 콜백</param>
        private void FadeOut(System.Action onComplete = null)
        {
            fadeTween?.Kill();
            fadeTween = DOTween.To(() => GetAlpha(), x => SetAlpha(x), 0f, 1f / fadeSpeed)
                .SetEase(Ease.InQuad)
                .SetAutoKill(true)
                .OnComplete(() => {
                    fadeTween = null;
                    onComplete?.Invoke();
                });
        }

        /// <summary>
        /// 현재 알파값을 반환합니다.
        /// </summary>
        /// <returns>알파값</returns>
        private float GetAlpha()
        {
            if (iconImage != null)
                return iconImage.color.a;
            return 1f;
        }

        /// <summary>
        /// 알파값을 설정합니다.
        /// </summary>
        /// <param name="alpha">알파값</param>
        private void SetAlpha(float alpha)
        {
            if (iconImage != null)
            {
                Color color = iconImage.color;
                color.a = alpha;
                iconImage.color = color;
            }

            if (backgroundImage != null)
            {
                Color color = backgroundImage.color;
                color.a = alpha * 0.3f;
                backgroundImage.color = color;
            }

            if (borderImage != null)
            {
                Color color = borderImage.color;
                color.a = alpha;
                borderImage.color = color;
            }

            if (durationText != null)
            {
                Color color = durationText.color;
                color.a = alpha;
                durationText.color = color;
            }
        }

        #endregion

        #region 마우스 이벤트

        /// <summary>
        /// 마우스 호버 시 호출됩니다.
        /// </summary>
        public void OnMouseEnter()
        {
            // 스케일 업 애니메이션
            Game.UtilitySystem.HoverEffectHelper.PlayHoverScaleWithCleanup(
                ref scaleTween,
                transform,
                hoverScale,
                0.2f);
        }

        /// <summary>
        /// 마우스 호버 종료 시 호출됩니다.
        /// </summary>
        public void OnMouseExit()
        {
            // 스케일 다운 애니메이션
            Game.UtilitySystem.HoverEffectHelper.ResetScaleWithCleanup(
                ref scaleTween,
                transform,
                0.2f);
        }

        #endregion

        #region 공개 메서드

        /// <summary>
        /// 지속 시간을 연장합니다.
        /// </summary>
        /// <param name="additionalTime">추가 시간 (초)</param>
        public void ExtendDuration(float additionalTime)
        {
            if (isPermanent) return;

            remainingTime += additionalTime;
            duration += additionalTime;

            durationTween?.Kill();
            StartDurationTimer();

            Debug.Log($"[BuffDebuffIcon] 지속 시간 연장: {effectId}, +{additionalTime}초");
        }

        /// <summary>
        /// 지속 시간을 즉시 만료시킵니다.
        /// </summary>
        public void ExpireImmediately()
        {
            if (isPermanent) return;

            durationTween?.Kill();
            remainingTime = 0f;
            OnDurationExpired();
        }

        /// <summary>
        /// 효과 ID를 반환합니다.
        /// </summary>
        /// <returns>효과 ID</returns>
        public string GetEffectId()
        {
            return effectId;
        }

        /// <summary>
        /// 버프 여부를 반환합니다.
        /// </summary>
        /// <returns>버프 여부</returns>
        public bool IsBuff()
        {
            return isBuff;
        }

        /// <summary>
        /// 남은 지속 시간을 반환합니다.
        /// </summary>
        /// <returns>남은 지속 시간 (초)</returns>
        public float GetRemainingTime()
        {
            return isPermanent ? -1f : remainingTime;
        }

        #endregion
    }
}
