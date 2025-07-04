using UnityEditor;
using UnityEngine;
using System.IO;
using System.Text;

public class EncodingBatchConverter : EditorWindow
{
    private string targetFolder = "Assets";
    private bool includeBOM = false;
    private bool createBackup = true;
    private string statusMessage = "";

    [MenuItem("Tools/Encoding/Batch Convert .cs Files to UTF-8")]
    private static void ShowWindow()
    {
        GetWindow<EncodingBatchConverter>("Batch Encoding Converter");
    }

    private void OnGUI()
    {
        GUILayout.Label("UTF-8 인코딩 일괄 변환", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        GUILayout.Label("변환 대상 폴더:", EditorStyles.label);
        EditorGUILayout.BeginHorizontal();
        targetFolder = EditorGUILayout.TextField(targetFolder);
        if (GUILayout.Button("폴더 선택", GUILayout.MaxWidth(100)))
        {
            string selected = EditorUtility.OpenFolderPanel("폴더 선택", Application.dataPath, "");
            if (!string.IsNullOrEmpty(selected))
            {
                // Assets 경로로 변환
                if (selected.StartsWith(Application.dataPath))
                {
                    targetFolder = "Assets" + selected.Substring(Application.dataPath.Length);
                }
                else
                {
                    targetFolder = selected;
                }
            }
        }
        EditorGUILayout.EndHorizontal();

        includeBOM = EditorGUILayout.Toggle("BOM 포함", includeBOM);
        createBackup = EditorGUILayout.Toggle("백업 생성 (.bak)", createBackup);

        EditorGUILayout.Space();
        if (GUILayout.Button("선택한 폴더에서 변환 시작"))
        {
            if (string.IsNullOrEmpty(targetFolder) || !Directory.Exists(targetFolder))
            {
                statusMessage = "❗ 유효한 폴더를 선택하세요.";
            }
            else
            {
                ConvertAllCSFiles(targetFolder, includeBOM, createBackup);
            }
        }

        EditorGUILayout.Space();
        EditorGUILayout.HelpBox(statusMessage, MessageType.Info);
    }

    private static void ConvertAllCSFiles(string relativeFolder, bool includeBOM, bool createBackup)
    {
        string absoluteFolder = Path.Combine(Directory.GetCurrentDirectory(), relativeFolder);
        if (!Directory.Exists(absoluteFolder))
        {
            ShowMessageInWindow($"❗ 폴더 없음: {absoluteFolder}");
            return;
        }

        string[] files = Directory.GetFiles(absoluteFolder, "*.cs", SearchOption.AllDirectories);
        int converted = 0;
        int skipped = 0;

        Encoding sourceEncoding;
        try
        {
            // 한국어 윈도우 ANSI 코드페이지
            sourceEncoding = Encoding.GetEncoding(949);
        }
        catch
        {
            sourceEncoding = Encoding.Default;
        }

        foreach (string file in files)
        {
            try
            {
                if (IsFileLocked(file))
                {
                    Debug.LogWarning($"🔒 Locked, skipped: {file}");
                    skipped++;
                    continue;
                }

                if (createBackup)
                {
                    string backup = file + ".bak";
                    if (!File.Exists(backup))
                        File.Copy(file, backup);
                }

                string content = File.ReadAllText(file, sourceEncoding);

                var utf8 = new UTF8Encoding(includeBOM);
                File.WriteAllText(file, content, utf8);

                Debug.Log($"✅ Converted: {file}");
                converted++;
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"⚠️ Failed to convert: {file}\n{ex.Message}");
                skipped++;
            }
        }

        AssetDatabase.Refresh();
        ShowMessageInWindow($"✅ 변환 완료! 변환: {converted}, 스킵: {skipped}");
    }

    private static bool IsFileLocked(string file)
    {
        try
        {
            using (FileStream stream = File.Open(file, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
            {
                stream.Close();
            }
            return false;
        }
        catch
        {
            return true;
        }
    }

    private static void ShowMessageInWindow(string message)
    {
        var window = GetWindow<EncodingBatchConverter>();
        window.statusMessage = message;
    }
}
