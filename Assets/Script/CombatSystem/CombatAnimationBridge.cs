using UnityEngine;
using Game.CombatSystem;
using AnimationSystem.Interface;
using AnimationSystem.Manager;
using Game.SkillCardSystem.Interface;
using Game.CharacterSystem.Data;
using Game.CombatSystem.Slot;

namespace Game.CombatSystem
{
    /// <summary>
    /// CombatSystem과 AnimationSystem을 연결하는 브리지 클래스
    /// 이벤트 기반 아키텍처를 통해 두 시스템을 느슨하게 결합합니다.
    /// </summary>
    public class CombatAnimationBridge : MonoBehaviour
    {
        [Header("애니메이션 시스템 참조")]
        [SerializeField] private AnimationSystem.Manager.AnimationFacade animationFacade;

        private void Awake()
        {
            // AnimationFacade 인스턴스가 없으면 자동으로 찾아서 할당
            if (animationFacade == null)
            {
                animationFacade = AnimationSystem.Manager.AnimationFacade.Instance;
                if (animationFacade == null)
                {
                    Debug.LogWarning("AnimationFacade 인스턴스를 찾을 수 없습니다. 자동으로 생성합니다.");
                    GameObject animationSystem = new GameObject("AnimationSystem");
                    animationFacade = animationSystem.AddComponent<AnimationSystem.Manager.AnimationFacade>();
                }
            }
        }

        private void Start()
        {
            SubscribeToCombatEvents();
        }

        private void OnDestroy()
        {
            UnsubscribeFromCombatEvents();
        }

        /// <summary>
        /// CombatSystem 이벤트들을 구독합니다.
        /// </summary>
        private void SubscribeToCombatEvents()
        {
            // 캐릭터 관련 이벤트 구독
            CombatEvents.Character.OnPlayerCharacterDeath += OnPlayerCharacterDeath;
            CombatEvents.Character.OnEnemyCharacterDeath += OnEnemyCharacterDeath;
            CombatEvents.Character.OnPlayerCharacterDamaged += OnPlayerCharacterDamaged;
            CombatEvents.Character.OnEnemyCharacterDamaged += OnEnemyCharacterDamaged;
            CombatEvents.Character.OnPlayerCharacterHealed += OnPlayerCharacterHealed;
            CombatEvents.Character.OnEnemyCharacterHealed += OnEnemyCharacterHealed;
            CombatEvents.Character.OnPlayerCharacterGuarded += OnPlayerCharacterGuarded;
            CombatEvents.Character.OnEnemyCharacterGuarded += OnEnemyCharacterGuarded;

            // 카드 관련 이벤트 구독
            CombatEvents.Card.OnPlayerCardSpawn += OnPlayerCardSpawn;
            CombatEvents.Card.OnEnemyCardSpawn += OnEnemyCardSpawn;
            CombatEvents.Card.OnPlayerCardUse += OnPlayerCardUse;
            CombatEvents.Card.OnEnemyCardUse += OnEnemyCardUse;
            CombatEvents.Card.OnPlayerCardDestroy += OnPlayerCardDestroy;
            CombatEvents.Card.OnEnemyCardDestroy += OnEnemyCardDestroy;
            CombatEvents.Card.OnPlayerCardMoved += OnPlayerCardMoved;
            CombatEvents.Card.OnEnemyCardMoved += OnEnemyCardMoved;

            // 전투 상태 관련 이벤트 구독
            CombatEvents.Combat.OnCombatStarted += OnCombatStarted;
            CombatEvents.Combat.OnCombatEnded += OnCombatEnded;
            CombatEvents.Combat.OnTurnStarted += OnTurnStarted;
            CombatEvents.Combat.OnTurnEnded += OnTurnEnded;
            CombatEvents.Combat.OnFirstAttackStarted += OnFirstAttackStarted;
            CombatEvents.Combat.OnSecondAttackStarted += OnSecondAttackStarted;
            CombatEvents.Combat.OnAttackResultProcessed += OnAttackResultProcessed;

            // 애니메이션 요청 이벤트 구독
            CombatEvents.Animation.OnCharacterAnimationRequested += OnCharacterAnimationRequested;
            CombatEvents.Animation.OnCharacterDeathAnimationRequested += OnCharacterDeathAnimationRequested;
            CombatEvents.Animation.OnSkillCardAnimationRequested += OnSkillCardAnimationRequested;
            CombatEvents.Animation.OnSkillCardAnimationWithCardRequested += OnSkillCardAnimationWithCardRequested;
            CombatEvents.Animation.OnSkillCardDragStartAnimationRequested += OnSkillCardDragStartAnimationRequested;
            CombatEvents.Animation.OnSkillCardDragEndAnimationRequested += OnSkillCardDragEndAnimationRequested;
            CombatEvents.Animation.OnSkillCardDropAnimationRequested += OnSkillCardDropAnimationRequested;
            CombatEvents.Animation.OnVanishCharacterSkillCardsRequested += OnVanishCharacterSkillCardsRequested;
        }

