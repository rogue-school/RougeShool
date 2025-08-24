using UnityEngine;
using System.IO;

public static class GameDataManager
{
    static string savePath = Application.persistentDataPath + "/save.json";

    public static void SavePlayer(Vector2 position, float hp)
    {
        PlayerData data = new PlayerData
        {
            x = position.x,
            y = position.y,
            hp = hp
        };

        string json = JsonUtility.ToJson(data);
        File.WriteAllText(savePath, json);
        Debug.Log("저장 완료: " + savePath);
    }

    public static PlayerData LoadPlayer()
    {
        if (!File.Exists(savePath))
        {
            Debug.LogWarning("저장된 데이터가 없습니다.");
            return null;
        }

        string json = File.ReadAllText(savePath);
        return JsonUtility.FromJson<PlayerData>(json);
    }
}
