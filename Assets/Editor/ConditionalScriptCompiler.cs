using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor.Build;

namespace Game.Editor
{
    /// <summary>
    /// 조건부 컴파일을 통한 스크립트 비활성화 시스템
    /// #if DISABLED_SCRIPTS로 감싸진 스크립트를 자동으로 비활성화합니다.
    /// </summary>
    public class ConditionalScriptCompiler : AssetPostprocessor
    {
        private const string DISABLED_FOLDER_PATH = "Assets/Script/Disabled";
        private const string DISABLED_DEFINE = "DISABLED_SCRIPTS";
        
        /// <summary>
        /// 스크립트가 Disabled 폴더로 이동될 때 자동으로 조건부 컴파일로 감쌉니다.
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
                    WrapWithConditionalCompilation(newPath);
                }
                
                // Disabled 폴더에서 다른 폴더로 이동된 경우
                if (Path.GetExtension(newPath) == ".cs" && 
                    !newPath.Contains(DISABLED_FOLDER_PATH) && 
                    oldPath.Contains(DISABLED_FOLDER_PATH))
                {
                    RemoveConditionalCompilation(newPath);
                }
            }
        }
        
        /// <summary>
        /// 스크립트를 조건부 컴파일로 감쌉니다.
        /// </summary>
        public static void WrapWithConditionalCompilation(string scriptPath)
        {
            string content = File.ReadAllText(scriptPath);
            
            // 이미 조건부 컴파일로 감싸진 경우 스킵
            if (content.Contains($"#if {DISABLED_DEFINE}"))
                return;
            
            // 스크립트 내용을 조건부 컴파일로 감싸기
            string wrappedContent = $@"#if {DISABLED_DEFINE}
{content}
#endif // {DISABLED_DEFINE}";
            
            File.WriteAllText(scriptPath, wrappedContent);
            
            Debug.Log($"[ConditionalScriptCompiler] 스크립트를 조건부 컴파일로 감쌌습니다: {scriptPath}");
        }
        
        /// <summary>
        /// 스크립트에서 조건부 컴파일을 제거합니다.
        /// </summary>
        private static void RemoveConditionalCompilation(string scriptPath)
        {
            string content = File.ReadAllText(scriptPath);
            
            // 조건부 컴파일 제거
            string pattern = $@"#if {DISABLED_DEFINE}\s*\n(.*?)\n#endif\s*//\s*{DISABLED_DEFINE}";
            content = Regex.Replace(content, pattern, "$1", RegexOptions.Singleline);
            
            File.WriteAllText(scriptPath, content);
            
            Debug.Log($"[ConditionalScriptCompiler] 스크립트에서 조건부 컴파일을 제거했습니다: {scriptPath}");
        }
    }
    
    /// <summary>
    /// 조건부 컴파일 설정 관리
    /// </summary>
    public class ConditionalCompilerMenu
    {
        private const string DISABLED_DEFINE = "DISABLED_SCRIPTS";
        
        [MenuItem("Tools/Conditional Compiler/Enable Disabled Scripts")]
        public static void EnableDisabledScripts()
        {
            // Player Settings에서 DISABLED_SCRIPTS 정의 추가
            var buildTarget = EditorUserBuildSettings.selectedBuildTargetGroup;
            
            // 최신 Unity API만 사용 (경고 제거)
            string defines = PlayerSettings.GetScriptingDefineSymbols(NamedBuildTarget.FromBuildTargetGroup(buildTarget));
            
            if (!defines.Contains(DISABLED_DEFINE))
            {
                defines += (string.IsNullOrEmpty(defines) ? "" : ";") + DISABLED_DEFINE;
                PlayerSettings.SetScriptingDefineSymbols(NamedBuildTarget.FromBuildTargetGroup(buildTarget), defines);
                Debug.Log($"[ConditionalCompiler] {DISABLED_DEFINE} 정의를 추가했습니다.");
            }
        }
        
        [MenuItem("Tools/Conditional Compiler/Disable Disabled Scripts")]
        public static void DisableDisabledScripts()
        {
            // Player Settings에서 DISABLED_SCRIPTS 정의 제거
            var buildTarget = EditorUserBuildSettings.selectedBuildTargetGroup;
            
            // 최신 Unity API만 사용 (경고 제거)
            string defines = PlayerSettings.GetScriptingDefineSymbols(NamedBuildTarget.FromBuildTargetGroup(buildTarget));
            
            if (defines.Contains(DISABLED_DEFINE))
            {
                defines = defines.Replace(DISABLED_DEFINE, "").Replace(";;", ";").Trim(';');
                PlayerSettings.SetScriptingDefineSymbols(NamedBuildTarget.FromBuildTargetGroup(buildTarget), defines);
                Debug.Log($"[ConditionalCompiler] {DISABLED_DEFINE} 정의를 제거했습니다.");
            }
        }
        
        [MenuItem("Tools/Conditional Compiler/Wrap All Disabled Scripts")]
        public static void WrapAllDisabledScripts()
        {
            string[] scripts = AssetDatabase.FindAssets("t:Script", new[] { "Assets/Script/Disabled" });
            
            foreach (string scriptGuid in scripts)
            {
                string scriptPath = AssetDatabase.GUIDToAssetPath(scriptGuid);
                ConditionalScriptCompiler.WrapWithConditionalCompilation(scriptPath);
            }
            
            Debug.Log($"[ConditionalCompiler] {scripts.Length}개의 스크립트를 조건부 컴파일로 감쌌습니다.");
        }
    }
}