        /// <summary>
        /// CombatSystem 이벤트 구독을 해제합니다.
        /// </summary>
        private void UnsubscribeFromCombatEvents()
        {
            // 캐릭터 관련 이벤트 구독 해제
            CombatEvents.Character.OnPlayerCharacterDeath -= OnPlayerCharacterDeath;
            CombatEvents.Character.OnEnemyCharacterDeath -= OnEnemyCharacterDeath;
            CombatEvents.Character.OnPlayerCharacterDamaged -= OnPlayerCharacterDamaged;
            CombatEvents.Character.OnEnemyCharacterDamaged -= OnEnemyCharacterDamaged;
            CombatEvents.Character.OnPlayerCharacterHealed -= OnPlayerCharacterHealed;
            CombatEvents.Character.OnEnemyCharacterHealed -= OnEnemyCharacterHealed;
            CombatEvents.Character.OnPlayerCharacterGuarded -= OnPlayerCharacterGuarded;
            CombatEvents.Character.OnEnemyCharacterGuarded -= OnEnemyCharacterGuarded;

            // 카드 관련 이벤트 구독 해제
            CombatEvents.Card.OnPlayerCardSpawn -= OnPlayerCardSpawn;
            CombatEvents.Card.OnEnemyCardSpawn -= OnEnemyCardSpawn;
            CombatEvents.Card.OnPlayerCardUse -= OnPlayerCardUse;
            CombatEvents.Card.OnEnemyCardUse -= OnEnemyCardUse;
            CombatEvents.Card.OnPlayerCardDestroy -= OnPlayerCardDestroy;
            CombatEvents.Card.OnEnemyCardDestroy -= OnEnemyCardDestroy;
            CombatEvents.Card.OnPlayerCardMoved -= OnPlayerCardMoved;
            CombatEvents.Card.OnEnemyCardMoved -= OnEnemyCardMoved;

            // 전투 상태 관련 이벤트 구독 해제
            CombatEvents.Combat.OnCombatStarted -= OnCombatStarted;
            CombatEvents.Combat.OnCombatEnded -= OnCombatEnded;
            CombatEvents.Combat.OnTurnStarted -= OnTurnStarted;
            CombatEvents.Combat.OnTurnEnded -= OnTurnEnded;
            CombatEvents.Combat.OnFirstAttackStarted -= OnFirstAttackStarted;
            CombatEvents.Combat.OnSecondAttackStarted -= OnSecondAttackStarted;
            CombatEvents.Combat.OnAttackResultProcessed -= OnAttackResultProcessed;

            // 애니메이션 요청 이벤트 구독 해제
            CombatEvents.Animation.OnCharacterAnimationRequested -= OnCharacterAnimationRequested;
            CombatEvents.Animation.OnCharacterDeathAnimationRequested -= OnCharacterDeathAnimationRequested;
            CombatEvents.Animation.OnSkillCardAnimationRequested -= OnSkillCardAnimationRequested;
            CombatEvents.Animation.OnSkillCardAnimationWithCardRequested -= OnSkillCardAnimationWithCardRequested;
            CombatEvents.Animation.OnSkillCardDragStartAnimationRequested -= OnSkillCardDragStartAnimationRequested;
            CombatEvents.Animation.OnSkillCardDragEndAnimationRequested -= OnSkillCardDragEndAnimationRequested;
            CombatEvents.Animation.OnSkillCardDropAnimationRequested -= OnSkillCardDropAnimationRequested;
            CombatEvents.Animation.OnVanishCharacterSkillCardsRequested -= OnVanishCharacterSkillCardsRequested;
        }

