using UnityEngine;
using DG.Tweening;
using Game.AnimationSystem.Interface;

namespace Game.AnimationSystem.Animator.CharacterAnimation.SpawnAnimation
{
	/// <summary>
	/// 캐릭터 등장 애니메이션 001: 페이드 인 + 약간의 스케일 팝업
	/// </summary>
	public class CharacterSpawnAnimation001 : MonoBehaviour, ICharacterSpawnAnimationScript
	{
		private Sequence currentSequence;

		public void PlayAnimation(string animationType, System.Action onComplete = null)
		{
			CompleteAnimation();
			var canvasGroup = gameObject.GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();
			var tr = transform;
			var orig = tr.localScale;
			canvasGroup.alpha = 0f;
			tr.localScale = orig * 0.9f;
			currentSequence = DOTween.Sequence();
			currentSequence.Join(canvasGroup.DOFade(1f, 0.35f).SetEase(Ease.OutQuad));
			currentSequence.Join(tr.DOScale(orig, 0.35f).SetEase(Ease.OutBack));
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


