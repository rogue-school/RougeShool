using System.IO;
using UnityEngine;

public static class SaveSystem
{
    private static string savePath = Application.persistentDataPath + "/save.json";

    // 저장
    public static void Save(SaveData data)
    {
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(savePath, json);
        Debug.Log("저장 완료: " + savePath);
    }

    // 불러오기
    public static SaveData Load()
    {
        if (File.Exists(savePath))
        {
            string json = File.ReadAllText(savePath);
            SaveData data = JsonUtility.FromJson<SaveData>(json);
            Debug.Log("불러오기 완료");
            return data;
        }
        else
        {
            Debug.LogWarning("세이브 파일 없음");
            return null;
        }
    }

    // 저장 파일이 있는지 확인
    public static bool SaveFileExists()
    {
        return File.Exists(savePath);
    }
}
