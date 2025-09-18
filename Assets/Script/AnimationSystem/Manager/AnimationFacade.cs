using UnityEngine;
using Zenject;
using System;
using Game.SkillCardSystem.Interface;
using System.Collections.Generic; // Added for List
using Game.CoreSystem.Animation;
using Game.AnimationSystem.Interface;
using Game.CoreSystem.Interface;

namespace Game.AnimationSystem.Manager
{
    public class AnimationFacade : MonoBehaviour, IAnimationFacade
    {
        #region Events
        public Action<ISkillCard, string> OnSkillCardAnimationPlayed { get; set; }
        public Action<string, string, bool> OnCharacterAnimationPlayed { get; set; }
        #endregion

        #region Private Fields
        private IAnimationDatabaseManager animationDatabaseManager;
        #endregion

        #region DI
        [Inject]
        public void Construct(IAnimationDatabaseManager animationDatabaseManager)
        {
            this.animationDatabaseManager = animationDatabaseManager;
        }
        #endregion

        private void Awake()
        {
            // 캐릭터 생성/사망
            Game.CombatSystem.CombatEvents.OnPlayerCharacterDeath += HandlePlayerCharacterDeath;
            Game.CombatSystem.CombatEvents.OnEnemyCharacterDeath += HandleEnemyCharacterDeath;
            // 스킬카드 이벤트 구독은 제거됨 - ISkillCard 기반 메서드만 사용
        }

        // 캐릭터 사망 애니메이션
        private void HandlePlayerCharacterDeath(Game.CharacterSystem.Data.PlayerCharacterData data, GameObject obj)
        {
            PlayPlayerCharacterDeathAnimation(data.name, obj);
            VanishAllHandCardsOnCharacterDeath(data.name, false);
        }
        private void HandleEnemyCharacterDeath(Game.CharacterSystem.Data.EnemyCharacterData data, GameObject obj)
        {
            PlayEnemyCharacterDeathAnimation(data.name, obj);
            VanishAllHandCardsOnCharacterDeath(data.name, true);
        }
        // 스킬카드 생성 애니메이션 - string cardId 기반 메서드들은 제거됨
        // ISkillCard 기반 메서드만 사용
        // (핸드 시스템 제거됨에 따라 미사용 로직 정리)

        // 핸드 슬롯 스킬카드 소멸 애니메이션 이벤트 핸들러
        private void HandleHandSkillCardsVanishOnCharacterDeath(bool isPlayer) {}

        // 데이터 로드
        public void LoadAllData() => animationDatabaseManager.ReloadDatabases();

        // 플레이어 캐릭터 애니메이션 실행
        public void PlayPlayerCharacterAnimation(string characterId, string animationType, GameObject target, System.Action onComplete = null)
        {
            Debug.Log($"[AnimationFacade] PlayPlayerCharacterAnimation 호출: characterId={characterId}, animationType={animationType}, target={target?.name}");
            animationDatabaseManager.PlayPlayerCharacterAnimation(characterId, animationType, target, onComplete);
        }

        // 적 캐릭터 애니메이션 실행
        public void PlayEnemyCharacterAnimation(string characterId, string animationType, GameObject target, System.Action onComplete = null)
        {
            Debug.Log($"[AnimationFacade] PlayEnemyCharacterAnimation 호출: characterId={characterId}, animationType={animationType}, target={target?.name}");
            animationDatabaseManager.PlayEnemyCharacterAnimation(characterId, animationType, target, onComplete);
        }

        // 캐릭터 사망 애니메이션 실행 (플레이어)
        public void PlayPlayerCharacterDeathAnimation(string characterId, GameObject target)
        {
            Debug.Log($"[AnimationFacade] PlayPlayerCharacterDeathAnimation 호출: characterId={characterId}, target={target?.name}");
            var entry = animationDatabaseManager.GetPlayerCharacterAnimationEntry(characterId);
            if (entry == null || entry.DeathAnimation.IsEmpty())
            {
                Debug.LogWarning($"[AnimationFacade] 캐릭터 {characterId}의 사망 애니메이션 타입이 설정되지 않음");
                return;
            }
            animationDatabaseManager.PlayPlayerCharacterAnimation(characterId, "death", target);
        }

        // 캐릭터 사망 애니메이션 실행 (적)
        public void PlayEnemyCharacterDeathAnimation(string characterId, GameObject target, System.Action onComplete = null)
        {
            Debug.Log($"[AnimationFacade] PlayEnemyCharacterDeathAnimation 호출: characterId={characterId}, target={target?.name}");
            var entry = animationDatabaseManager.GetEnemyCharacterAnimationEntry(characterId);
            if (entry == null || entry.DeathAnimation.IsEmpty())
            {
                Debug.LogWarning($"[AnimationFacade] 적 캐릭터 {characterId}의 사망 애니메이션 타입이 설정되지 않음");
                onComplete?.Invoke();
                return;
            }
            animationDatabaseManager.PlayEnemyCharacterAnimation(characterId, "death", target, onComplete);
        }

