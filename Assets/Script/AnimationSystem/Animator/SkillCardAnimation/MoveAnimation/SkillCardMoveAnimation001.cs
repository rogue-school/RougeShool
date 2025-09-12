using UnityEngine;
using DG.Tweening;
using Game.AnimationSystem.Interface;

namespace Game.AnimationSystem.Animator.SkillCardAnimation.MoveAnimation
{
	/// <summary>
	/// 스킬카드 이동 애니메이션 001: 부드러운 위치 보간
	/// </summary>
	public class SkillCardMoveAnimation001 : MonoBehaviour, ISkillCardMoveAnimationScript
	{
		private Tween currentTween;

		public void PlayAnimation(string animationType, System.Action onComplete = null)
		{
			CompleteAnimation();
			var rt = transform as RectTransform;
			var targetPos = Vector2.zero; // 부모 기준 자리로 이동하는 패턴을 가정
			currentTween = rt.DOAnchorPos(targetPos, 0.25f).SetEase(Ease.InOutQuad).OnComplete(() => onComplete?.Invoke());
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


