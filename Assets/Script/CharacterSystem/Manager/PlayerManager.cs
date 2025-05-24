using UnityEngine;
using Game.CharacterSystem.Interface;
using Game.IManager;
using Game.CharacterSystem.Core;
using Game.CharacterSystem.Data;
using Game.CombatSystem.Manager;
using Game.CombatSystem.Slot;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.Slot;
using Game.SkillCardSystem.UI;

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
            Debug.Log("[PlayerManager] CreateAndRegisterPlayer() 호출됨");

            if (player != null)
            {
                Debug.Log("[PlayerManager] 기존 플레이어 캐릭터 재사용");
                handManager.Inject(player, SlotRegistry.Instance, null);
                handManager.Initialize();
                handManager.GenerateInitialHand();
                return;
            }

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
            Debug.Log("[PlayerManager] 플레이어 프리팹 인스턴스화 완료");

            var character = playerGO.GetComponent<IPlayerCharacter>();
            if (character == null)
            {
                Debug.LogError("[PlayerManager] IPlayerCharacter 컴포넌트가 존재하지 않습니다.");
                return;
            }

            var slot = SlotRegistry.Instance?.GetCharacterSlot(SlotOwner.PLAYER);
            if (slot == null)
            {
                Debug.LogError("[PlayerManager] PLAYER용 캐릭터 슬롯을 찾을 수 없습니다.");
                return;
            }

            var parent = slot.GetTransform();
            if (playerGO.TryGetComponent<RectTransform>(out var rect))
            {
                rect.SetParent(parent as RectTransform, false);
                rect.anchoredPosition = Vector2.zero;
                rect.localRotation = Quaternion.identity;
                rect.localScale = Vector3.one;
            }
            else
            {
                playerGO.transform.SetParent(parent, false);
                playerGO.transform.localPosition = Vector3.zero;
                playerGO.transform.localRotation = Quaternion.identity;
                playerGO.transform.localScale = Vector3.one;
            }

            if (character is PlayerCharacter concreteCharacter)
            {
                concreteCharacter.SetCharacterData(selectedData);
                Debug.Log($"[PlayerManager] PlayerCharacterData 주입 완료: {selectedData.DisplayName}");
            }

            SetPlayer(character);

            if (handManager == null)
            {
                Debug.LogError("[PlayerManager] 핸드 매니저가 연결되지 않았습니다.");
                return;
            }

            handManager.Inject(character, SlotRegistry.Instance, null);
            Debug.Log("[PlayerManager] 핸드 매니저 Inject() 완료");

            handManager.Initialize();
            Debug.Log("[PlayerManager] 핸드 매니저 Initialize() 완료");

            handManager.GenerateInitialHand();
            Debug.Log("[PlayerManager] 핸드 카드 생성 완료");

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
            Debug.Log("[PlayerManager] 핸드 매니저 연결 완료");
        }

        public IPlayerHandManager GetPlayerHandManager() => handManager;

        public ISkillCard GetCardInSlot(SkillCardSlotPosition pos)
        {
            return handManager?.GetCardInSlot(pos);
        }

        public ISkillCardUI GetCardUIInSlot(SkillCardSlotPosition pos)
        {
            return handManager?.GetCardUIInSlot(pos);
        }
    }
}