        #region 캐릭터 이벤트 핸들러
        private void OnPlayerCharacterDeath(PlayerCharacterData data, GameObject obj)
        {
            if (animationFacade != null)
            {
                animationFacade.Character.PlayPlayerCharacterDeathAnimation(data.DisplayName, obj);
            }
        }

        private void OnEnemyCharacterDeath(EnemyCharacterData data, GameObject obj)
        {
            if (animationFacade != null)
            {
                animationFacade.Character.PlayEnemyCharacterDeathAnimation(data.DisplayName, obj);
            }
        }

        private void OnPlayerCharacterDamaged(PlayerCharacterData data, GameObject obj, int damage)
        {
            if (animationFacade != null)
            {
                animationFacade.Character.PlayPlayerCharacterAnimation(data.DisplayName, "Damaged", obj);
            }
        }

        private void OnEnemyCharacterDamaged(EnemyCharacterData data, GameObject obj, int damage)
        {
            if (animationFacade != null)
            {
                animationFacade.Character.PlayEnemyCharacterAnimation(data.DisplayName, "Damaged", obj);
            }
        }

        private void OnPlayerCharacterHealed(PlayerCharacterData data, GameObject obj, int heal)
        {
            if (animationFacade != null)
            {
                animationFacade.Character.PlayPlayerCharacterAnimation(data.DisplayName, "Healed", obj);
            }
        }

        private void OnEnemyCharacterHealed(EnemyCharacterData data, GameObject obj, int heal)
        {
            if (animationFacade != null)
            {
                animationFacade.Character.PlayEnemyCharacterAnimation(data.DisplayName, "Healed", obj);
            }
        }

        private void OnPlayerCharacterGuarded(PlayerCharacterData data, GameObject obj, int guard)
        {
            if (animationFacade != null)
            {
                animationFacade.Character.PlayPlayerCharacterAnimation(data.DisplayName, "Guarded", obj);
            }
        }

        private void OnEnemyCharacterGuarded(EnemyCharacterData data, GameObject obj, int guard)
        {
            if (animationFacade != null)
            {
                animationFacade.Character.PlayEnemyCharacterAnimation(data.DisplayName, "Guarded", obj);
            }
        }
        #endregion

        #region 카드 이벤트 핸들러
        private void OnPlayerCardSpawn(string cardId, GameObject obj)
        {
            if (animationFacade != null)
            {
                animationFacade.SkillCard.PlaySkillCardAnimation(cardId, "Spawn", obj);
            }
        }

        private void OnEnemyCardSpawn(string cardId, GameObject obj)
        {
            if (animationFacade != null)
            {
                animationFacade.SkillCard.PlaySkillCardAnimation(cardId, "Spawn", obj);
            }
        }

        private void OnPlayerCardUse(string cardId, GameObject obj)
        {
            if (animationFacade != null)
            {
                animationFacade.SkillCard.PlaySkillCardAnimation(cardId, "Use", obj);
            }
        }

        private void OnEnemyCardUse(string cardId, GameObject obj)
        {
            if (animationFacade != null)
            {
                animationFacade.SkillCard.PlaySkillCardAnimation(cardId, "Use", obj);
            }
        }

        private void OnPlayerCardDestroy(string cardId, GameObject obj)
        {
            if (animationFacade != null)
            {
                animationFacade.SkillCard.PlaySkillCardAnimation(cardId, "Destroy", obj);
            }
        }

        private void OnEnemyCardDestroy(string cardId, GameObject obj)
        {
            if (animationFacade != null)
            {
                animationFacade.SkillCard.PlaySkillCardAnimation(cardId, "Destroy", obj);
            }
        }

