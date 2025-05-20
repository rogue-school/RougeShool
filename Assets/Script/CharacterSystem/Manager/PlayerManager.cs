using UnityEngine;
using Game.CharacterSystem.Interface;
using Game.IManager;
using Game.CharacterSystem.Core;
using Game.CharacterSystem.Data;
using Game.CombatSystem.Manager;
using Game.CombatSystem.Slot;

namespace Game.Manager
{
    public class PlayerManager : MonoBehaviour, IPlayerManager
    {
        [Header("프리팹 및 핸드 매니저 연결")]
        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private PlayerHandManager handManager;

        [Header("기본 캐릭터 데이터 (선택되지 않았을 때 사용)")]
        [SerializeField] private PlayerCharacterData defaultCharacterData;

        private IPlayerCharacter player;

        public void CreateAndRegisterPlayer()
        {
            var selectedData = PlayerCharacterSelector.SelectedCharacter;

            if (selectedData == null)
            {
                Debug.LogWarning("[PlayerManager] 선택된 캐릭터 없음 → 기본 캐릭터 사용 시도");

                if (defaultCharacterData == null)
                {
                    Debug.LogError("[PlayerManager] 기본 캐릭터 데이터가 인스펙터에 할당되지 않았습니다.");
                    return;
                }

                selectedData = defaultCharacterData;
                PlayerCharacterSelector.ForceSetSelectedCharacter(selectedData);
            }

            var playerGO = Instantiate(playerPrefab);
            var character = playerGO.GetComponent<IPlayerCharacter>();
            if (character == null)
            {
                Debug.LogError("[PlayerManager] IPlayerCharacter 컴포넌트가 존재하지 않습니다.");
                return;
            }

            if (character is PlayerCharacter concreteCharacter)
                concreteCharacter.SetCharacterData(selectedData);

            SetPlayer(character);

            if (handManager == null)
            {
                Debug.LogError("[PlayerManager] 핸드 매니저가 연결되지 않았습니다.");
                return;
            }

            handManager.Inject(character, SlotRegistry.Instance, null);
            handManager.Initialize();
            handManager.GenerateInitialHand();

            SetPlayerHandManager(handManager);
        }

        public void SetPlayer(IPlayerCharacter player)
        {
            this.player = player;
            Debug.Log("[PlayerManager] 플레이어 캐릭터가 등록되었습니다.");
        }

        public IPlayerCharacter GetPlayer() => player;

        public void SetPlayerHandManager(IPlayerHandManager manager)
        {
            handManager = manager as PlayerHandManager;
        }

        public IPlayerHandManager GetPlayerHandManager() => handManager;
    }
}
