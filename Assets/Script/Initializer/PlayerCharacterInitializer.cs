using UnityEngine;
using Game.Player;
using Game.Data;
using Game.Interface;
using Game.Slots;
using Game.Managers;

namespace Game.Initialization
{
    /// <summary>
    /// 플레이어 캐릭터를 슬롯에 배치하고 초기화하는 클래스입니다.
    /// </summary>
    public class PlayerCharacterInitializer : MonoBehaviour
    {
        [SerializeField] private PlayerCharacter playerPrefab;
        [SerializeField] private PlayerCharacterData defaultData;

        public void Setup()
        {
            if (defaultData == null)
            {
                Debug.LogWarning("[PlayerCharacterInitializer] 기본 데이터가 설정되지 않았습니다.");
                return;
            }

            var slot = SlotRegistry.Instance.GetCharacterSlot(SlotOwner.PLAYER);

            if (slot == null)
            {
                Debug.LogError("[PlayerCharacterInitializer] 플레이어 캐릭터 슬롯이 등록되지 않았습니다.");
                return;
            }

            var playerGO = Instantiate(playerPrefab, ((MonoBehaviour)slot).transform.position, Quaternion.identity);
            PlayerCharacter player = playerGO.GetComponent<PlayerCharacter>();
            player.SetCharacterData(defaultData);
            slot.SetCharacter(player);

            Debug.Log("[PlayerCharacterInitializer] 플레이어 캐릭터가 슬롯에 배치되었습니다.");
        }
    }
}