        // PlayCharacterAnimation, PlayCharacterDeathAnimation 파사드 메서드 추가
        public void PlayCharacterAnimation(string characterId, string animationType, GameObject target, System.Action onComplete = null, bool isEnemy = false)
        {
            // Debug.Log($"[AnimationFacade] PlayCharacterAnimation 호출: characterId={characterId}, animationType={animationType}, target={target?.name}, isEnemy={isEnemy}");
            if (isEnemy)
                PlayEnemyCharacterAnimation(characterId, animationType, target, onComplete);
            else
                PlayPlayerCharacterAnimation(characterId, animationType, target, onComplete);
        }
        public void PlayCharacterDeathAnimation(string characterId, GameObject target, System.Action onComplete = null, bool isEnemy = false)
        {
            Debug.Log($"[AnimationFacade] PlayCharacterDeathAnimation 호출: characterId={characterId}, target={target?.name}, isEnemy={isEnemy}");
            if (isEnemy)
                PlayEnemyCharacterDeathAnimation(characterId, target, onComplete);
            else
                PlayPlayerCharacterDeathAnimation(characterId, target);
        }

        // 스킬카드 애니메이션 실행 (통합) - string cardId 오버로드는 제거됨
        // ISkillCard 기반 메서드만 사용

        // ISkillCard 기반 오버로드 (통합 메서드 사용)
        public void PlaySkillCardAnimation(ISkillCard skillCard, GameObject target, string animationType, System.Action onComplete = null)
        {
            // Debug.Log($"[AnimationFacade] PlaySkillCardAnimation(ISkillCard) 호출: skillCard={skillCard?.GetCardName()}, target={target?.name}, animationType={animationType}");
            if (skillCard == null)
            {
                Debug.LogWarning("[AnimationFacade] skillCard가 null입니다.");
                onComplete?.Invoke();
                return;
            }
            // 통합된 메서드 사용
            animationDatabaseManager.PlaySkillCardAnimation(skillCard, target, animationType, onComplete);
        }

        #region Drag Animation Methods
        /// <summary>
        /// 스킬카드 드래그 시작 애니메이션을 실행합니다.
        /// </summary>
        public void PlaySkillCardDragStartAnimation(ISkillCard card, GameObject target, System.Action onComplete = null)
        {
            if (card == null)
            {
                Debug.LogWarning("[AnimationFacade] card가 null입니다.");
                onComplete?.Invoke();
                return;
            }
            animationDatabaseManager.PlaySkillCardAnimation(card, target, "drag", onComplete);
        }

        /// <summary>
        /// 스킬카드 드래그 종료 애니메이션을 실행합니다.
        /// </summary>
        public void PlaySkillCardDragEndAnimation(ISkillCard card, GameObject target, System.Action onComplete = null)
        {
            if (card == null)
            {
                Debug.LogWarning("[AnimationFacade] card가 null입니다.");
                onComplete?.Invoke();
                return;
            }
            animationDatabaseManager.PlaySkillCardAnimation(card, target, "drag", onComplete);
        }

        // string cardId 기반 메서드들은 제거됨 - ISkillCard 기반 메서드만 사용
        #endregion

        #region Drop Animation Methods
        /// <summary>
        /// 스킬카드 드롭 애니메이션을 실행합니다.
        /// </summary>
        public void PlaySkillCardDropAnimation(ISkillCard card, GameObject target, System.Action onComplete = null)
        {
            if (card == null)
            {
                Debug.LogWarning("[AnimationFacade] card가 null입니다.");
                onComplete?.Invoke();
                return;
            }
            animationDatabaseManager.PlaySkillCardAnimation(card, target, "drop", onComplete);
        }

        // string cardId 기반 드롭 메서드들은 제거됨 - ISkillCard 기반 메서드만 사용
        #endregion

        // 상태 출력
        public void PrintStatus() => animationDatabaseManager.DebugDatabaseStatus();

        public bool IsHandVanishAnimationPlaying { get; private set; } = false;

        /// <summary>
        /// 캐릭터 사망 시 해당 캐릭터의 핸드 슬롯에 남아있는 모든 카드를 소멸 애니메이션으로 처리한다.
        /// </summary>
        /// <param name="characterId">캐릭터 ID</param>
        /// <param name="isEnemy">적 캐릭터 여부</param>
        public void VanishAllHandCardsOnCharacterDeath(string characterId, bool isEnemy = false)
        {
            // 핸드 시스템 제거됨: 무시하고 종료
            IsHandVanishAnimationPlaying = false;
            Debug.Log("[AnimationFacade] 핸드 시스템이 제거되어 소멸 애니메이션을 건너뜁니다.");
        }
        
        /// <summary>
        /// 특정 캐릭터의 스킬카드들을 찾습니다.
        /// </summary>
        /// <param name="characterName">캐릭터 이름</param>
        /// <param name="isPlayerCharacter">플레이어 캐릭터 여부</param>
        /// <returns>스킬카드 GameObject 리스트</returns>
        private List<GameObject> FindCharacterSkillCards(string characterName, bool isPlayerCharacter) { return new List<GameObject>(); }
        
        /// <summary>
        /// 스킬카드가 특정 캐릭터에 속하는지 확인합니다.
        /// </summary>
        /// <param name="skillCard">스킬카드</param>
        /// <param name="characterName">캐릭터 이름</param>
        /// <param name="isPlayerCharacter">플레이어 캐릭터 여부</param>
        /// <returns>캐릭터에 속하는지 여부</returns>
        private bool IsSkillCardBelongsToCharacter(Game.SkillCardSystem.Interface.ISkillCard skillCard, string characterName, bool isPlayerCharacter) { return false; }
        
        #region 추가 인터페이스 메서드 구현
        
        // VanishAllHandCardsOnCharacterDeath 메서드는 위에서 이미 구현됨
        
        #endregion
    }
} 