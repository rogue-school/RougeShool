using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Threading.Tasks;
using System.Collections;

namespace Game.CombatSystem.Animation
{
    [RequireComponent(typeof(RectTransform))]
    public class SkillCardShiftAnimator : MonoBehaviour
    {
        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip moveStartClip;
        [SerializeField] private AudioClip dropClip;

        // 스킬 카드에서만 쓰는 파라미터
        private float liftHeight = 10f;
        private float liftDuration = 0.15f;
        private float moveDuration = 0.22f;
        private float landDuration = 0.12f;
        private float shakeStrength = 3f;
        private int shakeVibrato = 4;
        private float shadowAlpha = 0.7f;

        private RectTransform rectTransform;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
        }

        public IEnumerator PlayMoveAnimationCoroutine(RectTransform targetSlot, bool moveShadowWithCard = false)
        {
            var task = PlayMoveAnimationAsync(targetSlot, moveShadowWithCard);
            while (!task.IsCompleted)
                yield return null;
        }

        public async Task PlayMoveAnimationAsync(RectTransform targetSlot, bool moveShadowWithCard = false)
        {
            //Debug.Log("[SkillCardShiftAnimator] 이동 애니메이션 시작");

            await Task.Yield();

            var tcs = new TaskCompletionSource<bool>();

            Vector2 startPos = rectTransform.anchoredPosition;
            Vector2 liftedPos = startPos + new Vector2(0, liftHeight);
            Vector2 targetAnchoredPos = GetTargetAnchoredPosition(targetSlot);
            Vector2 targetLandingPos = targetAnchoredPos;

            Debug.Log($"[SkillCardShiftAnimator] 시작 위치: {startPos}, 목표 위치: {targetLandingPos}");

            if (Vector2.Distance(startPos, targetLandingPos) < 1f)
            {
                Debug.LogWarning("[SkillCardShiftAnimator] 이동할 필요가 없는 위치입니다. 애니메이션 생략");
                tcs.SetResult(true);
                await tcs.Task;
                return;
            }

            if (rectTransform.parent != targetSlot.parent)
            {
                Debug.LogWarning("[SkillCardShiftAnimator] 이동할 부모가 다릅니다. 위치 계산 주의.");
            }

            GameObject shadowGO = CreateSimpleShadow(out Image shadowImage, out RectTransform shadowRect);
            shadowRect.anchoredPosition = startPos;
            shadowImage.color = new Color(0f, 0f, 0f, 0f);

            Sequence seq = DOTween.Sequence();

            // 카드 들어올리기 (리프트)
            seq.Append(rectTransform
                .DOAnchorPos(liftedPos, liftDuration)
                .SetEase(Ease.OutQuart));
            seq.Join(shadowImage
                .DOFade(shadowAlpha, liftDuration * 0.9f)
                .SetEase(Ease.InSine));

            seq.AppendCallback(() =>
            {
                //Debug.Log("[SkillCardShiftAnimator] 이동 시작");
                if (audioSource && moveStartClip)
                    audioSource.PlayOneShot(moveStartClip);
            });

            // 이동: 리프트 상태에서 목표 지점으로
            if (moveShadowWithCard)
            {
                // 그림자도 카드와 함께 이동
                seq.Append(rectTransform
                    .DOAnchorPos(targetAnchoredPos + new Vector2(0, liftHeight), moveDuration)
                    .SetEase(Ease.InOutQuad));
                seq.Join(shadowRect
                    .DOAnchorPos(targetAnchoredPos + new Vector2(0, liftHeight), moveDuration)
                    .SetEase(Ease.InOutQuad));
            }
            else
            {
                // 그림자는 수평 이동만
                seq.Append(rectTransform
                    .DOAnchorPos(targetAnchoredPos + new Vector2(0, liftHeight), moveDuration)
                    .SetEase(Ease.InOutQuad));
                seq.Join(shadowRect
                    .DOAnchorPos(new Vector2(targetAnchoredPos.x, startPos.y), moveDuration)
                    .SetEase(Ease.InOutQuad));
            }

            seq.AppendCallback(() =>
            {
                //Debug.Log("[SkillCardShiftAnimator] 착지 사운드");
                if (audioSource && dropClip)
                    audioSource.PlayOneShot(dropClip);
            });

            // 착지: 최종 위치로
            if (moveShadowWithCard)
            {
                seq.Append(rectTransform
                    .DOAnchorPos(targetLandingPos, landDuration)
                    .SetEase(Ease.InCubic));
                seq.Join(shadowRect
                    .DOAnchorPos(targetLandingPos, landDuration)
                    .SetEase(Ease.InCubic));
            }
            else
            {
                seq.Append(rectTransform
                    .DOAnchorPos(targetLandingPos, landDuration)
                    .SetEase(Ease.InCubic));
                seq.Join(shadowRect
                    .DOAnchorPos(new Vector2(targetLandingPos.x, startPos.y), landDuration)
                    .SetEase(Ease.InCubic));
            }

            seq.OnComplete(() =>
            {
                //Debug.Log("[SkillCardShiftAnimator] 이동 애니메이션 종료");
                Destroy(shadowGO);
                tcs.SetResult(true);
            });

            await tcs.Task;
        }

        private Vector2 GetTargetAnchoredPosition(RectTransform target)
        {
            Vector3 worldPos = target.TransformPoint(target.rect.center);
            Vector3 local = rectTransform.parent.InverseTransformPoint(worldPos);
            //Debug.Log($"[SkillCardShiftAnimator] 변환된 로컬 위치: {local}");
            return new Vector2(local.x, local.y);
        }

        private GameObject CreateSimpleShadow(out Image shadowImage, out RectTransform shadowRect)
        {
            GameObject shadowGO = new GameObject("MoveShadow", typeof(RectTransform), typeof(Image));

            // 이동할 카드의 부모와 동일한 부모에 생성
            shadowGO.transform.SetParent(rectTransform.parent, false);

            shadowRect = shadowGO.GetComponent<RectTransform>();
            shadowRect.anchorMin = rectTransform.anchorMin;
            shadowRect.anchorMax = rectTransform.anchorMax;
            shadowRect.pivot = rectTransform.pivot;
            shadowRect.sizeDelta = rectTransform.sizeDelta;
            shadowRect.anchoredPosition = rectTransform.anchoredPosition; // 초기 위치
            shadowRect.localScale = Vector3.one;
            shadowRect.SetSiblingIndex(rectTransform.GetSiblingIndex());

            shadowImage = shadowGO.GetComponent<Image>();
            shadowImage.sprite = null;
            shadowImage.color = new Color(0f, 0f, 0f, 0f);
            shadowImage.raycastTarget = false;

            return shadowGO;
        }
    }
}
