using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Game.CharacterSystem.Data;
using Game.CoreSystem.Interface;
using Zenject;

namespace Game.CharacterSystem.Core
{
    public class PlayerCharacterSelector : MonoBehaviour
{
    [Header("등록된 캐릭터 목록 (데이터와 버튼 순서 1:1 매칭)")]
    [SerializeField] private List<PlayerCharacterData> characterCandidates;
    [Header("캐릭터 선택 버튼 (데이터와 1:1 순서 매칭)")]
    [SerializeField] private List<Button> characterButtons;
    
    // 의존성 주입
    [Inject] private IPlayerCharacterSelectionManager playerCharacterSelectionManager;
    [Inject] private ISceneTransitionManager sceneTransitionManager;

    private void Start()
    {
        // 리스트 유효성 검사
        if (characterCandidates == null || characterCandidates.Count == 0)
        {
            Debug.LogError("[PlayerCharacterSelector] characterCandidates가 설정되지 않았습니다.");
            return;
        }
        
        if (characterButtons == null || characterButtons.Count == 0)
        {
            Debug.LogError("[PlayerCharacterSelector] characterButtons가 설정되지 않았습니다.");
            return;
        }
        
        if (characterCandidates.Count != characterButtons.Count)
        {
            Debug.LogError($"[PlayerCharacterSelector] 캐릭터 데이터 수({characterCandidates.Count})와 버튼 수({characterButtons.Count})가 일치하지 않습니다.");
            return;
        }
        
        for (int i = 0; i < characterCandidates.Count; i++)
        {
            int idx = i;
            characterButtons[i].onClick.RemoveAllListeners();
            characterButtons[i].onClick.AddListener(() => OnSelectCharacter(idx));
        }
        
        Debug.Log($"[PlayerCharacterSelector] {characterCandidates.Count}개의 캐릭터 버튼이 설정되었습니다.");
    }

    private async void OnSelectCharacter(int index)
    {
        // 인덱스 유효성 검사
        if (index < 0 || index >= characterCandidates.Count)
        {
            Debug.LogError($"[PlayerCharacterSelector] 잘못된 캐릭터 인덱스: {index}");
            return;
        }
        
        // 캐릭터 선택 로직
        var selectedCharacter = characterCandidates[index];
        
        // 선택된 캐릭터 유효성 검사
        if (selectedCharacter == null)
        {
            Debug.LogError($"[PlayerCharacterSelector] 인덱스 {index}의 캐릭터 데이터가 null입니다.");
            return;
        }
        
        Debug.Log($"선택된 캐릭터: {selectedCharacter.DisplayName ?? "이름 없음"}");
        
        // 캐릭터 선택 매니저에 선택된 캐릭터 저장
        if (playerCharacterSelectionManager != null)
        {
            playerCharacterSelectionManager.SelectCharacter(selectedCharacter);
        }
        else
        {
            Debug.LogError("[PlayerCharacterSelector] PlayerCharacterSelectionManager를 찾을 수 없습니다.");
            return;
        }
        
        // 새로운 씬 전환 시스템 사용
        await sceneTransitionManager.TransitionToStageScene();
    }
}
}
