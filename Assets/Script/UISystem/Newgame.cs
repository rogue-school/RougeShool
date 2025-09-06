using UnityEngine;
using UnityEngine.SceneManagement;

public class Playbutton : MonoBehaviour
{
    public string sceneToLoad; // Inspector���� ���� ����

    public void OnButtonClicked()
    {
        if (!string.IsNullOrEmpty(sceneToLoad))
        {
            Debug.Log("��ư�� Ŭ���߽��ϴ�. �� �̵� ��: " + sceneToLoad);
            SceneManager.LoadScene(sceneToLoad);
        }
        else
        {
            Debug.Log("��ư�� Ŭ���߽��ϴ�. �̵��� �� �̸��� �������� �ʾҽ��ϴ�.");
        }
    }
}