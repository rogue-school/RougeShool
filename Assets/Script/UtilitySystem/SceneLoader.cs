using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Game.Utility.GameFlow;

public class SceneLoader : MonoBehaviour, ISceneLoader
{
    [SerializeField] private Button targetButton;
    [SerializeField] private string sceneToLoad;

    private void Start()
    {
        if (targetButton != null)
        {
            targetButton.onClick.AddListener(OnButtonClicked);
        }
        else
        {
            Debug.LogWarning("SceneLoader: targetButton이 설정되지 않았습니다.");
        }
    }

    private void OnButtonClicked()
    {
        if (!string.IsNullOrEmpty(sceneToLoad))
        {
            LoadScene(sceneToLoad);
        }
        else
        {
            Debug.LogWarning("이동할 씬 이름이 설정되지 않았습니다.");
        }
    }

    public void LoadScene(string sceneName)
    {
        Debug.Log($"[SceneLoader] 씬 이동 중: {sceneName}");
        SceneManager.LoadScene(sceneName);
    }
}
