using UnityEngine;
using DG.Tweening;
using Game.AnimationSystem.Interface;

namespace Game.AnimationSystem.Animator.SkillCardAnimation.UseAnimation
{
	/// <summary>
	/// 스킬카드 사용 애니메이션 001: 살짝 앞으로 튀며 발광 후 원위치
	/// </summary>
	public class SkillCardUseAnimation001 : MonoBehaviour, ISkillCardUseAnimationScript
	{
		private Sequence currentSequence;

		public void PlayAnimation(string animationType, System.Action onComplete = null)
		{
			CompleteAnimation();
			var tr = transform as RectTransform;
			var startPos = tr.anchoredPosition;
			var startScale = tr.localScale;

			var canvasGroup = gameObject.GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();
			canvasGroup.alpha = 1f;

			currentSequence = DOTween.Sequence();
			// 앞으로 튀기
			currentSequence.Append(tr.DOAnchorPos(startPos + new Vector2(0f, 30f), 0.12f).SetEase(Ease.OutQuad));
			// 살짝 커지며 발광
			currentSequence.Join(tr.DOScale(startScale * 1.08f, 0.12f).SetEase(Ease.OutQuad));
			currentSequence.Join(canvasGroup.DOFade(1f, 0.12f));
			// 원위치 복귀
			currentSequence.Append(tr.DOAnchorPos(startPos, 0.16f).SetEase(Ease.InOutQuad));
			currentSequence.Join(tr.DOScale(startScale, 0.16f).SetEase(Ease.InOutQuad));
			currentSequence.OnComplete(() => onComplete?.Invoke());
		}

		public void StopAnimation()
		{
			if (currentSequence != null && currentSequence.IsActive())
			{
				currentSequence.Kill(false);
				currentSequence = null;
			}
		}

		public void CompleteAnimation()
		{
			if (currentSequence != null && currentSequence.IsActive())
			{
				currentSequence.Complete();
				currentSequence = null;
			}
		}
	}
}


