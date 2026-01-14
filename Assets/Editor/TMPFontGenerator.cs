using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TextCore.LowLevel;

namespace Game.Editor.FontTools
{
    /// <summary>
    /// TextMesh Pro 폰트 에셋을 영어, 한글, 특수문자 완벽 지원으로 생성/수정하는 에디터 툴
    /// </summary>
    public class TMPFontGenerator : EditorWindow
    {
        #region Constants

        private const int HANGUL_START = 0xAC00; // 가
        private const int HANGUL_END = 0xD7A3;   // 힣

        private const string ASCII_CHARACTERS =
            " !\"#$%&'()*+,-./0123456789:;<=>?@" +
            "ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`" +
            "abcdefghijklmnopqrstuvwxyz{|}~";

        private const string SPECIAL_CHARACTERS =
            "■□◆◇★☆♥♡▲△▼▽◀◁▶▷" +
            "①②③④⑤⑥⑦⑧⑨⑩" +
            "「」『』【】《》〈〉" +
            "※◎○●" +
            "→←↑↓" +
            "─│┌┐┘└├┬┤┴┼" +
            "°℃№€￦₩";

        #endregion

        #region Fields

        private TMP_FontAsset selectedFontAsset;
        private Font sourceFontFile;
        private Vector2 scrollPosition;

        // Atlas 설정
        private int atlasWidth = 4096;
        private int atlasHeight = 4096;
        private int pointSize = 60;
        private int padding = 5;

        // 문자 포함 옵션
        private bool includeAscii = true;
        private bool includeHangul = true;
        private bool includeSpecialCharacters = true;
        private string customCharacters = "";

        #endregion

        #region Menu

        [MenuItem("Tools/TMP/Font Generator (한글 지원)", priority = 100)]
        public static void ShowWindow()
        {
            var window = GetWindow<TMPFontGenerator>("TMP 폰트 생성기");
            window.minSize = new Vector2(450, 600);
            window.Show();
        }

        #endregion

        #region GUI

        private void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            DrawHeader();
            EditorGUILayout.Space(10);

            DrawFontSelection();
            EditorGUILayout.Space(10);

            DrawAtlasSettings();
            EditorGUILayout.Space(10);

            DrawCharacterSettings();
            EditorGUILayout.Space(10);

            DrawActions();

            EditorGUILayout.EndScrollView();
        }

        private void DrawHeader()
        {
            EditorGUILayout.LabelField("TMP 폰트 생성기 (한글 완벽 지원)", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "영어, 한글(가-힣 11,172자), 특수문자를 지원하는 TMP 폰트를 생성합니다.\n" +
                "Dynamic Mode로 런타임에 필요한 글자를 자동으로 추가합니다.",
                MessageType.Info);
        }

