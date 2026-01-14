using Game.CharacterSystem.Interface;
using Game.CharacterSystem.Manager;
using Game.CoreSystem.Interface;
using Game.CoreSystem.Utility;
using Game.ItemSystem.Constants;
using Game.ItemSystem.Data;
using Game.ItemSystem.Interface;
using Game.ItemSystem.Runtime;
using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Game.ItemSystem.Service
{
    /// <summary>
    /// 아이템 시스템의 핵심 서비스 구현체입니다.
    /// 액티브/패시브 아이템 관리, 사용, 상태 관리를 담당합니다.
    /// </summary>
    public class ItemService : MonoBehaviour, IItemService
    {
        #region 상수

        private const int ACTIVE_SLOT_COUNT = ItemConstants.ACTIVE_SLOT_COUNT;

        #endregion

        #region 필드


        // 액티브 아이템 슬롯 관리
        private ActiveItemSlotData[] activeSlots = new ActiveItemSlotData[ACTIVE_SLOT_COUNT];

        // 패시브 아이템 관리 (강화 단계 시스템)
        private Dictionary<string, int> skillStarRanks = new Dictionary<string, int>();
        private Dictionary<string, PassiveItemDefinition> passiveItemDefinitions = new Dictionary<string, PassiveItemDefinition>();

        // 의존성 주입
        [Inject(Optional = true)] private PlayerManager playerManager;
        [Inject(Optional = true)] private IAudioManager audioManager;
        [Inject(Optional = true)] private IVFXManager vfxManager;
        [Inject(Optional = true)] private Game.CombatSystem.Manager.TurnManager turnManager;
        [Inject(Optional = true)] private Game.CombatSystem.State.CombatStateMachine combatStateMachine;
        [Inject(Optional = true)] private Game.CharacterSystem.Manager.EnemyManager enemyManager;
        [Inject(Optional = true)] private InventoryPanelController inventoryController;

        #endregion

        #region 이벤트

        /// <summary>
        /// 액티브 아이템 사용 시 발생하는 이벤트 (아이템 정의, 슬롯 인덱스)
        /// </summary>
        public event Action<ActiveItemDefinition, int> OnActiveItemUsed;
        
        /// <summary>
        /// 스킬 강화 단계 업그레이드 시 발생하는 이벤트 (스킬 ID, 새로운 단계)
        /// </summary>
        public event Action<string, int> OnEnhancementUpgraded;
        
        /// <summary>
        /// 스킬 성급 업그레이드 시 발생하는 이벤트 (사용 중지됨, OnEnhancementUpgraded 사용 권장)
        /// </summary>
        [System.Obsolete("Use OnEnhancementUpgraded instead")]
        public event Action<string, int> OnSkillStarUpgraded;
        
        /// <summary>
        /// 액티브 아이템 추가 시 발생하는 이벤트 (아이템 정의, 슬롯 인덱스)
        /// </summary>
        public event Action<ActiveItemDefinition, int> OnActiveItemAdded;
        
        /// <summary>
        /// 액티브 아이템 제거 시 발생하는 이벤트 (아이템 정의, 슬롯 인덱스)
        /// </summary>
        public event Action<ActiveItemDefinition, int> OnActiveItemRemoved;
        
        /// <summary>
        /// 패시브 아이템 추가 시 발생하는 이벤트 (패시브 아이템 정의)
        /// </summary>
        public event Action<PassiveItemDefinition> OnPassiveItemAdded;

        #endregion

        #region Unity 생명주기

        private void Awake()
        {
            InitializeActiveSlots();

            // PlayerManager 주입 상태 확인 및 폴백
            EnsureDependenciesInjected();
        }

        /// <summary>
        /// 의존성이 주입되지 않았으면 주입을 시도합니다.
        /// </summary>
        private void EnsureDependenciesInjected()
        {
            if (playerManager == null)
            {
                EnsurePlayerManagerInjected();
            }
            if (turnManager == null)
            {
                EnsureTurnManagerInjected();
            }
            if (combatStateMachine == null)
            {
                EnsureCombatStateMachineInjected();
            }
        }

        /// <summary>
        /// PlayerManager가 null이면 주입을 시도합니다.
        /// </summary>
        private void EnsurePlayerManagerInjected()
        {
            if (playerManager != null) return;

            try
            {
                var projectContext = Zenject.ProjectContext.Instance;
                if (projectContext != null && projectContext.Container != null)
                {
                    // 개별 의존성만 주입 (전체 Inject로 인한 의존성 체인 문제 방지)
                    var resolvedManager = projectContext.Container.TryResolve<PlayerManager>();
                    if (resolvedManager != null)
                    {
                        playerManager = resolvedManager;
                        GameLogger.LogInfo("[ItemService] PlayerManager 주입 완료 (ProjectContext)", GameLogger.LogCategory.Core);
                        return;
                    }
                }

                var foundManager = UnityEngine.Object.FindFirstObjectByType<PlayerManager>(UnityEngine.FindObjectsInactive.Include);
                if (foundManager != null)
                {
                    playerManager = foundManager;
                    GameLogger.LogInfo("[ItemService] PlayerManager 직접 찾기 완료", GameLogger.LogCategory.Core);
                }
            }
            catch (System.Exception ex)
            {
                GameLogger.LogWarning($"[ItemService] PlayerManager 주입 시도 중 오류: {ex.Message}", GameLogger.LogCategory.Core);
            }
        }

        /// <summary>
        /// TurnManager가 null이면 주입을 시도합니다.
        /// </summary>
        private void EnsureTurnManagerInjected()
        {
            if (turnManager != null) return;

            try
            {
                var projectContext = Zenject.ProjectContext.Instance;
                if (projectContext != null && projectContext.Container != null)
                {
                    // SceneContext에서 먼저 시도
                    Zenject.DiContainer sceneContainer = null;
                    try
                    {
                        var sceneContextRegistry = projectContext.Container.Resolve<Zenject.SceneContextRegistry>();
                        sceneContainer = sceneContextRegistry.TryGetContainerForScene(gameObject.scene);
                    }
                    catch (System.Exception ex)
                    {
                        GameLogger.LogWarning($"[ItemService] SceneContextRegistry를 찾을 수 없거나 씬 컨테이너 획득 중 오류: {ex.Message}", GameLogger.LogCategory.Core);
                    }

                    // SceneContext에서 먼저 시도
                    if (sceneContainer != null)
                    {
                        var resolvedManager = sceneContainer.TryResolve<Game.CombatSystem.Manager.TurnManager>();
                        if (resolvedManager != null)
                        {
                            turnManager = resolvedManager;
                            GameLogger.LogInfo("[ItemService] TurnManager 주입 완료 (SceneContext)", GameLogger.LogCategory.Core);
                            return;
                        }
                    }

                    // ProjectContext에서 시도
                    var projectResolvedManager = projectContext.Container.TryResolve<Game.CombatSystem.Manager.TurnManager>();
                    if (projectResolvedManager != null)
                    {
                        turnManager = projectResolvedManager;
                        GameLogger.LogInfo("[ItemService] TurnManager 주입 완료 (ProjectContext)", GameLogger.LogCategory.Core);
                        return;
                    }
                }

                var foundManager = UnityEngine.Object.FindFirstObjectByType<Game.CombatSystem.Manager.TurnManager>(UnityEngine.FindObjectsInactive.Include);
                if (foundManager != null)
                {
                    turnManager = foundManager;
                    GameLogger.LogInfo("[ItemService] TurnManager 직접 찾기 완료", GameLogger.LogCategory.Core);
                }
            }
            catch (System.Exception ex)
            {
                GameLogger.LogWarning($"[ItemService] TurnManager 주입 시도 중 오류: {ex.Message}", GameLogger.LogCategory.Core);
            }
        }

        /// <summary>
        /// CombatStateMachine이 null이면 주입을 시도합니다.
        /// </summary>
        private void EnsureCombatStateMachineInjected()
        {
            if (combatStateMachine != null) return;

            try
            {
                var projectContext = Zenject.ProjectContext.Instance;
                if (projectContext != null && projectContext.Container != null)
                {
                    // SceneContext에서 먼저 시도
                    Zenject.DiContainer sceneContainer = null;
                    try
                    {
                        var sceneContextRegistry = projectContext.Container.Resolve<Zenject.SceneContextRegistry>();
                        sceneContainer = sceneContextRegistry.TryGetContainerForScene(gameObject.scene);
                    }
                    catch (System.Exception ex)
                    {
                        GameLogger.LogWarning($"[ItemService] SceneContextRegistry를 찾을 수 없거나 씬 컨테이너 획득 중 오류: {ex.Message}", GameLogger.LogCategory.Core);
                    }

                    // SceneContext에서 먼저 시도
                    if (sceneContainer != null)
                    {
                        var resolvedManager = sceneContainer.TryResolve<Game.CombatSystem.State.CombatStateMachine>();
                        if (resolvedManager != null)
                        {
                            combatStateMachine = resolvedManager;
                            GameLogger.LogInfo("[ItemService] CombatStateMachine 주입 완료 (SceneContext)", GameLogger.LogCategory.Core);
                            return;
                        }
                    }

                    // ProjectContext에서 시도
                    var projectResolvedManager = projectContext.Container.TryResolve<Game.CombatSystem.State.CombatStateMachine>();
                    if (projectResolvedManager != null)
                    {
                        combatStateMachine = projectResolvedManager;
                        GameLogger.LogInfo("[ItemService] CombatStateMachine 주입 완료 (ProjectContext)", GameLogger.LogCategory.Core);
                        return;
                    }
                }

                var foundManager = UnityEngine.Object.FindFirstObjectByType<Game.CombatSystem.State.CombatStateMachine>(UnityEngine.FindObjectsInactive.Include);
                if (foundManager != null)
                {
                    combatStateMachine = foundManager;
                    GameLogger.LogInfo("[ItemService] CombatStateMachine 직접 찾기 완료", GameLogger.LogCategory.Core);
                }
            }
            catch (System.Exception ex)
            {
                GameLogger.LogWarning($"[ItemService] CombatStateMachine 주입 시도 중 오류: {ex.Message}", GameLogger.LogCategory.Core);
            }
        }

        private void Start()
        {
            // playerManager는 DI로 주입받음
            if (playerManager == null)
            {
                GameLogger.LogWarning("[ItemService] PlayerManager가 주입되지 않았습니다.", GameLogger.LogCategory.Core);
            }
        }

        #endregion

        #region 플레이어 캐릭터 관리

        /// <summary>
        /// PlayerManager를 통해 플레이어 캐릭터를 가져옵니다.
        /// PlayerManager가 없으면 씬에서 직접 찾습니다.
        /// </summary>
        /// <returns>플레이어 캐릭터 또는 null</returns>
        private ICharacter GetPlayerCharacter()
        {
            // PlayerManager가 null이면 주입 시도
            if (playerManager == null)
            {
                EnsurePlayerManagerInjected();
            }

            // 1. PlayerManager를 통해 가져오기 시도
            if (playerManager != null)
            {
                var playerCharacter = playerManager.GetCharacter();
                if (playerCharacter != null)
                {
                    return playerCharacter;
                }
                else
                {
                    GameLogger.LogWarning("[ItemService] PlayerManager는 있지만 캐릭터가 null입니다", GameLogger.LogCategory.Core);
                }
            }
            else
            {
                GameLogger.LogWarning("[ItemService] PlayerManager가 주입되지 않았습니다", GameLogger.LogCategory.Core);
            }

            // 2. 씬에서 직접 찾기 (ICharacter 구현체들)
            var allCharacters = FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            foreach (var obj in allCharacters)
            {
                if (obj is ICharacter character)
                {
                    // PlayerCharacter 우선 선택
                    if (obj is Game.CharacterSystem.Core.PlayerCharacter)
                    {
                        return character;
                    }
                }
            }

            // 3. PlayerCharacter가 없으면 다른 ICharacter라도 반환
            foreach (var obj in allCharacters)
            {
                if (obj is ICharacter character)
                {
                    return character;
                }
            }

            GameLogger.LogWarning("[ItemService] 플레이어 캐릭터를 찾을 수 없습니다", GameLogger.LogCategory.Core);
            return null;
        }

        #endregion

        #region 초기화

        /// <summary>
        /// 액티브 슬롯들을 초기화합니다.
        /// </summary>
        private void InitializeActiveSlots()
        {
            for (int i = 0; i < ACTIVE_SLOT_COUNT; i++)
            {
                activeSlots[i] = new ActiveItemSlotData();
            }
        }

        #endregion

        #region IItemService 구현

        /// <summary>
        /// 액티브 아이템을 사용합니다.
        /// </summary>
        /// <param name="slotIndex">슬롯 인덱스</param>
        /// <returns>사용 성공 여부</returns>
        public bool UseActiveItem(int slotIndex)
        {
            if (!IsValidSlotIndex(slotIndex))
            {
                GameLogger.LogError($"잘못된 슬롯 인덱스: {slotIndex}", GameLogger.LogCategory.Core);
                return false;
            }

            var slot = activeSlots[slotIndex];
            if (slot.isEmpty || slot.item == null)
            {
                GameLogger.LogWarning($"슬롯 {slotIndex}이 비어있습니다", GameLogger.LogCategory.Core);
                return false;
            }

            // PlayerManager를 통해 플레이어 캐릭터 가져오기
            var playerCharacter = GetPlayerCharacter();
            if (playerCharacter == null)
            {
                GameLogger.LogWarning("플레이어 캐릭터가 아직 초기화되지 않았습니다. 아이템 사용을 건너뜁니다", GameLogger.LogCategory.Core);

                // 임시 해결책: 아이템 효과만 실행 (캐릭터 없이)
                // 소모품인 경우 슬롯에서 제거
                if (slot.item.Type == ItemType.Active)
                {
                    RemoveActiveItem(slotIndex);
                }

                OnActiveItemUsed?.Invoke(slot.item, slotIndex);
                return true;
            }

            // 모든 아이템은 플레이어 턴에만 사용 가능 (부활 아이템 제외)
            bool isReviveItem = slot.item.DisplayName.Contains("부활") || 
                               slot.item.DisplayName.Contains("Revive") ||
                               slot.item.DisplayName.Contains("징표");
            
            if (!IsPlayerTurn() && !isReviveItem)
            {
                GameLogger.LogWarning($"{slot.item.DisplayName}은 플레이어 턴에만 사용할 수 있습니다", GameLogger.LogCategory.Core);
                return false;
            }

            // 부활 아이템은 죽었을 때만 사용 가능 (특수 조건)
            if (isReviveItem && !playerCharacter.IsDead())
            {
                GameLogger.LogWarning($"{slot.item.DisplayName}은 죽었을 때만 사용할 수 있습니다", GameLogger.LogCategory.Core);
                return false;
            }

            // 아이템 런타임 인스턴스 생성 및 사용
            var activeItem = new ActiveItem(slot.item, audioManager, vfxManager);

            // 데이터 기반 타겟 결정
            var targetCharacter = DetermineItemTarget(slot.item, playerCharacter);

            bool success = activeItem.UseItem(playerCharacter, targetCharacter);

            if (success)
            {
                // 이벤트/로그 전에 아이템 참조를 보관 (제거 시 null 방지)
                var usedItem = slot.item;

                // 소모품인 경우 슬롯에서 제거
                if (usedItem != null && usedItem.Type == ItemType.Active)
                {
                    RemoveActiveItem(slotIndex);
                }
                
                OnActiveItemUsed?.Invoke(usedItem, slotIndex);
            }
            else
            {
                GameLogger.LogError($"아이템 사용 실패: {slot.item.DisplayName}", GameLogger.LogCategory.Core);
            }

            return success;
        }

        /// <summary>
        /// 액티브 아이템을 슬롯에 추가합니다.
        /// </summary>
        /// <param name="itemDefinition">아이템 정의</param>
        /// <returns>추가 성공 여부</returns>
        public bool AddActiveItem(ActiveItemDefinition itemDefinition)
        {
            if (itemDefinition == null)
            {
                GameLogger.LogError("아이템 정의가 null입니다", GameLogger.LogCategory.Core);
                return false;
            }

            // 빈 슬롯 찾기
            int emptySlotIndex = FindEmptySlot();
            if (emptySlotIndex == -1)
            {
                GameLogger.LogWarning("모든 슬롯이 가득 찼습니다", GameLogger.LogCategory.Core);
                return false;
            }

            // 슬롯에 아이템 추가
            activeSlots[emptySlotIndex].item = itemDefinition;
            activeSlots[emptySlotIndex].isEmpty = false;
            
            OnActiveItemAdded?.Invoke(itemDefinition, emptySlotIndex);

            return true;
        }

        /// <summary>
        /// 액티브 아이템을 슬롯에서 제거합니다.
        /// </summary>
        /// <param name="slotIndex">슬롯 인덱스</param>
        /// <returns>제거 성공 여부</returns>
        public bool RemoveActiveItem(int slotIndex)
        {
            if (!IsValidSlotIndex(slotIndex))
            {
                GameLogger.LogError($"잘못된 슬롯 인덱스: {slotIndex}", GameLogger.LogCategory.Core);
                return false;
            }

            var slot = activeSlots[slotIndex];
            if (slot.isEmpty)
            {
                GameLogger.LogWarning($"슬롯 {slotIndex}이 이미 비어있습니다", GameLogger.LogCategory.Core);
                return false;
            }

            // 제거 전 아이템 정보 저장
            var removedItem = slot.item;

            slot.item = null;
            slot.isEmpty = true;
            
            OnActiveItemRemoved?.Invoke(removedItem, slotIndex);

            return true;
        }

        /// <summary>
        /// 액티브 아이템 슬롯 정보를 가져옵니다.
        /// </summary>
        /// <returns>슬롯 정보 배열</returns>
        public ActiveItemSlotData[] GetActiveSlots()
        {
            return activeSlots;
        }

        /// <summary>
        /// 액티브 인벤토리가 가득 찼는지 확인합니다.
        /// </summary>
        /// <returns>가득 참 여부</returns>
        public bool IsActiveInventoryFull()
        {
            return FindEmptySlot() == -1;
        }

        /// <summary>
        /// 패시브 아이템을 추가합니다 (강화 단계 시스템).
        /// </summary>
        /// <param name="passiveItemDefinition">패시브 아이템 정의</param>
        public void AddPassiveItem(PassiveItemDefinition passiveItemDefinition)
        {
            if (passiveItemDefinition == null)
            {
                GameLogger.LogError("패시브 아이템 정의가 null입니다", GameLogger.LogCategory.Core);
                return;
            }

            string skillId = null;
            if (passiveItemDefinition.IsPlayerHealthBonus)
            {
                // 공용 체력 보너스이지만, 아이템별로 독립된 성급을 관리하기 위해 아이템 ID를 키에 포함합니다
                // 예: __PLAYER_HP__:116_망토, __PLAYER_HP__:117_투구, __PLAYER_HP__:118_갑옷
                var itemKey = !string.IsNullOrEmpty(passiveItemDefinition.ItemId) ? passiveItemDefinition.ItemId : Guid.NewGuid().ToString();
                skillId = $"__PLAYER_HP__:{itemKey}";
            }
            else
            {
                // 대상 스킬은 참조 기반으로만 사용 (displayName → cardId 폴백)
                var target = passiveItemDefinition.TargetSkill;
                if (target != null)
                {
                    skillId = !string.IsNullOrEmpty(target.displayName) ? target.displayName : target.cardId;
                }

                if (string.IsNullOrEmpty(skillId))
                {
                    GameLogger.LogError("패시브 아이템의 대상 스킬 참조가 비어있습니다", GameLogger.LogCategory.Core);
                    return;
                }
            }

            // 강화 단계 증가 (최대 상수 제한)
            if (!skillStarRanks.ContainsKey(skillId))
            {
                skillStarRanks[skillId] = 0;
            }

                if (skillStarRanks[skillId] < ItemConstants.MAX_ENHANCEMENT_LEVEL)
            {
                skillStarRanks[skillId]++;
                OnEnhancementUpgraded?.Invoke(skillId, skillStarRanks[skillId]);
                OnSkillStarUpgraded?.Invoke(skillId, skillStarRanks[skillId]);

                // 정의 보관 (보너스 계산/HP 증가용)
                if (passiveItemDefinition != null && !string.IsNullOrEmpty(passiveItemDefinition.ItemId))
                {
                    passiveItemDefinitions[passiveItemDefinition.ItemId] = passiveItemDefinition;
                    OnPassiveItemAdded?.Invoke(passiveItemDefinition);
                }

                // 플레이어 체력 보너스 처리
                if (passiveItemDefinition.IsPlayerHealthBonus)
                {
                    try
                    {
                        var player = GetPlayerCharacter();
                        if (player != null)
                        {
                            var incs = passiveItemDefinition.EnhancementIncrements;
                            int level = skillStarRanks[skillId];
                            int add = 0;
                            if (level >= 1 && level <= incs.Length)
                                add = incs[level - 1];

                            if (add > 0)
                            {
                                int prevHP = player.GetCurrentHP();
                                int prevMax = player.GetMaxHP();
                                int newMax = prevMax + add;
                                
                                GameLogger.LogInfo($"[ItemService] 패시브 아이템으로 최대 체력 증가: {prevMax} → {newMax} (+{add})", GameLogger.LogCategory.Core);
                                
                                // ICharacter는 SetMaxHP가 없으므로 CharacterBase로 캐스팅하여 적용
                                if (player is Game.CharacterSystem.Core.CharacterBase cb)
                                {
                                    // SetMaxHP는 자동으로 현재 체력을 비례하여 증가시킴
                                    // 추가로 증가한 만큼 회복 (이전 HP + 증가량, 단 최대치 초과 금지)
                                    cb.SetMaxHP(newMax);
                                    int healed = Mathf.Min(prevHP + add, newMax);
                                    cb.SetCurrentHP(healed);
                                    
                                    GameLogger.LogInfo($"[ItemService] 체력 회복: {prevHP} → {healed}/{newMax}", GameLogger.LogCategory.Core);
                                }
                                else
                                {
                                    GameLogger.LogWarning("[ItemService] SetMaxHP를 적용할 수 없습니다 (CharacterBase 아님)", GameLogger.LogCategory.Core);
                                }
                            }
                            else
                            {
                                GameLogger.LogWarning($"[ItemService] 패시브 아이템 체력 증가량이 0입니다 (level: {level}, increments: {incs.Length})", GameLogger.LogCategory.Core);
                            }
                        }
                    }
                    catch (System.Exception ex)
                    {
                        GameLogger.LogError($"[ItemService] 체력 보너스 적용 중 오류: {ex.Message}", GameLogger.LogCategory.Error);
                    }
                }
            }
            else
            {
            }
        }

        /// <summary>
        /// 스킬의 데미지 보너스를 반환합니다.
        /// 강화 단계 누적 보너스(1~현재단계까지의 합) + 스킬별 고정 보너스를 합산합니다.
        /// </summary>
        /// <param name="skillId">스킬 ID</param>
        /// <returns>데미지 보너스</returns>
        public int GetSkillDamageBonus(string skillId)
        {
            if (string.IsNullOrEmpty(skillId))
            {
                return 0;
            }

            int totalBonus = 0;

            // 강화 단계 보너스 (단계별 가중치를 누적)
            if (skillStarRanks.ContainsKey(skillId))
            {
                int level = skillStarRanks[skillId];

                // 해당 스킬을 타겟으로 하는 패시브 정의 검색 (displayName 또는 cardId 모두 허용)
                PassiveItemDefinition matched = null;
                foreach (var def in passiveItemDefinitions.Values)
                {
                    if (def == null) continue;
                    bool match = false;
                    var target = def.TargetSkill;
                    if (target != null)
                    {
                        if (!string.IsNullOrEmpty(target.displayName) && target.displayName == skillId) match = true;
                        else if (!string.IsNullOrEmpty(target.cardId) && target.cardId == skillId) match = true;
                    }

                    if (match)
                    {
                        matched = def;
                        break;
                    }
                }

                if (matched != null)
                {
                    var increments = matched.EnhancementIncrements;
                    int sum = 0;
                    int maxSumCount = Mathf.Min(level, increments.Length);
                    for (int i = 0; i < maxSumCount; i++)
                    {
                        sum += increments[i];
                    }
                    totalBonus += sum;
                }
                else
                {
                    // 정의를 찾지 못한 경우 단계당 +1로 폴백
                    totalBonus += level;
                }
            }

            // 고정 보너스는 제거되었음: 모든 수치는 강화 단계 누적 합계로 계산

            return totalBonus;
        }

        /// <summary>
        /// 스킬의 강화 단계를 반환합니다.
        /// </summary>
        /// <param name="skillId">스킬 ID</param>
        /// <returns>강화 단계 (0-3)</returns>
        public int GetSkillEnhancementLevel(string skillId)
        {
            if (string.IsNullOrEmpty(skillId) || !skillStarRanks.ContainsKey(skillId))
            {
                return 0;
            }

            return skillStarRanks[skillId];
        }

        /// <summary>
        /// 스킬의 성급을 반환합니다 (사용 중지됨, GetSkillEnhancementLevel 사용 권장)
        /// </summary>
        /// <param name="skillId">스킬 ID</param>
        /// <returns>성급 (0-3)</returns>
        [System.Obsolete("Use GetSkillEnhancementLevel instead")]
        public int GetSkillStarRank(string skillId) => GetSkillEnhancementLevel(skillId);

        /// <summary>
        /// 모든 스킬의 성급 정보를 가져옵니다.
        /// </summary>
        /// <returns>스킬 ID → 성급 매핑</returns>
        public Dictionary<string, int> GetAllSkillStarRanks()
        {
            return new Dictionary<string, int>(skillStarRanks);
        }

        #endregion

        #region 아이템 사용 조건 확인

        /// <summary>
        /// 현재 플레이어 턴인지 확인합니다.
        /// 턴 상태와 전투 상태를 모두 확인하여 완전한 플레이어 턴에서만 사용 가능하도록 합니다.
        /// </summary>
        /// <returns>완전한 플레이어 턴이면 true, 아니면 false</returns>
        private bool IsPlayerTurn()
        {
            // 1단계: TurnManager 턴 상태 확인
            if (turnManager == null)
            {
                EnsureTurnManagerInjected();
                if (turnManager == null)
                {
                    GameLogger.LogWarning("TurnManager를 찾을 수 없습니다. 아이템 사용을 차단합니다", GameLogger.LogCategory.Core);
                    return false; // TurnManager가 없으면 안전하게 차단
                }
            }

            bool isTurnPlayerTurn = turnManager.IsPlayerTurn();

            // 2단계: CombatStateMachine 전투 상태 확인
            if (combatStateMachine == null)
            {
                EnsureCombatStateMachineInjected();
                if (combatStateMachine == null)
                {
                    GameLogger.LogWarning("CombatStateMachine을 찾을 수 없습니다. 아이템 사용을 차단합니다", GameLogger.LogCategory.Core);
                    return false;
                }
            }

            var currentState = combatStateMachine.GetCurrentState();
            if (currentState == null)
            {
                GameLogger.LogWarning("현재 전투 상태가 없습니다. 아이템 사용을 차단합니다", GameLogger.LogCategory.Core);
                return false;
            }

            // 3단계: 완전한 플레이어 턴 상태인지 확인
            bool isCompletePlayerTurn = isTurnPlayerTurn && 
                                       currentState is Game.CombatSystem.State.PlayerTurnState &&
                                       currentState.AllowPlayerCardDrag;

            if (!isCompletePlayerTurn)
            {
            }

            return isCompletePlayerTurn;
        }

        #endregion

        #region 아이템 대상 결정

        /// <summary>
        /// 아이템의 대상 캐릭터를 데이터 기반으로 결정합니다.
        /// </summary>
        /// <param name="item">아이템 정의</param>
        /// <param name="playerCharacter">플레이어 캐릭터</param>
        /// <returns>대상 캐릭터</returns>
        private ICharacter DetermineItemTarget(ActiveItemDefinition item, ICharacter playerCharacter)
        {
            switch (item.targetType)
            {
                case ItemTargetType.Self:
                    // 플레이어 자신에게 사용
                    return playerCharacter;

                case ItemTargetType.Enemy:
                    // 적에게 사용
                    var enemyCharacter = enemyManager?.GetCurrentEnemy();

                    if (enemyCharacter != null)
                    {
                        return enemyCharacter;
                    }
                    else
                    {
                        GameLogger.LogWarning("[ItemService.DetermineItemTarget] 적 캐릭터를 찾을 수 없어 플레이어를 대상으로 설정합니다", GameLogger.LogCategory.Core);
                        return playerCharacter;
                    }

                case ItemTargetType.Both:
                    // 양쪽 모두 (특수 처리 필요 시 확장 가능)
                    return playerCharacter;

                default:
                    GameLogger.LogWarning($"[ItemService.DetermineItemTarget] 알 수 없는 타겟 타입: {item.targetType}, 기본값(Self) 사용", GameLogger.LogCategory.Core);
                    return playerCharacter;
            }
        }

        /// <summary>
        /// 현재 보유한 모든 패시브 아이템을 가져옵니다.
        /// </summary>
        /// <returns>패시브 아이템 정의 리스트</returns>
        public List<PassiveItemDefinition> GetPassiveItems()
        {
            var items = new List<PassiveItemDefinition>();
            foreach (var item in passiveItemDefinitions.Values)
            {
                if (item != null)
                {
                    items.Add(item);
                }
            }
            return items;
        }

        /// <summary>
        /// 새 게임을 위한 인벤토리 초기화
        /// </summary>
        public void ResetInventoryForNewGame()
        {
            // 액티브 슬롯 초기화
            for (int i = 0; i < ACTIVE_SLOT_COUNT; i++)
            {
                activeSlots[i] = new ActiveItemSlotData();
            }
            
            // 패시브 아이템 성급 초기화
            skillStarRanks.Clear();
            passiveItemDefinitions.Clear();
            
            // 인벤토리 UI 자식 오브젝트들 제거
            ClearInventoryUI();
        }

        /// <summary>
        /// 인벤토리 UI의 자식 오브젝트들을 제거합니다.
        /// </summary>
        private void ClearInventoryUI()
        {
            try
            {
                // InventoryPanelController는 DI로 주입받음
                if (inventoryController != null)
                {
                    inventoryController.ClearAllItemPrefabs();
                    GameLogger.LogInfo("[ItemService] 인벤토리 UI 정리 완료", GameLogger.LogCategory.Core);
                }
                else
                {
                    // Fallback: FindFirstObjectByType으로 찾기
                    var foundController = UnityEngine.Object.FindFirstObjectByType<Game.ItemSystem.Runtime.InventoryPanelController>(UnityEngine.FindObjectsInactive.Include);
                    if (foundController != null)
                    {
                        foundController.ClearAllItemPrefabs();
                        GameLogger.LogInfo("[ItemService] InventoryPanelController를 FindFirstObjectByType으로 찾아서 UI 정리 완료", GameLogger.LogCategory.Core);
                    }
                    else
                    {
                        GameLogger.LogWarning("[ItemService] InventoryPanelController를 찾을 수 없습니다", GameLogger.LogCategory.Core);
                    }
                }
            }
            catch (System.Exception ex)
            {
                GameLogger.LogError($"[ItemService] 인벤토리 UI 정리 중 오류: {ex.Message}", GameLogger.LogCategory.Error);
            }
        }

        #endregion

        #region 유틸리티

        /// <summary>
        /// 슬롯 인덱스가 유효한지 확인합니다.
        /// </summary>
        /// <param name="slotIndex">슬롯 인덱스</param>
        /// <returns>유효성 여부</returns>
        private bool IsValidSlotIndex(int slotIndex)
        {
            return slotIndex >= 0 && slotIndex < ACTIVE_SLOT_COUNT;
        }

        /// <summary>
        /// 빈 슬롯을 찾습니다.
        /// </summary>
        /// <returns>빈 슬롯 인덱스 (-1이면 없음)</returns>
        private int FindEmptySlot()
        {
            for (int i = 0; i < ACTIVE_SLOT_COUNT; i++)
            {
                if (activeSlots[i].isEmpty)
                {
                    return i;
                }
            }
            return -1;
        }

        #endregion
    }
}