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

        [Header("Shadow Settings")]
        [SerializeField] private float shadowAlpha = 0.7f;

        [Header("Movement Settings")]
        [SerializeField] private float liftHeight = 20f;
        [SerializeField] private float liftDuration = 0.5f;
        [SerializeField] private float moveDuration = 0.7f;
        [SerializeField] private float landDuration = 0.4f;
        [SerializeField] private float shakeStrength = 3f;
        [SerializeField] private int shakeVibrato = 4;

        private RectTransform rectTransform;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
        }
        public IEnumerator PlayMoveAnimationCoroutine(RectTransform targetSlot)
        {
            if (TryGetComponent(out RectTransform rt))
            {
                Vector3 targetPos = targetSlot.position;
                yield return rt.DOMove(targetPos, 0.4f).SetEase(Ease.InOutCubic).WaitForCompletion();
            }
        }


        public async Task PlayMoveAnimationAsync(RectTransform targetSlot)
        {
            Debug.Log("[SkillCardShiftAnimator] 이동 애니메이션 시작");

            await Task.Yield();

            var tcs = new TaskCompletionSource<bool>();

            Vector2 startPos = rectTransform.anchoredPosition;
            Vector2 liftedPos = startPos + new Vector2(0, liftHeight);
            Vector2 targetAnchoredPos = GetTargetAnchoredPosition(targetSlot);
            Vector2 targetLandingPos = targetAnchoredPos;

            Debug.Log($"[SkillCardShiftAnimator] 현재 위치: {startPos}, 타겟 위치: {targetLandingPos}");

            if (Vector2.Distance(startPos, targetLandingPos) < 1f)
            {
                Debug.LogWarning("[SkillCardShiftAnimator] 카드가 이미 대상 위치에 있습니다. 애니메이션 생략");
                tcs.SetResult(true);
                await tcs.Task;
                return;
            }

            if (rectTransform.parent != targetSlot.parent)
            {
                Debug.LogWarning("[SkillCardShiftAnimator] 카드와 슬롯의 부모가 다릅니다. 위치 계산이 어긋날 수 있습니다.");
            }

            GameObject shadowGO = CreateSimpleShadow(out Image shadowImage, out RectTransform shadowRect);
            shadowRect.anchoredPosition = startPos;
            shadowImage.color = new Color(0f, 0f, 0f, 0f);

            Sequence seq = DOTween.Sequence();

            seq.Append(rectTransform
                .DOAnchorPos(liftedPos, liftDuration)
                .SetEase(Ease.OutQuart));

            seq.Join(shadowImage
                .DOFade(shadowAlpha, liftDuration * 0.9f)
                .SetEase(Ease.InSine));

            seq.AppendCallback(() =>
            {
                Debug.Log("[SkillCardShiftAnimator] 이동 시작");
                if (audioSource && moveStartClip)
                    audioSource.PlayOneShot(moveStartClip);
            });

            seq.Append(rectTransform
                .DOAnchorPos(targetAnchoredPos + new Vector2(0, liftHeight), moveDuration)
                .SetEase(Ease.InOutQuad));

            seq.AppendCallback(() =>
            {
                Debug.Log("[SkillCardShiftAnimator] 착지 사운드 재생");
                if (audioSource && dropClip)
                    audioSource.PlayOneShot(dropClip);
            });

            seq.Append(rectTransform
                .DOAnchorPos(targetLandingPos, landDuration)
                .SetEase(Ease.InCubic));

            seq.Join(rectTransform
                .DOShakeAnchorPos(landDuration, shakeStrength, shakeVibrato, 90, false)
                .SetEase(Ease.OutSine));

            seq.OnComplete(() =>
            {
                Debug.Log("[SkillCardShiftAnimator] 이동 애니메이션 완료");
                Destroy(shadowGO);
                tcs.SetResult(true);
            });

            await tcs.Task;
        }

        private Vector2 GetTargetAnchoredPosition(RectTransform target)
        {
            Vector3 worldPos = target.TransformPoint(target.rect.center);
            Vector3 local = rectTransform.parent.InverseTransformPoint(worldPos);
            Debug.Log($"[SkillCardShiftAnimator] 변환된 타겟 anchoredPosition: {local}");
            return new Vector2(local.x, local.y);
        }


        private GameObject CreateSimpleShadow(out Image shadowImage, out RectTransform shadowRect)
        {
            GameObject shadowGO = new GameObject("MoveShadow", typeof(RectTransform), typeof(Image));

            // 카드와 같은 부모를 갖게 한다 (UI 기준 좌표 동일하게 유지)
            shadowGO.transform.SetParent(rectTransform.parent, false);

            shadowRect = shadowGO.GetComponent<RectTransform>();
            shadowRect.anchorMin = rectTransform.anchorMin;
            shadowRect.anchorMax = rectTransform.anchorMax;
            shadowRect.pivot = rectTransform.pivot;
            shadowRect.sizeDelta = rectTransform.sizeDelta;
            shadowRect.anchoredPosition = rectTransform.anchoredPosition; // 위치 일치
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
