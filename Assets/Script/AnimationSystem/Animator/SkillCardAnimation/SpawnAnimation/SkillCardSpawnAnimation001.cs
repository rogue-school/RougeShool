using UnityEngine;
using DG.Tweening;
using Game.AnimationSystem.Interface;

namespace Game.AnimationSystem.Animator.SkillCardAnimation.SpawnAnimation
{
	/// <summary>
	/// 스킬카드 등장 애니메이션 001: 팝업 스케일
	/// </summary>
	public class SkillCardSpawnAnimation001 : MonoBehaviour, ISkillCardSpawnAnimationScript
	{
		private Sequence currentSequence;

		public void PlayAnimation(string animationType, System.Action onComplete = null)
		{
			CompleteAnimation();
			var tr = transform;
			var orig = tr.localScale;
			tr.localScale = Vector3.zero;
			currentSequence = DOTween.Sequence();
			currentSequence.Append(tr.DOScale(orig * 1.05f, 0.2f).SetEase(Ease.OutBack));
			currentSequence.Append(tr.DOScale(orig, 0.12f).SetEase(Ease.OutQuad));
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


