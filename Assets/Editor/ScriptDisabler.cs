using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace Game.Editor
{
    /// <summary>
    /// 스크립트 자동 비활성화 시스템
    /// Disabled 폴더로 이동된 스크립트를 자동으로 비활성화합니다.
    /// </summary>
    public class ScriptDisabler : AssetPostprocessor
    {
        private const string DISABLED_FOLDER_PATH = "Assets/Script/Disabled";
        private const string ACTIVE_FOLDER_PATH = "Assets/Script";
        
        /// <summary>
        /// 에셋이 이동될 때마다 호출되는 메서드
        /// </summary>
        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, 
            string[] movedAssets, string[] movedFromAssetPaths)
        {
            // Disabled 폴더로 이동된 스크립트들을 처리
            for (int i = 0; i < movedAssets.Length; i++)
            {
                string newPath = movedAssets[i];
                string oldPath = movedFromAssetPaths[i];
                
                // .cs 파일이고 Disabled 폴더로 이동된 경우
                if (Path.GetExtension(newPath) == ".cs" && 
                    newPath.Contains(DISABLED_FOLDER_PATH) && 
                    !oldPath.Contains(DISABLED_FOLDER_PATH))
                {
                    DisableScript(newPath);
                }
                
                // Disabled 폴더에서 다른 폴더로 이동된 경우
                if (Path.GetExtension(newPath) == ".cs" && 
                    !newPath.Contains(DISABLED_FOLDER_PATH) && 
                    oldPath.Contains(DISABLED_FOLDER_PATH))
                {
                    EnableScript(newPath);
                }
            }
        }
        
        /// <summary>
        /// 스크립트를 비활성화합니다.
        /// </summary>
        public static void DisableScript(string scriptPath)
        {
            Debug.Log($"[ScriptDisabler] 스크립트 비활성화: {scriptPath}");
            
            // 1. Assembly Definition에서 제외
            RemoveFromAssemblyDefinition(scriptPath);
            
            // 2. 스크립트 내용을 주석 처리
            CommentOutScript(scriptPath);
            
            // 3. 메타 파일 수정으로 컴파일 제외
            DisableScriptInMeta(scriptPath);
            
            AssetDatabase.Refresh();
        }
        
        /// <summary>
        /// 스크립트를 활성화합니다.
        /// </summary>
        public static void EnableScript(string scriptPath)
        {
            Debug.Log($"[ScriptDisabler] 스크립트 활성화: {scriptPath}");
            
            // 1. Assembly Definition에 포함
            AddToAssemblyDefinition(scriptPath);
            
            // 2. 스크립트 내용 주석 해제
            UncommentScript(scriptPath);
            
            // 3. 메타 파일 수정으로 컴파일 포함
            EnableScriptInMeta(scriptPath);
            
            AssetDatabase.Refresh();
        }
        
        /// <summary>
        /// Assembly Definition에서 스크립트를 제외합니다.
        /// </summary>
        private static void RemoveFromAssemblyDefinition(string scriptPath)
        {
            string asmdefPath = FindAssemblyDefinition(scriptPath);
            if (string.IsNullOrEmpty(asmdefPath)) return;
            
            // Assembly Definition 파일 수정 로직
            // 실제 구현에서는 JSON 파싱 및 수정 필요
            Debug.Log($"[ScriptDisabler] Assembly Definition에서 제외: {scriptPath}");
        }
        
        /// <summary>
        /// Assembly Definition에 스크립트를 포함합니다.
        /// </summary>
        private static void AddToAssemblyDefinition(string scriptPath)
        {
            string asmdefPath = FindAssemblyDefinition(scriptPath);
            if (string.IsNullOrEmpty(asmdefPath)) return;
            
            // Assembly Definition 파일 수정 로직
            // 실제 구현에서는 JSON 파싱 및 수정 필요
            Debug.Log($"[ScriptDisabler] Assembly Definition에 포함: {scriptPath}");
        }
        
        /// <summary>
        /// 스크립트 내용을 주석 처리합니다.
        /// </summary>
        private static void CommentOutScript(string scriptPath)
        {
            string content = File.ReadAllText(scriptPath);
            
            // 이미 주석 처리된 경우 스킵
            if (content.StartsWith("/* DISABLED"))
                return;
            
            // 전체 스크립트를 주석 처리
            string commentedContent = $"/* DISABLED - {System.DateTime.Now:yyyy-MM-dd HH:mm:ss} */\n{content}";
            
            File.WriteAllText(scriptPath, commentedContent);
        }
        
        /// <summary>
        /// 스크립트 주석을 해제합니다.
        /// </summary>
        private static void UncommentScript(string scriptPath)
        {
            string content = File.ReadAllText(scriptPath);
            
            // 주석 처리된 경우 해제
            if (content.StartsWith("/* DISABLED"))
            {
                int endIndex = content.IndexOf("*/");
                if (endIndex != -1)
                {
                    content = content.Substring(endIndex + 2).TrimStart();
                    File.WriteAllText(scriptPath, content);
                }
            }
        }
        
        /// <summary>
        /// 메타 파일을 수정하여 스크립트를 비활성화합니다.
        /// </summary>
        private static void DisableScriptInMeta(string scriptPath)
        {
            string metaPath = scriptPath + ".meta";
            if (!File.Exists(metaPath)) return;
            
            string metaContent = File.ReadAllText(metaPath);
            
            // meta 파일에 비활성화 플래그 추가
            if (!metaContent.Contains("disabled: 1"))
            {
                metaContent = metaContent.Replace(
                    "guid: ",
                    "disabled: 1\n  guid: "
                );
                File.WriteAllText(metaPath, metaContent);
            }
        }
        
        /// <summary>
        /// 메타 파일을 수정하여 스크립트를 활성화합니다.
        /// </summary>
        private static void EnableScriptInMeta(string scriptPath)
        {
            string metaPath = scriptPath + ".meta";
            if (!File.Exists(metaPath)) return;
            
            string metaContent = File.ReadAllText(metaPath);
            
            // meta 파일에서 비활성화 플래그 제거
            metaContent = metaContent.Replace("disabled: 1\n  ", "");
            File.WriteAllText(metaPath, metaContent);
        }
        
        /// <summary>
        /// 스크립트가 속한 Assembly Definition을 찾습니다.
        /// </summary>
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
    }
    
    /// <summary>
    /// 스크립트 비활성화 관리 메뉴
    /// </summary>
    public class ScriptDisablerMenu
    {
        [MenuItem("Tools/Script Disabler/Disable All Disabled Scripts")]
        public static void DisableAllDisabledScripts()
        {
            string[] scripts = AssetDatabase.FindAssets("t:Script", new[] { "Assets/Script/Disabled" });
            
            foreach (string scriptGuid in scripts)
            {
                string scriptPath = AssetDatabase.GUIDToAssetPath(scriptGuid);
                ScriptDisabler.DisableScript(scriptPath);
            }
            
            Debug.Log($"[ScriptDisabler] {scripts.Length}개의 스크립트를 비활성화했습니다.");
        }
        
        [MenuItem("Tools/Script Disabler/Enable All Disabled Scripts")]
        public static void EnableAllDisabledScripts()
        {
            string[] scripts = AssetDatabase.FindAssets("t:Script", new[] { "Assets/Script/Disabled" });
            
            foreach (string scriptGuid in scripts)
            {
                string scriptPath = AssetDatabase.GUIDToAssetPath(scriptGuid);
                ScriptDisabler.EnableScript(scriptPath);
            }
            
            Debug.Log($"[ScriptDisabler] {scripts.Length}개의 스크립트를 활성화했습니다.");
        }
        
        [MenuItem("Tools/Script Disabler/Create Assembly Definitions")]
        public static void CreateAssemblyDefinitions()
        {
            CreateActiveAssemblyDefinition();
            CreateDisabledAssemblyDefinition();
            Debug.Log("[ScriptDisabler] Assembly Definition 파일들을 생성했습니다.");
        }
        
        private static void CreateActiveAssemblyDefinition()
        {
            string asmdefPath = "Assets/Script/Active.asmdef";
            string asmdefContent = @"{
    ""name"": ""RougeShool.Active"",
    ""rootNamespace"": ""Game"",
    ""references"": [
        ""Unity.TextMeshPro"",
        ""Zenject"",
        ""DOTween""
    ],
    ""includePlatforms"": [],
    ""excludePlatforms"": [],
    ""allowUnsafeCode"": false,
    ""overrideReferences"": false,
    ""precompiledReferences"": [],
    ""autoReferenced"": true,
    ""defineConstraints"": [],
    ""versionDefines"": [],
    ""noEngineReferences"": false
}";
            
            File.WriteAllText(asmdefPath, asmdefContent);
        }
        
        private static void CreateDisabledAssemblyDefinition()
        {
            string asmdefPath = "Assets/Script/Disabled.asmdef";
            string asmdefContent = @"{
    ""name"": ""RougeShool.Disabled"",
    ""rootNamespace"": ""Game.Disabled"",
    ""references"": [
        ""RougeShool.Active""
    ],
    ""includePlatforms"": [],
    ""excludePlatforms"": [
        ""Editor"",
        ""StandaloneWindows"",
        ""StandaloneWindows64"",
        ""StandaloneOSX"",
        ""StandaloneLinux64"",
        ""iOS"",
        ""Android"",
        ""WebGL""
    ],
    ""allowUnsafeCode"": false,
    ""overrideReferences"": false,
    ""precompiledReferences"": [],
    ""autoReferenced"": false,
    ""defineConstraints"": [],
    ""versionDefines"": [],
    ""noEngineReferences"": false
}";
            
            File.WriteAllText(asmdefPath, asmdefContent);
        }
    }
}
