using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class ReplaceTMPFonts
{
    private const string TargetFontAssetPath = "Assets/Resources/Font/DungGeunMo TTF/DungGeunMo SDF.asset";

    [MenuItem("Tools/TMP/Replace Fonts in Scenes & Resources", priority = 1000)]
    public static void ReplaceAll()
    {
        try
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;

            var targetFont = LoadTargetFont();
            if (targetFont == null)
            {
                EditorUtility.DisplayDialog("TMP Font Replace", "대상 폰트 자산을 찾을 수 없습니다.\n경로: " + TargetFontAssetPath, "확인");
                return;
            }

            int totalChanged = 0;
            int sceneChanged = 0;
            int prefabChanged = 0;

            try
            {
                EditorUtility.DisplayProgressBar("TMP Font Replace", "씬 처리 중...", 0f);
                sceneChanged = ProcessScenes(targetFont);
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            try
            {
                EditorUtility.DisplayProgressBar("TMP Font Replace", "리소스 프리팹 처리 중...", 0f);
                prefabChanged = ProcessPrefabs(targetFont);
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            totalChanged = sceneChanged + prefabChanged;

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog(
                "TMP Font Replace",
                $"교체 완료\n\n변경된 컴포넌트 수: {totalChanged}\n- 씬 내 변경: {sceneChanged}\n- 프리팹 내 변경: {prefabChanged}",
                "확인");
        }
        catch (Exception e)
        {
            Debug.LogError($"[TMP Font Replace] 오류: {e}");
            EditorUtility.ClearProgressBar();
        }
    }

    private static TMP_FontAsset LoadTargetFont()
    {
        var fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(TargetFontAssetPath);
        if (fontAsset != null)
            return fontAsset;

        // Fallback: 이름으로 검색 (경로가 바뀐 경우)
        var guids = AssetDatabase.FindAssets("t:TMP_FontAsset DungGeunMo SDF");
        foreach (var guid in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var candidate = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
            if (candidate != null)
                return candidate;
        }

        // 마지막 Fallback: Resources 경로로 로드
        var fromResources = Resources.Load<TMP_FontAsset>("Font/DungGeunMo TTF/DungGeunMo SDF");
        return fromResources;
    }

    private static int ProcessScenes(TMP_FontAsset targetFont)
    {
        int changedCount = 0;
        var sceneGuids = AssetDatabase.FindAssets("t:Scene", new[] { "Assets/Scenes" });
        for (int i = 0; i < sceneGuids.Length; i++)
        {
            var scenePath = AssetDatabase.GUIDToAssetPath(sceneGuids[i]);
            EditorUtility.DisplayProgressBar("TMP Font Replace - Scenes", scenePath, (float)i / Math.Max(1, sceneGuids.Length));

            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            changedCount += ReplaceInOpenScene(scene, targetFont);
            EditorSceneManager.SaveScene(scene);
        }

        return changedCount;
    }

    private static int ReplaceInOpenScene(Scene scene, TMP_FontAsset targetFont)
    {
        int changed = 0;

        // TMP_Text (UGUI & 3D)
        var texts = UnityEngine.Object.FindObjectsByType<TMP_Text>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var t in texts)
        {
            if (t != null && t.font != targetFont)
            {
                t.font = targetFont;
                EditorUtility.SetDirty(t);
                changed++;
            }
        }

        // TMP_InputField
        var inputs = UnityEngine.Object.FindObjectsByType<TMP_InputField>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var input in inputs)
        {
            if (input != null && input.textComponent != null && input.textComponent.font != targetFont)
            {
                input.textComponent.font = targetFont;
                EditorUtility.SetDirty(input);
                changed++;
            }
            if (input != null && input.placeholder is TMP_Text placeholderText && placeholderText.font != targetFont)
            {
                placeholderText.font = targetFont;
                EditorUtility.SetDirty(placeholderText);
                changed++;
            }
        }

        // TMP_Dropdown
        var dropdowns = UnityEngine.Object.FindObjectsByType<TMP_Dropdown>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var dd in dropdowns)
        {
            if (dd != null && dd.captionText != null && dd.captionText.font != targetFont)
            {
                dd.captionText.font = targetFont;
                EditorUtility.SetDirty(dd.captionText);
                changed++;
            }
            if (dd != null && dd.itemText != null && dd.itemText.font != targetFont)
            {
                dd.itemText.font = targetFont;
                EditorUtility.SetDirty(dd.itemText);
                changed++;
            }
        }

        if (changed > 0)
        {
            EditorSceneManager.MarkSceneDirty(scene);
        }

        return changed;
    }

    private static int ProcessPrefabs(TMP_FontAsset targetFont)
    {
        int changedCount = 0;
        var prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Resources" });
        for (int i = 0; i < prefabGuids.Length; i++)
        {
            var path = AssetDatabase.GUIDToAssetPath(prefabGuids[i]);
            EditorUtility.DisplayProgressBar("TMP Font Replace - Prefabs", path, (float)i / Math.Max(1, prefabGuids.Length));

            var root = PrefabUtility.LoadPrefabContents(path);
            if (root == null)
                continue;

            int before = changedCount;

            // TMP_Text
            var texts = root.GetComponentsInChildren<TMP_Text>(true);
            foreach (var t in texts)
            {
                if (t != null && t.font != targetFont)
                {
                    t.font = targetFont;
                    EditorUtility.SetDirty(t);
                    changedCount++;
                }
            }

            // TMP_InputField
            var inputs = root.GetComponentsInChildren<TMP_InputField>(true);
            foreach (var input in inputs)
            {
                if (input != null && input.textComponent != null && input.textComponent.font != targetFont)
                {
                    input.textComponent.font = targetFont;
                    EditorUtility.SetDirty(input);
                    changedCount++;
                }
                if (input != null && input.placeholder is TMP_Text placeholderText && placeholderText.font != targetFont)
                {
                    placeholderText.font = targetFont;
                    EditorUtility.SetDirty(placeholderText);
                    changedCount++;
                }
            }

            // TMP_Dropdown
            var dropdowns = root.GetComponentsInChildren<TMP_Dropdown>(true);
            foreach (var dd in dropdowns)
            {
                if (dd != null && dd.captionText != null && dd.captionText.font != targetFont)
                {
                    dd.captionText.font = targetFont;
                    EditorUtility.SetDirty(dd.captionText);
                    changedCount++;
                }
                if (dd != null && dd.itemText != null && dd.itemText.font != targetFont)
                {
                    dd.itemText.font = targetFont;
                    EditorUtility.SetDirty(dd.itemText);
                    changedCount++;
                }
            }

            if (changedCount > before)
            {
                PrefabUtility.SaveAsPrefabAsset(root, path);
            }

            PrefabUtility.UnloadPrefabContents(root);
        }

        return changedCount;
    }
}


