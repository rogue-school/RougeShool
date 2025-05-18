using UnityEngine;
using Game.CharacterSystem.Data;
using Game.CombatSystem.Slot;
using Game.CharacterSystem.Core;
using Game.CharacterSystem.Interface;
using Game.IManager;

namespace Game.CombatSystem.Initialization
{
    /// <summary>
    /// 플레이어 캐릭터를 초기화하고 슬롯에 배치합니다.
    /// </summary>
    public class PlayerCharacterInitializer : MonoBehaviour, IPlayerCharacterInitializer
    {
        [SerializeField] private PlayerCharacter playerPrefab;
        [SerializeField] private PlayerCharacterData defaultData;

        private IPlayerManager playerManager;

        public void Inject(IPlayerManager playerManager)
        {
            this.playerManager = playerManager;
            //Debug.Log("[PlayerCharacterInitializer] IPlayerManager 주입 완료");
        }

        public void Setup()
        {
            //Debug.Log("[PlayerCharacterInitializer] Setup() 호출됨");

            if (!ValidateData()) return;

            var slot = GetPlayerSlot();
            if (slot == null)
            {
                //Debug.LogError("[PlayerCharacterInitializer] 플레이어 슬롯을 찾을 수 없습니다.");
                return;
            }

            ClearSlotChildren(slot);

            var player = InstantiateAndConfigureCharacter(slot);
            if (player == null)
            {
                //Debug.LogError("[PlayerCharacterInitializer] 캐릭터 인스턴스 생성 실패");
                return;
            }

            ApplyCharacterData(player);
            RegisterToManager(slot, player);
        }

        private bool ValidateData()
        {
            if (playerPrefab == null)
            {
                //Debug.LogError("[PlayerCharacterInitializer] playerPrefab이 지정되지 않았습니다.");
                return false;
            }

            if (defaultData == null)
            {
                //Debug.LogWarning("[PlayerCharacterInitializer] defaultData가 null입니다.");
                return false;
            }

            if (SlotRegistry.Instance == null)
            {
                //Debug.LogError("[PlayerCharacterInitializer] SlotRegistry 인스턴스를 찾을 수 없습니다.");
                return false;
            }

            return true;
        }

        private ICharacterSlot GetPlayerSlot()
        {
            //Debug.Log("[PlayerCharacterInitializer] GetPlayerSlot 호출됨");

            var slot = SlotRegistry.Instance.GetCharacterSlot(SlotOwner.PLAYER);
            if (slot == null)
            {
                //Debug.LogError("[PlayerCharacterInitializer] SlotRegistry에서 플레이어 슬롯을 가져오지 못했습니다.");
            }
            else
            {
                //Debug.Log($"[PlayerCharacterInitializer] 슬롯 이름: {((MonoBehaviour)slot).gameObject.name}");
            }

            return slot;
        }

        private void ClearSlotChildren(ICharacterSlot slot)
        {
            //Debug.Log("[PlayerCharacterInitializer] 기존 슬롯 자식 오브젝트 제거 시작");
            foreach (Transform child in ((MonoBehaviour)slot).transform)
            {
                //Debug.Log($" → 제거 대상: {child.name}");
                Destroy(child.gameObject);
            }
        }

        private PlayerCharacter InstantiateAndConfigureCharacter(ICharacterSlot slot)
        {
            //Debug.Log("[PlayerCharacterInitializer] 캐릭터 프리팹 인스턴스 생성 중");

            var playerGO = Instantiate(playerPrefab, ((MonoBehaviour)slot).transform);
            playerGO.name = "PlayerCharacter";

            if (playerGO.TryGetComponent(out RectTransform rt))
            {
                rt.anchoredPosition = Vector2.zero;
                rt.localRotation = Quaternion.identity;
                rt.localScale = Vector3.one;
            }

            if (!playerGO.TryGetComponent(out PlayerCharacter player))
            {
                //Debug.LogError("[PlayerCharacterInitializer] PlayerCharacter 컴포넌트를 찾을 수 없습니다.");
                Destroy(playerGO);
                return null;
            }

            return player;
        }

        private void ApplyCharacterData(PlayerCharacter player)
        {
            //Debug.Log("[PlayerCharacterInitializer] 캐릭터 데이터 적용 시작");
            player.SetCharacterData(defaultData);
        }

        private void RegisterToManager(ICharacterSlot slot, PlayerCharacter player)
        {
            //Debug.Log("[PlayerCharacterInitializer] 슬롯 및 매니저에 캐릭터 등록");
            slot.SetCharacter(player);

            if (playerManager != null)
            {
                playerManager.SetPlayer(player);
                //Debug.Log("[PlayerCharacterInitializer] PlayerManager에 캐릭터 등록 완료");
            }
            else
            {
                //Debug.LogWarning("[PlayerCharacterInitializer] PlayerManager가 주입되지 않았습니다.");
            }

            //Debug.Log("[PlayerCharacterInitializer] 플레이어 캐릭터 초기화 완료");
        }
    }
}
