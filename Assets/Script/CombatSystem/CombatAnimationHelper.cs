using UnityEngine;
using AnimationSystem.Interface;
using AnimationSystem.Manager;
using Game.SkillCardSystem.Interface;
using Game.CharacterSystem.Data;

namespace Game.CombatSystem
{
    /// <summary>
    /// CombatSystem에서 AnimationSystem을 쉽게 사용할 수 있는 헬퍼 클래스
    /// 싱글톤 패턴을 사용하여 전역적으로 접근 가능합니다.
    /// </summary>
    public class CombatAnimationHelper : MonoBehaviour
    {
        public static CombatAnimationHelper Instance { get; private set; }

        private IAnimationFacade animationFacade;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeAnimationFacade();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// AnimationFacade를 초기화합니다.
        /// </summary>
        private void InitializeAnimationFacade()
        {
            animationFacade = AnimationSystem.Manager.AnimationFacade.Instance;
            if (animationFacade == null)
            {
                Debug.LogWarning("AnimationFacade 인스턴스를 찾을 수 없습니다. 자동으로 생성합니다.");
                GameObject animationSystem = new GameObject("AnimationSystem");
                animationFacade = animationSystem.AddComponent<AnimationSystem.Manager.AnimationFacade>();
            }
        }

        #region 캐릭터 애니메이션 헬퍼 메서드
        /// <summary>
        /// 플레이어 캐릭터 애니메이션을 재생합니다.
        /// </summary>
        public void PlayPlayerCharacterAnimation(string characterId, string animationType, GameObject target, System.Action onComplete = null)
        {
            if (animationFacade != null)
            {
                animationFacade.Character.PlayPlayerCharacterAnimation(characterId, animationType, target, onComplete);
            }
            else
            {
                Debug.LogError("AnimationFacade가 null입니다.");
            }
        }

        /// <summary>
        /// 적 캐릭터 애니메이션을 재생합니다.
        /// </summary>
        public void PlayEnemyCharacterAnimation(string characterId, string animationType, GameObject target, System.Action onComplete = null)
        {
            if (animationFacade != null)
            {
                animationFacade.Character.PlayEnemyCharacterAnimation(characterId, animationType, target, onComplete);
            }
            else
            {
                Debug.LogError("AnimationFacade가 null입니다.");
            }
        }

        /// <summary>
        /// 캐릭터 애니메이션을 재생합니다 (플레이어/적 구분).
        /// </summary>
        public void PlayCharacterAnimation(string characterId, string animationType, GameObject target, System.Action onComplete = null, bool isEnemy = false)
        {
            if (animationFacade != null)
            {
                animationFacade.Character.PlayCharacterAnimation(characterId, animationType, target, onComplete, isEnemy);
            }
            else
            {
                Debug.LogError("AnimationFacade가 null입니다.");
            }
        }

        /// <summary>
        /// 플레이어 캐릭터 사망 애니메이션을 재생합니다.
        /// </summary>
        public void PlayPlayerCharacterDeathAnimation(string characterId, GameObject target)
        {
            if (animationFacade != null)
            {
                animationFacade.Character.PlayPlayerCharacterDeathAnimation(characterId, target);
            }
            else
            {
                Debug.LogError("AnimationFacade가 null입니다.");
            }
        }

        /// <summary>
        /// 적 캐릭터 사망 애니메이션을 재생합니다.
        /// </summary>
        public void PlayEnemyCharacterDeathAnimation(string characterId, GameObject target, System.Action onComplete = null)
        {
            if (animationFacade != null)
            {
                animationFacade.Character.PlayEnemyCharacterDeathAnimation(characterId, target, onComplete);
            }
            else
            {
                Debug.LogError("AnimationFacade가 null입니다.");
            }
        }

        /// <summary>
        /// 캐릭터 사망 애니메이션을 재생합니다 (플레이어/적 구분).
        /// </summary>
        public void PlayCharacterDeathAnimation(string characterId, GameObject target, System.Action onComplete = null, bool isEnemy = false)
        {
            if (animationFacade != null)
            {
                animationFacade.Character.PlayCharacterDeathAnimation(characterId, target, onComplete, isEnemy);
            }
            else
            {
                Debug.LogError("AnimationFacade가 null입니다.");
            }
        }

