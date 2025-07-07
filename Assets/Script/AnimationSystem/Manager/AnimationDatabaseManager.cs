using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;
using AnimationSystem.Data;

using AnimationSystem.Interface;
using Game.SkillCardSystem.Core;
using Game.CharacterSystem.Data;
using Game.SkillCardSystem.Data;

namespace AnimationSystem.Manager
{
    /// <summary>
    /// 애니메이션 데이터베이스 매니저
    /// 스킬카드와 캐릭터 애니메이션을 통합 관리하는 싱글톤 매니저입니다.
    /// 플레이어/적용 데이터베이스를 분리하여 관리합니다.
    /// </summary>
    public class AnimationDatabaseManager : MonoBehaviour
    {
        [Header("플레이어 데이터베이스 참조")]
        [SerializeField] private PlayerSkillCardAnimationDatabase playerSkillCardDatabase;
        [SerializeField] private PlayerCharacterAnimationDatabase playerCharacterDatabase;
        
        [Header("적용 데이터베이스 참조")]
        [SerializeField] private EnemySkillCardAnimationDatabase enemySkillCardDatabase;
        [SerializeField] private EnemyCharacterAnimationDatabase enemyCharacterDatabase;
        
        [Header("캐싱 설정")]
        [SerializeField] private bool enableCaching = true;
        [SerializeField] private int maxCacheSize = 100;
        
        // 캐싱
        private Dictionary<string, PlayerSkillCardAnimationEntry> playerSkillCardCache = new();
        private Dictionary<string, EnemySkillCardAnimationEntry> enemySkillCardCache = new();
        private Dictionary<string, PlayerCharacterAnimationEntry> playerCharacterCache = new();
        private Dictionary<string, EnemyCharacterAnimationEntry> enemyCharacterCache = new();
        
        // 이벤트
        public System.Action<string> OnSkillCardAnimationPlayed;
        public System.Action<string> OnCharacterAnimationPlayed;
        public System.Action<string> OnAnimationCacheUpdated;
        