        private void OnPlayerCardMoved(string cardId, GameObject obj, CombatSlotPosition position)
        {
            if (animationFacade != null)
            {
                animationFacade.SkillCard.PlaySkillCardAnimation(cardId, "Move", obj);
            }
        }

        private void OnEnemyCardMoved(string cardId, GameObject obj, CombatSlotPosition position)
        {
            if (animationFacade != null)
            {
                animationFacade.SkillCard.PlaySkillCardAnimation(cardId, "Move", obj);
            }
        }
        #endregion

        #region 전투 상태 이벤트 핸들러
        private void OnCombatStarted()
        {
            // 전투 시작 애니메이션은 필요에 따라 추가
            Debug.Log("전투 시작 애니메이션 실행");
        }

        private void OnCombatEnded(bool isVictory)
        {
            // 전투 종료 애니메이션은 필요에 따라 추가
            Debug.Log($"전투 종료 애니메이션 실행 - 승리: {isVictory}");
        }

        private void OnTurnStarted()
        {
            // 턴 시작 애니메이션은 필요에 따라 추가
            Debug.Log("턴 시작 애니메이션 실행");
        }

        private void OnTurnEnded()
        {
            // 턴 종료 애니메이션은 필요에 따라 추가
            Debug.Log("턴 종료 애니메이션 실행");
        }

        private void OnFirstAttackStarted()
        {
            // 첫 번째 공격 시작 애니메이션은 필요에 따라 추가
            Debug.Log("첫 번째 공격 시작 애니메이션 실행");
        }

        private void OnSecondAttackStarted()
        {
            // 두 번째 공격 시작 애니메이션은 필요에 따라 추가
            Debug.Log("두 번째 공격 시작 애니메이션 실행");
        }

        private void OnAttackResultProcessed()
        {
            // 공격 결과 처리 애니메이션은 필요에 따라 추가
            Debug.Log("공격 결과 처리 애니메이션 실행");
        }
        #endregion

        #region 애니메이션 요청 이벤트 핸들러
        private void OnCharacterAnimationRequested(string characterId, string animationType, GameObject target, System.Action onComplete)
        {
            if (animationFacade != null)
            {
                animationFacade.Character.PlayCharacterAnimation(characterId, animationType, target, onComplete);
            }
        }

        private void OnCharacterDeathAnimationRequested(string characterId, GameObject target, System.Action onComplete)
        {
            if (animationFacade != null)
            {
                animationFacade.Character.PlayCharacterDeathAnimation(characterId, target, onComplete);
            }
        }

        private void OnSkillCardAnimationRequested(string cardId, string animationType, GameObject target, System.Action onComplete)
        {
            if (animationFacade != null)
            {
                animationFacade.SkillCard.PlaySkillCardAnimation(cardId, animationType, target, onComplete);
            }
        }

        private void OnSkillCardAnimationWithCardRequested(ISkillCard card, string animationType, GameObject target, System.Action onComplete)
        {
            if (animationFacade != null)
            {
                animationFacade.SkillCard.PlaySkillCardAnimation(card, animationType, target, onComplete);
            }
        }

        private void OnSkillCardDragStartAnimationRequested(ISkillCard card, GameObject target, System.Action onComplete)
        {
            if (animationFacade != null)
            {
                animationFacade.SkillCard.PlaySkillCardDragStartAnimation(card, target, onComplete);
            }
        }

        private void OnSkillCardDragEndAnimationRequested(ISkillCard card, GameObject target, System.Action onComplete)
        {
            if (animationFacade != null)
            {
                animationFacade.SkillCard.PlaySkillCardDragEndAnimation(card, target, onComplete);
            }
        }

        private void OnSkillCardDropAnimationRequested(ISkillCard card, GameObject target, System.Action onComplete)
        {
            if (animationFacade != null)
            {
                animationFacade.SkillCard.PlaySkillCardDropAnimation(card, target, onComplete);
            }
        }

        private void OnVanishCharacterSkillCardsRequested(string characterName, bool isPlayerCharacter, System.Action onComplete)
        {
            if (animationFacade != null)
            {
                animationFacade.SkillCard.VanishCharacterSkillCards(characterName, isPlayerCharacter, onComplete);
            }
        }
        #endregion

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
    }
} 