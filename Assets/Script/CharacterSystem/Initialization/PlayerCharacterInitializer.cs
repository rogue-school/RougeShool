using UnityEngine;
using Zenject;
using System.Collections;
using Game.CharacterSystem.Data;
using Game.CombatSystem.Data;
using Game.CombatSystem.Slot;
using Game.CharacterSystem.Core;
using Game.CharacterSystem.Interface;
using Game.IManager;
using Game.CombatSystem.Interface;
using Game.CoreSystem.Interface;
using Game.CoreSystem.Utility;

namespace Game.CombatSystem.Initialization
{
    /// <summary>
    /// 플레이어 캐릭터를 생성하고 슬롯에 배치하는 초기화 컴포넌트입니다.
    /// 전투 시작 시 실행되며, ICombatInitializerStep을 통해 순차적으로 실행됩니다.
    /// </summary>
    public class PlayerCharacterInitializer : MonoBehaviour, IPlayerCharacterInitializer, ICombatInitializerStep
    {
        [Header("플레이어 캐릭터 프리팹 및 기본 데이터")]
        [SerializeField] private PlayerCharacter playerPrefab;
        [SerializeField] private PlayerCharacterData defaultData;

        [Header("초기화 순서 (낮을수록 먼저 실행됨)")]
        [SerializeField] private int order = 10;
        public int Order => order;

        private IPlayerManager playerManager;
        private ISlotRegistry slotRegistry;
        private IPlayerCharacterSelectionManager playerCharacterSelectionManager;
        private IGameStateManager gameStateManager;

        #region 의존성 주입

        /// <summary>
        /// 의존성 주입 메서드입니다.
        /// </summary>
        [Inject]
        public void Inject(IPlayerManager playerManager, ISlotRegistry slotRegistry, IPlayerCharacterSelectionManager playerCharacterSelectionManager, IGameStateManager gameStateManager)
        {
            this.playerManager = playerManager;
            this.slotRegistry = slotRegistry;
            this.playerCharacterSelectionManager = playerCharacterSelectionManager;
            this.gameStateManager = gameStateManager;
        }

        #endregion

        #region ICombatInitializerStep 구현

        /// <summary>
        /// 슬롯 시스템 초기화 완료 후 실행됩니다.
        /// </summary>
        public IEnumerator Initialize()
        {
            GameLogger.LogInfo("플레이어 캐릭터 초기화 시작", GameLogger.LogCategory.Character);
            
            // 1. 슬롯 초기화 대기
            yield return new WaitUntil(() =>
                slotRegistry is SlotRegistry concrete && concrete.IsInitialized);

            // 2. 캐릭터 생성 및 부모 설정
            var slot = GetPlayerSlot();
            if (slot == null) yield break;
            Transform slotTransform = ((MonoBehaviour)slot).transform;

            foreach (Transform child in slotTransform)
                Object.Destroy(child.gameObject);

            // 프리팹 해결 (null 방어 및 리소스 로드 시도)
            var prefabToUse = ResolvePlayerPrefab();
            if (prefabToUse == null)
            {
                GameLogger.LogError(" playerPrefab이 설정되지 않았고 리소스 로드에도 실패했습니다.");
                yield break;
            }

            var player = Object.Instantiate(prefabToUse);
            player.name = "PlayerCharacter";
            player.transform.SetParent(slotTransform, false);

            if (player.TryGetComponent(out RectTransform rt))
            {
                rt.anchorMin = new Vector2(0.5f, 0.5f);
                rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.anchoredPosition = Vector2.zero;
                rt.localPosition = Vector3.zero;
                rt.localRotation = Quaternion.identity;
                rt.localScale = Vector3.one;
            }

            // 3. PlayerCharacter 컴포넌트 및 데이터 세팅 (애니메이션 전에)
            if (!player.TryGetComponent(out PlayerCharacter character))
            {
                GameLogger.LogError(" PlayerCharacter 컴포넌트를 찾을 수 없습니다.");
                Object.Destroy(player.gameObject);
                yield break;
            }

            var data = ResolvePlayerData();
            if (data == null)
            {
                GameLogger.LogError(" 캐릭터 데이터가 없습니다.");
                yield break;
            }

            character.SetCharacterData(data); // ★ 데이터 먼저 주입

            // 4. 등장 애니메이션 건너뛰기 (AnimationSystem 제거로 인해 임시 비활성화)
            GameLogger.LogInfo("애니메이션을 건너뜁니다.", GameLogger.LogCategory.Character);
            yield return new WaitForSeconds(0.1f); // 짧은 대기 시간

            // 5. 슬롯/매니저에 등록
            slot.SetCharacter(character);
            playerManager?.SetPlayer(character);
            
            GameLogger.LogInfo(" 플레이어 캐릭터 초기화 완료</color>");
        }

        #endregion

        #region 캐릭터 배치 및 설정