        #region Singleton
        private static AnimationDatabaseManager instance;
        public static AnimationDatabaseManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<AnimationDatabaseManager>();
                    if (instance == null)
                    {
                        var go = new GameObject("AnimationDatabaseManager");
                        instance = go.AddComponent<AnimationDatabaseManager>();
                        DontDestroyOnLoad(go);
                    }
                }
                return instance;
            }
        }
        #endregion
        
        #region Properties
        public PlayerSkillCardAnimationDatabase PlayerSkillCardDatabase => playerSkillCardDatabase;
        public EnemySkillCardAnimationDatabase EnemySkillCardDatabase => enemySkillCardDatabase;
        public PlayerCharacterAnimationDatabase PlayerCharacterDatabase => playerCharacterDatabase;
        public EnemyCharacterAnimationDatabase EnemyCharacterDatabase => enemyCharacterDatabase;
        #endregion
        
        #region Unity Lifecycle
        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeManager();
            }
            else if (instance != this)
            {
                Destroy(gameObject);
            }
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
            if (playerSkillCardDatabase == null)
            {
                playerSkillCardDatabase = Resources.Load<PlayerSkillCardAnimationDatabase>("AnimationSystem/PlayerSkillCardAnimationDatabase");
            }
            
            if (enemySkillCardDatabase == null)
            {
                enemySkillCardDatabase = Resources.Load<EnemySkillCardAnimationDatabase>("AnimationSystem/EnemySkillCardAnimationDatabase");
            }
            
            if (playerCharacterDatabase == null)
            {
                playerCharacterDatabase = Resources.Load<PlayerCharacterAnimationDatabase>("AnimationSystem/PlayerCharacterAnimationDatabase");
            }
            
            if (enemyCharacterDatabase == null)
            {
                enemyCharacterDatabase = Resources.Load<EnemyCharacterAnimationDatabase>("AnimationSystem/EnemyCharacterAnimationDatabase");
            }
        }
        
        /// <summary>
        /// 데이터베이스를 로드합니다.
        /// </summary>
        private void LoadDatabases()
        {
            if (playerSkillCardDatabase == null)
            {
                Debug.LogWarning("플레이어 스킬카드 애니메이션 데이터베이스를 찾을 수 없습니다.");
            }
            
            if (enemySkillCardDatabase == null)
            {
                Debug.LogWarning("적용 스킬카드 애니메이션 데이터베이스를 찾을 수 없습니다.");
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
        private void InitializeCaching()
        {
            if (enableCaching)
            {
                playerSkillCardCache.Clear();
                enemySkillCardCache.Clear();
                playerCharacterCache.Clear();
                enemyCharacterCache.Clear();
            }
        }
        #endregion
        
        #region SkillCard Animation Methods
        /// <summary>
        /// 플레이어 스킬카드 애니메이션을 재생합니다.
        /// </summary>
        public void PlayPlayerSkillCardAnimation(string skillCardId, GameObject target, string animationType = "cast")
        {
            var entry = GetPlayerSkillCardAnimationEntry(skillCardId);
            if (entry != null)
            {
                var settings = GetSkillCardAnimationSettings(entry, animationType);
                if (!settings.IsEmpty())
                {
                    settings.PlayAnimation(target, animationType);
                    OnSkillCardAnimationPlayed?.Invoke(skillCardId);
                }
            }
        }
        
        /// <summary>
        /// 적용 스킬카드 애니메이션을 재생합니다.
        /// </summary>
        public void PlayEnemySkillCardAnimation(string skillCardId, GameObject target, string animationType = "cast")
        {
            var entry = GetEnemySkillCardAnimationEntry(skillCardId);
            if (entry != null)
            {
                var settings = GetSkillCardAnimationSettings(entry, animationType);
                if (!settings.IsEmpty())
                {
                    settings.PlayAnimation(target, animationType);
                    OnSkillCardAnimationPlayed?.Invoke(skillCardId);
                }
            }
        }
        
        /// <summary>
        /// 플레이어 스킬카드 애니메이션 항목을 가져옵니다.
        /// </summary>
        public PlayerSkillCardAnimationEntry GetPlayerSkillCardAnimationEntry(string skillCardId)
        {
            if (string.IsNullOrEmpty(skillCardId) || playerSkillCardDatabase == null)
                return null;
                
            // 캐시 확인
            if (enableCaching && playerSkillCardCache.ContainsKey(skillCardId))
            {
                return playerSkillCardCache[skillCardId];
            }
            
            // 데이터베이스에서 검색
            var entry = playerSkillCardDatabase.SkillCardAnimations.Find(e => 
                e.SkillCardData != null && e.SkillCardData.name == skillCardId);
                
            // 캐시에 저장
            if (enableCaching && entry != null)
            {
                if (playerSkillCardCache.Count >= maxCacheSize)
                {
                    var firstKey = playerSkillCardCache.Keys.GetEnumerator();
                    firstKey.MoveNext();
                    playerSkillCardCache.Remove(firstKey.Current);
                }
                playerSkillCardCache[skillCardId] = entry;
            }
            
            return entry;
        }
        
        /// <summary>
        /// 적용 스킬카드 애니메이션 항목을 가져옵니다.
        /// </summary>
        public EnemySkillCardAnimationEntry GetEnemySkillCardAnimationEntry(string skillCardId)
        {
            if (string.IsNullOrEmpty(skillCardId) || enemySkillCardDatabase == null)
                return null;
                
            // 캐시 확인
            if (enableCaching && enemySkillCardCache.ContainsKey(skillCardId))
            {
                return enemySkillCardCache[skillCardId];
            }
            
            // 데이터베이스에서 검색
            var entry = enemySkillCardDatabase.SkillCardAnimations.Find(e => 
                e.SkillCardData != null && e.SkillCardData.name == skillCardId);
                
            // 캐시에 저장
            if (enableCaching && entry != null)
            {
                if (enemySkillCardCache.Count >= maxCacheSize)
                {
                    var firstKey = enemySkillCardCache.Keys.GetEnumerator();
                    firstKey.MoveNext();
                    enemySkillCardCache.Remove(firstKey.Current);
                }
                enemySkillCardCache[skillCardId] = entry;
            }
            
            return entry;
        }
        #endregion
        
        #region Character Animation Methods
        /// <summary>
        /// 플레이어 캐릭터 애니메이션을 재생합니다.
        /// </summary>
        public void PlayPlayerCharacterAnimation(string characterId, GameObject target, string animationType = "spawn")
        {
            var entry = GetPlayerCharacterAnimationEntry(characterId);
            if (entry != null)
            {
                var settings = GetCharacterAnimationSettings(entry, animationType);
                if (!settings.IsEmpty())
                {
                    settings.PlayAnimation(target, animationType);
                    OnCharacterAnimationPlayed?.Invoke(characterId);
                }
            }
        }
        
        /// <summary>
        /// 적용 캐릭터 애니메이션을 재생합니다.
        /// </summary>
        public void PlayEnemyCharacterAnimation(string characterId, GameObject target, string animationType = "spawn")
        {
            var entry = GetEnemyCharacterAnimationEntry(characterId);
            if (entry != null)
            {
                var settings = GetCharacterAnimationSettings(entry, animationType);
                if (!settings.IsEmpty())
                {
                    settings.PlayAnimation(target, animationType);
                    OnCharacterAnimationPlayed?.Invoke(characterId);
                }
            }
        }
        
        /// <summary>
        /// 플레이어 캐릭터 애니메이션 항목을 가져옵니다.
        /// </summary>
        public PlayerCharacterAnimationEntry GetPlayerCharacterAnimationEntry(string characterId)
        {
            if (string.IsNullOrEmpty(characterId) || playerCharacterDatabase == null)
                return null;
                
            // 캐시 확인
            if (enableCaching && playerCharacterCache.ContainsKey(characterId))
            {
                return playerCharacterCache[characterId];
            }
            
            // 데이터베이스에서 검색
            var entry = playerCharacterDatabase.CharacterAnimations.Find(e => 
                e.PlayerCharacterData != null && e.PlayerCharacterData.name == characterId);
                
            // 캐시에 저장
            if (enableCaching && entry != null)
            {
                if (playerCharacterCache.Count >= maxCacheSize)
                {
                    var firstKey = playerCharacterCache.Keys.GetEnumerator();
                    firstKey.MoveNext();
                    playerCharacterCache.Remove(firstKey.Current);
                }
                playerCharacterCache[characterId] = entry;
            }
            
            return entry;
        }
        
        /// <summary>
        /// 적용 캐릭터 애니메이션 항목을 가져옵니다.
        /// </summary>
        public EnemyCharacterAnimationEntry GetEnemyCharacterAnimationEntry(string characterId)
        {
            if (string.IsNullOrEmpty(characterId) || enemyCharacterDatabase == null)
                return null;
                
            // 캐시 확인
            if (enableCaching && enemyCharacterCache.ContainsKey(characterId))
            {
                return enemyCharacterCache[characterId];
            }
            
            // 데이터베이스에서 검색
            var entry = enemyCharacterDatabase.CharacterAnimations.Find(e => 
                e.EnemyCharacterData != null && e.EnemyCharacterData.name == characterId);
                
            // 캐시에 저장
            if (enableCaching && entry != null)
            {
                if (enemyCharacterCache.Count >= maxCacheSize)
                {
                    var firstKey = enemyCharacterCache.Keys.GetEnumerator();
                    firstKey.MoveNext();
                    enemyCharacterCache.Remove(firstKey.Current);
                }
                enemyCharacterCache[characterId] = entry;
            }
            
            return entry;
        }
        #endregion
        
        #region Utility Methods
        /// <summary>
        /// 스킬카드 애니메이션 설정을 가져옵니다.
        /// </summary>
        private AnimationSystem.Data.SkillCardAnimationSettings GetSkillCardAnimationSettings(PlayerSkillCardAnimationEntry entry, string animationType)
        {
            return animationType.ToLower() switch
            {
                "cast" => entry.CastAnimation,
                "use" => entry.UseAnimation,
                "hover" => entry.HoverAnimation,
                _ => SkillCardAnimationSettings.Default
            };
        }
        
        /// <summary>
        /// 스킬카드 애니메이션 설정을 가져옵니다.
        /// </summary>
        private AnimationSystem.Data.SkillCardAnimationSettings GetSkillCardAnimationSettings(EnemySkillCardAnimationEntry entry, string animationType)
        {
            return animationType.ToLower() switch
            {
                "cast" => entry.CastAnimation,
                "slotmove" => entry.SlotMoveAnimation,
                "battleslotplace" => entry.BattleSlotPlaceAnimation,
                "use" => entry.UseAnimation,
                _ => SkillCardAnimationSettings.Default
            };
        }
        
        /// <summary>
        /// 캐릭터 애니메이션 설정을 가져옵니다.
        /// </summary>
        private AnimationSystem.Data.CharacterAnimationSettings GetCharacterAnimationSettings(PlayerCharacterAnimationEntry entry, string animationType)
        {
            return animationType.ToLower() switch
            {
                "spawn" => entry.SpawnAnimation,
                "death" => entry.DeathAnimation,
                _ => CharacterAnimationSettings.Default
            };
        }
        
        /// <summary>
        /// 캐릭터 애니메이션 설정을 가져옵니다.
        /// </summary>
        private AnimationSystem.Data.CharacterAnimationSettings GetCharacterAnimationSettings(EnemyCharacterAnimationEntry entry, string animationType)
        {
            return animationType.ToLower() switch
            {
                "spawn" => entry.SpawnAnimation,
                "death" => entry.DeathAnimation,
                _ => CharacterAnimationSettings.Default
            };
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
            playerSkillCardCache.Clear();
            enemySkillCardCache.Clear();
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
        /// 캐시 상태를 가져옵니다.
        /// </summary>
        public Dictionary<string, object> GetCacheStatus()
        {
            return new Dictionary<string, object>
            {
                ["PlayerSkillCardCache"] = playerSkillCardCache.Count,
                ["EnemySkillCardCache"] = enemySkillCardCache.Count,
                ["PlayerCharacterCache"] = playerCharacterCache.Count,
                ["EnemyCharacterCache"] = enemyCharacterCache.Count,
                ["MaxCacheSize"] = maxCacheSize,
                ["CachingEnabled"] = enableCaching
            };
        }
        #endregion
    }
} 