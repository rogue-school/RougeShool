using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;
using Game.AnimationSystem.Data;
using Game.CharacterSystem.Data;
using Game.SkillCardSystem.Data;
using Game.SkillCardSystem.Interface;
using Game.CombatSystem.Data;
using Game.CombatSystem.Slot;
using Game.CoreSystem.Interface;
using Zenject;

namespace Game.CoreSystem.Animation
{
    /// <summary>
    /// 애니메이션 데이터베이스 매니저 (Zenject DI 기반)
    /// 스킬카드와 캐릭터 애니메이션을 통합 관리하는 매니저입니다.
    /// 플레이어/적용 데이터베이스를 분리하여 관리합니다.
    /// </summary>
    public class AnimationDatabaseManager : MonoBehaviour, IAnimationDatabaseManager
    {
        [Header("통합 데이터베이스 참조")]
        [SerializeField] private UnifiedSkillCardAnimationDatabase unifiedSkillCardDatabase;
        
        [Header("캐릭터 데이터베이스 참조")]
        [SerializeField] private PlayerCharacterAnimationDatabase playerCharacterDatabase;
        [SerializeField] private EnemyCharacterAnimationDatabase enemyCharacterDatabase;
        
        [Header("캐싱 설정")]
        [SerializeField] private bool enableCaching = true;
        [SerializeField] private int maxCacheSize = 100;
        
        // 캐싱
        private Dictionary<string, UnifiedSkillCardAnimationEntry> unifiedSkillCardCache = new();
        private Dictionary<string, PlayerCharacterAnimationEntry> playerCharacterCache = new();
        private Dictionary<string, EnemyCharacterAnimationEntry> enemyCharacterCache = new();
        
        // 이벤트
        public System.Action<string> OnSkillCardAnimationPlayed;
        public System.Action<string> OnCharacterAnimationPlayed;
        public System.Action<string> OnAnimationCacheUpdated;
        
        
        #region Properties
        public UnifiedSkillCardAnimationDatabase UnifiedSkillCardDatabase => unifiedSkillCardDatabase;
        public PlayerCharacterAnimationDatabase PlayerCharacterDatabase => playerCharacterDatabase;
        public EnemyCharacterAnimationDatabase EnemyCharacterDatabase => enemyCharacterDatabase;
        #endregion
        
        #region Unity Lifecycle
        private void Awake()
        {
            InitializeManager();
        }
        
        private void Start()
        {
            LoadDatabases();
            InitializeCaching();
        }
        
        private void OnDestroy()
        {
            ClearCache();
        }
        #endregion
        
        #region Initialization
        /// <summary>
        /// 매니저를 초기화합니다.
        /// </summary>
        private void InitializeManager()
        {
            if (unifiedSkillCardDatabase == null)
            {
                unifiedSkillCardDatabase = Resources.Load<UnifiedSkillCardAnimationDatabase>("Data/Animation/Unified/UnifiedSkillCardAnimationDatabase");
            }
            
            if (playerCharacterDatabase == null)
            {
                playerCharacterDatabase = Resources.Load<PlayerCharacterAnimationDatabase>("Data/Animation/PlayerCharcter/PlayerCharacterAnimationDatabase");
            }
            
            if (enemyCharacterDatabase == null)
            {
                enemyCharacterDatabase = Resources.Load<EnemyCharacterAnimationDatabase>("Data/Animation/EnemyCharacter/EnemyCharacterAnimationDatabase");
            }
        }
        
        /// <summary>
        /// 데이터베이스를 로드합니다.
        /// </summary>
        public void LoadDatabases()
        {
            if (unifiedSkillCardDatabase == null)
            {
                Debug.LogWarning("통합 스킬카드 애니메이션 데이터베이스를 찾을 수 없습니다.");
            }
            
            if (playerCharacterDatabase == null)
            {
                Debug.LogWarning("플레이어 캐릭터 애니메이션 데이터베이스를 찾을 수 없습니다.");
            }
            
            if (enemyCharacterDatabase == null)
            {
                Debug.LogWarning("적용 캐릭터 애니메이션 데이터베이스를 찾을 수 없습니다.");
            }
        }
        
