using System.IO;
using UnityEngine;

namespace Game.Save
{
    public static class SaveSystem
    {
        private static readonly string PathFile =
            System.IO.Path.Combine(Application.persistentDataPath, "battle_save.json");

        public static void Save(BattleSaveData data)
        {
            var json = JsonUtility.ToJson(data, true);
            File.WriteAllText(PathFile, json);
            Debug.Log($"[SaveSystem] 저장 완료: {PathFile}");
        }

        public static BattleSaveData Load()
        {
            if (!File.Exists(PathFile))
            {
                Debug.LogWarning("[SaveSystem] 저장 파일 없음");
                return null;
            }
            var json = File.ReadAllText(PathFile);
            return JsonUtility.FromJson<BattleSaveData>(json);
        }

        public static bool Exists() => File.Exists(PathFile);
        public static void Delete()
        {
            if (File.Exists(PathFile)) File.Delete(PathFile);
        }
    }
}
