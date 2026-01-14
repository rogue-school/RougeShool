using UnityEngine;
using TMPro;
using DG.Tweening;

namespace Game.CombatSystem.UI
{
    /// <summary>
    /// 전투 중 데미지/회복 등 수치를 떠오르게 표시하는 UI
    /// </summary>
    public class DamageTextUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI damageText;
        [SerializeField] private float duration = 1.0f;

        [Header("팝 애니메이션 설정")]
        [Tooltip("시작 스케일 (작게 시작)")]
        [SerializeField] private float startScale = 0.5f;
        [Tooltip("팝 최대 스케일")]
        [SerializeField] private float popScale = 1.5f;
        [Tooltip("팝 상승 시간 (초)")]
        [SerializeField] private float popDuration = 0.1f;
        [Tooltip("안정화 스케일 (최종)")]
        [SerializeField] private float settleScale = 1.0f;
        [Tooltip("안정화 시간 (초)")]
        [SerializeField] private float settleDuration = 0.1f;

        private RectTransform rectTransform;
        private Sequence scaleSequence;
        private Sequence positionSequence;
        private Color originalColor;
        private Color flashColor;
        private Vector2 basePosition;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
        }

        /// <summary>
        /// 데미지 또는 회복 수치를 표시합니다.
        /// </summary>
        /// <param name="amount">표시할 수치</param>
        /// <param name="color">텍스트 색상</param>
        /// <param name="prefix">접두사 (+ 또는 -)</param>
        /// <param name="initialYOffset">초기 Y 오프셋 (기존 텍스트 개수에 따라 조정)</param>
        public void Show(int amount, Color color, string prefix = "", float initialYOffset = 0f)
        {
            // GameObject가 비활성화 상태면 Coroutine 시작 불가
            if (!gameObject.activeInHierarchy)
            {
                Destroy(gameObject);
                return;
            }

            if (damageText != null)
            {
                damageText.text = $"{prefix}{amount}";
                originalColor = color;
                // 시작 시 불투명하게 보장
                color.a = 1f;
                damageText.color = color;

                // 플래시 색상 선택: 데미지('-')=노란색, 회복('+')=흰색, 기타=화이트
                if (prefix == "-")
                {
                    // 데미지 플래시: 붉은색에 가까운 앰버(레드 기운이 강한 노란색)
                    flashColor = new Color(1f, 0.45f, 0.05f);
                }
                else if (prefix == "+")
                {
                    flashColor = Color.white;
                }
                else
                {
                    flashColor = Color.white;
                }
            }

            // 초기 Y 오프셋을 고려한 위치 설정
            // 메이플스토리 스타일: 새 텍스트가 기존 텍스트 위에 생성됨 (Y값이 증가하면 위로)
            basePosition = new Vector2(30f, 40f + initialYOffset);
            rectTransform.anchoredPosition = basePosition;
            PlayPopScale();
            StopAllCoroutines();
            StartCoroutine(FloatAndFade(originalColor));
        }

        /// <summary>
        /// 위로 떠오르면서 알파값이 점점 줄어듭니다.
        /// </summary>
        private System.Collections.IEnumerator FloatAndFade(Color startColor)
        {
            float elapsed = 0f;

            while (elapsed < duration)
            {
                float delta = Time.deltaTime;
                elapsed += delta;

                // basePosition을 기준으로 위치 유지 (MoveUp으로 변경된 위치 반영)
                // DOTween 애니메이션이 진행 중이면 그 위치를 유지
                if (positionSequence == null || !positionSequence.IsActive())
                {
                    rectTransform.anchoredPosition = basePosition;
                }

                // 점점 투명하게 (현재 색상의 RGB는 유지하고 알파만 감소)
                if (damageText != null)
                {
                    Color current = damageText.color;
                    current.a = Mathf.Lerp(1f, 0f, elapsed / duration);
                    damageText.color = current;
                }

                yield return null;
            }

            Destroy(gameObject);
        }

        /// <summary>
        /// 텍스트가 작게 시작하여 버블처럼 커졌다가 안정화되는 스케일 애니메이션을 재생합니다.
        /// </summary>
        private void PlayPopScale()
        {
            // 기존 시퀀스 정리
            if (scaleSequence != null && scaleSequence.IsActive())
            {
                scaleSequence.Kill();
                scaleSequence = null;
            }

            rectTransform.localScale = Vector3.one * startScale;

            scaleSequence = DOTween.Sequence()
                .Append(rectTransform.DOScale(popScale, popDuration).SetEase(Ease.OutBack))
                .Join(damageText != null ? damageText.DOColor(flashColor, popDuration * 0.8f) : null)
                .Append(rectTransform.DOScale(settleScale, settleDuration).SetEase(Ease.OutQuad))
                .Join(damageText != null ? damageText.DOColor(originalColor, settleDuration) : null)
                .SetAutoKill(true)
                .OnComplete(() => { scaleSequence = null; });
        }

        private void OnDisable()
        {
            if (scaleSequence != null && scaleSequence.IsActive())
            {
                scaleSequence.Kill();
                scaleSequence = null;
            }
        }

        private void OnDestroy()
        {
            if (scaleSequence != null && scaleSequence.IsActive())
            {
                scaleSequence.Kill();
                scaleSequence = null;
            }
            if (positionSequence != null && positionSequence.IsActive())
            {
                positionSequence.Kill();
                positionSequence = null;
            }
        }

        /// <summary>
        /// 데미지 텍스트를 위로 이동시킵니다 (메이플스토리 스타일)
        /// </summary>
        /// <param name="offsetY">Y 오프셋 (위로 이동할 거리)</param>
        public void MoveUp(float offsetY)
        {
            if (rectTransform == null) return;

            // 기존 위치 이동 애니메이션 정리
            if (positionSequence != null && positionSequence.IsActive())
            {
                positionSequence.Kill();
            }

            // 새로운 기준 위치 계산
            basePosition = new Vector2(basePosition.x, basePosition.y + offsetY);

            // 부드럽게 위로 이동
            positionSequence = DOTween.Sequence()
                .Append(rectTransform.DOAnchorPosY(basePosition.y, 0.2f).SetEase(Ease.OutQuad))
                .SetAutoKill(true)
                .OnComplete(() => { positionSequence = null; });
        }

        /// <summary>
        /// 현재 기준 위치를 반환합니다
        /// </summary>
        public Vector2 GetBasePosition()
        {
            return basePosition;
        }
    }
}
