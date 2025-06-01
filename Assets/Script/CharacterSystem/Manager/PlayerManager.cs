using UnityEngine;
using Game.CharacterSystem.Interface;
using Game.IManager;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.Slot;
using Game.SkillCardSystem.UI;
using Game.CombatSystem.Interface;
using Game.CharacterSystem.Data;

namespace Game.Manager
{
    public class PlayerManager : MonoBehaviour, IPlayerManager
    {
        [Header("프리팹 및 슬롯")]
        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private Transform playerSlot;

        [Header("기본 캐릭터 데이터")]
        [SerializeField] private PlayerCharacterData defaultCharacterData;

        private IPlayerCharacter playerCharacter;
        private IPlayerCharacterSelector selector;
        private IPlayerHandManager handManager;
        private IHandSlotRegistry slotRegistry;

        public void SetPlayerCharacterSelector(IPlayerCharacterSelector selector) => this.selector = selector;

        public void SetSlotRegistry(IHandSlotRegistry registry)
        {
            this.slotRegistry = registry;
            Debug.Log("[PlayerManager] 핸드 슬롯 레지스트리 등록 완료");
        }

        public void SetPlayerHandManager(IPlayerHandManager manager)
        {
            this.handManager = manager;
            Debug.Log("[PlayerManager] 핸드 매니저 등록 완료");

            if (playerCharacter != null)
                playerCharacter.InjectHandManager(handManager);
        }

        public void CreateAndRegisterPlayer()
        {
            Debug.Log("[PlayerManager] CreateAndRegisterPlayer() 호출됨");

            if (playerCharacter != null)
            {
                Debug.Log("[PlayerManager] 기존 플레이어 재사용");
                InjectHandAndInitialize();
                return;
            }

            var selectedData = selector?.GetSelectedCharacter() ?? defaultCharacterData;
            if (selectedData == null)
            {
                Debug.LogError("[PlayerManager] 선택된 캐릭터 데이터가 없습니다.");
                return;
            }

            var instance = Instantiate(playerPrefab);
            instance.name = "PlayerCharacter";
            instance.transform.SetParent(playerSlot, false); // UI 기준 부모 설정

            // UI 정렬
            if (instance.TryGetComponent(out RectTransform rt))
            {
                rt.anchorMin = new Vector2(0.5f, 0.5f);
                rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.anchoredPosition = Vector2.zero;
                rt.localRotation = Quaternion.identity;
                rt.localScale = Vector3.one;
            }
            else
            {
                // 일반 Transform 처리 (비 UI 상황)
                instance.transform.localPosition = Vector3.zero;
                instance.transform.localRotation = Quaternion.identity;
                instance.transform.localScale = Vector3.one;
            }

            if (!instance.TryGetComponent(out IPlayerCharacter character))
            {
                Debug.LogError("[PlayerManager] IPlayerCharacter 컴포넌트 누락됨");
                Destroy(instance);
                return;
            }

            character.SetCharacterData(selectedData);
            SetPlayer(character);
            InjectHandAndInitialize();
        }

        private void InjectHandAndInitialize()
        {
            if (playerCharacter == null || handManager == null || slotRegistry == null)
            {
                Debug.LogError("[PlayerManager] 의존성이 누락되었습니다.");
                return;
            }

            handManager.Inject(playerCharacter, slotRegistry, null);
            handManager.Initialize();
            handManager.GenerateInitialHand();
            handManager.LogPlayerHandSlotStates();

            playerCharacter.InjectHandManager(handManager);
        }

        public void SetPlayer(IPlayerCharacter player)
        {
            this.playerCharacter = player;
            Debug.Log("[PlayerManager] 플레이어 등록 완료");
        }

        public IPlayerCharacter GetPlayer() => playerCharacter;
        public IPlayerHandManager GetPlayerHandManager() => handManager;

        public ISkillCard GetCardInSlot(SkillCardSlotPosition pos) => handManager?.GetCardInSlot(pos);
        public ISkillCardUI GetCardUIInSlot(SkillCardSlotPosition pos) => handManager?.GetCardUIInSlot(pos);

        public void Reset()
        {
            Debug.Log("[PlayerManager] Reset 호출");
        }
    }
}
