using UnityEngine;
using DG.Tweening;

namespace Game.AnimationSystem.Data
{
    /// <summary>
    /// 스킬 카드 애니메이션 설정을 위한 구조체
    /// </summary>
    [System.Serializable]
    public class SkillCardAnimationSettings
    {
        [Header("애니메이션 스크립트 타입")]
        [SerializeField] private string animationScriptType;
        
        // 파라미터 제거: 모든 애니메이션은 스크립트(001 또는 지정)로만 동작
        
        /// <summary>
        /// 애니메이션 스크립트 타입
        /// </summary>
        public string AnimationScriptType
        {
            get => animationScriptType;
            set => animationScriptType = value;
        }
        
        public static SkillCardAnimationSettings Default => new SkillCardAnimationSettings(string.Empty);
        
        /// <summary>
        /// 설정이 비어있는지 확인합니다.
        /// </summary>
        public bool IsEmpty()
        {
            return string.IsNullOrEmpty(animationScriptType);
        }
        
        /// <summary>
        /// 애니메이션을 재생합니다.
        /// </summary>
        /// <param name="target">타겟 오브젝트</param>
        /// <param name="animationType">애니메이션 타입</param>
        public void PlayAnimation(GameObject target, string animationType, System.Action onComplete = null)
        {
            var scriptType = GetScriptTypeForAnimation(animationType);

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
                Debug.LogError($"[SkillCardAnimationSettings] 유효한 애니메이션 스크립트를 찾지 못했습니다. animationType={animationType}");
                onComplete?.Invoke();
            }
        }
        
        private System.Type GetScriptTypeForAnimation(string animationType)
        {
            // 1) 인스펙터 타입 우선
            if (!string.IsNullOrEmpty(animationScriptType))
            {
                var type = System.Type.GetType(animationScriptType);
                if (type == null)
                {
                    var assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
                    foreach (var assembly in assemblies)
                    {
                        type = assembly.GetType(animationScriptType);
                        if (type != null)
                            break;
                    }
                }
                if (type != null) return type;
            }

            // 2) 비어있으면 슬롯별 디폴트(001) 고정 사용
            string @default = animationType switch
            {
                "spawn" => "Game.AnimationSystem.Animator.SkillCardAnimation.SpawnAnimation.SkillCardSpawnAnimation001",
                "move" => "Game.AnimationSystem.Animator.SkillCardAnimation.MoveAnimation.SkillCardMoveAnimation001",
                "moveToCombatSlot" => "Game.AnimationSystem.Animator.SkillCardAnimation.MoveToCombatSlotAnimation.SkillCardCombatSlotMoveAnimation001",
                "drop" => "Game.AnimationSystem.Animator.SkillCardAnimation.DropAnimation.SkillCardDropAnimation001",
                "drag" => "Game.AnimationSystem.Animator.SkillCardAnimation.DragAnimation.SkillCardDragAnimation001",
                "use" => "Game.AnimationSystem.Animator.SkillCardAnimation.UseAnimation.SkillCardUseAnimation001",
                "vanish" => "Game.AnimationSystem.Animator.SkillCardAnimation.VanishAnimation.SkillCardVanishAnimation001",
                _ => null
            };
            if (!string.IsNullOrEmpty(@default))
            {
                var type = System.Type.GetType(@default);
                if (type == null)
                {
                    var assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
                    foreach (var assembly in assemblies)
                    {
                        type = assembly.GetType(@default);
                        if (type != null) break;
                    }
                }
                return type;
            }

            return null;
        }
        
        public System.Type GetScriptType()
        {
            if (string.IsNullOrEmpty(animationScriptType))
                return null;
                
            // 먼저 전체 타입명으로 시도
            var type = System.Type.GetType(animationScriptType);
            if (type == null)
            {
                // 현재 어셈블리에서 타입 찾기 시도
                var assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
                foreach (var assembly in assemblies)
                {
                    type = assembly.GetType(animationScriptType);
                    if (type != null)
                        break;
                }
            }
            
            if (type == null)
            {
                Debug.LogError($"[SkillCardAnimationSettings] 애니메이션 타입을 찾을 수 없습니다: {animationScriptType}");
            }
            return type;
        }
        
        /// <summary>
        /// 매개변수 생성자
        /// </summary>
        /// <param name="scriptType">애니메이션 스크립트 타입</param>
        /// <param name="duration">지속 시간</param>
        /// <param name="useEasing">이징 사용 여부</param>
        /// <param name="offset">위치 오프셋</param>
        /// <param name="scale">스케일 값</param>
        public SkillCardAnimationSettings(string scriptType, float duration = 1.0f, bool useEasing = true, Vector3 offset = default, float scale = 1.0f)
        {
            this.animationScriptType = scriptType;
            // 파라미터 제거
        }
    }
} 