using UnityEngine;
using UnityEngine.SceneManagement;
using Game.Save;

public class BattleExitExample : MonoBehaviour
{
    public BattleSaveController controller;
    public string mainMenuSceneName = "MainMenu";

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SaveAndExit();
        }
    }

    public void SaveAndExit()
    {
        var data = controller.Capture();
        if (data != null)
        {
            SaveSystem.Save(data);
        }
        SceneManager.LoadScene(mainMenuSceneName);
    }
}