        /// <summary>
        /// 플레이어 캐릭터를 생성하고 슬롯에 배치합니다.
        /// </summary>
        public void Setup()
        {
            if (!ValidateData())
            {
                GameLogger.LogError(" 유효하지 않은 초기화 데이터입니다.");
                return;
            }

            var slot = GetPlayerSlot();
            if (slot == null) return;

            Transform slotTransform = ((MonoBehaviour)slot).transform;

            // 기존 자식 제거
            foreach (Transform child in slotTransform)
                Destroy(child.gameObject);

            // 캐릭터 생성 및 부모 설정
            var player = Instantiate(playerPrefab);
            player.name = "PlayerCharacter";
            player.transform.SetParent(slotTransform, false);

            // RectTransform 정렬
            if (player.TryGetComponent(out RectTransform rt))
            {
                rt.anchorMin = new Vector2(0.5f, 0.5f);
                rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.anchoredPosition = Vector2.zero;
                rt.localPosition = Vector3.zero;
                rt.localRotation = Quaternion.identity;
                rt.localScale = Vector3.one;
            }

            // PlayerCharacter 컴포넌트 확인
            if (!player.TryGetComponent(out PlayerCharacter character))
            {
                GameLogger.LogError(" PlayerCharacter 컴포넌트를 찾을 수 없습니다.");
                Destroy(player.gameObject);
                return;
            }

            // 데이터 설정
            var data = ResolvePlayerData();
            if (data == null)
            {
                GameLogger.LogError(" 캐릭터 데이터가 없습니다.");
                return;
            }

            character.SetCharacterData(data);
            slot.SetCharacter(character);
            playerManager?.SetPlayer(character);
        }

        #endregion

        #region 내부 유틸리티

        /// <summary>
        /// 플레이어 프리팹과 슬롯 레지스트리의 유효성을 검사합니다.
        /// </summary>
        private bool ValidateData()
        {
            return playerPrefab != null && slotRegistry != null;
        }

        /// <summary>
        /// 플레이어 캐릭터를 배치할 슬롯을 조회합니다.
        /// </summary>
        private ICharacterSlot GetPlayerSlot()
        {
            var registry = slotRegistry?.GetCharacterSlotRegistry();
            if (registry == null)
            {
                GameLogger.LogError(" CharacterSlotRegistry가 null입니다.");
                return null;
            }

            var slot = registry.GetCharacterSlot(SlotOwner.PLAYER);
            if (slot == null)
                GameLogger.LogError(" SlotOwner.PLAYER에 해당하는 캐릭터 슬롯이 없습니다.");

            return slot;
        }

        /// <summary>
        /// 플레이어 캐릭터 데이터 선택 로직입니다.
        /// 선택된 캐릭터가 있으면 그것을 사용하고, 없으면 기본 데이터를 사용합니다.
        /// </summary>
        private PlayerCharacterData ResolvePlayerData()
        {
            // 새로운 캐릭터 선택 매니저에서 선택된 캐릭터 우선 사용
            if (playerCharacterSelectionManager != null && playerCharacterSelectionManager.HasSelectedCharacter())
            {
                var selectedCharacter = playerCharacterSelectionManager.GetSelectedCharacter();
                if (selectedCharacter != null && !string.IsNullOrEmpty(selectedCharacter.DisplayName))
                {
                    Debug.Log($"[PlayerCharacterInitializer] 선택된 캐릭터 사용: {selectedCharacter.DisplayName}");
                    return selectedCharacter;
                }
                else
                {
                    GameLogger.LogError(" 선택된 캐릭터가 null이거나 DisplayName이 설정되지 않았습니다.");
                }
            }
            
            // GameStateManager의 선택 데이터 사용 (하위 호환성)
            if (gameStateManager != null && gameStateManager.SelectedCharacter != null)
            {
                Debug.Log($"[PlayerCharacterInitializer] GameStateManager에서 캐릭터 사용: {gameStateManager.SelectedCharacter.DisplayName}");
                return gameStateManager.SelectedCharacter;
            }

            // 기본 데이터 사용
            if (defaultData != null)
            {
                Debug.Log($"[PlayerCharacterInitializer] 기본 캐릭터 데이터 사용: {defaultData.DisplayName}");
                return defaultData;
            }

            GameLogger.LogError(" 캐릭터 데이터가 없습니다. (선택된 캐릭터, GameManager, 기본 데이터 모두 null)");
            return null;
        }

        /// <summary>
        /// 플레이어 캐릭터 프리팹을 해석합니다. (직접 지정 > Resources 로드 > 실패 시 null)
        /// </summary>
        private PlayerCharacter ResolvePlayerPrefab()
        {
            if (playerPrefab != null)
                return playerPrefab;

            // 우선 표준 경로 시도
            var fromResources = Resources.Load<PlayerCharacter>("Prefab/PlayerCharacter");
            if (fromResources != null)
                return fromResources;

            // 폴백: 전체에서 첫 번째 프리팹 시도 (비용 적음)
            var all = Resources.LoadAll<PlayerCharacter>(string.Empty);
            if (all != null && all.Length > 0)
                return all[0];

            return null;
        }

        #endregion
    }
}