        /// <summary>
        /// PlayerCharacterData를 사용하여 캐릭터 애니메이션을 재생합니다.
        /// </summary>
        public void PlayPlayerCharacterAnimation(PlayerCharacterData data, string animationType, GameObject target, System.Action onComplete = null)
        {
            PlayPlayerCharacterAnimation(data.DisplayName, animationType, target, onComplete);
        }

        /// <summary>
        /// EnemyCharacterData를 사용하여 캐릭터 애니메이션을 재생합니다.
        /// </summary>
        public void PlayEnemyCharacterAnimation(EnemyCharacterData data, string animationType, GameObject target, System.Action onComplete = null)
        {
            PlayEnemyCharacterAnimation(data.DisplayName, animationType, target, onComplete);
        }

        /// <summary>
        /// PlayerCharacterData를 사용하여 캐릭터 사망 애니메이션을 재생합니다.
        /// </summary>
        public void PlayPlayerCharacterDeathAnimation(PlayerCharacterData data, GameObject target)
        {
            PlayPlayerCharacterDeathAnimation(data.DisplayName, target);
        }

        /// <summary>
        /// EnemyCharacterData를 사용하여 캐릭터 사망 애니메이션을 재생합니다.
        /// </summary>
        public void PlayEnemyCharacterDeathAnimation(EnemyCharacterData data, GameObject target, System.Action onComplete = null)
        {
            PlayEnemyCharacterDeathAnimation(data.DisplayName, target, onComplete);
        }
        #endregion

        #region 스킬카드 애니메이션 헬퍼 메서드
        /// <summary>
        /// 스킬카드 애니메이션을 재생합니다.
        /// </summary>
        public void PlaySkillCardAnimation(string cardId, string animationType, GameObject target)
        {
            if (animationFacade != null)
            {
                animationFacade.SkillCard.PlaySkillCardAnimation(cardId, animationType, target);
            }
            else
            {
                Debug.LogError("AnimationFacade가 null입니다.");
            }
        }

        /// <summary>
        /// 스킬카드 애니메이션을 재생합니다 (콜백 포함).
        /// </summary>
        public void PlaySkillCardAnimation(string cardId, string animationType, GameObject target, System.Action onComplete)
        {
            if (animationFacade != null)
            {
                animationFacade.SkillCard.PlaySkillCardAnimation(cardId, animationType, target, onComplete);
            }
            else
            {
                Debug.LogError("AnimationFacade가 null입니다.");
            }
        }

        /// <summary>
        /// ISkillCard 객체를 사용하여 스킬카드 애니메이션을 재생합니다.
        /// </summary>
        public void PlaySkillCardAnimation(ISkillCard card, string animationType, GameObject target, System.Action onComplete = null)
        {
            if (animationFacade != null)
            {
                animationFacade.SkillCard.PlaySkillCardAnimation(card, animationType, target, onComplete);
            }
            else
            {
                Debug.LogError("AnimationFacade가 null입니다.");
            }
        }

        /// <summary>
        /// 스킬카드 드래그 시작 애니메이션을 재생합니다.
        /// </summary>
        public void PlaySkillCardDragStartAnimation(ISkillCard card, GameObject target, System.Action onComplete = null)
        {
            if (animationFacade != null)
            {
                animationFacade.SkillCard.PlaySkillCardDragStartAnimation(card, target, onComplete);
            }
            else
            {
                Debug.LogError("AnimationFacade가 null입니다.");
            }
        }

        /// <summary>
        /// 스킬카드 드래그 종료 애니메이션을 재생합니다.
        /// </summary>
        public void PlaySkillCardDragEndAnimation(ISkillCard card, GameObject target, System.Action onComplete = null)
        {
            if (animationFacade != null)
            {
                animationFacade.SkillCard.PlaySkillCardDragEndAnimation(card, target, onComplete);
            }
            else
            {
                Debug.LogError("AnimationFacade가 null입니다.");
            }
        }

        /// <summary>
        /// 스킬카드 드롭 애니메이션을 재생합니다.
        /// </summary>
        public void PlaySkillCardDropAnimation(ISkillCard card, GameObject target, System.Action onComplete = null)
        {
            if (animationFacade != null)
            {
                animationFacade.SkillCard.PlaySkillCardDropAnimation(card, target, onComplete);
            }
            else
            {
                Debug.LogError("AnimationFacade가 null입니다.");
            }
        }

