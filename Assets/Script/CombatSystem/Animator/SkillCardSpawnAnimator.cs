using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Threading.Tasks;
using System.Collections;

namespace Game.CombatSystem.Animation
{
    /// <summary>
    /// 스킬 카드가 공중에서 내려오는 애니메이션과 그림자 효과를 처리합니다.
    /// 그림자는 고정된 위치에 배치되고, 카드가 내려오는 동안 점점 진해집니다.
    /// </summary>
    [RequireComponent(typeof(RectTransform), typeof(CanvasGroup))]
    public class SkillCardSpawnAnimator : MonoBehaviour
    {
        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip spawnSound;

        [Header("Visual Effect")]
        [SerializeField] private GameObject spawnEffectPrefab;

        [Header("Shadow Settings")]
        [SerializeField] private float initialShadowAlpha = 0.5f;
        [SerializeField] private float finalShadowAlpha = 0.8f;

        [Header("Offset Settings")]
        [SerializeField] private float cardStartOffsetY = 40f;

        private RectTransform rectTransform;
        private CanvasGroup canvasGroup;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            canvasGroup = GetComponent<CanvasGroup>();
        }

        /// <summary>
        /// 생성 애니메이션 실행
        /// </summary>
        /// <param name="onComplete">애니메이션 완료 후 호출될 콜백 (월드 좌표)</param>
        public void PlaySpawnAnimation(System.Action<Vector3> onComplete = null)
        {
            float totalDuration = 0.8f;
            float cardDelay = 0.1f;
            float cardDropDuration = totalDuration - cardDelay;

            // 카드 초기 위치 설정
            Vector2 targetPos = rectTransform.anchoredPosition;
            Vector2 cardStartPos = targetPos + new Vector2(0, cardStartOffsetY);
            rectTransform.anchoredPosition = cardStartPos;
            canvasGroup.alpha = 0f;

            // 그림자 생성 (위치 고정)
            GameObject shadowGO = CreateSimpleRectShadow(out Image shadowImage, out RectTransform shadowRect);
            shadowRect.anchoredPosition = targetPos;
            shadowImage.color = new Color(0f, 0f, 0f, initialShadowAlpha);

            Sequence sequence = DOTween.Sequence();

            // 사운드 재생
            sequence.OnStart(() =>
            {
                if (audioSource != null && spawnSound != null)
                    audioSource.PlayOneShot(spawnSound);
            });

            // 카드 등장 애니메이션
            sequence.AppendInterval(cardDelay);
            sequence.Append(rectTransform
                .DOAnchorPos(targetPos, cardDropDuration)
                .SetEase(Ease.InOutCubic));

            // 동시에: 카드 알파 & 그림자 알파
            sequence.Join(canvasGroup
                .DOFade(1f, cardDropDuration * 0.9f));
            sequence.Join(shadowImage
                .DOFade(finalShadowAlpha, cardDropDuration)
                .SetEase(Ease.InOutSine));

            // 완료 시 효과
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
        /// 단순 사각형 그림자를 생성합니다 (Sprite 없이 Color만으로 그림자 효과).
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
        /// 비동기 방식으로 생성 애니메이션을 실행합니다.
        /// </summary>
        public Task PlaySpawnAnimationAsync()
        {
            var tcs = new TaskCompletionSource<bool>();
            PlaySpawnAnimation(_ => tcs.SetResult(true));
            return tcs.Task;
        }
        public IEnumerator PlaySpawnAnimationCoroutine()
        {
            yield return new WaitForSeconds(0.3f); // DOTween Sequence 또는 단순한 등장 효과 코루틴
        }

    }
}