using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SAVE_EnemyUIController : MonoBehaviour
{
    public Button registerButton;
    public Button saveButton;
    public Button loadButton;
    public Button toSceneBButton;
    public Button backToSceneAButton;
    public Text enemyInfoText;

    void Start()
    {
        if (registerButton != null) registerButton.onClick.AddListener(() => SAVE_EnemyManager.Instance.RegisterNewEnemy());
        if (saveButton != null) saveButton.onClick.AddListener(() => SAVE_EnemyManager.Instance.SaveEnemy());
        if (loadButton != null) loadButton.onClick.AddListener(() => SAVE_EnemyManager.Instance.LoadEnemy());
        if (toSceneBButton != null) toSceneBButton.onClick.AddListener(() => SceneManager.LoadScene("SceneB"));
        if (backToSceneAButton != null) backToSceneAButton.onClick.AddListener(() => SceneManager.LoadScene("SceneA"));
    }

    void Update()
    {
        if (enemyInfoText != null)
        {
            enemyInfoText.text = SAVE_EnemyManager.Instance.GetEnemyInfo();
        }
    }
}
