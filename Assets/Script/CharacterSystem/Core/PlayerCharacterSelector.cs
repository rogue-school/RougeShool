using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Game.CharacterSystem.Data;

public class PlayerCharacterSelector : MonoBehaviour
{
    [Header("등록된 캐릭터 목록 (데이터와 버튼 순서 1:1 매칭)")]
    [SerializeField] private List<PlayerCharacterData> characterCandidates;
    [Header("캐릭터 선택 버튼 (데이터와 1:1 순서 매칭)")]
    [SerializeField] private List<Button> characterButtons;

    private void Start()
    {
        for (int i = 0; i < characterCandidates.Count; i++)
        {
            int idx = i;
            characterButtons[i].onClick.RemoveAllListeners();
            characterButtons[i].onClick.AddListener(() => OnSelectCharacter(idx));
        }
    }

    private void OnSelectCharacter(int index)
    {
        if (GameManager.Instance != null)
            GameManager.Instance.selectedCharacter = characterCandidates[index];
        Debug.Log($"선택된 캐릭터: {characterCandidates[index].DisplayName}");

        if (GameManager.Instance != null)
            GameManager.Instance.StartBattle();
    }
}
