using UnityEngine;
using Zenject;
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

        private IPlayerCharacterSelector characterSelector;
        private IPlayerHandManager handManager;

        [Inject]
        public void Construct(
            IPlayerCharacterSelector characterSelector,
            IPlayerHandManager handManager)
        {
            this.characterSelector = characterSelector;
            this.handManager = handManager;
        }

        public void CreateAndRegisterPlayer()
        {
            if (playerCharacter != null)
            {
                InitializeHandManager();
                return;
            }

            var selectedData = characterSelector?.GetSelectedCharacter() ?? defaultCharacterData;
            if (selectedData == null)
            {
                Debug.LogError("[PlayerManager] 선택된 캐릭터 데이터가 없습니다.");
                return;
            }

            var instance = Instantiate(playerPrefab, playerSlot);
            if (!instance.TryGetComponent(out IPlayerCharacter character))
            {
                Debug.LogError("[PlayerManager] IPlayerCharacter 컴포넌트 누락");
                Destroy(instance);
                return;
            }

            character.SetCharacterData(selectedData);
            SetPlayer(character);
            InitializeHandManager();
        }

        private void InitializeHandManager()
        {
            handManager.GenerateInitialHand();
            handManager.LogPlayerHandSlotStates();
            playerCharacter.InjectHandManager(handManager);
        }

        public void SetPlayer(IPlayerCharacter player)
        {
            playerCharacter = player;
        }

        public IPlayerCharacter GetPlayer() => playerCharacter;

        public IPlayerHandManager GetPlayerHandManager() => handManager;

        public ISkillCard GetCardInSlot(SkillCardSlotPosition pos) =>
            handManager?.GetCardInSlot(pos);

        public ISkillCardUI GetCardUIInSlot(SkillCardSlotPosition pos) =>
            handManager?.GetCardUIInSlot(pos);

        public void Reset()
        {
            Debug.Log("[PlayerManager] Reset 호출됨");
        }
    }
}
