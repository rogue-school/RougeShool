using System.IO;
using UnityEngine;

public static class SAVE_EnemySaveSystem
{
    private static readonly string SavePath = Application.persistentDataPath + "/enemy.json";

    public static void Save(SAVE_EnemyCharacter enemy)
    {
        SAVE_EnemyData data = new SAVE_EnemyData
        {
            name = enemy.Name,
            level = enemy.Level,
            hp = enemy.HP
        };

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(SavePath, json);
        Debug.Log("[SaveSystem] 적 저장 완료");
    }

    public static SAVE_EnemyCharacter Load()
    {
        if (!File.Exists(SavePath))
        {
            Debug.LogWarning("[SaveSystem] 저장된 적 없음");
            return null;
        }

        string json = File.ReadAllText(SavePath);
        SAVE_EnemyData data = JsonUtility.FromJson<SAVE_EnemyData>(json);
        return new SAVE_EnemyCharacter(data.name, data.level, data.hp);
    }
}
