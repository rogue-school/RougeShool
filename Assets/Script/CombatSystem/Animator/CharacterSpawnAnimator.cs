using UnityEngine;
using DG.Tweening;

namespace Game.CombatSystem.Animation
{
    [RequireComponent(typeof(RectTransform), typeof(CanvasGroup))]
    public class CharacterSpawnAnimator : MonoBehaviour
    {
        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip spawnSound;

        [Header("Visual Effect")]
        [SerializeField] private GameObject spawnEffectPrefab;

        [Header("Shadow")]
        [SerializeField] private RectTransform shadowTransform;
        [SerializeField] private float shadowOffsetY = -20f;

        private RectTransform rectTransform;
        private CanvasGroup canvasGroup;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            canvasGroup = GetComponent<CanvasGroup>();
        }

        /// <summary>
        /// 등장 방향(오프셋)과 속도를 지정할 수 있는 등장 애니메이션
        /// </summary>
        public void PlaySpawnAnimation(float verticalOffset, float duration = 0.8f, System.Action<Vector3> onComplete = null)
        {
            float totalDuration = duration;
            float shadowDuration = totalDuration * 0.5f;
            float characterDuration = totalDuration * 0.5f;

            // 기본 위치 계산
            Vector2 targetPos = rectTransform.anchoredPosition;
            Vector2 startPos = targetPos + new Vector2(0, verticalOffset);
            rectTransform.anchoredPosition = startPos;
            canvasGroup.alpha = 0f;

            // 그림자 설정
            if (shadowTransform != null)
            {
                Vector2 shadowStart = startPos + new Vector2(0, shadowOffsetY);
                shadowTransform.anchoredPosition = shadowStart;
            }

            Sequence sequence = DOTween.Sequence();

            sequence.OnStart(() =>
            {
                if (audioSource != null && spawnSound != null)
                    audioSource.PlayOneShot(spawnSound);
            });

            // 1단계: 그림자 먼저 착지
            if (shadowTransform != null)
            {
                sequence.Append(
                    shadowTransform.DOAnchorPos(targetPos + new Vector2(0, shadowOffsetY), shadowDuration)
                        .SetEase(Ease.OutCubic)
                );
            }
            else
            {
                sequence.AppendInterval(shadowDuration); // 그림자가 없다면 대기
            }

            // 2단계: 캐릭터 내려오기 + 페이드 인
            sequence.Append(
                rectTransform.DOAnchorPos(targetPos, characterDuration)
                    .SetEase(Ease.OutBack)
            );
            sequence.Join(canvasGroup.DOFade(1f, characterDuration * 0.8f));

            // 종료 후 이펙트 재생 + 콜백
            sequence.OnComplete(() =>
            {
                Vector3 worldPos = rectTransform.position;

                if (spawnEffectPrefab != null)
                {
                    GameObject fx = Instantiate(spawnEffectPrefab, worldPos, Quaternion.identity);
                    Destroy(fx, 1.5f);
                }

                onComplete?.Invoke(worldPos);
            });
        }
    }
}
