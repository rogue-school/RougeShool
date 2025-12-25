using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using DG.Tweening;

namespace Game.UISystem
{
    /// <summary>
    /// 버튼에 언더라인 호버 효과를 적용합니다.
    /// - 포인터 진입 시 언더라인이 좌→우로 확장
    /// - 포인터 이탈, 비활성화 시 언더라인 축소 및 정리
    /// </summary>
    public class UnderlineHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("언더라인 생성 설정")]
        [Tooltip("언더라인 두께(픽셀)")]
        [SerializeField] private float underlineHeight = 2f;

        [Tooltip("언더라인 Y 오프셋(아래 여백)")]
        [SerializeField] private float bottomOffset = 4f;

        [Tooltip("언더라인 색상")]
        [SerializeField] private Color underlineColor = new Color(0.42f, 0.75f, 1f, 1f); // 하늘색 기본값

        [Header("애니메이션 설정")]
        [Tooltip("호버 진입 시간(초)")]
        [SerializeField] private float expandDuration = 0.2f;

        [Tooltip("호버 종료 시간(초)")]
        [SerializeField] private float collapseDuration = 0.15f;

        [SerializeField] private Ease expandEase = Ease.OutQuad;
        [SerializeField] private Ease collapseEase = Ease.InQuad;

        // 런타임 생성된 언더라인 RectTransform
        [SerializeField] private RectTransform underline;

        private Coroutine animCoroutine;
        private Button cachedButton;
        private CanvasGroup cachedCanvasGroup;

        private void Awake()
        {
            cachedButton = GetComponent<Button>();
            cachedCanvasGroup = GetComponent<CanvasGroup>();
            EnsureUnderlineExists();
        }

        private void OnEnable()
        {
            EnsureUnderlineExists();
            ResetUnderline();
        }

        private void OnDisable()
        {
            // 비활성화 시 트윈/코루틴 정리 및 언더라인 접기
            if (animCoroutine != null) StopCoroutine(animCoroutine);
            animCoroutine = null;
            if (underline != null)
            {
                underline.DOKill(false);
                underline.localScale = new Vector3(0f, 1f, 1f);
            }
        }

        private void OnDestroy()
        {
            if (underline != null)
            {
                underline.DOKill(false);
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!CanInteract()) return;
            if (underline == null) return;

            if (animCoroutine != null) StopCoroutine(animCoroutine);
            underline.DOKill(false);
            underline.localScale = new Vector3(Mathf.Clamp01(underline.localScale.x), 1f, 1f);
            underline.DOScaleX(1f, expandDuration)
                .SetEase(expandEase)
                .SetAutoKill(true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (underline == null) return;

            if (animCoroutine != null) StopCoroutine(animCoroutine);
            underline.DOKill(false);
            underline.DOScaleX(0f, collapseDuration)
                .SetEase(collapseEase)
                .SetAutoKill(true);
        }

        /// <summary>
        /// 외부에서 언더라인을 지정합니다.
        /// </summary>
        public void SetUnderline(RectTransform rect)
        {
            underline = rect;
            ResetUnderline();
        }

        private IEnumerator AnimateUnderline(float from, float to)
        {
            // 코루틴 경로는 호환용 (기존 호출 남아 있을 수 있음) - DOTween으로 즉시 위임
            if (underline == null) yield break;
            underline.DOKill(false);
            underline.localScale = new Vector3(Mathf.Clamp01(from), 1f, 1f);
            float duration = to > from ? expandDuration : collapseDuration;
            Ease ease = to > from ? expandEase : collapseEase;
            var tween = underline.DOScaleX(to, duration)
                .SetEase(ease)
                .SetAutoKill(true);
            yield return tween.WaitForCompletion();
        }

        private void ResetUnderline()
        {
            if (underline == null) return;
            underline.pivot = new Vector2(0f, 0.5f);
            underline.localScale = new Vector3(0f, 1f, 1f);
        }

        private bool CanInteract()
        {
            if (cachedButton != null && !cachedButton.interactable) return false;
            if (cachedCanvasGroup != null && (cachedCanvasGroup.interactable == false || cachedCanvasGroup.blocksRaycasts == false)) return false;
            return true;
        }

        /// <summary>
        /// 버튼 하위에 언더라인 오브젝트를 생성/보장합니다.
        /// 버튼 크기가 달라도 Anchor를 이용해 자동으로 가로폭을 맞춥니다.
        /// </summary>
        private void EnsureUnderlineExists()
        {
            if (underline != null) return;

            var go = new GameObject("Underline", typeof(RectTransform), typeof(Image));
            go.layer = gameObject.layer;
            var rect = go.GetComponent<RectTransform>();
            var img = go.GetComponent<Image>();

            rect.SetParent(transform, false);
            rect.anchorMin = new Vector2(0f, 0f);
            rect.anchorMax = new Vector2(1f, 0f);
            rect.pivot = new Vector2(0f, 0.5f);
            rect.sizeDelta = new Vector2(0f, Mathf.Max(1f, underlineHeight));
            rect.anchoredPosition = new Vector2(0f, bottomOffset);

            img.color = underlineColor;
            img.raycastTarget = false;

            underline = rect;
            underline.localScale = new Vector3(0f, 1f, 1f);
        }
    }
}