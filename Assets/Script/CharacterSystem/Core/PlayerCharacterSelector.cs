using System.Collections.Generic;
using UnityEngine;
using Game.CharacterSystem.Data;
using Game.IManager;

namespace Game.CharacterSystem.Core
{
    public class PlayerCharacterSelector : MonoBehaviour
    {
        [Header("등록된 플레이어 캐릭터 데이터들")]
        [SerializeField] private List<PlayerCharacterData> characterCandidates;

        [Header("선택된 캐릭터 저장용")]
        public static PlayerCharacterData SelectedCharacter { get; private set; }

        public void SelectCharacter(int index)
        {
            if (index < 0 || index >= characterCandidates.Count)
            {
                Debug.LogError($"[Selector] 인덱스 범위 초과: {index}");
                return;
            }

            SelectedCharacter = characterCandidates[index];
            Debug.Log($"[Selector] {SelectedCharacter.displayName} 선택됨");

            // 씬 전환 또는 초기화 시작
            GameManager.Instance.LoadCombatScene(); // 예시
        }
        public static void ForceSetSelectedCharacter(PlayerCharacterData data)
        {
            SelectedCharacter = data;
            Debug.Log($"[Selector] 강제 캐릭터 설정됨 → {data.displayName}");
        }
    }
}
