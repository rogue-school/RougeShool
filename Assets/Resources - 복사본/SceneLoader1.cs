using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader1 : MonoBehaviour
{
    public void GoToGame()
    {
        SceneManager.LoadScene("GAME OVER");
    }
}
