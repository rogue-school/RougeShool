using UnityEngine;
using Game.CharacterSystem.Interface;
using Game.IManager;
using Game.CharacterSystem.Core;
using Game.CharacterSystem.Data;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.Slot;
using Game.SkillCardSystem.UI;
using Game.CombatSystem.Slot;
using Game.CombatSystem.Interface;
using Game.CombatSystem.Manager;

namespace Game.Manager
{
    public class PlayerManager : MonoBehaviour, IPlayerManager
    {
        [Header("프리팹 및 연결")]
        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private Transform playerSlot;
        [SerializeField] private PlayerHandManager handManager;

        [Header("기본 캐릭터 데이터")]
        [SerializeField] private PlayerCharacterData defaultCharacterData;

        private IPlayerCharacter playerCharacter;
        private IPlayerCharacterSelector selector;
        private ISlotRegistry slotRegistry;

        public void SetPlayerCharacterSelector(IPlayerCharacterSelector selector)
        {
            this.selector = selector;
        }

        public void SetSlotRegistry(ISlotRegistry registry)
        {
            this.slotRegistry = registry;
            Debug.Log("[PlayerManager] 슬롯 레지스트리 등록 완료");
        }


        public void SetPlayerHandManager(IPlayerHandManager manager)
        {
            this.handManager = manager as PlayerHandManager;
            Debug.Log("[PlayerManager] 핸드 매니저 등록 완료");

            if (playerCharacter != null)
            {
                playerCharacter.InjectHandManager(this.handManager);
            }
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

            var playerGO = Instantiate(playerPrefab, playerSlot.position, Quaternion.identity);
            var character = playerGO.GetComponent<IPlayerCharacter>();
            if (character == null)
            {
                Debug.LogError("[PlayerManager] IPlayerCharacter 컴포넌트 누락됨");
                return;
            }

            if (character is PlayerCharacter concrete)
            {
                concrete.SetCharacterData(selectedData);
            }

            SetTransform(playerGO, playerSlot);
            SetPlayer(character);
            InjectHandAndInitialize();
        }

        private void InjectHandAndInitialize()
        {
            if (playerCharacter == null || handManager == null || slotRegistry == null)
            {
                Debug.LogError("[PlayerManager] 플레이어 또는 핸드 매니저 또는 슬롯 레지스트리가 누락되었습니다.");
                return;
            }

            handManager.Inject(playerCharacter, slotRegistry, null);
            handManager.Initialize();
            handManager.GenerateInitialHand();
            handManager.LogPlayerHandSlotStates();

            playerCharacter.InjectHandManager(handManager);
        }

        private void SetTransform(GameObject obj, Transform parent)
        {
            if (obj.TryGetComponent(out RectTransform rect))
            {
                rect.SetParent(parent as RectTransform, false);
                rect.anchoredPosition = Vector2.zero;
            }
            else
            {
                obj.transform.SetParent(parent, false);
                obj.transform.localPosition = Vector3.zero;
            }

            obj.transform.localRotation = Quaternion.identity;
            obj.transform.localScale = Vector3.one;
        }

        public void SetPlayer(IPlayerCharacter player)
        {
            this.playerCharacter = player;
            Debug.Log("[PlayerManager] 플레이어 등록 완료");
        }
        public void Reset()
        {
            // 플레이어 상태 초기화 로직 구현
            Debug.Log("[PlayerManager] Reset");
        }

        public IPlayerCharacter GetPlayer() => playerCharacter;

        public IPlayerHandManager GetPlayerHandManager() => handManager;

        public ISkillCard GetCardInSlot(SkillCardSlotPosition pos) => handManager?.GetCardInSlot(pos);

        public ISkillCardUI GetCardUIInSlot(SkillCardSlotPosition pos) => handManager?.GetCardUIInSlot(pos);
    }
}
