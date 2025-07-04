using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Threading.Tasks;
using System.Collections;

namespace Game.CombatSystem.Animation
{
    /// <summary>
    /// 스킬 카드가 생성될 때 재생되는 애니메이션과 그림자 효과를 처리합니다.
    /// 그림자는 카드의 위치에 맞춰 생성되며, 카드가 떨어지면서 자연스럽게 변합니다.
    /// </summary>
    [RequireComponent(typeof(RectTransform), typeof(CanvasGroup))]
    public class SkillCardSpawnAnimator : MonoBehaviour
    {
        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip spawnSound;

        // 스킬 카드 전용 파라미터
        private float cardStartOffsetY = 10f;
        private float initialShadowAlpha = 0.5f;
        private float finalShadowAlpha = 0.8f;

        [Header("Visual Effect")]
        [SerializeField] private GameObject spawnEffectPrefab;

        private RectTransform rectTransform;
        private CanvasGroup canvasGroup;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            canvasGroup = GetComponent<CanvasGroup>();
        }

        /// <summary>
        /// 생성 애니메이션 재생
        /// </summary>
        /// <param name="onComplete">애니메이션 완료 후 호출되는 콜백 (월드 좌표 반환)</param>
        public void PlaySpawnAnimation(System.Action<Vector3> onComplete = null)
        {
            float totalDuration = 0.32f;
            float cardDelay = 0.02f;
            float cardDropDuration = totalDuration - cardDelay;

            // 카드 초기 위치 설정
            Vector2 targetPos = rectTransform.anchoredPosition;
            Vector2 cardStartPos = targetPos + new Vector2(0, cardStartOffsetY);
            rectTransform.anchoredPosition = cardStartPos;
            canvasGroup.alpha = 0f;

            // 그림자 생성 (위치 설정)
            GameObject shadowGO = CreateSimpleRectShadow(out Image shadowImage, out RectTransform shadowRect);
            shadowRect.anchoredPosition = targetPos;
            shadowImage.color = new Color(0f, 0f, 0f, initialShadowAlpha);

            Sequence sequence = DOTween.Sequence();

            // 시작 시 효과음 재생
            sequence.OnStart(() =>
            {
                if (audioSource != null && spawnSound != null)
                    audioSource.PlayOneShot(spawnSound);
            });

            // 카드 낙하 애니메이션
            sequence.AppendInterval(cardDelay);
            sequence.Append(rectTransform
                .DOAnchorPos(targetPos, cardDropDuration)
                .SetEase(Ease.InOutCubic));

            // 동시에: 카드 페이드 & 그림자 페이드
            sequence.Join(canvasGroup
                .DOFade(1f, cardDropDuration * 0.9f));
            sequence.Join(shadowImage
                .DOFade(finalShadowAlpha, cardDropDuration)
                .SetEase(Ease.InOutSine));

            // 완료 시 효과 처리
            sequence.OnComplete(() =>
            {
                Vector3 worldPos = rectTransform.position;

                if (spawnEffectPrefab != null)
                {
                    GameObject fx = Instantiate(spawnEffectPrefab, worldPos, Quaternion.identity);
                    Destroy(fx, 1.5f);
                }

                Destroy(shadowGO);
                onComplete?.Invoke(worldPos);
            });
        }

        /// <summary>
        /// 간단한 사각형 그림자를 생성합니다 (Sprite 없이 Color만으로 그림자 효과를 만듭니다).
        /// </summary>
        private GameObject CreateSimpleRectShadow(out Image shadowImage, out RectTransform shadowRect)
        {
            GameObject shadowGO = new GameObject("SimpleShadow", typeof(RectTransform), typeof(Image));
            shadowGO.transform.SetParent(transform.parent, false);

            shadowRect = shadowGO.GetComponent<RectTransform>();
            shadowRect.anchorMin = rectTransform.anchorMin;
            shadowRect.anchorMax = rectTransform.anchorMax;
            shadowRect.pivot = rectTransform.pivot;
            shadowRect.sizeDelta = rectTransform.sizeDelta;
            shadowRect.SetSiblingIndex(rectTransform.GetSiblingIndex());

            shadowImage = shadowGO.GetComponent<Image>();
            shadowImage.sprite = null;
            shadowImage.color = new Color(0f, 0f, 0f, initialShadowAlpha);
            shadowImage.raycastTarget = false;

            return shadowGO;
        }

        /// <summary>
        /// 비동기 방식으로 애니메이션을 재생합니다.
        /// </summary>
        public Task PlaySpawnAnimationAsync()
        {
            var tcs = new TaskCompletionSource<bool>();
            PlaySpawnAnimation(_ => tcs.SetResult(true));
            return tcs.Task;
        }

        /// <summary>
        /// 코루틴으로 애니메이션을 재생합니다.
        /// </summary>
        public IEnumerator PlaySpawnAnimationCoroutine()
        {
            float totalDuration = 0.32f; // PlaySpawnAnimation의 실제 애니메이션 총 시간과 맞춤
            PlaySpawnAnimation();
            yield return new WaitForSeconds(totalDuration);
        }
    }
}
