using System;
using Game.CharacterSystem.Data;
using UnityEngine;
using Game.CombatSystem.Slot;
using Game.SkillCardSystem.Interface;
using AnimationSystem.Interface;

namespace Game.CombatSystem
{
    /// <summary>
    /// 전투 시스템의 모든 이벤트를 관리하는 클래스
    /// 이벤트를 그룹화하여 구조를 개선하고 유지보수성을 향상시킵니다.
    /// </summary>
    public static class CombatEvents
    {
        #region 캐릭터 관련 이벤트 그룹
        public static class Character
        {
            // 플레이어 캐릭터 사망
            public static event Action<PlayerCharacterData, GameObject> OnPlayerCharacterDeath;
            // 적 캐릭터 사망
            public static event Action<Game.CharacterSystem.Data.EnemyCharacterData, GameObject> OnEnemyCharacterDeath;
            // 플레이어 캐릭터 피해
            public static event Action<PlayerCharacterData, GameObject, int> OnPlayerCharacterDamaged;
            // 적 캐릭터 피해
            public static event Action<Game.CharacterSystem.Data.EnemyCharacterData, GameObject, int> OnEnemyCharacterDamaged;
            // 플레이어 캐릭터 회복
            public static event Action<PlayerCharacterData, GameObject, int> OnPlayerCharacterHealed;
            // 적 캐릭터 회복
            public static event Action<Game.CharacterSystem.Data.EnemyCharacterData, GameObject, int> OnEnemyCharacterHealed;
            // 플레이어 캐릭터 가드
            public static event Action<PlayerCharacterData, GameObject, int> OnPlayerCharacterGuarded;
            // 적 캐릭터 가드
            public static event Action<Game.CharacterSystem.Data.EnemyCharacterData, GameObject, int> OnEnemyCharacterGuarded;

            // 이벤트 발행 메서드
            public static void RaisePlayerCharacterDeath(PlayerCharacterData data, GameObject obj) => OnPlayerCharacterDeath?.Invoke(data, obj);
            public static void RaiseEnemyCharacterDeath(Game.CharacterSystem.Data.EnemyCharacterData data, GameObject obj) => OnEnemyCharacterDeath?.Invoke(data, obj);
            public static void RaisePlayerCharacterDamaged(PlayerCharacterData data, GameObject obj, int damage) => OnPlayerCharacterDamaged?.Invoke(data, obj, damage);
            public static void RaiseEnemyCharacterDamaged(Game.CharacterSystem.Data.EnemyCharacterData data, GameObject obj, int damage) => OnEnemyCharacterDamaged?.Invoke(data, obj, damage);
            public static void RaisePlayerCharacterHealed(PlayerCharacterData data, GameObject obj, int heal) => OnPlayerCharacterHealed?.Invoke(data, obj, heal);
            public static void RaiseEnemyCharacterHealed(Game.CharacterSystem.Data.EnemyCharacterData data, GameObject obj, int heal) => OnEnemyCharacterHealed?.Invoke(data, obj, heal);
            public static void RaisePlayerCharacterGuarded(PlayerCharacterData data, GameObject obj, int guard) => OnPlayerCharacterGuarded?.Invoke(data, obj, guard);
            public static void RaiseEnemyCharacterGuarded(Game.CharacterSystem.Data.EnemyCharacterData data, GameObject obj, int guard) => OnEnemyCharacterGuarded?.Invoke(data, obj, guard);
        }
        #endregion

        #region 카드 관련 이벤트 그룹
        public static class Card
        {
            // 플레이어 카드 생성
            public static event Action<string, GameObject> OnPlayerCardSpawn;
            // 적 카드 생성
            public static event Action<string, GameObject> OnEnemyCardSpawn;
            // 플레이어 카드 사용
            public static event Action<string, GameObject> OnPlayerCardUse;
            // 적 카드 사용
            public static event Action<string, GameObject> OnEnemyCardUse;
            // 플레이어 카드 파괴
            public static event Action<string, GameObject> OnPlayerCardDestroy;
            // 적 카드 파괴
            public static event Action<string, GameObject> OnEnemyCardDestroy;
            // 플레이어 카드 이동
            public static event Action<string, GameObject, CombatSlotPosition> OnPlayerCardMoved;
            // 적 카드 이동
            public static event Action<string, GameObject, CombatSlotPosition> OnEnemyCardMoved;

