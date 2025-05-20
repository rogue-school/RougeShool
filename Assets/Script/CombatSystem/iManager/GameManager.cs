using UnityEngine;
using UnityEngine.SceneManagement;
using Game.CharacterSystem.Data;

namespace Game.IManager
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public PlayerCharacterData SelectedCharacter { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void SetSelectedCharacter(PlayerCharacterData characterData)
        {
            SelectedCharacter = characterData;
        }

        public void LoadCombatScene()
        {
            Debug.Log("[GameManager] 전투 씬 로딩 중...");
            SceneManager.LoadScene("CombatScene"); // 실제 씬 이름 사용
        }
    }
}
