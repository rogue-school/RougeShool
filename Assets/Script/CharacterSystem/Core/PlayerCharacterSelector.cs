using System.Collections.Generic;
using UnityEngine;
using Game.CharacterSystem.Data;
using Game.IManager;
using Game.Utility.GameFlow;

namespace Game.CharacterSystem.Core
{
    public class PlayerCharacterSelector : MonoBehaviour
    {
        [Header("등록된 플레이어 캐릭터 데이터들")]
        [SerializeField] private List<PlayerCharacterData> characterCandidates;

        private ISceneLoader sceneLoader;

        [Header("선택된 캐릭터 저장용")]
        public static PlayerCharacterData SelectedCharacter { get; private set; }

        /// <summary>
        /// 씬 로더 주입
        /// </summary>
        public void Inject(ISceneLoader loader)
        {
            sceneLoader = loader;
        }

        /// <summary>
        /// 인덱스를 통해 캐릭터를 선택하고 전투 씬으로 전환합니다.
        /// </summary>
        public void SelectCharacter(int index)
        {
            if (index < 0 || index >= characterCandidates.Count)
            {
                Debug.LogError($"[Selector] 인덱스 범위 초과: {index}");
                return;
            }

            SelectedCharacter = characterCandidates[index];
            Debug.Log($"[Selector] {SelectedCharacter.DisplayName} 선택됨");

            sceneLoader.LoadScene("Combat");
        }

        /// <summary>
        /// 외부에서 강제로 캐릭터를 설정합니다 (테스트용).
        /// </summary>
        public static void ForceSetSelectedCharacter(PlayerCharacterData data)
        {
            SelectedCharacter = data;
            Debug.Log($"[Selector] 강제 캐릭터 설정됨 → {data.DisplayName}");
        }
    }
}