            // 이벤트 발행 메서드
            public static void RaisePlayerCardSpawn(string cardId, GameObject obj) => OnPlayerCardSpawn?.Invoke(cardId, obj);
            public static void RaiseEnemyCardSpawn(string cardId, GameObject obj) => OnEnemyCardSpawn?.Invoke(cardId, obj);
            public static void RaisePlayerCardUse(string cardId, GameObject obj) => OnPlayerCardUse?.Invoke(cardId, obj);
            public static void RaiseEnemyCardUse(string cardId, GameObject obj) => OnEnemyCardUse?.Invoke(cardId, obj);
            public static void RaisePlayerCardDestroy(string cardId, GameObject obj) => OnPlayerCardDestroy?.Invoke(cardId, obj);
            public static void RaiseEnemyCardDestroy(string cardId, GameObject obj) => OnEnemyCardDestroy?.Invoke(cardId, obj);
            public static void RaisePlayerCardMoved(string cardId, GameObject obj, CombatSlotPosition position) => OnPlayerCardMoved?.Invoke(cardId, obj, position);
            public static void RaiseEnemyCardMoved(string cardId, GameObject obj, CombatSlotPosition position) => OnEnemyCardMoved?.Invoke(cardId, obj, position);
        }
        #endregion

        #region 전투 상태 관련 이벤트 그룹
        public static class Combat
        {
            // 전투 시작
            public static event Action OnCombatStarted;
            // 전투 종료
            public static event Action<bool> OnCombatEnded; // bool: 승리 여부
            // 턴 시작
            public static event Action OnTurnStarted;
            // 턴 종료
            public static event Action OnTurnEnded;
            // 첫 번째 공격 시작
            public static event Action OnFirstAttackStarted;
            // 두 번째 공격 시작
            public static event Action OnSecondAttackStarted;
            // 공격 결과 처리
            public static event Action OnAttackResultProcessed;

            // 이벤트 발행 메서드
            public static void RaiseCombatStarted() => OnCombatStarted?.Invoke();
            public static void RaiseCombatEnded(bool isVictory) => OnCombatEnded?.Invoke(isVictory);
            public static void RaiseTurnStarted() => OnTurnStarted?.Invoke();
            public static void RaiseTurnEnded() => OnTurnEnded?.Invoke();
            public static void RaiseFirstAttackStarted() => OnFirstAttackStarted?.Invoke();
            public static void RaiseSecondAttackStarted() => OnSecondAttackStarted?.Invoke();
            public static void RaiseAttackResultProcessed() => OnAttackResultProcessed?.Invoke();
        }
        #endregion

        #region 전투 결과 관련 이벤트 그룹
        public static class Result
        {
            // 승리
            public static event Action OnVictory;
            // 패배
            public static event Action OnDefeat;
            // 게임 오버
            public static event Action OnGameOver;
            // 다음 적 스폰
            public static event Action<Game.CharacterSystem.Data.EnemyCharacterData> OnNextEnemySpawned;

            // 이벤트 발행 메서드
            public static void RaiseVictory() => OnVictory?.Invoke();
            public static void RaiseDefeat() => OnDefeat?.Invoke();
            public static void RaiseGameOver() => OnGameOver?.Invoke();
            public static void RaiseNextEnemySpawned(Game.CharacterSystem.Data.EnemyCharacterData enemyData) => OnNextEnemySpawned?.Invoke(enemyData);
        }
        #endregion

        #region UI 관련 이벤트 그룹
        public static class UI
        {
            // 플레이어 입력 활성화
            public static event Action OnPlayerInputEnabled;
            // 플레이어 입력 비활성화
            public static event Action OnPlayerInputDisabled;
            // 시작 버튼 활성화
            public static event Action OnStartButtonEnabled;
            // 시작 버튼 비활성화
            public static event Action OnStartButtonDisabled;

            // 이벤트 발행 메서드
            public static void RaisePlayerInputEnabled() => OnPlayerInputEnabled?.Invoke();
            public static void RaisePlayerInputDisabled() => OnPlayerInputDisabled?.Invoke();
            public static void RaiseStartButtonEnabled() => OnStartButtonEnabled?.Invoke();
            public static void RaiseStartButtonDisabled() => OnStartButtonDisabled?.Invoke();
        }
        #endregion

