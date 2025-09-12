using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;
using Game.AnimationSystem.Data;

using Game.SkillCardSystem.Core;
using Game.CharacterSystem.Data;
using Game.SkillCardSystem.Data;
using Game.SkillCardSystem.Interface;
using Game.CombatSystem.Data;
using Game.CombatSystem.Slot;

namespace Game.CoreSystem.Animation
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
                    instance = FindFirstObjectByType<AnimationDatabaseManager>();
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
                InitializeManager();
                // AnimationEventListener 자동 추가
                // if (GetComponent<AnimationEventListener>() == null)
                //     gameObject.AddComponent<AnimationEventListener>();
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
                playerSkillCardDatabase = Resources.Load<PlayerSkillCardAnimationDatabase>("Data/Animation/PlayerSkillcard/PlayerSkillCardAnimationDatabase");
            }
            
            if (enemySkillCardDatabase == null)
            {
                enemySkillCardDatabase = Resources.Load<EnemySkillCardAnimationDatabase>("Data/Animation/EnmySkillcard/EnemySkillCardAnimationDatabase");
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
        /// 플레이어 스킬카드 애니메이션을 재생합니다. (콜백 미지원)
        /// </summary>
        public void PlayPlayerSkillCardAnimation(string skillCardId, GameObject target, string animationType = "cast")
        {
            PlayPlayerSkillCardAnimation(skillCardId, target, animationType, null);
        }
        /// <summary>
        /// 플레이어 스킬카드 애니메이션을 재생합니다. (콜백 지원)
        /// </summary>
        public void PlayPlayerSkillCardAnimation(string skillCardId, GameObject target, string animationType, System.Action onComplete)
        {
            var entry = GetPlayerSkillCardAnimationEntry(skillCardId);
            if (entry != null)
            {
                var settings = GetSkillCardAnimationSettings(entry, animationType);
                if (!settings.IsEmpty())
                {
                    settings.PlayAnimation(target, animationType, onComplete);
                    OnSkillCardAnimationPlayed?.Invoke(skillCardId);
                }
                else
                {
                    Debug.LogWarning($"플레이어 스킬카드 '{skillCardId}'의 '{animationType}' 애니메이션이 설정되지 않았습니다.");
                    onComplete?.Invoke();
                }
            }
            else
            {
                Debug.LogWarning($"플레이어 스킬카드 '{skillCardId}'를 찾을 수 없습니다.");
                onComplete?.Invoke();
            }
        }
        
        /// <summary>
        /// 적용 스킬카드 애니메이션을 재생합니다.
        /// </summary>
        public void PlayEnemySkillCardAnimation(string skillCardId, GameObject target, string animationType = "cast", System.Action onComplete = null)
        {
            // 타겟 오브젝트 유효성 검사
            if (target == null)
            {
                Debug.LogError($"타겟 오브젝트가 null입니다: {skillCardId}");
                onComplete?.Invoke();
                return;
            }

            var entry = GetEnemySkillCardAnimationEntry(skillCardId);
            if (entry != null)
            {
                var settings = GetSkillCardAnimationSettings(entry, animationType);
                if (!settings.IsEmpty())
                {
                    try
                    {
                        settings.PlayAnimation(target, animationType, () => {
                            onComplete?.Invoke();
                        });
                        OnSkillCardAnimationPlayed?.Invoke(skillCardId);
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError($"적 스킬카드 애니메이션 실행 중 오류: {skillCardId} - {e.Message}");
                        onComplete?.Invoke();
                    }
                }
                else
                {
                    Debug.LogWarning($"적 스킬카드 '{skillCardId}'의 '{animationType}' 애니메이션 설정이 비어있습니다.");
                    onComplete?.Invoke();
                }
            }
            else
            {
                Debug.LogWarning($"적 스킬카드 '{skillCardId}'를 찾을 수 없습니다.");
                onComplete?.Invoke();
            }
        }
        
        /// <summary>
        /// 스킬카드 애니메이션 항목을 가져옵니다. (플레이어/적 통합)
        /// </summary>
        public PlayerSkillCardAnimationEntry GetPlayerSkillCardAnimationEntry(string skillCardId)
        {
            if (string.IsNullOrEmpty(skillCardId) || playerSkillCardDatabase == null)
                return null;
            if (enableCaching && playerSkillCardCache.ContainsKey(skillCardId))
                return playerSkillCardCache[skillCardId];
            var entry = playerSkillCardDatabase.SkillCardAnimations.Find(e => e.SkillCardDefinition != null && e.SkillCardDefinition.displayName == skillCardId);
            if (enableCaching && entry != null)
            {
                if (playerSkillCardCache.Count >= maxCacheSize)
                {
                    var firstKey = playerSkillCardCache.Keys.GetEnumerator();
                    if (firstKey.MoveNext()) playerSkillCardCache.Remove(firstKey.Current);
                }
                playerSkillCardCache[skillCardId] = entry;
            }
            return entry;
        }
        public EnemySkillCardAnimationEntry GetEnemySkillCardAnimationEntry(string skillCardId)
        {
            if (string.IsNullOrEmpty(skillCardId) || enemySkillCardDatabase == null)
                return null;
            if (enableCaching && enemySkillCardCache.ContainsKey(skillCardId))
                return enemySkillCardCache[skillCardId];
            var entry = enemySkillCardDatabase.SkillCardAnimations.Find(e => e.EnemySkillCard != null && e.EnemySkillCard.CardDefinition != null && e.EnemySkillCard.CardDefinition.displayName == skillCardId);
            if (enableCaching && entry != null)
            {
                if (enemySkillCardCache.Count >= maxCacheSize)
                {
                    var firstKey = enemySkillCardCache.Keys.GetEnumerator();
                    if (firstKey.MoveNext()) enemySkillCardCache.Remove(firstKey.Current);
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
        /// 스킬카드 애니메이션 설정을 가져옵니다.
        /// </summary>
        private Game.AnimationSystem.Data.SkillCardAnimationSettings GetSkillCardAnimationSettings(PlayerSkillCardAnimationEntry entry, string animationType)
        {
            if (entry == null || string.IsNullOrEmpty(animationType))
                return SkillCardAnimationSettings.Default;
            var settings = entry.GetSettingsByType(animationType.ToLower());
            return settings ?? SkillCardAnimationSettings.Default;
        }
        
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
        
        private Game.AnimationSystem.Data.SkillCardAnimationSettings GetSkillCardAnimationSettings(EnemySkillCardAnimationEntry entry, string animationType)
        {
            if (entry == null || string.IsNullOrEmpty(animationType))
                return SkillCardAnimationSettings.Default;
            var settings = entry.GetSettingsByType(animationType.ToLower());
            return settings ?? SkillCardAnimationSettings.Default;
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
        /// 데이터베이스 상태를 디버그로 출력합니다.
        /// </summary>
        public void DebugDatabaseStatus()
        {
            Debug.Log("=== 애니메이션 데이터베이스 상태 ===");
            
            if (playerSkillCardDatabase != null)
            {
                Debug.Log($"플레이어 스킬카드 데이터베이스:");
                foreach (var entry in playerSkillCardDatabase.SkillCardAnimations)
                {
                    if (entry.SkillCardDefinition != null)
                        Debug.Log($"  - {entry.SkillCardDefinition.displayName}");
                }
            }
            
            if (enemySkillCardDatabase != null)
            {
                Debug.Log($"적 스킬카드 데이터베이스:");
                foreach (var entry in enemySkillCardDatabase.SkillCardAnimations)
                {
                    if (entry.EnemySkillCard != null && entry.EnemySkillCard.CardDefinition != null)
                        Debug.Log($"  - {entry.EnemySkillCard.CardDefinition.displayName}");
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
                ["PlayerSkillCardCache"] = playerSkillCardCache.Count,
                ["EnemySkillCardCache"] = enemySkillCardCache.Count,
                ["PlayerCharacterCache"] = playerCharacterCache.Count,
                ["EnemyCharacterCache"] = enemyCharacterCache.Count
            };
        }
        
        /// <summary>
        /// 애니메이션 스크립트를 동적으로 추가하고 실행하는 유틸리티 메서드
        /// </summary>
        private void PlayAnimationWithScript(GameObject target, Game.AnimationSystem.Data.SkillCardAnimationSettings settings, string animationType, System.Action onComplete)
        {
            if (target == null || settings == null || string.IsNullOrEmpty(settings.AnimationScriptType))
            {
                Debug.LogWarning("[AnimationDatabaseManager] PlayAnimationWithScript: 잘못된 파라미터");
                onComplete?.Invoke();
                return;
            }

            // 타입 찾기 (SkillCardAnimationSettings의 매핑 로직 사용)
            var type = settings.GetScriptType();
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

        // ISkillCard 기반 오버로드 추가
        public void PlaySkillCardAnimation(ISkillCard card, GameObject target, string animationType = "cast", System.Action onComplete = null)
        {
            if (card == null)
            {
                Debug.LogError("[AnimationDatabaseManager] card가 null입니다.");
                onComplete?.Invoke();
                return;
            }
            var owner = card.GetOwner();
            if (owner == SlotOwner.ENEMY)
                PlayEnemySkillCardAnimation(card.CardDefinition.displayName, target, animationType, onComplete);
            else
                PlayPlayerSkillCardAnimation(card.CardDefinition.displayName, target, animationType, onComplete);
        }

        #region Drag Animation Methods
        /// <summary>
        /// 플레이어 스킬카드 드래그 시작 애니메이션을 재생합니다.
        /// </summary>
        public void PlayPlayerSkillCardDragStartAnimation(string skillCardId, GameObject target, System.Action onComplete = null)
        {
            Debug.Log($"플레이어 스킬카드 드래그 시작 애니메이션 시도 - 카드: {skillCardId}, 타겟: {target?.name}");
            var entry = GetPlayerSkillCardAnimationEntry(skillCardId);
            
            if (entry == null)
            {
                Debug.LogWarning($"[AnimationDatabaseManager] 플레이어 스킬카드 엔트리를 찾을 수 없습니다: {skillCardId}");
                onComplete?.Invoke();
                return;
            }

            var settings = GetSkillCardAnimationSettings(entry, "drag");
            if (settings == null || string.IsNullOrEmpty(settings.AnimationScriptType))
            {
                Debug.LogWarning($"[AnimationDatabaseManager] 플레이어 스킬카드 드래그 애니메이션 설정을 찾을 수 없습니다: {skillCardId}");
                onComplete?.Invoke();
                return;
            }

            PlayAnimationWithScript(target, settings, "start", onComplete);
        }

        /// <summary>
        /// 플레이어 스킬카드 드래그 종료 애니메이션을 재생합니다.
        /// </summary>
        public void PlayPlayerSkillCardDragEndAnimation(string skillCardId, GameObject target, System.Action onComplete = null)
        {
            Debug.Log($"플레이어 스킬카드 드래그 종료 애니메이션 시도 - 카드: {skillCardId}, 타겟: {target?.name}");
            var entry = GetPlayerSkillCardAnimationEntry(skillCardId);
            
            if (entry == null)
            {
                Debug.LogWarning($"[AnimationDatabaseManager] 플레이어 스킬카드 엔트리를 찾을 수 없습니다: {skillCardId}");
                onComplete?.Invoke();
                return;
            }

            var settings = GetSkillCardAnimationSettings(entry, "drag");
            if (settings == null || string.IsNullOrEmpty(settings.AnimationScriptType))
            {
                Debug.LogWarning($"[AnimationDatabaseManager] 플레이어 스킬카드 드래그 애니메이션 설정을 찾을 수 없습니다: {skillCardId}");
                onComplete?.Invoke();
                return;
            }

            PlayAnimationWithScript(target, settings, "end", onComplete);
        }

        /// <summary>
        /// 적 스킬카드 드래그 시작 애니메이션을 재생합니다.
        /// </summary>
        public void PlayEnemySkillCardDragStartAnimation(string skillCardId, GameObject target, System.Action onComplete = null)
        {
            Debug.Log($"적 스킬카드 드래그 시작 애니메이션 시도 - 카드: {skillCardId}, 타겟: {target?.name}");
            var entry = GetEnemySkillCardAnimationEntry(skillCardId);
            
            if (entry == null)
            {
                Debug.LogWarning($"[AnimationDatabaseManager] 적 스킬카드 엔트리를 찾을 수 없습니다: {skillCardId}");
                onComplete?.Invoke();
                return;
            }

            var settings = GetSkillCardAnimationSettings(entry, "drag");
            if (settings == null || string.IsNullOrEmpty(settings.AnimationScriptType))
            {
                Debug.LogWarning($"[AnimationDatabaseManager] 적 스킬카드 드래그 애니메이션 설정을 찾을 수 없습니다: {skillCardId}");
                onComplete?.Invoke();
                return;
            }

            PlayAnimationWithScript(target, settings, "start", onComplete);
        }

        /// <summary>
        /// 적 스킬카드 드래그 종료 애니메이션을 재생합니다.
        /// </summary>
        public void PlayEnemySkillCardDragEndAnimation(string skillCardId, GameObject target, System.Action onComplete = null)
        {
            Debug.Log($"적 스킬카드 드래그 종료 애니메이션 시도 - 카드: {skillCardId}, 타겟: {target?.name}");
            var entry = GetEnemySkillCardAnimationEntry(skillCardId);
            
            if (entry == null)
            {
                Debug.LogWarning($"[AnimationDatabaseManager] 적 스킬카드 엔트리를 찾을 수 없습니다: {skillCardId}");
                onComplete?.Invoke();
                return;
            }

            var settings = GetSkillCardAnimationSettings(entry, "drag");
            if (settings == null || string.IsNullOrEmpty(settings.AnimationScriptType))
            {
                Debug.LogWarning($"[AnimationDatabaseManager] 적 스킬카드 드래그 애니메이션 설정을 찾을 수 없습니다: {skillCardId}");
                onComplete?.Invoke();
                return;
            }

            PlayAnimationWithScript(target, settings, "end", onComplete);
        }

        /// <summary>
        /// ISkillCard 기반 드래그 애니메이션 (플레이어/적 자동 구분)
        /// </summary>
        public void PlaySkillCardDragStartAnimation(ISkillCard card, GameObject target, System.Action onComplete = null)
        {
            if (card == null)
            {
                Debug.LogError("[AnimationDatabaseManager] card가 null입니다.");
                onComplete?.Invoke();
                return;
            }
            var owner = card.GetOwner();
            if (owner == SlotOwner.ENEMY)
                PlayEnemySkillCardDragStartAnimation(card.CardDefinition.displayName, target, onComplete);
            else
                PlayPlayerSkillCardDragStartAnimation(card.CardDefinition.displayName, target, onComplete);
        }

        /// <summary>
        /// ISkillCard 기반 드래그 종료 애니메이션 (플레이어/적 자동 구분)
        /// </summary>
        public void PlaySkillCardDragEndAnimation(ISkillCard card, GameObject target, System.Action onComplete = null)
        {
            if (card == null)
            {
                Debug.LogError("[AnimationDatabaseManager] card가 null입니다.");
                onComplete?.Invoke();
                return;
            }
            var owner = card.GetOwner();
            if (owner == SlotOwner.ENEMY)
                PlayEnemySkillCardDragEndAnimation(card.CardDefinition.displayName, target, onComplete);
            else
                PlayPlayerSkillCardDragEndAnimation(card.CardDefinition.displayName, target, onComplete);
        }
        #endregion

        #region Drop Animation Methods
        /// <summary>
        /// 플레이어 스킬카드 드롭 애니메이션을 재생합니다.
        /// </summary>
        public void PlayPlayerSkillCardDropAnimation(string skillCardId, GameObject target, System.Action onComplete = null)
        {
            Debug.Log($"플레이어 스킬카드 드롭 애니메이션 시도 - 카드: {skillCardId}, 타겟: {target?.name}");
            var entry = GetPlayerSkillCardAnimationEntry(skillCardId);
            if (entry == null)
            {
                Debug.LogWarning($"[AnimationDatabaseManager] 플레이어 스킬카드 엔트리를 찾을 수 없습니다: {skillCardId}");
                onComplete?.Invoke();
                return;
            }
            var settings = GetSkillCardAnimationSettings(entry, "drop");
            if (settings == null || string.IsNullOrEmpty(settings.AnimationScriptType))
            {
                Debug.LogWarning($"[AnimationDatabaseManager] 플레이어 스킬카드 드롭 애니메이션 설정을 찾을 수 없습니다: {skillCardId}");
                onComplete?.Invoke();
                return;
            }
            PlayAnimationWithScript(target, settings, "drop", onComplete);
        }

        /// <summary>
        /// 적 스킬카드 드롭 애니메이션을 재생합니다.
        /// </summary>
        public void PlayEnemySkillCardDropAnimation(string skillCardId, GameObject target, System.Action onComplete = null)
        {
            Debug.Log($"적 스킬카드 드롭 애니메이션 시도 - 카드: {skillCardId}, 타겟: {target?.name}");
            var entry = GetEnemySkillCardAnimationEntry(skillCardId);
            if (entry == null)
            {
                Debug.LogWarning($"[AnimationDatabaseManager] 적 스킬카드 엔트리를 찾을 수 없습니다: {skillCardId}");
                onComplete?.Invoke();
                return;
            }
            var settings = GetSkillCardAnimationSettings(entry, "drop");
            if (settings == null || string.IsNullOrEmpty(settings.AnimationScriptType))
            {
                Debug.LogWarning($"[AnimationDatabaseManager] 적 스킬카드 드롭 애니메이션 설정을 찾을 수 없습니다: {skillCardId}");
                onComplete?.Invoke();
                return;
            }
            PlayAnimationWithScript(target, settings, "drop", onComplete);
        }

        /// <summary>
        /// ISkillCard 기반 드롭 애니메이션 (플레이어/적 자동 구분)
        /// </summary>
        public void PlaySkillCardDropAnimation(ISkillCard card, GameObject target, System.Action onComplete = null)
        {
            if (card == null)
            {
                Debug.LogError("[AnimationDatabaseManager] card가 null입니다.");
                onComplete?.Invoke();
                return;
            }
            var owner = card.GetOwner();
            if (owner == SlotOwner.ENEMY)
                PlayEnemySkillCardDropAnimation(card.CardDefinition.displayName, target, onComplete);
            else
                PlayPlayerSkillCardDropAnimation(card.CardDefinition.displayName, target, onComplete);
        }
        #endregion
    }
} 