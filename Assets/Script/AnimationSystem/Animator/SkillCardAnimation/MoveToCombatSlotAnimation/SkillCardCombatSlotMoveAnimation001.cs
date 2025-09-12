using UnityEngine;
using DG.Tweening;
using Game.AnimationSystem.Interface;

namespace Game.AnimationSystem.Animator.SkillCardAnimation.MoveToCombatSlotAnimation
{
	/// <summary>
	/// 스킬카드 전투 슬롯 이동 애니메이션 001: 위로 튕긴 뒤 자리잡기
	/// </summary>
	public class SkillCardCombatSlotMoveAnimation001 : MonoBehaviour, ISkillCardCombatSlotMoveAnimationScript
	{
		private Sequence currentSequence;

		public void PlayAnimation(string animationType, System.Action onComplete = null)
		{
			CompleteAnimation();
			var rt = transform as RectTransform;
			var start = rt.anchoredPosition;
			var mid = start + new Vector2(0f, 60f);
			currentSequence = DOTween.Sequence();
			currentSequence.Append(rt.DOAnchorPos(mid, 0.12f).SetEase(Ease.OutQuad));
			currentSequence.Append(rt.DOAnchorPos(Vector2.zero, 0.18f).SetEase(Ease.InOutQuad));
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


