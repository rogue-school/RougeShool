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
    /// <summary>
    /// 플레이어 캐릭터 및 핸드 관리 책임을 가지는 매니저.
    /// SRP: 캐릭터 생성/등록 및 핸드 초기화만 책임.
    /// DIP/ISP: 인터페이스 분리 및 의존 역전 적용.
    /// </summary>
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
        private IHandSlotRegistry slotRegistry; // ✅ ISlotRegistry → IHandSlotRegistry

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

            var instance = Instantiate(playerPrefab, playerSlot.position, Quaternion.identity);
            instance.transform.SetParent(playerSlot, false);
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = Quaternion.identity;
            instance.transform.localScale = Vector3.one;

            if (!instance.TryGetComponent(out IPlayerCharacter character))
            {
                Debug.LogError("[PlayerManager] IPlayerCharacter 컴포넌트 누락됨");
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
            // 필요한 경우 상태 초기화 가능
        }
    }
}
