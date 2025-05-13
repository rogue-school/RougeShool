using UnityEngine;
using Game.CharacterSystem.Core;
using Game.CharacterSystem.Data;
using Game.CombatSystem.Player;
using Game.CombatSystem.Slot;

namespace Game.CombatSystem.Intialization
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

            // 슬롯 위치에 프리팹 생성 + 슬롯을 부모로 지정
            var playerGO = Instantiate(playerPrefab, ((MonoBehaviour)slot).transform);

            // 위치 초기화
            RectTransform rt = playerGO.GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.anchoredPosition = Vector2.zero;
                rt.localRotation = Quaternion.identity;
                rt.localScale = Vector3.one;
            }

            PlayerCharacter player = playerGO.GetComponent<PlayerCharacter>();
            player.SetCharacterData(defaultData);
            slot.SetCharacter(player);

            PlayerManager.Instance.SetPlayer(player); // 매니저에 등록 (선택적)

            Debug.Log("[PlayerCharacterInitializer] 플레이어 캐릭터가 슬롯에 배치되었습니다.");
        }
    }
}