        private void DrawFontSelection()
        {
            EditorGUILayout.LabelField("1. 폰트 선택", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUI.BeginChangeCheck();
            selectedFontAsset = EditorGUILayout.ObjectField(
                "수정할 TMP 폰트 (선택)",
                selectedFontAsset,
                typeof(TMP_FontAsset),
                false) as TMP_FontAsset;

            if (EditorGUI.EndChangeCheck() && selectedFontAsset != null)
            {
                LoadSettingsFromFontAsset();
            }

            sourceFontFile = EditorGUILayout.ObjectField(
                "소스 폰트 (.otf, .ttf)",
                sourceFontFile,
                typeof(Font),
                false) as Font;

            if (selectedFontAsset == null && sourceFontFile == null)
            {
                EditorGUILayout.HelpBox("소스 폰트 파일을 선택하세요.", MessageType.Warning);
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawAtlasSettings()
        {
            EditorGUILayout.LabelField("2. Atlas 설정", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            // Atlas 크기 프리셋
            EditorGUILayout.LabelField("Atlas 크기 (한글 지원을 위해 4096 이상 권장)");
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("2048", atlasWidth == 2048 ? "ButtonMid" : "Button"))
            {
                atlasWidth = atlasHeight = 2048;
            }
            if (GUILayout.Button("4096 ★", atlasWidth == 4096 ? "ButtonMid" : "Button"))
            {
                atlasWidth = atlasHeight = 4096;
            }
            if (GUILayout.Button("8192", atlasWidth == 8192 ? "ButtonMid" : "Button"))
            {
                atlasWidth = atlasHeight = 8192;
            }

            EditorGUILayout.EndHorizontal();

            // 상세 설정
            EditorGUILayout.Space(5);
            pointSize = EditorGUILayout.IntSlider("Point Size (작을수록 많은 글자)", pointSize, 30, 120);
            padding = EditorGUILayout.IntSlider("Padding", padding, 3, 10);

            EditorGUILayout.EndVertical();
        }

        private void DrawCharacterSettings()
        {
            EditorGUILayout.LabelField("3. 포함할 문자", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            includeAscii = EditorGUILayout.Toggle("ASCII (영어, 숫자, 기본 기호)", includeAscii);
            includeHangul = EditorGUILayout.Toggle("한글 완성형 (가-힣, 11,172자)", includeHangul);
            includeSpecialCharacters = EditorGUILayout.Toggle("특수문자 (화살표, 도형 등)", includeSpecialCharacters);

            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("추가 커스텀 문자:");
            customCharacters = EditorGUILayout.TextArea(customCharacters, GUILayout.Height(50));

            // 예상 문자 수
            int estimatedChars = CalculateCharacterCount();
            EditorGUILayout.HelpBox($"예상 총 문자 수: {estimatedChars:N0}자", MessageType.Info);

            EditorGUILayout.EndVertical();
        }

        private void DrawActions()
        {
            EditorGUILayout.LabelField("4. 실행", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUI.BeginDisabledGroup(selectedFontAsset == null && sourceFontFile == null);

            if (selectedFontAsset != null)
            {
                if (GUILayout.Button("선택한 폰트 에셋 수정하기", GUILayout.Height(35)))
                {
                    UpdateFontAsset();
                }
            }

            if (sourceFontFile != null)
            {
                if (GUILayout.Button("새 TMP 폰트 생성하기", GUILayout.Height(35)))
                {
                    CreateFontAsset();
                }
            }

            EditorGUI.EndDisabledGroup();

            EditorGUILayout.Space(10);

            EditorGUI.BeginDisabledGroup(selectedFontAsset == null);
            if (GUILayout.Button("프로젝트 전체 폰트 교체", GUILayout.Height(30)))
            {
                ReplaceFontsInProject();
            }
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.EndVertical();
        }

        #endregion

        #region Font Operations

        private void CreateFontAsset()
        {
            if (sourceFontFile == null)
            {
                EditorUtility.DisplayDialog("오류", "소스 폰트 파일을 선택하세요.", "확인");
                return;
            }

            string sourcePath = AssetDatabase.GetAssetPath(sourceFontFile);
            string directory = Path.GetDirectoryName(sourcePath);
            string fontName = Path.GetFileNameWithoutExtension(sourcePath);

            string savePath = EditorUtility.SaveFilePanel(
                "TMP 폰트 저장",
                directory,
                $"{fontName} SDF",
                "asset");

            if (string.IsNullOrEmpty(savePath))
                return;

            savePath = FileUtil.GetProjectRelativePath(savePath);
            if (string.IsNullOrEmpty(savePath))
            {
                EditorUtility.DisplayDialog("오류", "Unity 프로젝트 폴더 내에 저장해야 합니다.", "확인");
                return;
            }

            try
            {
                EditorUtility.DisplayProgressBar("폰트 생성", "폰트 에셋 생성 중...", 0.5f);

                // TMP 폰트 생성
                var fontAsset = TMP_FontAsset.CreateFontAsset(
                    sourceFontFile,
                    pointSize,
                    padding,
                    GlyphRenderMode.SDFAA,
                    atlasWidth,
                    atlasHeight,
                    AtlasPopulationMode.Dynamic);

                if (fontAsset == null)
                {
                    throw new Exception("폰트 생성 실패");
                }

                // 에셋 저장
                AssetDatabase.CreateAsset(fontAsset, savePath);

                // Material 저장
                if (fontAsset.material != null)
                {
                    string matPath = savePath.Replace(".asset", " Material.mat");
                    AssetDatabase.CreateAsset(fontAsset.material, matPath);
                }

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                selectedFontAsset = fontAsset;
                Selection.activeObject = fontAsset;

                EditorUtility.DisplayDialog(
                    "완료",
                    $"폰트 생성 완료!\n\n경로: {savePath}\nAtlas: {atlasWidth}x{atlasHeight}\nMode: Dynamic\n\n" +
                    "Dynamic Mode로 런타임에 필요한 글자가 자동 추가됩니다.",
                    "확인");
            }
            catch (Exception e)
            {
                Debug.LogError($"[TMPFontGenerator] 폰트 생성 실패: {e.Message}");
                EditorUtility.DisplayDialog("오류", $"폰트 생성 실패:\n{e.Message}", "확인");
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        private void UpdateFontAsset()
        {
            if (selectedFontAsset == null)
            {
                EditorUtility.DisplayDialog("오류", "수정할 폰트 에셋을 선택하세요.", "확인");
                return;
            }

            if (!EditorUtility.DisplayDialog(
                "폰트 수정",
                "폰트 에셋의 Atlas 설정을 수정합니다.\n계속하시겠습니까?",
                "수정",
                "취소"))
            {
                return;
            }

            try
            {
                EditorUtility.DisplayProgressBar("폰트 수정", "Atlas 설정 업데이트 중...", 0.5f);

                // Atlas 설정 업데이트
                selectedFontAsset.atlasPopulationMode = AtlasPopulationMode.Dynamic;

                // Creation Settings 업데이트
                var settings = selectedFontAsset.creationSettings;
                settings.atlasWidth = atlasWidth;
                settings.atlasHeight = atlasHeight;
                settings.pointSize = pointSize;
                settings.padding = padding;

                // Atlas 클리어 (Dynamic Mode에서는 런타임에 자동 생성)
                selectedFontAsset.ClearFontAssetData(true);

                EditorUtility.SetDirty(selectedFontAsset);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                EditorUtility.DisplayDialog(
                    "완료",
                    $"폰트 수정 완료!\n\nAtlas: {atlasWidth}x{atlasHeight}\nMode: Dynamic\n\n" +
                    "Dynamic Mode로 런타임에 필요한 글자가 자동 추가됩니다.",
                    "확인");
            }
            catch (Exception e)
            {
                Debug.LogError($"[TMPFontGenerator] 폰트 수정 실패: {e.Message}");
                EditorUtility.DisplayDialog("오류", $"폰트 수정 실패:\n{e.Message}", "확인");
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        private void ReplaceFontsInProject()
        {
            if (selectedFontAsset == null)
            {
                EditorUtility.DisplayDialog("오류", "교체할 폰트를 선택하세요.", "확인");
                return;
            }

            if (!EditorUtility.DisplayDialog(
                "폰트 일괄 교체",
                $"모든 씬과 프리팹의 TMP 폰트를\n'{selectedFontAsset.name}'로 교체합니다.\n\n계속하시겠습니까?",
                "교체",
                "취소"))
            {
                return;
            }

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;

            try
            {
                int sceneCount = 0;
                int prefabCount = 0;

                // 씬 처리
                EditorUtility.DisplayProgressBar("폰트 교체", "씬 처리 중...", 0.3f);
                sceneCount = ReplaceInScenes(selectedFontAsset);

                // 프리팹 처리
                EditorUtility.DisplayProgressBar("폰트 교체", "프리팹 처리 중...", 0.6f);
                prefabCount = ReplaceInPrefabs(selectedFontAsset);

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                EditorUtility.DisplayDialog(
                    "완료",
                    $"폰트 교체 완료!\n\n씬: {sceneCount}개\n프리팹: {prefabCount}개",
                    "확인");
            }
            catch (Exception e)
            {
                Debug.LogError($"[TMPFontGenerator] 폰트 교체 실패: {e.Message}");
                EditorUtility.DisplayDialog("오류", $"폰트 교체 실패:\n{e.Message}", "확인");
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        private int ReplaceInScenes(TMP_FontAsset targetFont)
        {
            int totalChanged = 0;
            var sceneGuids = AssetDatabase.FindAssets("t:Scene", new[] { "Assets" });

            for (int i = 0; i < sceneGuids.Length; i++)
            {
                string scenePath = AssetDatabase.GUIDToAssetPath(sceneGuids[i]);
                EditorUtility.DisplayProgressBar(
                    "씬 처리",
                    $"{i + 1}/{sceneGuids.Length}: {Path.GetFileName(scenePath)}",
                    (float)i / sceneGuids.Length);

                var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

                var texts = UnityEngine.Object.FindObjectsByType<TMP_Text>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None);

                int changed = 0;
                foreach (var text in texts)
                {
                    if (text != null && text.font != targetFont)
                    {
                        text.font = targetFont;
                        EditorUtility.SetDirty(text);
                        changed++;
                    }
                }

                if (changed > 0)
                {
                    EditorSceneManager.SaveScene(scene);
                    totalChanged += changed;
                }
            }

            return totalChanged;
        }

        private int ReplaceInPrefabs(TMP_FontAsset targetFont)
        {
            int totalChanged = 0;
            var prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets" });

            for (int i = 0; i < prefabGuids.Length; i++)
            {
                string prefabPath = AssetDatabase.GUIDToAssetPath(prefabGuids[i]);
                EditorUtility.DisplayProgressBar(
                    "프리팹 처리",
                    $"{i + 1}/{prefabGuids.Length}: {Path.GetFileName(prefabPath)}",
                    (float)i / prefabGuids.Length);

                var root = PrefabUtility.LoadPrefabContents(prefabPath);
                if (root == null) continue;

                var texts = root.GetComponentsInChildren<TMP_Text>(true);
                int changed = 0;

                foreach (var text in texts)
                {
                    if (text != null && text.font != targetFont)
                    {
                        text.font = targetFont;
                        EditorUtility.SetDirty(text);
                        changed++;
                    }
                }

                if (changed > 0)
                {
                    PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
                    totalChanged += changed;
                }

                PrefabUtility.UnloadPrefabContents(root);
            }

            return totalChanged;
        }

        #endregion

        #region Helper Methods

        private void LoadSettingsFromFontAsset()
        {
            if (selectedFontAsset == null) return;

            var settings = selectedFontAsset.creationSettings;
            atlasWidth = settings.atlasWidth;
            atlasHeight = settings.atlasHeight;
            pointSize = settings.pointSize;
            padding = settings.padding;

            if (selectedFontAsset.sourceFontFile != null)
            {
                sourceFontFile = selectedFontAsset.sourceFontFile;
            }
        }

        private int CalculateCharacterCount()
        {
            int count = 0;

            if (includeAscii)
                count += ASCII_CHARACTERS.Length;

            if (includeHangul)
                count += (HANGUL_END - HANGUL_START + 1);

            if (includeSpecialCharacters)
                count += SPECIAL_CHARACTERS.Length;

            if (!string.IsNullOrEmpty(customCharacters))
                count += customCharacters.Distinct().Count();

            return count;
        }

        #endregion
    }
}
