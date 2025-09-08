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
using Game.SkillCardSystem.Core;
using Game.AnimationSystem.Manager;
using Game.CoreSystem.Manager;

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

        #region 의존성 주입

        /// <summary>
        /// 의존성 주입 메서드입니다.
        /// </summary>
        [Inject]
        public void Inject(IPlayerManager playerManager, ISlotRegistry slotRegistry)
        {
            this.playerManager = playerManager;
            this.slotRegistry = slotRegistry;
        }

        #endregion

        #region ICombatInitializerStep 구현

        /// <summary>
        /// 슬롯 시스템 초기화 완료 후 실행됩니다.
        /// </summary>
        public IEnumerator Initialize()
        {
            Debug.Log("<color=cyan>[PlayerCharacterInitializer] 플레이어 캐릭터 초기화 시작</color>");
            
            // 1. 슬롯 초기화 대기
            yield return new WaitUntil(() =>
                slotRegistry is SlotRegistry concrete && concrete.IsInitialized);

            // 2. 캐릭터 생성 및 부모 설정
            var slot = GetPlayerSlot();
            if (slot == null) yield break;
            Transform slotTransform = ((MonoBehaviour)slot).transform;

            foreach (Transform child in slotTransform)
                Object.Destroy(child.gameObject);

            var player = Object.Instantiate(playerPrefab);
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
                Debug.LogError("[PlayerCharacterInitializer] PlayerCharacter 컴포넌트를 찾을 수 없습니다.");
                Object.Destroy(player.gameObject);
                yield break;
            }

            var data = ResolvePlayerData();
            if (data == null)
            {
                Debug.LogError("[PlayerCharacterInitializer] 캐릭터 데이터가 없습니다.");
                yield break;
            }

            character.SetCharacterData(data); // ★ 데이터 먼저 주입

            // 4. 등장 애니메이션 실행 및 대기 (데이터베이스 기반)
            bool animDone = false;
            string characterId = data.name; // ScriptableObject의 name
            
            // AnimationFacade가 사용 가능한지 확인
            if (AnimationFacade.Instance != null)
            {
                AnimationFacade.Instance.PlayCharacterAnimation(characterId, "spawn", player.gameObject, () => animDone = true, false);
                yield return new WaitUntil(() => animDone);
            }
            else
            {
                Debug.LogWarning("[PlayerCharacterInitializer] AnimationFacade가 사용 불가능합니다. 애니메이션을 건너뜁니다.");
                // 애니메이션 없이 바로 완료 처리
                animDone = true;
            }

            // 5. 슬롯/매니저에 등록
            slot.SetCharacter(character);
            playerManager?.SetPlayer(character);
            
            Debug.Log("<color=cyan>[PlayerCharacterInitializer] 플레이어 캐릭터 초기화 완료</color>");
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
                Debug.LogError("[PlayerCharacterInitializer] 유효하지 않은 초기화 데이터입니다.");
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
                Debug.LogError("[PlayerCharacterInitializer] PlayerCharacter 컴포넌트를 찾을 수 없습니다.");
                Destroy(player.gameObject);
                return;
            }

            // 데이터 설정
            var data = ResolvePlayerData();
            if (data == null)
            {
                Debug.LogError("[PlayerCharacterInitializer] 캐릭터 데이터가 없습니다.");
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
                Debug.LogError("[PlayerCharacterInitializer] CharacterSlotRegistry가 null입니다.");
                return null;
            }

            var slot = registry.GetCharacterSlot(SlotOwner.PLAYER);
            if (slot == null)
                Debug.LogError("[PlayerCharacterInitializer] SlotOwner.PLAYER에 해당하는 캐릭터 슬롯이 없습니다.");

            return slot;
        }

        /// <summary>
        /// 플레이어 캐릭터 데이터 선택 로직입니다.
        /// 선택된 캐릭터가 있으면 그것을 사용하고, 없으면 기본 데이터를 사용합니다.
        /// </summary>
        private PlayerCharacterData ResolvePlayerData()
        {
            // 새로운 캐릭터 선택 매니저에서 선택된 캐릭터 우선 사용
            if (PlayerCharacterSelectionManager.Instance != null && PlayerCharacterSelectionManager.Instance.HasSelectedCharacter())
            {
                var selectedCharacter = PlayerCharacterSelectionManager.Instance.GetSelectedCharacter();
                if (selectedCharacter != null && !string.IsNullOrEmpty(selectedCharacter.DisplayName))
                {
                    Debug.Log($"[PlayerCharacterInitializer] 선택된 캐릭터 사용: {selectedCharacter.DisplayName}");
                    return selectedCharacter;
                }
                else
                {
                    Debug.LogError("[PlayerCharacterInitializer] 선택된 캐릭터가 null이거나 DisplayName이 설정되지 않았습니다.");
                }
            }
            
            // GameManager의 선택 데이터 사용 (하위 호환성)
            if (GameManager.Instance != null && GameManager.Instance.selectedCharacter != null)
            {
                Debug.Log($"[PlayerCharacterInitializer] GameManager에서 캐릭터 사용: {GameManager.Instance.selectedCharacter.DisplayName}");
                return GameManager.Instance.selectedCharacter;
            }

            // 기본 데이터 사용
            if (defaultData != null)
            {
                Debug.Log($"[PlayerCharacterInitializer] 기본 캐릭터 데이터 사용: {defaultData.DisplayName}");
                return defaultData;
            }

            Debug.LogError("[PlayerCharacterInitializer] 캐릭터 데이터가 없습니다. (선택된 캐릭터, GameManager, 기본 데이터 모두 null)");
            return null;
        }

        #endregion
    }
}
