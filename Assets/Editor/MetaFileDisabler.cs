using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text.RegularExpressions;
using System.Linq;

namespace Game.Editor
{
    /// <summary>
    /// Meta 파일을 조작하여 스크립트를 완전히 비활성화하는 시스템
    /// 가장 확실한 방법으로 Unity가 스크립트를 인식하지 못하게 합니다.
    /// </summary>
    public class MetaFileDisabler : AssetPostprocessor
    {
        private const string DISABLED_FOLDER_PATH = "Assets/Script/Disabled";
        private const string BACKUP_SUFFIX = ".disabled_backup";
        
        /// <summary>
        /// 스크립트가 Disabled 폴더로 이동될 때 자동으로 비활성화합니다.
        /// </summary>
        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, 
            string[] movedAssets, string[] movedFromAssetPaths)
        {
            for (int i = 0; i < movedAssets.Length; i++)
            {
                string newPath = movedAssets[i];
                string oldPath = movedFromAssetPaths[i];
                
                // Disabled 폴더로 이동된 경우
                if (Path.GetExtension(newPath) == ".cs" && 
                    newPath.Contains(DISABLED_FOLDER_PATH) && 
                    !oldPath.Contains(DISABLED_FOLDER_PATH))
                {
                    DisableScriptViaMeta(newPath);
                }
                
                // Disabled 폴더에서 다른 폴더로 이동된 경우
                if (Path.GetExtension(newPath) == ".cs" && 
                    !newPath.Contains(DISABLED_FOLDER_PATH) && 
                    oldPath.Contains(DISABLED_FOLDER_PATH))
                {
                    EnableScriptViaMeta(newPath);
                }
            }
        }
        
        /// <summary>
        /// Meta 파일을 조작하여 스크립트를 비활성화합니다.
        /// </summary>
        public static void DisableScriptViaMeta(string scriptPath)
        {
            string metaPath = scriptPath + ".meta";
            
            if (!File.Exists(metaPath))
            {
                Debug.LogWarning($"[MetaFileDisabler] Meta 파일을 찾을 수 없습니다: {metaPath}");
                return;
            }
            
            // 1. 원본 스크립트를 백업
            BackupScript(scriptPath);
            
            // 2. 스크립트 파일을 임시로 이동
            string tempPath = scriptPath + ".temp_disabled";
            File.Move(scriptPath, tempPath);
            
            // 3. Meta 파일을 수정하여 스크립트 타입을 변경
            ModifyMetaFile(metaPath, true);
            
            // 4. 스크립트 파일을 다시 이동 (확장자 변경)
            string disabledPath = scriptPath.Replace(".cs", ".cs.disabled");
            File.Move(tempPath, disabledPath);
            
            Debug.Log($"[MetaFileDisabler] 스크립트를 비활성화했습니다: {scriptPath}");
        }
        
        /// <summary>
        /// Meta 파일을 조작하여 스크립트를 활성화합니다.
        /// </summary>
        public static void EnableScriptViaMeta(string scriptPath)
        {
            string metaPath = scriptPath + ".meta";
            
            if (!File.Exists(metaPath))
            {
                Debug.LogWarning($"[MetaFileDisabler] Meta 파일을 찾을 수 없습니다: {metaPath}");
                return;
            }
            
            // 1. 비활성화된 스크립트 파일 찾기
            string disabledPath = scriptPath.Replace(".cs", ".cs.disabled");
            if (File.Exists(disabledPath))
            {
                // 2. 스크립트 파일을 원래 위치로 이동
                File.Move(disabledPath, scriptPath);
                
                // 3. Meta 파일을 원래대로 복원
                ModifyMetaFile(metaPath, false);
                
                Debug.Log($"[MetaFileDisabler] 스크립트를 활성화했습니다: {scriptPath}");
            }
        }
        
        /// <summary>
        /// 스크립트를 백업합니다.
        /// </summary>
        private static void BackupScript(string scriptPath)
        {
            string backupPath = scriptPath + BACKUP_SUFFIX;
            if (!File.Exists(backupPath))
            {
                File.Copy(scriptPath, backupPath);
            }
        }
        
        /// <summary>
        /// Meta 파일을 수정합니다.
        /// </summary>
        private static void ModifyMetaFile(string metaPath, bool disable)
        {
            string metaContent = File.ReadAllText(metaPath);
            
            if (disable)
            {
                // 스크립트를 비활성화하는 메타 파일 수정
                metaContent = metaContent.Replace(
                    "MonoImporter:",
                    "MonoImporter:\n  disabled: 1"
                );
                
                // 스크립트 타입을 변경하여 Unity가 인식하지 못하게 함
                metaContent = metaContent.Replace(
                    "script: {fileID: 11500000",
                    "script: {fileID: 0"
                );
            }
            else
            {
                // 스크립트를 활성화하는 메타 파일 복원
                metaContent = metaContent.Replace("disabled: 1\n  ", "");
                metaContent = metaContent.Replace(
                    "script: {fileID: 0",
                    "script: {fileID: 11500000"
                );
            }
            
            File.WriteAllText(metaPath, metaContent);
        }
    }
    
    /// <summary>
    /// Meta 파일 기반 비활성화 관리 메뉴
    /// </summary>
    public class MetaFileDisablerMenu
    {
        [MenuItem("Tools/Meta Disabler/Disable All Disabled Scripts")]
        public static void DisableAllDisabledScripts()
        {
            string[] scripts = AssetDatabase.FindAssets("t:Script", new[] { "Assets/Script/Disabled" });
            
            foreach (string scriptGuid in scripts)
            {
                string scriptPath = AssetDatabase.GUIDToAssetPath(scriptGuid);
                MetaFileDisabler.DisableScriptViaMeta(scriptPath);
            }
            
            AssetDatabase.Refresh();
            Debug.Log($"[MetaFileDisabler] {scripts.Length}개의 스크립트를 비활성화했습니다.");
        }
        
        [MenuItem("Tools/Meta Disabler/Enable All Disabled Scripts")]
        public static void EnableAllDisabledScripts()
        {
            string[] scripts = AssetDatabase.FindAssets("t:Script", new[] { "Assets/Script/Disabled" });
            
            foreach (string scriptGuid in scripts)
            {
                string scriptPath = AssetDatabase.GUIDToAssetPath(scriptGuid);
                MetaFileDisabler.EnableScriptViaMeta(scriptPath);
            }
            
            AssetDatabase.Refresh();
            Debug.Log($"[MetaFileDisabler] {scripts.Length}개의 스크립트를 활성화했습니다.");
        }
        
        [MenuItem("Tools/Meta Disabler/Cleanup Backup Files")]
        public static void CleanupBackupFiles()
        {
            string[] allAssets = AssetDatabase.FindAssets("", new[] { "Assets/Script/Disabled" });
            string[] backupFiles = allAssets
                .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
                .Where(path => path.EndsWith(".disabled_backup"))
                .ToArray();
            
            foreach (string backupFile in backupFiles)
            {
                AssetDatabase.DeleteAsset(backupFile);
            }
            
            Debug.Log($"[MetaFileDisabler] {backupFiles.Length}개의 백업 파일을 정리했습니다.");
        }
    }
}
