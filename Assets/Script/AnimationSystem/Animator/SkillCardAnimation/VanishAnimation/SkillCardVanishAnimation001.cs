using UnityEngine;
using DG.Tweening;
using Game.AnimationSystem.Interface;

namespace Game.AnimationSystem.Animator.SkillCardAnimation.VanishAnimation
{
	/// <summary>
	/// 스킬카드 소멸 애니메이션 001: 페이드 아웃 후 비활성화
	/// </summary>
	public class SkillCardVanishAnimation001 : MonoBehaviour, ISkillCardVanishAnimationScript
	{
		private Sequence currentSequence;

		public void PlayAnimation(string animationType, System.Action onComplete = null)
		{
			CompleteAnimation();
			var canvasGroup = gameObject.GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();
			canvasGroup.alpha = 1f;
			currentSequence = DOTween.Sequence();
			currentSequence.Append(canvasGroup.DOFade(0f, 0.25f).SetEase(Ease.InQuad));
			currentSequence.OnComplete(() =>
			{
				gameObject.SetActive(false);
				onComplete?.Invoke();
			});
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