        /// <summary>
        /// 캐싱을 초기화합니다.
        /// </summary>
        public void InitializeCaching()
        {
            if (enableCaching)
            {
                unifiedSkillCardCache.Clear();
                playerCharacterCache.Clear();
                enemyCharacterCache.Clear();
            }
        }
        #endregion
        
        #region Unified SkillCard Animation Methods
        /// <summary>
        /// 통합된 스킬카드 애니메이션을 재생합니다.
        /// </summary>
        public void PlaySkillCardAnimation(ISkillCard card, GameObject target, string animationType = "cast", System.Action onComplete = null)
        {
            if (card == null || target == null)
            {
                Debug.LogError("[AnimationDatabaseManager] 스킬카드 애니메이션 재생 실패: 필수 파라미터가 null입니다.");
                onComplete?.Invoke();
                return;
            }

            // 통합 데이터베이스 우선 사용
            if (unifiedSkillCardDatabase != null)
            {
                PlayUnifiedSkillCardAnimation(card, target, animationType, onComplete);
                return;
            }

            // 통합 데이터베이스가 없으면 경고
            Debug.LogWarning("통합 스킬카드 애니메이션 데이터베이스가 설정되지 않았습니다.");
            onComplete?.Invoke();
        }

        /// <summary>
        /// 통합된 스킬카드 애니메이션을 재생합니다. (string cardId 오버로드)
        /// </summary>
        public void PlaySkillCardAnimation(string cardId, GameObject target, string animationType = "cast", System.Action onComplete = null)
        {
            if (string.IsNullOrEmpty(cardId) || target == null || unifiedSkillCardDatabase == null)
            {
                Debug.LogError("[AnimationDatabaseManager] 통합 애니메이션 재생 실패: 필수 파라미터가 null입니다.");
                onComplete?.Invoke();
                return;
            }

            var entry = GetUnifiedSkillCardAnimationEntry(cardId);
            
            if (entry == null)
            {
                Debug.LogWarning($"통합 스킬카드 '{cardId}'를 찾을 수 없습니다.");
                onComplete?.Invoke();
                return;
            }

            // 기본적으로 플레이어 소유로 처리 (cardId만으로는 소유자 구분 불가)
            var cardOwner = Owner.Player;

            // 소유자 정책 확인
            if (!entry.CanUseAnimation(cardOwner, animationType))
            {
                Debug.LogWarning($"스킬카드 '{cardId}'의 '{animationType}' 애니메이션을 소유자 '{cardOwner}'가 사용할 수 없습니다.");
                onComplete?.Invoke();
                return;
            }

            var settings = entry.GetSettingsByType(animationType);
            if (!settings.IsEmpty())
            {
                settings.PlayAnimation(target, animationType, onComplete);
                OnSkillCardAnimationPlayed?.Invoke(cardId);
            }
            else
            {
                Debug.LogWarning($"스킬카드 '{cardId}'의 '{animationType}' 애니메이션이 설정되지 않았습니다.");
                onComplete?.Invoke();
            }
        }

        /// <summary>
        /// 통합된 스킬카드 애니메이션을 재생합니다.
        /// </summary>
        public void PlayUnifiedSkillCardAnimation(ISkillCard card, GameObject target, string animationType = "cast", System.Action onComplete = null)
        {
            if (card == null || target == null || unifiedSkillCardDatabase == null)
            {
                Debug.LogError("[AnimationDatabaseManager] 통합 애니메이션 재생 실패: 필수 파라미터가 null입니다.");
                onComplete?.Invoke();
                return;
            }

            var cardOwner = card.GetOwner() == SlotOwner.ENEMY ? Owner.Enemy : Owner.Player;
            var entry = GetUnifiedSkillCardAnimationEntry(card.CardDefinition.cardId);
            
            if (entry == null)
            {
                Debug.LogWarning($"통합 스킬카드 '{card.CardDefinition.cardId}'를 찾을 수 없습니다.");
                onComplete?.Invoke();
                return;
            }

            // 소유자 정책 확인
            if (!entry.CanUseAnimation(cardOwner, animationType))
            {
                Debug.LogWarning($"스킬카드 '{card.CardDefinition.cardId}'의 '{animationType}' 애니메이션을 소유자 '{cardOwner}'가 사용할 수 없습니다.");
                onComplete?.Invoke();
                return;
            }

            var settings = entry.GetSettingsByType(animationType);
            if (!settings.IsEmpty())
            {
                settings.PlayAnimation(target, animationType, onComplete);
                OnSkillCardAnimationPlayed?.Invoke(card.CardDefinition.cardId);
            }
            else
            {
                Debug.LogWarning($"스킬카드 '{card.CardDefinition.cardId}'의 '{animationType}' 애니메이션이 설정되지 않았습니다.");
                onComplete?.Invoke();
            }
        }

