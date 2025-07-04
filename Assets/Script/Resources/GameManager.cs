using UnityEngine;
using UnityEngine.SceneManagement;
using Game.CharacterSystem.Data;

public enum GameState
{
    MainMenu,
    Playing,
    Paused,
    GameOver
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    // 게임 상태
    public GameState CurrentState { get; private set; } = GameState.MainMenu;

    // 글로벌 데이터
    public int currentStage = 1;
    public string selectedWeapon = "기본 검";
    public PlayerCharacterData selectedCharacter;

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

    // 게임 상태 변경
    public void SetGameState(GameState newState)
    {
        CurrentState = newState;
        Debug.Log($"[GameManager] 상태 변경: {newState}");
        // 상태별 추가 처리 가능
    }

    // 씬 전환
    public void GoToMainMenu()
    {
        SetGameState(GameState.MainMenu);
        SceneManager.LoadScene("MainScenes");
    }

    public void StartBattle()
    {
        SetGameState(GameState.Playing);
        SceneManager.LoadScene("BattleScenes");
    }

    public void PauseGame()
    {
        SetGameState(GameState.Paused);
        Time.timeScale = 0f;
    }

    public void ResumeGame()
    {
        SetGameState(GameState.Playing);
        Time.timeScale = 1f;
    }

    public void GameOver()
    {
        SetGameState(GameState.GameOver);
        // 추가 GameOver 처리
    }

    // 예시: 세이브/로드(간단 버전)
    public void SaveProgress()
    {
        PlayerPrefs.SetInt("CurrentStage", currentStage);
        PlayerPrefs.SetString("SelectedWeapon", selectedWeapon);
        // 캐릭터 데이터 등은 별도 처리 필요
        PlayerPrefs.Save();
        Debug.Log("[GameManager] 진행 상황 저장 완료");
    }

    public void LoadProgress()
    {
        currentStage = PlayerPrefs.GetInt("CurrentStage", 1);
        selectedWeapon = PlayerPrefs.GetString("SelectedWeapon", "기본 검");
        // 캐릭터 데이터 등은 별도 처리 필요
        Debug.Log("[GameManager] 진행 상황 불러오기 완료");
    }
}
