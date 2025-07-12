using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ReturnToPreviousScene : MonoBehaviour
{
    public Button returnButton;

    void Start()
    {
        returnButton.onClick.AddListener(ReturnToLastScene);
    }

    void ReturnToLastScene()
    {
        if (PlayerPrefs.HasKey("LastScene"))
        {
            string lastScene = PlayerPrefs.GetString("LastScene");
            SceneManager.LoadScene(lastScene);
        }
        else
        {
            Debug.LogWarning("이전 씬 정보가 없습니다.");
        }
    }
}