        /// <summary>
        /// 통합 스킬카드 애니메이션 엔트리를 가져옵니다.
        /// </summary>
        public UnifiedSkillCardAnimationEntry GetUnifiedSkillCardAnimationEntry(string cardId)
        {
            if (string.IsNullOrEmpty(cardId) || unifiedSkillCardDatabase == null)
                return null;

            if (enableCaching && unifiedSkillCardCache.ContainsKey(cardId))
                return unifiedSkillCardCache[cardId];

            var entry = unifiedSkillCardDatabase.FindEntryByCardId(cardId);
            if (enableCaching && entry != null)
            {
                if (unifiedSkillCardCache.Count >= maxCacheSize)
                {
                    var firstKey = unifiedSkillCardCache.Keys.GetEnumerator();
                    if (firstKey.MoveNext()) unifiedSkillCardCache.Remove(firstKey.Current);
                }
                unifiedSkillCardCache[cardId] = entry;
            }
            return entry;
        }
        #endregion
        
        #region Character Animation Methods
        /// <summary>
        /// 플레이어 캐릭터 애니메이션을 재생합니다.
        /// </summary>
        public void PlayPlayerCharacterAnimation(string characterId, GameObject target, string animationType = "spawn", System.Action onComplete = null)
        {
            var entry = GetPlayerCharacterAnimationEntry(characterId);
            if (entry != null)
            {
                var settings = GetCharacterAnimationSettings(entry, animationType);
                if (!settings.IsEmpty())
                {
                    settings.PlayAnimation(target, animationType, onComplete);
                    OnCharacterAnimationPlayed?.Invoke(characterId);
                }
                else
                {
                    Debug.LogWarning($"[애니메이션 DB] ScriptType 조회 실패: {characterId}, {animationType}, {target?.name}");
                    onComplete?.Invoke();
                }
            }
            else
            {
                Debug.LogWarning($"[애니메이션 DB] 캐릭터 엔트리 없음: {characterId}, {animationType}, {target?.name}");
                onComplete?.Invoke();
            }
        }
        
        /// <summary>
        /// 적용 캐릭터 애니메이션을 재생합니다.
        /// </summary>
        public void PlayEnemyCharacterAnimation(string characterId, GameObject target, string animationType = "spawn", System.Action onComplete = null)
        {
            var entry = GetEnemyCharacterAnimationEntry(characterId);
            if (entry != null)
            {
                var settings = GetCharacterAnimationSettings(entry, animationType);
                if (!settings.IsEmpty())
                {
                    settings.PlayAnimation(target, animationType, onComplete);
                }
                else
                {
                    Debug.LogWarning($"[애니메이션 DB] ScriptType 조회 실패: {characterId}, {animationType}, {target?.name}");
                    onComplete?.Invoke();
                }
            }
            else
            {
                Debug.LogWarning($"[애니메이션 DB] 캐릭터 엔트리 없음: {characterId}, {animationType}, {target?.name}");
                onComplete?.Invoke();
            }
        }
        
