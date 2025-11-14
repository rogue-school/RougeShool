using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

namespace Game.UISystem
{
    /// <summary>
    /// 버튼에 호버 효과를 적용합니다.
    /// - 포인터 진입 시 스케일 확대 및 밝기 증가
    /// - 포인터 이탈 시 원래 상태로 복원
    /// </summary>
    public class ButtonHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("호버 효과 설정")]
        [Tooltip("호버 시 스케일 배율")]
        [SerializeField] private float hoverScale = 1.1f;

        [Tooltip("호버 시 밝기 배율 (1.0 = 원래 밝기)")]
        [SerializeField] private float hoverBrightness = 1.2f;

        [Header("애니메이션 설정")]
        [Tooltip("호버 진입 시간(초)")]
        [SerializeField] private float enterDuration = 0.2f;

        [Tooltip("호버 종료 시간(초)")]
        [SerializeField] private float exitDuration = 0.15f;

        [Tooltip("호버 진입 이징")]
        [SerializeField] private Ease enterEase = Ease.OutQuad;

        [Tooltip("호버 종료 이징")]
        [SerializeField] private Ease exitEase = Ease.InQuad;

        private RectTransform _rectTransform;
        private Image _image;
        private Button _button;
        private CanvasGroup _canvasGroup;

        private Vector3 _originalScale;
        private Color _originalColor;
        private Tween _scaleTween;
        private Tween _colorTween;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _image = GetComponent<Image>();
            _button = GetComponent<Button>();
            _canvasGroup = GetComponent<CanvasGroup>();

            if (_rectTransform != null)
            {
                _originalScale = _rectTransform.localScale;
            }

            if (_image != null)
            {
                _originalColor = _image.color;
            }
        }

        private void OnDisable()
        {
            KillAllTweens();
            ResetToOriginalState();
        }

        private void OnDestroy()
        {
            KillAllTweens();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!CanInteract()) return;

            KillAllTweens();

            if (_rectTransform != null)
            {
                _scaleTween = _rectTransform.DOScale(_originalScale * hoverScale, enterDuration)
                    .SetEase(enterEase)
                    .SetAutoKill(true);
            }

            if (_image != null)
            {
                Color targetColor = _originalColor * hoverBrightness;
                targetColor.a = _originalColor.a;
                _colorTween = _image.DOColor(targetColor, enterDuration)
                    .SetEase(enterEase)
                    .SetAutoKill(true);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            KillAllTweens();

            if (_rectTransform != null)
            {
                _scaleTween = _rectTransform.DOScale(_originalScale, exitDuration)
                    .SetEase(exitEase)
                    .SetAutoKill(true);
            }

            if (_image != null)
            {
                _colorTween = _image.DOColor(_originalColor, exitDuration)
                    .SetEase(exitEase)
                    .SetAutoKill(true);
            }
        }

        private bool CanInteract()
        {
            if (_button != null && !_button.interactable) return false;
            if (_canvasGroup != null && (!_canvasGroup.interactable || !_canvasGroup.blocksRaycasts)) return false;
            return true;
        }

        private void KillAllTweens()
        {
            _scaleTween?.Kill();
            _colorTween?.Kill();
            _scaleTween = null;
            _colorTween = null;
        }

        private void ResetToOriginalState()
        {
            if (_rectTransform != null)
            {
                _rectTransform.localScale = _originalScale;
            }

            if (_image != null)
            {
                _image.color = _originalColor;
            }
        }
    }
}

