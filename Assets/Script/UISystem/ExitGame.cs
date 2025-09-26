using UnityEngine;

public class ExitGame : MonoBehaviour
{
    public void QuitGame()
    {
        Debug.Log("게임을 종료합니다.");
        Application.Quit(); // 실제 앱 종료
    }
}