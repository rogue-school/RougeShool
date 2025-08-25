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

    // ─────────────────────────────────────────────────────
    // [세션(이번 런) 전용 데이터]  ← "포기하기" 시 초기화 대상
    // ─────────────────────────────────────────────────────
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

    // ─────────────────────────────────────────────────────
    // 상태 전환
    // ─────────────────────────────────────────────────────
    public void SetGameState(GameState newState)
    {
        CurrentState = newState;
        Debug.Log($"[GameManager] 상태 변경: {newState}");
        // 필요 시 상태별 추가 처리
    }

    // ─────────────────────────────────────────────────────
    // 씬 전환
    // ─────────────────────────────────────────────────────
    public void GoToMainMenu()
    {
        // 혹시 정지 상태면 복구
        Time.timeScale = 1f;

        SetGameState(GameState.MainMenu);
        SceneManager.LoadScene("MainScenes");
    }

    public void StartBattle()
    {
        SetGameState(GameState.Playing);
        SceneManager.LoadScene("BattleScenes");
    }

    // ─────────────────────────────────────────────────────
    // 일시정지/재개
    // ─────────────────────────────────────────────────────
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

    // ─────────────────────────────────────────────────────
    // 저장/불러오기 (예시)
    // ─────────────────────────────────────────────────────
    public void SaveProgress()
    {
        PlayerPrefs.SetInt("CurrentStage", currentStage);
        PlayerPrefs.SetString("SelectedWeapon", selectedWeapon);
        // 캐릭터 데이터 등은 별도 직렬화 필요
        PlayerPrefs.Save();
        Debug.Log("[GameManager] 진행 상황 저장 완료");
    }

    public void LoadProgress()
    {
        currentStage = PlayerPrefs.GetInt("CurrentStage", 1);
        selectedWeapon = PlayerPrefs.GetString("SelectedWeapon", "기본 검");
        // 캐릭터 데이터 등은 별도 역직렬화 필요
        Debug.Log("[GameManager] 진행 상황 불러오기 완료");
    }

    // ─────────────────────────────────────────────────────
    // ★ 핵심: 세션 초기화 (이번 런만 날리고, 영구 메타는 유지)
    // ─────────────────────────────────────────────────────
    public void ResetSession()
    {
        Debug.Log("[GameManager] 세션 초기화 실행 (이번 런만 삭제, 영구 메타 보존)");

        // 1) 런타임 세션 데이터 초기화
        currentStage = 1;
        selectedWeapon = "기본 검";
        selectedCharacter = null;

        // 2) PlayerPrefs 중 '세션 전용' 키만 정리
        //    - 여기서는 예시로 CurrentStage / SelectedWeapon만 삭제
        //    - 추후 세션용 키(run_* 등)를 사용한다면 여기에 추가
        PlayerPrefs.DeleteKey("CurrentStage");
        PlayerPrefs.DeleteKey("SelectedWeapon");
        // 예: PlayerPrefs.DeleteKey("run_inventory");
        // 예: PlayerPrefs.DeleteKey("run_relics");
        // 예: PlayerPrefs.DeleteKey("run_seed");
        PlayerPrefs.Save();

        // ⚠ 영구 메타(예: meta_currency, permanent_upgrades, unlocks 등)는 절대 건드리지 않음
    }
}
