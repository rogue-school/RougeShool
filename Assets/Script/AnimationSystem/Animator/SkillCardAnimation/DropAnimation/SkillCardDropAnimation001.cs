using UnityEngine;
using DG.Tweening;
using Game.AnimationSystem.Interface;

namespace Game.AnimationSystem.Animator.SkillCardAnimation.DropAnimation
{
	/// <summary>
	/// 스킬카드 드롭 애니메이션 001: 살짝 흔들리며 자리 고정
	/// </summary>
	public class SkillCardDropAnimation001 : MonoBehaviour, ISkillCardDropAnimationScript
	{
		private Sequence currentSequence;

		public void PlayAnimation(string animationType, System.Action onComplete = null)
		{
			CompleteAnimation();
			var rt = transform as RectTransform;
			currentSequence = DOTween.Sequence();
			currentSequence.Append(rt.DOAnchorPos(Vector2.zero, 0.12f).SetEase(Ease.OutQuad));
			currentSequence.Append(rt.DOPunchRotation(new Vector3(0, 0, 6f), 0.15f, 10, 0.7f));
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


