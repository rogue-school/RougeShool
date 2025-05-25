using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // Button 사용을 위해 필요

public class SceneLoader : MonoBehaviour
{
    [SerializeField] private Button targetButton; // 버튼 연결 (Inspector에서 지정)
    [SerializeField] private string sceneToLoad;  // 이동할 씬 이름

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
            Debug.Log("버튼을 클릭했습니다. 씬 이동 중: " + sceneToLoad);
            SceneManager.LoadScene(sceneToLoad);
        }
        else
        {
            Debug.LogWarning("이동할 씬 이름이 설정되지 않았습니다.");
        }
    }
}