        #region 애니메이션 관련 이벤트 그룹
        public static class Animation
        {
            // 캐릭터 애니메이션 이벤트
            public static event Action<string, string, GameObject, System.Action> OnCharacterAnimationRequested;
            public static event Action<string, GameObject, System.Action> OnCharacterDeathAnimationRequested;
            
            // 스킬카드 애니메이션 이벤트
            public static event Action<string, string, GameObject, System.Action> OnSkillCardAnimationRequested;
            public static event Action<ISkillCard, string, GameObject, System.Action> OnSkillCardAnimationWithCardRequested;
            public static event Action<ISkillCard, GameObject, System.Action> OnSkillCardDragStartAnimationRequested;
            public static event Action<ISkillCard, GameObject, System.Action> OnSkillCardDragEndAnimationRequested;
            public static event Action<ISkillCard, GameObject, System.Action> OnSkillCardDropAnimationRequested;
            public static event Action<string, bool, System.Action> OnVanishCharacterSkillCardsRequested;

            // 이벤트 발행 메서드
            public static void RaiseCharacterAnimationRequested(string characterId, string animationType, GameObject target, System.Action onComplete = null)
                => OnCharacterAnimationRequested?.Invoke(characterId, animationType, target, onComplete);
            
            public static void RaiseCharacterDeathAnimationRequested(string characterId, GameObject target, System.Action onComplete = null)
                => OnCharacterDeathAnimationRequested?.Invoke(characterId, target, onComplete);
            
            public static void RaiseSkillCardAnimationRequested(string cardId, string animationType, GameObject target, System.Action onComplete = null)
                => OnSkillCardAnimationRequested?.Invoke(cardId, animationType, target, onComplete);
            
            public static void RaiseSkillCardAnimationWithCardRequested(ISkillCard card, string animationType, GameObject target, System.Action onComplete = null)
                => OnSkillCardAnimationWithCardRequested?.Invoke(card, animationType, target, onComplete);
            
            public static void RaiseSkillCardDragStartAnimationRequested(ISkillCard card, GameObject target, System.Action onComplete = null)
                => OnSkillCardDragStartAnimationRequested?.Invoke(card, target, onComplete);
            
            public static void RaiseSkillCardDragEndAnimationRequested(ISkillCard card, GameObject target, System.Action onComplete = null)
                => OnSkillCardDragEndAnimationRequested?.Invoke(card, target, onComplete);
            
            public static void RaiseSkillCardDropAnimationRequested(ISkillCard card, GameObject target, System.Action onComplete = null)
                => OnSkillCardDropAnimationRequested?.Invoke(card, target, onComplete);
            
            public static void RaiseVanishCharacterSkillCardsRequested(string characterName, bool isPlayerCharacter, System.Action onComplete = null)
                => OnVanishCharacterSkillCardsRequested?.Invoke(characterName, isPlayerCharacter, onComplete);
        }
        #endregion

        #region 레거시 호환성 (기존 코드와의 호환성을 위해 유지)
        // 캐릭터 관련 이벤트 (레거시)
        public static event Action<PlayerCharacterData, GameObject> OnPlayerCharacterDeath
        {
            add => Character.OnPlayerCharacterDeath += value;
            remove => Character.OnPlayerCharacterDeath -= value;
        }
        public static event Action<Game.CharacterSystem.Data.EnemyCharacterData, GameObject> OnEnemyCharacterDeath
        {
            add => Character.OnEnemyCharacterDeath += value;
            remove => Character.OnEnemyCharacterDeath -= value;
        }

        // 전투 상태 관련 이벤트 (레거시)
        public static event Action OnCombatStarted
        {
            add => Combat.OnCombatStarted += value;
            remove => Combat.OnCombatStarted -= value;
        }
        public static event Action<bool> OnCombatEnded
        {
            add => Combat.OnCombatEnded += value;
            remove => Combat.OnCombatEnded -= value;
        }

        // 이벤트 발행 메서드 (레거시)
        public static void RaisePlayerCharacterDeath(PlayerCharacterData data, GameObject obj) => Character.RaisePlayerCharacterDeath(data, obj);
        public static void RaiseEnemyCharacterDeath(Game.CharacterSystem.Data.EnemyCharacterData data, GameObject obj) => Character.RaiseEnemyCharacterDeath(data, obj);
        public static void RaiseCombatStarted() => Combat.RaiseCombatStarted();
        public static void RaiseCombatEnded(bool isVictory) => Combat.RaiseCombatEnded(isVictory);
        #endregion
    }
} 