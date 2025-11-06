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
        [Inject] private IAudioManager audioManager;
        [Inject(Optional = true)] private IVFXManager vfxManager;

        #endregion

        #region 이벤트

        public event Action<ActiveItemDefinition, int> OnActiveItemUsed;
        public event Action<string, int> OnEnhancementUpgraded;
        [System.Obsolete("Use OnEnhancementUpgraded instead")]
        public event Action<string, int> OnSkillStarUpgraded;
        public event Action<ActiveItemDefinition, int> OnActiveItemAdded;
        public event Action<ActiveItemDefinition, int> OnActiveItemRemoved;
        public event Action<PassiveItemDefinition> OnPassiveItemAdded;

        #endregion

        #region Unity 생명주기

        private void Awake()
        {
            InitializeActiveSlots();

            // PlayerManager 주입 상태 확인
            if (playerManager == null)
            {
                GameLogger.LogWarning("[ItemService] PlayerManager가 주입되지 않았습니다. 나중에 다시 시도합니다.", GameLogger.LogCategory.Core);
            }
        }

        private void Start()
        {
            // Start에서 PlayerManager 재확인
            if (playerManager == null)
            {
                playerManager = FindFirstObjectByType<PlayerManager>();
                if (playerManager != null)
                {
                    GameLogger.LogInfo("[ItemService] Start에서 PlayerManager를 찾았습니다", GameLogger.LogCategory.Core);
                }
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
            // 1. PlayerManager를 통해 가져오기 시도
            if (playerManager != null)
            {
                var playerCharacter = playerManager.GetCharacter();
                if (playerCharacter != null)
                {
                    GameLogger.LogInfo($"[ItemService] PlayerManager를 통해 캐릭터 발견: {playerCharacter.GetType().Name}", GameLogger.LogCategory.Core);
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
                        GameLogger.LogInfo($"[ItemService] 씬에서 PlayerCharacter 발견: {obj.name}", GameLogger.LogCategory.Core);
                        return character;
                    }
                }
            }

            // 3. PlayerCharacter가 없으면 다른 ICharacter라도 반환
            foreach (var obj in allCharacters)
            {
                if (obj is ICharacter character)
                {
                    GameLogger.LogInfo($"[ItemService] 씬에서 {obj.GetType().Name} 발견: {obj.name}", GameLogger.LogCategory.Core);
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
                GameLogger.LogInfo($"아이템 효과 시뮬레이션: {slot.item.DisplayName}", GameLogger.LogCategory.Core);

                // 소모품인 경우 슬롯에서 제거
                if (slot.item.Type == ItemType.Active)
                {
                    RemoveActiveItem(slotIndex);
                }

                OnActiveItemUsed?.Invoke(slot.item, slotIndex);
                GameLogger.LogInfo($"아이템 사용 성공 (시뮬레이션): {slot.item.DisplayName}", GameLogger.LogCategory.Core);
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
            GameLogger.LogInfo($"[ItemService] 아이템 타겟 결정 시작: {slot.item.DisplayName}, 타겟타입={slot.item.targetType}", GameLogger.LogCategory.Core);
            var targetCharacter = DetermineItemTarget(slot.item, playerCharacter);
            GameLogger.LogInfo($"[ItemService] 아이템 사용 대상 최종 결정: {slot.item.DisplayName} → {targetCharacter?.GetCharacterName()}, 플레이어인가={targetCharacter?.IsPlayerControlled()}", GameLogger.LogCategory.Core);

            bool success = activeItem.UseItem(playerCharacter, targetCharacter);

            if (success)
            {
                // 이벤트/로그 전에 아이템 참조를 보관 (제거 시 null 방지)
                var usedItem = slot.item;

                // 소모품인 경우 슬롯에서 제거
                if (usedItem != null && usedItem.Type == ItemType.Active)
                {
                    GameLogger.LogInfo($"[ItemService] 아이템 제거 시작: {usedItem.DisplayName} (슬롯 {slotIndex})", GameLogger.LogCategory.Core);
                    RemoveActiveItem(slotIndex);
                    GameLogger.LogInfo($"[ItemService] 아이템 제거 완료: {usedItem.DisplayName} (슬롯 {slotIndex})", GameLogger.LogCategory.Core);
                }

                OnActiveItemUsed?.Invoke(usedItem, slotIndex);
                if (usedItem != null)
                {
                    GameLogger.LogInfo($"아이템 사용 성공: {usedItem.DisplayName}", GameLogger.LogCategory.Core);
                }
                else
                {
                    GameLogger.LogInfo($"아이템 사용 성공: (제거됨)", GameLogger.LogCategory.Core);
                }
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
            GameLogger.LogInfo($"아이템 추가됨: {itemDefinition.DisplayName} (슬롯 {emptySlotIndex})", GameLogger.LogCategory.Core);

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
            GameLogger.LogInfo($"아이템 제거됨: {removedItem?.DisplayName ?? "Unknown"} (슬롯 {slotIndex})", GameLogger.LogCategory.Core);

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
                GameLogger.LogInfo($"스킬 강화 단계 증가: {skillId} → ★{skillStarRanks[skillId]}", GameLogger.LogCategory.Core);

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
                                int newMax = player.GetMaxHP() + add;
                                GameLogger.LogInfo($"[ItemService] 플레이어 최대 체력 증가: +{add} → {newMax}", GameLogger.LogCategory.Core);
                                // ICharacter는 SetMaxHP가 없으므로 CharacterBase로 캐스팅하여 적용
                                if (player is Game.CharacterSystem.Core.CharacterBase cb)
                                {
                                    cb.SetMaxHP(newMax);
                                    // 증가한 만큼 즉시 회복 (이전 HP + 증가량, 단 최대치 초과 금지)
                                    int healed = Mathf.Min(prevHP + add, newMax);
                                    cb.SetCurrentHP(healed);
                                }
                                else
                                {
                                    GameLogger.LogWarning("[ItemService] SetMaxHP를 적용할 수 없습니다 (CharacterBase 아님)", GameLogger.LogCategory.Core);
                                }
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
                GameLogger.LogInfo($"스킬 {skillId}이 이미 최대 강화 단계(★{ItemConstants.MAX_ENHANCEMENT_LEVEL})입니다", GameLogger.LogCategory.Core);
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
            var turnManager = UnityEngine.Object.FindFirstObjectByType<Game.CombatSystem.Manager.TurnManager>();
            if (turnManager == null)
            {
                GameLogger.LogWarning("TurnManager를 찾을 수 없습니다. 아이템 사용을 차단합니다", GameLogger.LogCategory.Core);
                return false; // TurnManager가 없으면 안전하게 차단
            }

            bool isTurnPlayerTurn = turnManager.IsPlayerTurn();

            // 2단계: CombatStateMachine 전투 상태 확인
            var combatStateMachine = UnityEngine.Object.FindFirstObjectByType<Game.CombatSystem.State.CombatStateMachine>();
            if (combatStateMachine == null)
            {
                GameLogger.LogWarning("CombatStateMachine을 찾을 수 없습니다. 아이템 사용을 차단합니다", GameLogger.LogCategory.Core);
                return false;
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
                GameLogger.LogInfo($"아이템 사용 불가 - 턴상태: {isTurnPlayerTurn}, 전투상태: {currentState.StateName}, 드래그허용: {currentState.AllowPlayerCardDrag}", GameLogger.LogCategory.Core);
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
            GameLogger.LogInfo($"[ItemService.DetermineItemTarget] 타겟 결정 로직 시작: 아이템={item.DisplayName}, 타겟타입={item.targetType}", GameLogger.LogCategory.Core);

            switch (item.targetType)
            {
                case ItemTargetType.Self:
                    // 플레이어 자신에게 사용
                    GameLogger.LogInfo($"[ItemService.DetermineItemTarget] Self 타입 → 플레이어 ({playerCharacter?.GetCharacterName()})", GameLogger.LogCategory.Core);
                    return playerCharacter;

                case ItemTargetType.Enemy:
                    // 적에게 사용
                    var enemyManager = UnityEngine.Object.FindFirstObjectByType<Game.CharacterSystem.Manager.EnemyManager>();
                    GameLogger.LogInfo($"[ItemService.DetermineItemTarget] Enemy 타입 → EnemyManager 찾기 결과={enemyManager != null}", GameLogger.LogCategory.Core);

                    var enemyCharacter = enemyManager?.GetCurrentEnemy();
                    GameLogger.LogInfo($"[ItemService.DetermineItemTarget] 현재 적 캐릭터={enemyCharacter?.GetCharacterName() ?? "null"}, 플레이어인가={enemyCharacter?.IsPlayerControlled()}", GameLogger.LogCategory.Core);

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
                    GameLogger.LogInfo("[ItemService.DetermineItemTarget] Both 타입 → 플레이어 우선", GameLogger.LogCategory.Core);
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
            GameLogger.LogInfo("[ItemService] 새 게임을 위한 인벤토리 초기화 시작", GameLogger.LogCategory.Core);
            
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
            
            GameLogger.LogInfo("[ItemService] 인벤토리 초기화 완료", GameLogger.LogCategory.Core);
        }

        /// <summary>
        /// 인벤토리 UI의 자식 오브젝트들을 제거합니다.
        /// </summary>
        private void ClearInventoryUI()
        {
            try
            {
                // InventoryPanelController 찾기
                var inventoryController = FindFirstObjectByType<Game.ItemSystem.Runtime.InventoryPanelController>();
                if (inventoryController != null)
                {
                    inventoryController.ClearAllItemPrefabs();
                    GameLogger.LogInfo("[ItemService] 인벤토리 UI 자식 오브젝트 제거 완료", GameLogger.LogCategory.Core);
                }
                else
                {
                    GameLogger.LogWarning("[ItemService] InventoryPanelController를 찾을 수 없습니다", GameLogger.LogCategory.Core);
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