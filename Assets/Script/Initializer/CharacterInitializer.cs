using UnityEngine;
using Game.Characters;
using Game.Slots;
using Game.Managers;

namespace Game.Battle
{
    /// <summary>
    /// 전투 시작 시 플레이어 캐릭터를 지정된 슬롯에 배치하고 초기화합니다.
    /// </summary>
    public class CharacterInitializer : MonoBehaviour
    {
        [SerializeField] private GameObject playerPrefab;

        private void Awake()
        {
            Setup(); // 자동 실행도 유지
        }

        /// <summary>
        /// 외부에서도 호출 가능한 Setup 함수
        /// </summary>
        public void Setup()
        {
            // 슬롯 레지스트리에서 플레이어용 FIRST 슬롯을 가져온다
            var slot = SlotRegistry.Instance.GetSlot(
                SlotOwner.Player,
                SlotRole.CharacterSpawn,
                BattleSlotPosition.FIRST
            );

            if (slot == null)
            {
                Debug.LogError("[CharacterInitializer] Player FIRST 슬롯을 찾을 수 없습니다.");
                return;
            }

            // 슬롯 위치에 플레이어 프리팹 생성
            GameObject instance = Instantiate(playerPrefab, slot.transform.position, Quaternion.identity);
            CharacterBase character = instance.GetComponent<CharacterBase>();

            if (character == null)
            {
                Debug.LogError("[CharacterInitializer] 생성된 오브젝트에 CharacterBase가 없습니다.");
                return;
            }

            character.Initialize(character.MaxHP);
            Debug.Log("[CharacterInitializer] 플레이어 캐릭터가 슬롯에 배치되었습니다.");
        }
    }
}
