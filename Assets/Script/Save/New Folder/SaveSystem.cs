using System.IO;
using UnityEngine;

namespace Game.Save
{
    /// <summary>
    /// JSON 저장/불러오기 유틸
    /// </summary>
    public static class SaveSystem
    {
        private static readonly string SavePath =
            Path.Combine(Application.persistentDataPath, "battle_save.json");

        public static void Save(BattleSaveData data)
        {
            var json = JsonUtility.ToJson(data, prettyPrint: true);
            File.WriteAllText(SavePath, json);
            Debug.Log($"[SaveSystem] 저장 완료: {SavePath}");
        }

        public static BattleSaveData Load()
        {
            if (!File.Exists(SavePath))
            {
                Debug.LogWarning("[SaveSystem] 저장 파일이 없습니다.");
                return null;
            }

            var json = File.ReadAllText(SavePath);
            var data = JsonUtility.FromJson<BattleSaveData>(json);
            Debug.Log($"[SaveSystem] 불러오기 완료: {SavePath}");
            return data;
        }

        public static bool Exists() => File.Exists(SavePath);

        public static void Delete()
        {
            if (File.Exists(SavePath))
            {
                File.Delete(SavePath);
                Debug.Log("[SaveSystem] 저장 파일 삭제");
            }
        }
    }
}
