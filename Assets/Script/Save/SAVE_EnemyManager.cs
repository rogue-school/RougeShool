using UnityEngine;

public class SAVE_EnemyManager : MonoBehaviour
{
    public static SAVE_EnemyManager Instance;

    public SAVE_EnemyCharacter CurrentEnemy;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void RegisterNewEnemy()
    {
        CurrentEnemy = new SAVE_EnemyCharacter("Goblin", 3, 100);
        Debug.Log("[EnemyManager] 새 적 등록: Goblin Lv3 HP100");
    }

    public void SaveEnemy()
    {
        if (CurrentEnemy != null)
        {
            SAVE_EnemySaveSystem.Save(CurrentEnemy);
        }
    }

    public void LoadEnemy()
    {
        SAVE_EnemyCharacter loaded = SAVE_EnemySaveSystem.Load();
        if (loaded != null)
        {
            CurrentEnemy = loaded;
            Debug.Log($"[EnemyManager] 적 불러오기 완료: {loaded.Name} Lv{loaded.Level} HP{loaded.HP}");
        }
    }

    public string GetEnemyInfo()
    {
        if (CurrentEnemy == null) return "적 없음";
        return $"적: {CurrentEnemy.Name} / 레벨: {CurrentEnemy.Level} / HP: {CurrentEnemy.HP}";
    }
}
