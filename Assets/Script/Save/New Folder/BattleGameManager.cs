using UnityEngine;
using UnityEngine.SceneManagement;

public class BattleGameManager : MonoBehaviour
{
    public string playerName = "용사";
    public int playerHP = 100;

    void Update()
    {
        // 예시: ESC키 누르면 마을씬으로 이동하면서 저장
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SaveAndExitBattle();
        }
    }

    public void SaveAndExitBattle()
    {
        // 저장 데이터 만들기
        SaveData data = new SaveData();
        data.playerName = playerName;
        data.playerHP = playerHP;
        data.currentScene = "BattleScene"; // 현재 씬 이름 기록

        // 저장 실행
        SaveSystem.Save(data);

        // 다른 씬으로 이동
        SceneManager.LoadScene("TownScene");
    }
}
