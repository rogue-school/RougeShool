using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void NewGame()
    {
        // 그냥 새 게임 시작
        SceneManager.LoadScene("BattleScene");
    }

    public void ContinueGame()
    {
        if (SaveSystem.SaveFileExists())
        {
            SaveData data = SaveSystem.Load();

            // 불러온 데이터로 원하는 값 세팅 가능
            Debug.Log("플레이어 이름: " + data.playerName);
            Debug.Log("체력: " + data.playerHP);

            // 저장된 씬으로 이동
            SceneManager.LoadScene(data.currentScene);
        }
        else
        {
            Debug.LogWarning("저장된 데이터가 없어 새 게임을 시작합니다.");
            NewGame();
        }
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
