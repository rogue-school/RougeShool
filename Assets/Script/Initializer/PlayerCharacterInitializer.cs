using UnityEngine;
using Game.Player;
using Game.Data;

namespace Game.Initialization
{
    public class PlayerCharacterInitializer : MonoBehaviour
    {
        [SerializeField] private PlayerCharacter playerPrefab;
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private PlayerCharacterData defaultData;

        public void SpawnPlayer(PlayerCharacterData data)
        {
            var player = Instantiate(playerPrefab, spawnPoint.position, Quaternion.identity);
            player.SetCharacterData(data);
        }

        public void Setup()
        {
            if (defaultData != null)
                SpawnPlayer(defaultData);
            else
                Debug.LogWarning("[PlayerCharacterInitializer] 기본 데이터가 설정되지 않았습니다.");
        }
    }
}
