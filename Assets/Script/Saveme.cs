using System.IO;
using UnityEngine;
using Game.CharacterSystem.UI;

/// <summary>
/// JSON 저장/불러오기 기본 구조
/// </summary>
public class SaveManager : MonoBehaviour
{
    // 저장할 데이터를 담는 클래스 (원하는 변수 넣기)
    [System.Serializable]
    public class SaveData
    {
        // 예시: public int playerHp;
        // 예시: public string playerName;
    }

    private string savePath;

    void Awake()
    {
        // 저장 경로 (플랫폼별로 Application.persistentDataPath 사용)
        savePath = Path.Combine(Application.persistentDataPath, "save.json");
        Debug.Log("Save Path: " + savePath);
    }

    /// <summary>
    /// 데이터 저장
    /// </summary>
    public void Save()
    {
        SaveData data = new SaveData();
        // TODO: data 안에 원하는 값 넣기
        // data.playerHp = 100;

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(savePath, json);
        Debug.Log("저장 완료");
    }

    /// <summary>
    /// 데이터 불러오기
    /// </summary>
    public SaveData Load()
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
            Debug.LogWarning("저장 파일 없음");
            return null;
        }
    }

    /// <summary>
    /// 저장 파일 삭제 (선택 사항)
    /// </summary>
    public void DeleteSave()
    {
        if (File.Exists(savePath))
        {
            File.Delete(savePath);
            Debug.Log("저장 파일 삭제 완료");
        }
    }
}
