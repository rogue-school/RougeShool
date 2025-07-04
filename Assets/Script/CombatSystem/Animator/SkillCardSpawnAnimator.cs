using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Threading.Tasks;
using System.Collections;

namespace Game.CombatSystem.Animation
{
    /// <summary>
    /// ��ų ī�尡 ���߿��� �������� �ִϸ��̼ǰ� �׸��� ȿ���� ó���մϴ�.
    /// �׸��ڴ� ������ ��ġ�� ��ġ�ǰ�, ī�尡 �������� ���� ���� �������ϴ�.
    /// </summary>
    [RequireComponent(typeof(RectTransform), typeof(CanvasGroup))]
    public class SkillCardSpawnAnimator : MonoBehaviour
    {
        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip spawnSound;

        // 나머지는 코드에서만 관리
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
        /// ���� �ִϸ��̼� ����
        /// </summary>
        /// <param name="onComplete">�ִϸ��̼� �Ϸ� �� ȣ��� �ݹ� (���� ��ǥ)</param>
        public void PlaySpawnAnimation(System.Action<Vector3> onComplete = null)
        {
            float totalDuration = 0.32f;
            float cardDelay = 0.02f;
            float cardDropDuration = totalDuration - cardDelay;

            // ī�� �ʱ� ��ġ ����
            Vector2 targetPos = rectTransform.anchoredPosition;
            Vector2 cardStartPos = targetPos + new Vector2(0, cardStartOffsetY);
            rectTransform.anchoredPosition = cardStartPos;
            canvasGroup.alpha = 0f;

            // �׸��� ���� (��ġ ����)
            GameObject shadowGO = CreateSimpleRectShadow(out Image shadowImage, out RectTransform shadowRect);
            shadowRect.anchoredPosition = targetPos;
            shadowImage.color = new Color(0f, 0f, 0f, initialShadowAlpha);

            Sequence sequence = DOTween.Sequence();

            // ���� ���
            sequence.OnStart(() =>
            {
                if (audioSource != null && spawnSound != null)
                    audioSource.PlayOneShot(spawnSound);
            });

            // ī�� ���� �ִϸ��̼�
            sequence.AppendInterval(cardDelay);
            sequence.Append(rectTransform
                .DOAnchorPos(targetPos, cardDropDuration)
                .SetEase(Ease.InOutCubic));

            // ���ÿ�: ī�� ���� & �׸��� ����
            sequence.Join(canvasGroup
                .DOFade(1f, cardDropDuration * 0.9f));
            sequence.Join(shadowImage
                .DOFade(finalShadowAlpha, cardDropDuration)
                .SetEase(Ease.InOutSine));

            // �Ϸ� �� ȿ��
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
        /// �ܼ� �簢�� �׸��ڸ� �����մϴ� (Sprite ���� Color������ �׸��� ȿ��).
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
        /// �񵿱� ������� ���� �ִϸ��̼��� �����մϴ�.
        /// </summary>
        public Task PlaySpawnAnimationAsync()
        {
            var tcs = new TaskCompletionSource<bool>();
            PlaySpawnAnimation(_ => tcs.SetResult(true));
            return tcs.Task;
        }
        public IEnumerator PlaySpawnAnimationCoroutine()
        {
            float totalDuration = 0.32f; // PlaySpawnAnimation의 실제 애니메이션 총 시간과 맞춤
            PlaySpawnAnimation();
            yield return new WaitForSeconds(totalDuration);
        }

    }
}