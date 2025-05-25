using System.Collections.Generic;
using UnityEngine;
using Game.CharacterSystem.Data;
using Game.IManager;
using Game.Utility.GameFlow;
using Game.CharacterSystem.Interface;

namespace Game.CharacterSystem.Core
{
    public class PlayerCharacterSelector : MonoBehaviour, IPlayerCharacterSelector
    {
        [Header("등록된 캐릭터 목록")]
        [SerializeField] private List<PlayerCharacterData> characterCandidates;

        private ISceneLoader sceneLoader;

        public static PlayerCharacterData SelectedCharacter { get; private set; }

        public void Inject(ISceneLoader loader)
        {
            sceneLoader = loader;
        }

        public void SelectCharacter(int index)
        {
            if (index < 0 || index >= characterCandidates.Count)
            {
                Debug.LogError($"[Selector] 잘못된 인덱스: {index}");
                return;
            }

            SelectedCharacter = characterCandidates[index];
            Debug.Log($"[Selector] 캐릭터 선택됨: {SelectedCharacter.DisplayName}");
            sceneLoader.LoadScene("Combat");
        }

        public static void ForceSetSelectedCharacter(PlayerCharacterData data)
        {
            SelectedCharacter = data;
            Debug.Log($"[Selector] 강제 캐릭터 설정됨: {data.DisplayName}");
        }
        public PlayerCharacterData GetSelectedCharacter()
        {
            return SelectedCharacter;
        }
    }
}
