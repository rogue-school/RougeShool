using UnityEngine;
using DG.Tweening;
using Game.AnimationSystem.Interface;

namespace Game.AnimationSystem.Animator.SkillCardAnimation.DragAnimation
{
	/// <summary>
	/// 스킬카드 드래그 애니메이션 001: 시작/종료 스케일 조정
	/// </summary>
	public class SkillCardDragAnimation001 : MonoBehaviour, ISkillCardDragAnimationScript
	{
		private Tween currentTween;

		public void PlayAnimation(string animationType, System.Action onComplete = null)
		{
			CompleteAnimation();
			var tr = transform;
			if (animationType == "start")
			{
				currentTween = tr.DOScale(1.06f, 0.1f).SetEase(Ease.OutQuad).OnComplete(() => onComplete?.Invoke());
			}
			else
			{
				currentTween = tr.DOScale(1.0f, 0.1f).SetEase(Ease.OutQuad).OnComplete(() => onComplete?.Invoke());
			}
		}

		public void StopAnimation()
		{
			if (currentTween != null && currentTween.IsActive())
			{
				currentTween.Kill(false);
				currentTween = null;
			}
		}

		public void CompleteAnimation()
		{
			if (currentTween != null && currentTween.IsActive())
			{
				currentTween.Complete();
				currentTween = null;
			}
		}
	}
}


