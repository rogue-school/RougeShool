using UnityEngine;
using AnimationSystem.Interface;

namespace AnimationSystem.Manager
{
    /// <summary>
    /// 애니메이션 시스템의 메인 파사드 구현체
    /// SOLID 원칙을 기반으로 설계된 완벽한 파사드 패턴
    /// </summary>
    public class AnimationFacade : MonoBehaviour, IAnimationFacade
    {
        public static AnimationFacade Instance { get; private set; }

        [Header("분리된 파사드들")]
        [SerializeField] private CharacterAnimationFacade characterAnimationFacade;
        [SerializeField] private SkillCardAnimationFacade skillCardAnimationFacade;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeFacades();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// 분리된 파사드들을 초기화합니다.
        /// </summary>
        private void InitializeFacades()
        {
            characterAnimationFacade ??= gameObject.AddComponent<CharacterAnimationFacade>();
            skillCardAnimationFacade ??= gameObject.AddComponent<SkillCardAnimationFacade>();
        }

        /// <summary>
        /// 캐릭터 애니메이션 파사드
        /// </summary>
        public ICharacterAnimationFacade Character => characterAnimationFacade;

        /// <summary>
        /// 스킬카드 애니메이션 파사드
        /// </summary>
        public ISkillCardAnimationFacade SkillCard => skillCardAnimationFacade;

        /// <summary>
        /// 애니메이션 데이터를 로드합니다.
        /// </summary>
        public void LoadAllData() => AnimationDatabaseManager.Instance.ReloadDatabases();

        /// <summary>
        /// 시스템 상태를 출력합니다.
        /// </summary>
        public void PrintStatus() => AnimationDatabaseManager.Instance.DebugDatabaseStatus();

        public void PlayCharacterAnimation(string characterId, string animationType, GameObject target, System.Action onComplete = null, bool isEnemy = false)
            => characterAnimationFacade.PlayCharacterAnimation(characterId, animationType, target, onComplete, isEnemy);

        #region Legacy Support
        
        public void PlayPlayerCharacterAnimation(string characterId, string animationType, GameObject target, System.Action onComplete = null)
            => Character.PlayPlayerCharacterAnimation(characterId, animationType, target, onComplete);

        public void PlayEnemyCharacterAnimation(string characterId, string animationType, GameObject target, System.Action onComplete = null)
            => Character.PlayEnemyCharacterAnimation(characterId, animationType, target, onComplete);

        public void PlayCharacterDeathAnimation(string characterId, GameObject target, System.Action onComplete = null, bool isEnemy = false)
            => Character.PlayCharacterDeathAnimation(characterId, target, onComplete, isEnemy);

        public void PlaySkillCardAnimation(string cardId, string animationType, GameObject target)
            => SkillCard.PlaySkillCardAnimation(cardId, animationType, target);

        public void PlaySkillCardAnimation(string cardId, string animationType, GameObject target, System.Action onComplete)
            => SkillCard.PlaySkillCardAnimation(cardId, animationType, target, onComplete);

        public void PlaySkillCardAnimation(Game.SkillCardSystem.Interface.ISkillCard card, string animationType, GameObject target, System.Action onComplete = null)
            => SkillCard.PlaySkillCardAnimation(card, animationType, target, onComplete);

        public void VanishCharacterSkillCards(string characterName, bool isPlayerCharacter, System.Action onComplete = null)
            => SkillCard.VanishCharacterSkillCards(characterName, isPlayerCharacter, onComplete);

        #endregion
    }
} 