        /// <summary>
        /// 캐릭터의 스킬카드들을 사라지게 합니다.
        /// </summary>
        public void VanishCharacterSkillCards(string characterName, bool isPlayerCharacter, System.Action onComplete = null)
        {
            if (animationFacade != null)
            {
                animationFacade.SkillCard.VanishCharacterSkillCards(characterName, isPlayerCharacter, onComplete);
            }
            else
            {
                Debug.LogError("AnimationFacade가 null입니다.");
            }
        }
        #endregion

        #region 전투 시나리오 애니메이션 헬퍼 메서드
        /// <summary>
        /// 캐릭터 피해 애니메이션을 재생합니다.
        /// </summary>
        public void PlayCharacterDamagedAnimation(PlayerCharacterData data, GameObject target, System.Action onComplete = null)
        {
            PlayPlayerCharacterAnimation(data, "Damaged", target, onComplete);
        }

        public void PlayCharacterDamagedAnimation(EnemyCharacterData data, GameObject target, System.Action onComplete = null)
        {
            PlayEnemyCharacterAnimation(data, "Damaged", target, onComplete);
        }

        /// <summary>
        /// 캐릭터 회복 애니메이션을 재생합니다.
        /// </summary>
        public void PlayCharacterHealedAnimation(PlayerCharacterData data, GameObject target, System.Action onComplete = null)
        {
            PlayPlayerCharacterAnimation(data, "Healed", target, onComplete);
        }

        public void PlayCharacterHealedAnimation(EnemyCharacterData data, GameObject target, System.Action onComplete = null)
        {
            PlayEnemyCharacterAnimation(data, "Healed", target, onComplete);
        }

        /// <summary>
        /// 캐릭터 가드 애니메이션을 재생합니다.
        /// </summary>
        public void PlayCharacterGuardedAnimation(PlayerCharacterData data, GameObject target, System.Action onComplete = null)
        {
            PlayPlayerCharacterAnimation(data, "Guarded", target, onComplete);
        }

        public void PlayCharacterGuardedAnimation(EnemyCharacterData data, GameObject target, System.Action onComplete = null)
        {
            PlayEnemyCharacterAnimation(data, "Guarded", target, onComplete);
        }

        /// <summary>
        /// 스킬카드 스폰 애니메이션을 재생합니다.
        /// </summary>
        public void PlaySkillCardSpawnAnimation(string cardId, GameObject target, System.Action onComplete = null)
        {
            PlaySkillCardAnimation(cardId, "Spawn", target, onComplete);
        }

        /// <summary>
        /// 스킬카드 사용 애니메이션을 재생합니다.
        /// </summary>
        public void PlaySkillCardUseAnimation(string cardId, GameObject target, System.Action onComplete = null)
        {
            PlaySkillCardAnimation(cardId, "Use", target, onComplete);
        }

        /// <summary>
        /// 스킬카드 파괴 애니메이션을 재생합니다.
        /// </summary>
        public void PlaySkillCardDestroyAnimation(string cardId, GameObject target, System.Action onComplete = null)
        {
            PlaySkillCardAnimation(cardId, "Destroy", target, onComplete);
        }

        /// <summary>
        /// 스킬카드 이동 애니메이션을 재생합니다.
        /// </summary>
        public void PlaySkillCardMoveAnimation(string cardId, GameObject target, System.Action onComplete = null)
        {
            PlaySkillCardAnimation(cardId, "Move", target, onComplete);
        }
        #endregion

        #region 시스템 상태 확인 메서드
        /// <summary>
        /// 애니메이션 시스템 상태를 확인합니다.
        /// </summary>
        public void CheckAnimationSystemStatus()
        {
            if (animationFacade != null)
            {
                animationFacade.PrintStatus();
            }
            else
            {
                Debug.LogError("AnimationFacade가 null입니다.");
            }
        }

        /// <summary>
        /// 애니메이션 데이터를 다시 로드합니다.
        /// </summary>
        public void ReloadAnimationData()
        {
            if (animationFacade != null)
            {
                animationFacade.LoadAllData();
            }
            else
            {
                Debug.LogError("AnimationFacade가 null입니다.");
            }
        }

        /// <summary>
        /// AnimationFacade 인스턴스가 유효한지 확인합니다.
        /// </summary>
        public bool IsAnimationSystemReady()
        {
            return animationFacade != null;
        }
        #endregion
    }
} 