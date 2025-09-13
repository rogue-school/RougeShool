using UnityEngine;
using DG.Tweening;

namespace Game.AnimationSystem.Data
{
    /// <summary>
    /// 애니메이션 스크립트 타입 열거형
    /// </summary>
    public enum AnimationScriptType
    {
        None,
        DefaultSkillCardSpawnAnimation,
        DefaultSkillCardMoveAnimation,
        DefaultSkillCardMoveToCombatSlotAnimation,
        DefaultSkillCardUseAnimation,
        DefaultSkillCardDragAnimation,
        DefaultSkillCardDropAnimation,
        DefaultSkillCardVanishAnimation
    }

    /// <summary>
    /// 스킬 카드 애니메이션 설정을 위한 구조체
    /// </summary>
    [System.Serializable]
    public class SkillCardAnimationSettings
    {
        [Header("애니메이션 스크립트 타입")]
        [Tooltip("사용할 애니메이션 스크립트 타입")]
        [SerializeField] private AnimationScriptType animationScriptType = AnimationScriptType.None;
        
        /// <summary>
        /// 애니메이션 스크립트 타입
        /// </summary>
        public AnimationScriptType AnimationScriptType
        {
            get => animationScriptType;
            set => animationScriptType = value;
        }
        
        /// <summary>
        /// 기본값을 반환하는 정적 메서드
        /// </summary>
        public static SkillCardAnimationSettings Default => new SkillCardAnimationSettings(AnimationScriptType.None);
        
        /// <summary>
        /// 설정이 비어있는지 확인합니다.
        /// </summary>
        public bool IsEmpty()
        {
            return animationScriptType == AnimationScriptType.None;
        }
        
        /// <summary>
        /// 애니메이션을 재생합니다.
        /// </summary>
        /// <param name="target">타겟 오브젝트</param>
        /// <param name="animationType">애니메이션 타입</param>
        public void PlayAnimation(GameObject target, string animationType, System.Action onComplete = null)
        {
            if (animationScriptType == AnimationScriptType.None)
            {
                // Fallback: 기본 애니메이션(페이드아웃/페이드인 등) 실행
                PlayFallbackAnimation(target, animationType, onComplete);
                return;
            }

            var scriptType = GetScriptTypeFromEnum();

            if (scriptType != null)
            {
                // 기존 컴포넌트 확인
                var animScript = target.GetComponent(scriptType) as AnimationSystem.Interface.IAnimationScript;
                
                // 컴포넌트가 없으면 동적으로 추가
                if (animScript == null)
                {
                    animScript = target.AddComponent(scriptType) as AnimationSystem.Interface.IAnimationScript;
                }

                // 인터페이스 캐스팅 확인
                if (animScript == null)
                {
                    Debug.LogError($"[SkillCardAnimationSettings] 인터페이스 캐스팅 실패: {scriptType.Name}이 IAnimationScript를 구현하지 않음");
                    onComplete?.Invoke();
                    return;
                }

                animScript.PlayAnimation(animationType, onComplete);
            }
            else
            {
                // Fallback: 기본 애니메이션(페이드아웃/페이드인 등) 실행
                PlayFallbackAnimation(target, animationType, onComplete);
            }
        }

        private void PlayFallbackAnimation(GameObject target, string animationType, System.Action onComplete)
        {
            var canvasGroup = target.GetComponent<UnityEngine.CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = target.AddComponent<UnityEngine.CanvasGroup>();

            if (animationType == "death")
            {
                canvasGroup.DOFade(0f, 0.5f).OnComplete(() => onComplete?.Invoke());
            }
            else if (animationType == "spawn")
            {
                canvasGroup.alpha = 0f;
                canvasGroup.DOFade(1f, 0.5f).OnComplete(() => onComplete?.Invoke());
            }
            else
            {
                onComplete?.Invoke();
            }
        }
        
        /// <summary>
        /// enum에서 실제 타입을 가져옵니다.
        /// </summary>
        public System.Type GetScriptTypeFromEnum()
        {
            string className = animationScriptType switch
            {
                AnimationScriptType.DefaultSkillCardSpawnAnimation => "Game.AnimationSystem.Animator.SkillCardAnimation.SpawnAnimation.DefaultSkillCardSpawnAnimation",
                AnimationScriptType.DefaultSkillCardMoveAnimation => "Game.AnimationSystem.Animator.SkillCardAnimation.MoveAnimation.DefaultSkillCardMoveAnimation",
                AnimationScriptType.DefaultSkillCardMoveToCombatSlotAnimation => "Game.AnimationSystem.Animator.SkillCardAnimation.MoveToCombatSlotAnimation.DefaultSkillCardMoveToCombatSlotAnimation",
                AnimationScriptType.DefaultSkillCardUseAnimation => "Game.AnimationSystem.Animator.SkillCardAnimation.UseAnimation.DefaultSkillCardUseAnimation",
                AnimationScriptType.DefaultSkillCardDragAnimation => "Game.AnimationSystem.Animator.SkillCardAnimation.DragAnimation.DefaultSkillCardDragAnimation",
                AnimationScriptType.DefaultSkillCardDropAnimation => "Game.AnimationSystem.Animator.SkillCardAnimation.DropAnimation.DefaultSkillCardDropAnimation",
                AnimationScriptType.DefaultSkillCardVanishAnimation => "Game.AnimationSystem.Animator.SkillCardAnimation.VanishAnimation.DefaultSkillCardVanishAnimation",
                _ => null
            };

            if (string.IsNullOrEmpty(className))
                return null;
                
            // 먼저 전체 타입명으로 시도
            var type = System.Type.GetType(className);
            if (type == null)
            {
                // 현재 어셈블리에서 타입 찾기 시도
                var assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
                foreach (var assembly in assemblies)
                {
                    type = assembly.GetType(className);
                    if (type != null)
                        break;
                }
            }
            
            if (type == null)
            {
                Debug.LogError($"[SkillCardAnimationSettings] 애니메이션 타입을 찾을 수 없습니다: {animationScriptType} -> {className}");
            }
            return type;
        }
        
        /// <summary>
        /// 매개변수 생성자
        /// </summary>
        /// <param name="scriptType">애니메이션 스크립트 타입</param>
        public SkillCardAnimationSettings(AnimationScriptType scriptType = AnimationScriptType.None)
        {
            this.animationScriptType = scriptType;
        }
    }
} 