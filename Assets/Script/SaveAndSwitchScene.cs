using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.IO;

public class SaveAndSwitchScene : MonoBehaviour
{
    public Button switchButton;
    public string targetSceneName = "SceneB";

    void Start()
    {
        switchButton.onClick.AddListener(OnSwitchScene);
    }

    void OnSwitchScene()
    {
        // 현재 씬 이름 저장
        PlayerPrefs.SetString("LastScene", SceneManager.GetActiveScene().name);
        PlayerPrefs.Save();

        // (예시) 파일 저장
        SaveData();

        // 씬 이동
        SceneManager.LoadScene(targetSceneName);
    }

    void SaveData()
    {
        string savePath = Application.persistentDataPath + "/savedata.txt";
        File.WriteAllText(savePath, "저장 시간: " + System.DateTime.Now.ToString());
        Debug.Log("저장 완료: " + savePath);
    }
}
