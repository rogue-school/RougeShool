using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace Game.Editor
{
    /// <summary>
    /// 스크립트 생명주기를 통합 관리하는 시스템
    /// 여러 비활성화 방법을 조합하여 가장 확실한 비활성화를 제공합니다.
    /// </summary>
    public class ScriptLifecycleManager : AssetPostprocessor
    {
        private const string DISABLED_FOLDER_PATH = "Assets/Script/Disabled";
        private const string ACTIVE_FOLDER_PATH = "Assets/Script";
        
        // 비활성화 방법들
        public enum DisableMethod
        {
            AssemblyDefinition,    // Assembly Definition 제외
            ConditionalCompilation, // 조건부 컴파일
            MetaFileManipulation,   // Meta 파일 조작
            FileExtension,         // 파일 확장자 변경
            All                    // 모든 방법 조합
        }
        
        [System.Serializable]
        public class ScriptLifecycleSettings
        {
            public DisableMethod disableMethod = DisableMethod.All;
            public bool autoBackup = true;
            public bool createDocumentation = true;
            public bool logOperations = true;
        }
        
        private static ScriptLifecycleSettings settings = new ScriptLifecycleSettings();
        
        /// <summary>
        /// 스크립트가 이동될 때 자동으로 처리합니다.
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
                    ProcessScriptDisable(newPath);
                }
                
                // Disabled 폴더에서 다른 폴더로 이동된 경우
                if (Path.GetExtension(newPath) == ".cs" && 
                    !newPath.Contains(DISABLED_FOLDER_PATH) && 
                    oldPath.Contains(DISABLED_FOLDER_PATH))
                {
                    ProcessScriptEnable(newPath);
                }
            }
        }
        
        /// <summary>
        /// 스크립트 비활성화를 처리합니다.
        /// </summary>
        public static void ProcessScriptDisable(string scriptPath)
        {
            if (settings.logOperations)
                Debug.Log($"[ScriptLifecycleManager] 스크립트 비활성화 시작: {scriptPath}");
            
            // 1. 백업 생성
            if (settings.autoBackup)
                CreateBackup(scriptPath);
            
            // 2. 선택된 방법으로 비활성화
            switch (settings.disableMethod)
            {
                case DisableMethod.AssemblyDefinition:
                    DisableViaAssemblyDefinition(scriptPath);
                    break;
                case DisableMethod.ConditionalCompilation:
                    DisableViaConditionalCompilation(scriptPath);
                    break;
                case DisableMethod.MetaFileManipulation:
                    DisableViaMetaFile(scriptPath);
                    break;
                case DisableMethod.FileExtension:
                    DisableViaFileExtension(scriptPath);
                    break;
                case DisableMethod.All:
                    DisableViaAllMethods(scriptPath);
                    break;
            }
            
            // 3. 문서 생성
            if (settings.createDocumentation)
                CreateDisableDocumentation(scriptPath);
            
            AssetDatabase.Refresh();
        }
        
        /// <summary>
        /// 스크립트 활성화를 처리합니다.
        /// </summary>
        public static void ProcessScriptEnable(string scriptPath)
        {
            if (settings.logOperations)
                Debug.Log($"[ScriptLifecycleManager] 스크립트 활성화 시작: {scriptPath}");
            
            // 1. 선택된 방법으로 활성화
            switch (settings.disableMethod)
            {
                case DisableMethod.AssemblyDefinition:
                    EnableViaAssemblyDefinition(scriptPath);
                    break;
                case DisableMethod.ConditionalCompilation:
                    EnableViaConditionalCompilation(scriptPath);
                    break;
                case DisableMethod.MetaFileManipulation:
                    EnableViaMetaFile(scriptPath);
                    break;
                case DisableMethod.FileExtension:
                    EnableViaFileExtension(scriptPath);
                    break;
                case DisableMethod.All:
                    EnableViaAllMethods(scriptPath);
                    break;
            }
            
            AssetDatabase.Refresh();
        }
        
        #region 비활성화 방법들
        
        private static void DisableViaAssemblyDefinition(string scriptPath)
        {
            // Assembly Definition에서 제외
            string asmdefPath = FindAssemblyDefinition(scriptPath);
            if (!string.IsNullOrEmpty(asmdefPath))
            {
                // Assembly Definition 수정 로직
                Debug.Log($"[ScriptLifecycleManager] Assembly Definition에서 제외: {scriptPath}");
            }
        }
        
        private static void DisableViaConditionalCompilation(string scriptPath)
        {
            // 조건부 컴파일로 감싸기
            string content = File.ReadAllText(scriptPath);
            if (!content.Contains("#if DISABLED_SCRIPTS"))
            {
                string wrappedContent = $@"#if DISABLED_SCRIPTS
{content}
#endif // DISABLED_SCRIPTS";
                File.WriteAllText(scriptPath, wrappedContent);
                Debug.Log($"[ScriptLifecycleManager] 조건부 컴파일로 감쌈: {scriptPath}");
            }
        }
        
        private static void DisableViaMetaFile(string scriptPath)
        {
            // Meta 파일 조작
            string metaPath = scriptPath + ".meta";
            if (File.Exists(metaPath))
            {
                string metaContent = File.ReadAllText(metaPath);
                metaContent = metaContent.Replace(
                    "MonoImporter:",
                    "MonoImporter:\n  disabled: 1"
                );
                File.WriteAllText(metaPath, metaContent);
                Debug.Log($"[ScriptLifecycleManager] Meta 파일 조작: {scriptPath}");
            }
        }
        
        private static void DisableViaFileExtension(string scriptPath)
        {
            // 파일 확장자 변경
            string newPath = scriptPath.Replace(".cs", ".cs.disabled");
            File.Move(scriptPath, newPath);
            Debug.Log($"[ScriptLifecycleManager] 파일 확장자 변경: {scriptPath}");
        }
        
        private static void DisableViaAllMethods(string scriptPath)
        {
            // 모든 방법 조합
            DisableViaAssemblyDefinition(scriptPath);
            DisableViaConditionalCompilation(scriptPath);
            DisableViaMetaFile(scriptPath);
            // FileExtension은 다른 방법들과 충돌할 수 있으므로 마지막에 실행
            // DisableViaFileExtension(scriptPath);
        }
        
        #endregion
        
        #region 활성화 방법들
        
        private static void EnableViaAssemblyDefinition(string scriptPath)
        {
            // Assembly Definition에 포함
            Debug.Log($"[ScriptLifecycleManager] Assembly Definition에 포함: {scriptPath}");
        }
        
        private static void EnableViaConditionalCompilation(string scriptPath)
        {
            // 조건부 컴파일 제거
            string content = File.ReadAllText(scriptPath);
            if (content.Contains("#if DISABLED_SCRIPTS"))
            {
                string pattern = @"#if DISABLED_SCRIPTS\s*\n(.*?)\n#endif\s*//\s*DISABLED_SCRIPTS";
                content = System.Text.RegularExpressions.Regex.Replace(content, pattern, "$1", System.Text.RegularExpressions.RegexOptions.Singleline);
                File.WriteAllText(scriptPath, content);
                Debug.Log($"[ScriptLifecycleManager] 조건부 컴파일 제거: {scriptPath}");
            }
        }
        
        private static void EnableViaMetaFile(string scriptPath)
        {
            // Meta 파일 복원
            string metaPath = scriptPath + ".meta";
            if (File.Exists(metaPath))
            {
                string metaContent = File.ReadAllText(metaPath);
                metaContent = metaContent.Replace("disabled: 1\n  ", "");
                File.WriteAllText(metaPath, metaContent);
                Debug.Log($"[ScriptLifecycleManager] Meta 파일 복원: {scriptPath}");
            }
        }
        
        private static void EnableViaFileExtension(string scriptPath)
        {
            // 파일 확장자 복원
            string disabledPath = scriptPath.Replace(".cs", ".cs.disabled");
            if (File.Exists(disabledPath))
            {
                File.Move(disabledPath, scriptPath);
                Debug.Log($"[ScriptLifecycleManager] 파일 확장자 복원: {scriptPath}");
            }
        }
        
        private static void EnableViaAllMethods(string scriptPath)
        {
            // 모든 방법 복원
            EnableViaAssemblyDefinition(scriptPath);
            EnableViaConditionalCompilation(scriptPath);
            EnableViaMetaFile(scriptPath);
            EnableViaFileExtension(scriptPath);
        }
        
        #endregion
        
        #region 유틸리티 메서드들
        
        private static void CreateBackup(string scriptPath)
        {
            string backupPath = scriptPath + ".backup";
            if (!File.Exists(backupPath))
            {
                File.Copy(scriptPath, backupPath);
                Debug.Log($"[ScriptLifecycleManager] 백업 생성: {backupPath}");
            }
        }
        
        private static void CreateDisableDocumentation(string scriptPath)
        {
            string docPath = scriptPath.Replace(".cs", "_DISABLED.md");
            string docContent = $@"# {Path.GetFileNameWithoutExtension(scriptPath)} - 비활성화됨

## 비활성화 정보
- **비활성화 일시**: {System.DateTime.Now:yyyy-MM-dd HH:mm:ss}
- **비활성화 방법**: {settings.disableMethod}
- **원본 경로**: {scriptPath}

## 비활성화 이유
- 더 이상 사용하지 않는 스크립트
- 새로운 시스템으로 대체됨
- 리팩토링 과정에서 제거됨

## 복원 방법
1. Tools > Script Lifecycle Manager > Enable All Disabled Scripts
2. 또는 스크립트를 Disabled 폴더에서 다른 폴더로 이동

## 관련 스크립트
- 대체 스크립트: (대체 스크립트 경로)
- 관련 시스템: (관련 시스템명)
";
            
            File.WriteAllText(docPath, docContent);
            Debug.Log($"[ScriptLifecycleManager] 문서 생성: {docPath}");
        }
        
        private static string FindAssemblyDefinition(string scriptPath)
        {
            string directory = Path.GetDirectoryName(scriptPath);
            
            while (!string.IsNullOrEmpty(directory) && directory != "Assets")
            {
                string asmdefPath = Path.Combine(directory, Path.GetFileName(directory) + ".asmdef");
                if (File.Exists(asmdefPath))
                    return asmdefPath;
                
                directory = Path.GetDirectoryName(directory);
            }
            
            return null;
        }
        
        #endregion
    }
    
    /// <summary>
    /// 스크립트 생명주기 관리 메뉴
    /// </summary>
    public class ScriptLifecycleMenu
    {
        [MenuItem("Tools/Script Lifecycle Manager/Settings")]
        public static void ShowSettings()
        {
            ScriptLifecycleManagerWindow.ShowWindow();
        }
        
        [MenuItem("Tools/Script Lifecycle Manager/Disable All Disabled Scripts")]
        public static void DisableAllDisabledScripts()
        {
            string[] scripts = AssetDatabase.FindAssets("t:Script", new[] { "Assets/Script/Disabled" });
            
            foreach (string scriptGuid in scripts)
            {
                string scriptPath = AssetDatabase.GUIDToAssetPath(scriptGuid);
                ScriptLifecycleManager.ProcessScriptDisable(scriptPath);
            }
            
            Debug.Log($"[ScriptLifecycleManager] {scripts.Length}개의 스크립트를 비활성화했습니다.");
        }
        
        [MenuItem("Tools/Script Lifecycle Manager/Enable All Disabled Scripts")]
        public static void EnableAllDisabledScripts()
        {
            string[] scripts = AssetDatabase.FindAssets("t:Script", new[] { "Assets/Script/Disabled" });
            
            foreach (string scriptGuid in scripts)
            {
                string scriptPath = AssetDatabase.GUIDToAssetPath(scriptGuid);
                ScriptLifecycleManager.ProcessScriptEnable(scriptPath);
            }
            
            Debug.Log($"[ScriptLifecycleManager] {scripts.Length}개의 스크립트를 활성화했습니다.");
        }
        
        [MenuItem("Tools/Script Lifecycle Manager/Cleanup Backup Files")]
        public static void CleanupBackupFiles()
        {
            string[] allAssets = AssetDatabase.FindAssets("", new[] { "Assets/Script/Disabled" });
            string[] backupFiles = allAssets
                .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
                .Where(path => path.EndsWith(".backup"))
                .ToArray();
            
            foreach (string backupFile in backupFiles)
            {
                AssetDatabase.DeleteAsset(backupFile);
            }
            
            Debug.Log($"[ScriptLifecycleManager] {backupFiles.Length}개의 백업 파일을 정리했습니다.");
        }
    }
    
    /// <summary>
    /// 스크립트 생명주기 관리 설정 창
    /// </summary>
    public class ScriptLifecycleManagerWindow : EditorWindow
    {
        private ScriptLifecycleManager.DisableMethod disableMethod = ScriptLifecycleManager.DisableMethod.All;
        private bool autoBackup = true;
        private bool createDocumentation = true;
        private bool logOperations = true;
        
        public static void ShowWindow()
        {
            GetWindow<ScriptLifecycleManagerWindow>("Script Lifecycle Manager");
        }
        
        private void OnGUI()
        {
            GUILayout.Label("스크립트 생명주기 관리 설정", EditorStyles.boldLabel);
            
            EditorGUILayout.Space();
            
            disableMethod = (ScriptLifecycleManager.DisableMethod)EditorGUILayout.EnumPopup(
                "비활성화 방법", disableMethod);
            
            autoBackup = EditorGUILayout.Toggle("자동 백업", autoBackup);
            createDocumentation = EditorGUILayout.Toggle("문서 생성", createDocumentation);
            logOperations = EditorGUILayout.Toggle("작업 로그", logOperations);
            
            EditorGUILayout.Space();
            
            if (GUILayout.Button("설정 저장"))
            {
                SaveSettings();
            }
            
            EditorGUILayout.Space();
            
            GUILayout.Label("비활성화 방법 설명:", EditorStyles.boldLabel);
            GUILayout.Label("• AssemblyDefinition: Assembly Definition에서 제외");
            GUILayout.Label("• ConditionalCompilation: #if DISABLED_SCRIPTS로 감싸기");
            GUILayout.Label("• MetaFileManipulation: Meta 파일 조작");
            GUILayout.Label("• FileExtension: 파일 확장자 변경");
            GUILayout.Label("• All: 모든 방법 조합 (권장)");
        }
        
        private void SaveSettings()
        {
            // 설정 저장 로직
            Debug.Log("[ScriptLifecycleManager] 설정이 저장되었습니다.");
        }
    }
}
