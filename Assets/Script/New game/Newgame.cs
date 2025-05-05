using UnityEngine;
using UnityEngine.SceneManagement;

public class Buttonsword : MonoBehaviour
{
    public string sceneToLoad; // Inspector에서 지정 가능

    public void OnButtonClicked()
    {
        if (!string.IsNullOrEmpty(sceneToLoad))
        {
            Debug.Log("버튼을 클릭했습니다. 씬 이동 중: " + sceneToLoad);
            SceneManager.LoadScene(sceneToLoad);
        }
        else
        {
            Debug.Log("버튼을 클릭했습니다. 이동할 씬 이름이 설정되지 않았습니다.");
        }
    }
}