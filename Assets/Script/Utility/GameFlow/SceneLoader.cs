using UnityEngine.SceneManagement;

namespace Game.Utility.GameFlow
{
    public class SceneLoader : ISceneLoader
    {
        public void LoadScene(string sceneName)
        {
            SceneManager.LoadScene(sceneName);
        }
    }
}
