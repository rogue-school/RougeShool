using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Threading.Tasks;

namespace Game.CombatSystem.Animation
{
    /// <summary>
    /// 스킬 카드가 슬롯으로 이동하는 애니메이션을 처리합니다.
    /// 이동 시 위로 살짝 떠오른 뒤 빠르게 이동하고 도착 후 흔들림과 함께 착지합니다.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class SkillCardShiftAnimator : MonoBehaviour
    {
        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip moveStartClip;
        [SerializeField] private AudioClip dropClip;

        [Header("Shadow Settings")]
        [SerializeField] private float shadowAlpha = 0.7f;

        [Header("Movement Settings")]
        [SerializeField] private float liftHeight = 20f;
        [SerializeField] private float liftDuration = 0.2f;
        [SerializeField] private float moveDuration = 0.4f;
        [SerializeField] private float shakeStrength = 10f;
        [SerializeField] private int shakeVibrato = 10;
        [SerializeField] private float dropDuration = 0.1f;

        private RectTransform rectTransform;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
        }

        /// <summary>
        /// 카드 이동 애니메이션 실행
        /// </summary>
        /// <param name="targetSlot">도착할 슬롯 위치</param>
        public async Task PlayMoveAnimationAsync(RectTransform targetSlot)
        {
            var tcs = new TaskCompletionSource<bool>();

            Vector2 originalPos = rectTransform.anchoredPosition;
            Vector2 liftedPos = originalPos + new Vector2(0, liftHeight);
            Vector2 targetPos = targetSlot.anchoredPosition;

            // 그림자 생성
            GameObject shadowGO = CreateSimpleShadow(out Image shadowImage, out RectTransform shadowRect);
            shadowRect.anchoredPosition = originalPos;
            shadowImage.color = new Color(0f, 0f, 0f, 0f); // 투명에서 시작

            Sequence sequence = DOTween.Sequence();

            // 1. 위로 살짝 떠오르기
            sequence.Append(rectTransform
                .DOAnchorPos(liftedPos, liftDuration)
                .SetEase(Ease.OutCubic));

            // 그림자 점점 진해짐
            sequence.Join(shadowImage
                .DOFade(shadowAlpha, liftDuration * 0.8f)
                .SetEase(Ease.InSine));

            // 2. 슬롯으로 빠르게 이동
            sequence.AppendCallback(() =>
            {
                if (audioSource != null && moveStartClip != null)
                    audioSource.PlayOneShot(moveStartClip);
            });

            sequence.Append(rectTransform
                .DOAnchorPos(targetPos, moveDuration)
                .SetEase(Ease.InOutCubic));

            // 3. 도착 후 흔들리면서 착지
            sequence.AppendCallback(() =>
            {
                if (audioSource != null && dropClip != null)
                    audioSource.PlayOneShot(dropClip);
            });

            sequence.Append(rectTransform
                .DOShakeAnchorPos(dropDuration, shakeStrength, shakeVibrato, 90, false)
                .SetEase(Ease.OutSine));

            sequence.OnComplete(() =>
            {
                Destroy(shadowGO);
                tcs.SetResult(true);
            });

            await tcs.Task;
        }

        /// <summary>
        /// 단순 사각형 그림자를 생성합니다 (Sprite 없이 Color만으로 그림자 효과).
        /// </summary>
        private GameObject CreateSimpleShadow(out Image shadowImage, out RectTransform shadowRect)
        {
            GameObject shadowGO = new GameObject("MoveShadow", typeof(RectTransform), typeof(Image));
            shadowGO.transform.SetParent(transform.parent, false);

            shadowRect = shadowGO.GetComponent<RectTransform>();
            shadowRect.anchorMin = rectTransform.anchorMin;
            shadowRect.anchorMax = rectTransform.anchorMax;
            shadowRect.pivot = rectTransform.pivot;
            shadowRect.sizeDelta = rectTransform.sizeDelta;
            shadowRect.SetSiblingIndex(rectTransform.GetSiblingIndex());

            shadowImage = shadowGO.GetComponent<Image>();
            shadowImage.sprite = null;
            shadowImage.color = new Color(0f, 0f, 0f, 0f);
            shadowImage.raycastTarget = false;

            return shadowGO;
        }
    }
}