        /// <summary>
        /// 캐릭터 애니메이션 항목을 가져옵니다. (플레이어/적 통합)
        /// </summary>
        public PlayerCharacterAnimationEntry GetPlayerCharacterAnimationEntry(string characterId)
        {
            if (string.IsNullOrEmpty(characterId) || playerCharacterDatabase == null)
                return null;
            if (enableCaching && playerCharacterCache.ContainsKey(characterId))
                return playerCharacterCache[characterId];
            var entry = playerCharacterDatabase.CharacterAnimations.Find(e => e.PlayerCharacter != null && e.PlayerCharacter.name == characterId);
            if (enableCaching && entry != null)
            {
                if (playerCharacterCache.Count >= maxCacheSize)
                {
                    var firstKey = playerCharacterCache.Keys.GetEnumerator();
                    if (firstKey.MoveNext()) playerCharacterCache.Remove(firstKey.Current);
                }
                playerCharacterCache[characterId] = entry;
            }
            return entry;
        }
        public EnemyCharacterAnimationEntry GetEnemyCharacterAnimationEntry(string characterId)
        {
            if (string.IsNullOrEmpty(characterId) || enemyCharacterDatabase == null)
                return null;
            if (enableCaching && enemyCharacterCache.ContainsKey(characterId))
                return enemyCharacterCache[characterId];
            var entry = enemyCharacterDatabase.CharacterAnimations.Find(e => e.EnemyCharacter != null && e.EnemyCharacter.name == characterId);
            if (enableCaching && entry != null)
            {
                if (enemyCharacterCache.Count >= maxCacheSize)
                {
                    var firstKey = enemyCharacterCache.Keys.GetEnumerator();
                    if (firstKey.MoveNext()) enemyCharacterCache.Remove(firstKey.Current);
                }
                enemyCharacterCache[characterId] = entry;
            }
            return entry;
        }
        #endregion
        
        #region Utility Methods
        
        /// <summary>
        /// 캐릭터 애니메이션 설정을 가져옵니다.
        /// </summary>
        private Game.AnimationSystem.Data.CharacterAnimationSettings GetCharacterAnimationSettings(PlayerCharacterAnimationEntry entry, string animationType)
        {
            if (entry == null || string.IsNullOrEmpty(animationType))
                return CharacterAnimationSettings.Default;
            var settings = entry.GetSettingsByType(animationType.ToLower());
            return settings ?? CharacterAnimationSettings.Default;
        }
        
        
        private Game.AnimationSystem.Data.CharacterAnimationSettings GetCharacterAnimationSettings(EnemyCharacterAnimationEntry entry, string animationType)
        {
            if (entry == null || string.IsNullOrEmpty(animationType))
                return CharacterAnimationSettings.Default;
            var settings = entry.GetSettingsByType(animationType.ToLower());
            return settings ?? CharacterAnimationSettings.Default;
        }
        
        /// <summary>
        /// 데이터베이스를 다시 로드합니다.
        /// </summary>
        public void ReloadDatabases()
        {
            LoadDatabases();
            ClearCache();
        }
        
        /// <summary>
        /// 스킬카드 캐시를 초기화합니다.
        /// </summary>
        public void ClearSkillCardCache()
        {
            unifiedSkillCardCache.Clear();
            OnAnimationCacheUpdated?.Invoke("SkillCard");
        }
        
        /// <summary>
        /// 캐릭터 캐시를 초기화합니다.
        /// </summary>
        public void ClearCharacterCache()
        {
            playerCharacterCache.Clear();
            enemyCharacterCache.Clear();
            OnAnimationCacheUpdated?.Invoke("Character");
        }
        
        /// <summary>
        /// 모든 캐시를 초기화합니다.
        /// </summary>
        public void ClearCache()
        {
            ClearSkillCardCache();
            ClearCharacterCache();
        }
        
