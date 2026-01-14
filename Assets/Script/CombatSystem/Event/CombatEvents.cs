using System;
using Game.CharacterSystem.Data;
using UnityEngine;
using Game.CombatSystem.Slot;
using Game.SkillCardSystem.Interface;

namespace Game.CombatSystem
{
    public static class CombatEvents
    {
        #region 캐릭터 관련 이벤트
        // 플레이어 캐릭터 사망
        public static event Action<PlayerCharacterData, GameObject> OnPlayerCharacterDeath;
        // 플레이어 캐릭터 사망 이펙트 완료
        public static event Action OnPlayerDeathEffectComplete;
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
        #endregion

        #region 카드 관련 이벤트
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
        // 핸드 슬롯 스킬카드 소멸 애니메이션 트리거
        public static event Action<bool> OnHandSkillCardsVanishOnCharacterDeath; // bool: true=플레이어, false=적
        // 출혈 턴 시작 이펙트 완료
        public static event Action OnBleedTurnStartEffectComplete;
        #endregion

        #region 전투 상태 관련 이벤트
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
        // 공격 결과 처리
        public static event Action OnAttackResultProcessed;
        #endregion

        #region 전투 결과 관련 이벤트
        // 승리
        public static event Action OnVictory;
        // 패배
        public static event Action OnDefeat;
        // 게임 오버
        public static event Action OnGameOver;
        // 다음 적 스폰
        public static event Action<Game.CharacterSystem.Data.EnemyCharacterData> OnNextEnemySpawned;
        #endregion

        #region UI 관련 이벤트
        // 플레이어 입력 활성화
        public static event Action OnPlayerInputEnabled;
        // 플레이어 입력 비활성화
        public static event Action OnPlayerInputDisabled;
        // 시작 버튼 활성화
        public static event Action OnStartButtonEnabled;
        // 시작 버튼 비활성화
        public static event Action OnStartButtonDisabled;
        #endregion

        #region 이벤트 발행 메서드
        // 캐릭터 관련
        public static void RaisePlayerCharacterDeath(PlayerCharacterData data, GameObject obj) => OnPlayerCharacterDeath?.Invoke(data, obj);
        public static void RaisePlayerDeathEffectComplete() => OnPlayerDeathEffectComplete?.Invoke();
        public static void RaiseEnemyCharacterDeath(Game.CharacterSystem.Data.EnemyCharacterData data, GameObject obj) => OnEnemyCharacterDeath?.Invoke(data, obj);
        public static void RaisePlayerCharacterDamaged(PlayerCharacterData data, GameObject obj, int damage) => OnPlayerCharacterDamaged?.Invoke(data, obj, damage);
        public static void RaiseEnemyCharacterDamaged(Game.CharacterSystem.Data.EnemyCharacterData data, GameObject obj, int damage) => OnEnemyCharacterDamaged?.Invoke(data, obj, damage);
        public static void RaisePlayerCharacterHealed(PlayerCharacterData data, GameObject obj, int heal) => OnPlayerCharacterHealed?.Invoke(data, obj, heal);
        public static void RaiseEnemyCharacterHealed(Game.CharacterSystem.Data.EnemyCharacterData data, GameObject obj, int heal) => OnEnemyCharacterHealed?.Invoke(data, obj, heal);
        public static void RaisePlayerCharacterGuarded(PlayerCharacterData data, GameObject obj, int guard) => OnPlayerCharacterGuarded?.Invoke(data, obj, guard);
        public static void RaiseEnemyCharacterGuarded(Game.CharacterSystem.Data.EnemyCharacterData data, GameObject obj, int guard) => OnEnemyCharacterGuarded?.Invoke(data, obj, guard);

        // 카드 관련
        public static void RaisePlayerCardSpawn(string cardId, GameObject obj) => OnPlayerCardSpawn?.Invoke(cardId, obj);
        public static void RaiseEnemyCardSpawn(string cardId, GameObject obj) => OnEnemyCardSpawn?.Invoke(cardId, obj);
        public static void RaisePlayerCardUse(string cardId, GameObject obj) => OnPlayerCardUse?.Invoke(cardId, obj);
        public static void RaiseEnemyCardUse(string cardId, GameObject obj) => OnEnemyCardUse?.Invoke(cardId, obj);
        public static void RaisePlayerCardDestroy(string cardId, GameObject obj) => OnPlayerCardDestroy?.Invoke(cardId, obj);
        public static void RaiseEnemyCardDestroy(string cardId, GameObject obj) => OnEnemyCardDestroy?.Invoke(cardId, obj);
        public static void RaisePlayerCardMoved(string cardId, GameObject obj, CombatSlotPosition position) => OnPlayerCardMoved?.Invoke(cardId, obj, position);
        public static void RaiseEnemyCardMoved(string cardId, GameObject obj, CombatSlotPosition position) => OnEnemyCardMoved?.Invoke(cardId, obj, position);

        // 핸드 슬롯 스킬카드 소멸 애니메이션 트리거
        public static void RaiseHandSkillCardsVanishOnCharacterDeath(bool isPlayer) => OnHandSkillCardsVanishOnCharacterDeath?.Invoke(isPlayer);
        // 출혈 턴 시작 이펙트 완료
        public static void RaiseBleedTurnStartEffectComplete() => OnBleedTurnStartEffectComplete?.Invoke();

        // 전투 상태 관련
        public static void RaiseCombatStarted() => OnCombatStarted?.Invoke();
        public static void RaiseCombatEnded(bool isVictory) => OnCombatEnded?.Invoke(isVictory);
        public static void RaiseTurnStarted() => OnTurnStarted?.Invoke();
        public static void RaiseTurnEnded() => OnTurnEnded?.Invoke();
        public static void RaiseFirstAttackStarted() => OnFirstAttackStarted?.Invoke();
        public static void RaiseAttackResultProcessed() => OnAttackResultProcessed?.Invoke();

        // 전투 결과 관련
        public static void RaiseVictory() => OnVictory?.Invoke();
        public static void RaiseDefeat() => OnDefeat?.Invoke();
        public static void RaiseGameOver() => OnGameOver?.Invoke();
        public static void RaiseNextEnemySpawned(Game.CharacterSystem.Data.EnemyCharacterData enemyData) => OnNextEnemySpawned?.Invoke(enemyData);

        // UI 관련
        public static void RaisePlayerInputEnabled() => OnPlayerInputEnabled?.Invoke();
        public static void RaisePlayerInputDisabled() => OnPlayerInputDisabled?.Invoke();
        public static void RaiseStartButtonEnabled() => OnStartButtonEnabled?.Invoke();
        public static void RaiseStartButtonDisabled() => OnStartButtonDisabled?.Invoke();
        #endregion
    }
} 