        /// <summary>
        /// 데이터베이스 상태를 디버그로 출력합니다.
        /// </summary>
        public void DebugDatabaseStatus()
        {
            Debug.Log("=== 애니메이션 데이터베이스 상태 ===");
            
            if (unifiedSkillCardDatabase != null)
            {
                Debug.Log($"통합 스킬카드 데이터베이스:");
                foreach (var entry in unifiedSkillCardDatabase.SkillCardAnimations)
                {
                    if (entry.SkillCardDefinition != null)
                        Debug.Log($"  - {entry.SkillCardDefinition.displayName} ({entry.OwnerPolicy})");
                }
            }
            
            if (playerCharacterDatabase != null)
            {
                Debug.Log($"플레이어 캐릭터 데이터베이스:");
                foreach (var entry in playerCharacterDatabase.CharacterAnimations)
                {
                    if (entry.PlayerCharacter != null)
                        Debug.Log($"  - {entry.PlayerCharacter.name}");
                }
            }
            
            if (enemyCharacterDatabase != null)
            {
                Debug.Log($"적 캐릭터 데이터베이스:");
                foreach (var entry in enemyCharacterDatabase.CharacterAnimations)
                {
                    if (entry.EnemyCharacter != null)
                        Debug.Log($"  - {entry.EnemyCharacter.name}");
                }
            }
            
            Debug.Log("=== 데이터베이스 상태 확인 완료 ===");
        }
        
        /// <summary>
        /// 캐시 상태를 디버그로 출력합니다.
        /// </summary>
        public Dictionary<string, object> GetCacheStatus()
        {
            return new Dictionary<string, object>
            {
                ["UnifiedSkillCardCache"] = unifiedSkillCardCache.Count,
                ["PlayerCharacterCache"] = playerCharacterCache.Count,
                ["EnemyCharacterCache"] = enemyCharacterCache.Count
            };
        }
        
        /// <summary>
        /// 애니메이션 스크립트를 동적으로 추가하고 실행하는 유틸리티 메서드
        /// </summary>
        private void PlayAnimationWithScript(GameObject target, Game.AnimationSystem.Data.SkillCardAnimationSettings settings, string animationType, System.Action onComplete)
        {
            if (target == null || settings == null || settings.AnimationScriptType == Game.AnimationSystem.Data.AnimationScriptType.None)
            {
                Debug.LogWarning("[AnimationDatabaseManager] PlayAnimationWithScript: 잘못된 파라미터");
                onComplete?.Invoke();
                return;
            }

            // 타입 찾기 (SkillCardAnimationSettings의 매핑 로직 사용)
            var type = settings.GetScriptTypeFromEnum();
            if (type == null)
            {
                Debug.LogWarning($"[AnimationDatabaseManager] 타입을 찾을 수 없습니다: {settings.AnimationScriptType}");
                onComplete?.Invoke();
                return;
            }

            // 기존 컴포넌트가 있으면 재사용, 없으면 추가
            var script = target.GetComponent(type) ?? target.AddComponent(type);

            // IAnimationScript 인터페이스 강제 캐스팅
            var animScript = script as Game.AnimationSystem.Interface.IAnimationScript;
            if (animScript == null)
            {
                Debug.LogWarning($"[AnimationDatabaseManager] IAnimationScript를 구현하지 않은 타입: {type.FullName}");
                onComplete?.Invoke();
                return;
            }

            // 애니메이션 실행
            animScript.PlayAnimation(animationType, onComplete);
        }
        #endregion
        
        #region 추가 인터페이스 메서드 구현
        
        /// <summary>
        /// 플레이어 캐릭터 애니메이션 실행
        /// </summary>
        public void PlayPlayerCharacterAnimation(string characterId, string animationType, GameObject target, System.Action onComplete = null)
        {
            Debug.Log($"[AnimationDatabaseManager] 플레이어 캐릭터 애니메이션 실행: characterId={characterId}, animationType={animationType}");
            // 실제 구현에서는 플레이어 캐릭터 애니메이션 실행
            onComplete?.Invoke();
        }
        
        /// <summary>
        /// 적 캐릭터 애니메이션 실행
        /// </summary>
        public void PlayEnemyCharacterAnimation(string characterId, string animationType, GameObject target, System.Action onComplete = null)
        {
            Debug.Log($"[AnimationDatabaseManager] 적 캐릭터 애니메이션 실행: characterId={characterId}, animationType={animationType}");
            // 실제 구현에서는 적 캐릭터 애니메이션 실행
            onComplete?.Invoke();
        }
        
        #endregion

    }
} 