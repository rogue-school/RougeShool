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
    /// TextMesh Pro 폰트 에셋을 영어, 한글, 특수문자 완벽 지원으로 생성/수정하는 에디터 툴입니다.
    /// </summary>
    public class TMPFontAtlasGenerator : EditorWindow
    {
        #region Constants

        // 한글 유니코드 범위
        private const int HANGUL_START = 0xAC00; // 가
        private const int HANGUL_END = 0xD7A3;   // 힣

        // 영어 및 기본 문자
        private const string ASCII_CHARACTERS =
            " !\"#$%&'()*+,-./0123456789:;<=>?@" +
            "ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`" +
            "abcdefghijklmnopqrstuvwxyz{|}~";

        // 추가 특수문자 (게임에서 자주 사용되는 것들)
        private const string SPECIAL_CHARACTERS =
            "■□◆◇★☆♥♡▲△▼▽◀◁▶▷" +
            "①②③④⑤⑥⑦⑧⑨⑩" +
            "➀➁➂➃➄➅➆➇➈➉" +
            "㈀㈁㈂㈃㈄㈅㈆㈇㈈㈉" +
            "㉠㉡㉢㉣㉤㉥㉦㉧㉨㉩" +
            "㉮㉯㉰㉱㉲㉳㉴㉵㉶㉷" +
            "「」『』【】《》〈〉" +
            "※◎○●◐◑◒◓" +
            "▣▤▥▦▧▨▩" +
            "→←↑↓↔↕⇒⇐⇑⇓⇔" +
            "─│┌┐┘└├┬┤┴┼" +
            "━┃┏┓┛┗┣┳┫┻╋" +
            "═║╔╗╝╚╠╦╣╩╬" +
            "°℃№€￦₩¥£" +
            "ⓐⓑⓒⓓⓔⓕⓖⓗⓘⓙⓚⓛⓜⓝⓞⓟⓠⓡⓢⓣⓤⓥⓦⓧⓨⓩ" +
            "ⒶⒷⒸⒹⒺⒻⒼⒽⒾⒿⓀⓁⓂⓃⓄⓅⓆⓇⓈⓉⓊⓋⓌⓍⓎⓏ" +
            "ⅠⅡⅢⅣⅤⅥⅦⅧⅨⅩ" +
            "ⅰⅱⅲⅳⅴⅵⅶⅷⅸⅹ" +
            "αβγδεζηθικλμνξοπρστυφχψω" +
            "ΑΒΓΔΕΖΗΘΙΚΛΜΝΞΟΠΡΣΤΥΦΧΨΩ" +
            "✓✕✗✘";

        #endregion

        #region Serialized Fields

        private TMP_FontAsset selectedFontAsset;
        private Font sourceFontFile;
        private Vector2 scrollPosition;

        // 설정
        private int atlasWidth = 4096;
        private int atlasHeight = 4096;
        private int pointSize = 60;
        private int padding = 5;
        private int renderMode = 4165; // SDFAA

        // 문자 포함 옵션
        private bool includeAscii = true;
        private bool includeHangul = true;
        private bool includeSpecialCharacters = true;
        private string customCharacters = "";

        // 최적화 옵션
        private bool useDynamicMode = true;
        private bool analyzeExistingText = false;

        #endregion

        #region Unity Menu

        [MenuItem("Tools/TMP/Font Atlas Generator", priority = 100)]
        public static void ShowWindow()
        {
            var window = GetWindow<TMPFontAtlasGenerator>("TMP Font Generator");
            window.minSize = new Vector2(500, 700);
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

            DrawOptimizationSettings();
            EditorGUILayout.Space(10);

            DrawActions();

            EditorGUILayout.EndScrollView();
        }

        private void DrawHeader()
        {
            EditorGUILayout.LabelField("TextMesh Pro 폰트 생성/수정 도구", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "영어, 한글(가-힣), 특수문자를 완벽하게 지원하는 TMP 폰트 에셋을 생성하거나 수정합니다.\n" +
                "기존 폰트 에셋을 선택하여 수정하거나, 소스 폰트 파일(.otf, .ttf)을 선택하여 새로 생성할 수 있습니다.",
                MessageType.Info);
        }

        private void DrawFontSelection()
        {
            EditorGUILayout.LabelField("폰트 선택", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();
            selectedFontAsset = EditorGUILayout.ObjectField(
                "기존 TMP 폰트 에셋",
                selectedFontAsset,
                typeof(TMP_FontAsset),
                false) as TMP_FontAsset;

            if (EditorGUI.EndChangeCheck() && selectedFontAsset != null)
            {
                LoadSettingsFromFontAsset();
            }

            EditorGUILayout.Space(5);

            sourceFontFile = EditorGUILayout.ObjectField(
                "소스 폰트 파일 (.otf, .ttf)",
                sourceFontFile,
                typeof(Font),
                false) as Font;

            if (selectedFontAsset == null && sourceFontFile == null)
            {
                EditorGUILayout.HelpBox(
                    "기존 TMP 폰트 에셋을 선택하여 수정하거나,\n" +
                    "소스 폰트 파일을 선택하여 새로 생성하세요.",
                    MessageType.Warning);
            }
        }

        private void DrawAtlasSettings()
        {
            EditorGUILayout.LabelField("Atlas 설정", EditorStyles.boldLabel);

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            // Atlas 크기
            EditorGUILayout.LabelField("Atlas 크기 (한글 지원을 위해 큰 크기 권장)");
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("2048x2048", atlasWidth == 2048 ? EditorStyles.miniButtonMid : EditorStyles.miniButton))
            {
                atlasWidth = atlasHeight = 2048;
            }
            if (GUILayout.Button("4096x4096 (권장)", atlasWidth == 4096 ? EditorStyles.miniButtonMid : EditorStyles.miniButton))
            {
                atlasWidth = atlasHeight = 4096;
            }
            if (GUILayout.Button("8192x8192", atlasWidth == 8192 ? EditorStyles.miniButtonMid : EditorStyles.miniButton))
            {
                atlasWidth = atlasHeight = 8192;
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            atlasWidth = EditorGUILayout.IntField("Width", atlasWidth);
            atlasHeight = EditorGUILayout.IntField("Height", atlasHeight);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(5);

            // Point Size
            pointSize = EditorGUILayout.IntSlider("Point Size (작을수록 더 많은 글자)", pointSize, 30, 120);

            // Padding
            padding = EditorGUILayout.IntSlider("Padding", padding, 3, 10);

            // Render Mode
            string[] renderModes = new string[] { "SMOOTH_HINTED", "SMOOTH", "RASTER_HINTED", "RASTER", "SDF", "SDFAA (권장)", "SDFAA_HINTED" };
            int[] renderModeValues = new int[] { 4097, 4098, 4099, 4100, 4166, 4165, 4167 };

            int currentIndex = Array.IndexOf(renderModeValues, renderMode);
            if (currentIndex == -1) currentIndex = 5; // Default to SDFAA

            int newIndex = EditorGUILayout.Popup("Render Mode", currentIndex, renderModes);
            renderMode = renderModeValues[newIndex];

            EditorGUILayout.EndVertical();
        }

        private void DrawCharacterSettings()
        {
            EditorGUILayout.LabelField("포함할 문자", EditorStyles.boldLabel);

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            includeAscii = EditorGUILayout.Toggle("ASCII 문자 (영어, 숫자, 기본 기호)", includeAscii);
            includeHangul = EditorGUILayout.Toggle("한글 완성형 (가-힣, 11,172자)", includeHangul);
            includeSpecialCharacters = EditorGUILayout.Toggle("게임용 특수문자 (화살표, 도형 등)", includeSpecialCharacters);

            EditorGUILayout.Space(5);

            EditorGUILayout.LabelField("추가 커스텀 문자:");
            customCharacters = EditorGUILayout.TextArea(customCharacters, GUILayout.Height(60));

            EditorGUILayout.Space(5);

            // 예상 문자 수 계산
            int estimatedCharCount = CalculateEstimatedCharacterCount();
            EditorGUILayout.HelpBox(
                $"예상 총 문자 수: {estimatedCharCount:N0}자\n" +
                GetAtlasCapacityEstimate(estimatedCharCount),
                estimatedCharCount > GetMaxCharactersForAtlas() ? MessageType.Warning : MessageType.Info);

            EditorGUILayout.EndVertical();
        }

        private void DrawOptimizationSettings()
        {
            EditorGUILayout.LabelField("최적화 옵션", EditorStyles.boldLabel);

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            useDynamicMode = EditorGUILayout.Toggle(
                new GUIContent(
                    "Dynamic Mode 사용",
                    "런타임에 필요한 글자를 자동으로 추가합니다. 메모리 효율적이지만 큰 Atlas가 필요합니다."),
                useDynamicMode);

            EditorGUI.BeginDisabledGroup(true); // 향후 구현 예정
            analyzeExistingText = EditorGUILayout.Toggle(
                new GUIContent(
                    "프로젝트 텍스트 분석 (미구현)",
                    "프로젝트의 모든 씬과 프리팹을 분석하여 실제 사용하는 문자만 포함합니다."),
                analyzeExistingText);
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.EndVertical();
        }

        private void DrawActions()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.LabelField("작업", EditorStyles.boldLabel);

            EditorGUI.BeginDisabledGroup(selectedFontAsset == null && sourceFontFile == null);

            if (selectedFontAsset != null)
            {
                if (GUILayout.Button("기존 폰트 에셋 수정하기", GUILayout.Height(40)))
                {
                    UpdateExistingFontAsset();
                }
            }
            else if (sourceFontFile != null)
            {
                if (GUILayout.Button("새 TMP 폰트 에셋 생성하기", GUILayout.Height(40)))
                {
                    CreateNewFontAsset();
                }
            }

            EditorGUI.EndDisabledGroup();

            EditorGUILayout.Space(10);

            if (GUILayout.Button("모든 씬/프리팹의 폰트 일괄 교체", GUILayout.Height(30)))
            {
                if (selectedFontAsset != null)
                {
                    ReplaceAllFontsInProject(selectedFontAsset);
                }
                else
                {
                    EditorUtility.DisplayDialog("오류", "교체할 폰트 에셋을 먼저 선택하세요.", "확인");
                }
            }

            EditorGUILayout.EndVertical();
        }

        #endregion

        #region Font Generation

        private void CreateNewFontAsset()
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
                "TMP 폰트 에셋 저장",
                directory,
                $"{fontName} SDF",
                "asset");

            if (string.IsNullOrEmpty(savePath))
                return;

            // Unity 프로젝트 내 경로로 변환
            savePath = FileUtil.GetProjectRelativePath(savePath);

            if (string.IsNullOrEmpty(savePath))
            {
                EditorUtility.DisplayDialog("오류", "Unity 프로젝트 내에 저장해야 합니다.", "확인");
                return;
            }

            try
            {
                EditorUtility.DisplayProgressBar("폰트 생성 중", "폰트 에셋을 생성하고 있습니다...", 0.5f);

                // 문자 세트 준비
                string characterSequence = BuildCharacterSequence();

                // TMP 폰트 생성
                var fontAsset = TMP_FontAsset.CreateFontAsset(
                    sourceFontFile,
                    pointSize,
                    padding,
                    GlyphRenderMode.SDFAA,
                    atlasWidth,
                    atlasHeight);

                if (fontAsset == null)
                {
                    throw new Exception("폰트 에셋 생성에 실패했습니다.");
                }

                // Atlas Population Mode 설정
                fontAsset.atlasPopulationMode = useDynamicMode
                    ? AtlasPopulationMode.Dynamic
                    : AtlasPopulationMode.Static;

                // 에셋 저장
                AssetDatabase.CreateAsset(fontAsset, savePath);

                // Material 저장
                string materialPath = savePath.Replace(".asset", " Material.mat");
                if (fontAsset.material != null)
                {
                    AssetDatabase.CreateAsset(fontAsset.material, materialPath);
                }

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                // 생성된 에셋 선택
                selectedFontAsset = fontAsset;
                Selection.activeObject = fontAsset;

                EditorUtility.DisplayDialog(
                    "완료",
                    $"폰트 에셋이 생성되었습니다.\n\n" +
                    $"경로: {savePath}\n" +
                    $"Atlas 크기: {atlasWidth}x{atlasHeight}\n" +
                    $"Mode: {(useDynamicMode ? "Dynamic" : "Static")}",
                    "확인");
            }
            catch (Exception e)
            {
                Debug.LogError($"[TMPFontGenerator] 폰트 생성 실패: {e.Message}\n{e.StackTrace}");
                EditorUtility.DisplayDialog("오류", $"폰트 생성 실패:\n{e.Message}", "확인");
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        private void UpdateExistingFontAsset()
        {
            if (selectedFontAsset == null)
            {
                EditorUtility.DisplayDialog("오류", "수정할 폰트 에셋을 선택하세요.", "확인");
                return;
            }

            if (!EditorUtility.DisplayDialog(
                "폰트 에셋 수정",
                "기존 폰트 에셋을 수정합니다.\n" +
                "이 작업은 되돌릴 수 없습니다.\n계속하시겠습니까?",
                "수정",
                "취소"))
            {
                return;
            }

            try
            {
                EditorUtility.DisplayProgressBar("폰트 수정 중", "폰트 Atlas를 업데이트하고 있습니다...", 0.5f);

                // Atlas 크기 업데이트
                var creationSettings = selectedFontAsset.creationSettings;
                creationSettings.atlasWidth = atlasWidth;
                creationSettings.atlasHeight = atlasHeight;
                creationSettings.pointSize = pointSize;
                creationSettings.padding = padding;

                // Atlas Population Mode 설정
                selectedFontAsset.atlasPopulationMode = useDynamicMode
                    ? AtlasPopulationMode.Dynamic
                    : AtlasPopulationMode.Static;

                // 문자 세트가 지정된 경우 업데이트
                if (!useDynamicMode)
                {
                    string characterSequence = BuildCharacterSequence();
                    creationSettings.characterSequence = characterSequence;
                }

                // Atlas 재생성
                selectedFontAsset.ClearFontAssetData(true);

                EditorUtility.SetDirty(selectedFontAsset);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                EditorUtility.DisplayDialog(
                    "완료",
                    $"폰트 에셋이 수정되었습니다.\n\n" +
                    $"Atlas 크기: {atlasWidth}x{atlasHeight}\n" +
                    $"Mode: {(useDynamicMode ? "Dynamic" : "Static")}\n\n" +
                    "Unity 에디터에서 Font Asset Creator를 통해\n" +
                    "최종 생성을 완료하세요.",
                    "확인");

                Selection.activeObject = selectedFontAsset;
            }
            catch (Exception e)
            {
                Debug.LogError($"[TMPFontGenerator] 폰트 수정 실패: {e.Message}\n{e.StackTrace}");
                EditorUtility.DisplayDialog("오류", $"폰트 수정 실패:\n{e.Message}", "확인");
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        #endregion

        #region Font Replacement

        private void ReplaceAllFontsInProject(TMP_FontAsset targetFont)
        {
            if (!EditorUtility.DisplayDialog(
                "폰트 일괄 교체",
                $"프로젝트의 모든 씬과 프리팹에서 TMP 폰트를\n'{targetFont.name}'(으)로 교체합니다.\n\n계속하시겠습니까?",
                "교체",
                "취소"))
            {
                return;
            }

            try
            {
                if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                    return;

                int totalChanged = 0;
                int sceneChanged = 0;
                int prefabChanged = 0;

                try
                {
                    EditorUtility.DisplayProgressBar("폰트 교체", "씬 처리 중...", 0f);
                    sceneChanged = ProcessScenes(targetFont);
                }
                finally
                {
                    EditorUtility.ClearProgressBar();
                }

                try
                {
                    EditorUtility.DisplayProgressBar("폰트 교체", "프리팹 처리 중...", 0f);
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
                    "폰트 교체 완료",
                    $"총 {totalChanged}개의 텍스트 컴포넌트가 교체되었습니다.\n\n" +
                    $"씬: {sceneChanged}개\n" +
                    $"프리팹: {prefabChanged}개",
                    "확인");
            }
            catch (Exception e)
            {
                Debug.LogError($"[TMPFontGenerator] 폰트 교체 실패: {e.Message}\n{e.StackTrace}");
                EditorUtility.DisplayDialog("오류", $"폰트 교체 실패:\n{e.Message}", "확인");
                EditorUtility.ClearProgressBar();
            }
        }

        private int ProcessScenes(TMP_FontAsset targetFont)
        {
            int changedCount = 0;
            var sceneGuids = AssetDatabase.FindAssets("t:Scene", new[] { "Assets" });

            for (int i = 0; i < sceneGuids.Length; i++)
            {
                var scenePath = AssetDatabase.GUIDToAssetPath(sceneGuids[i]);
                EditorUtility.DisplayProgressBar(
                    "씬 처리 중",
                    $"{i + 1}/{sceneGuids.Length}: {Path.GetFileName(scenePath)}",
                    (float)i / sceneGuids.Length);

                var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                changedCount += ReplaceInScene(scene, targetFont);
                EditorSceneManager.SaveScene(scene);
            }

            return changedCount;
        }

        private int ReplaceInScene(Scene scene, TMP_FontAsset targetFont)
        {
            int changed = 0;

            var texts = UnityEngine.Object.FindObjectsByType<TMP_Text>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);

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
                EditorSceneManager.MarkSceneDirty(scene);
            }

            return changed;
        }

        private int ProcessPrefabs(TMP_FontAsset targetFont)
        {
            int changedCount = 0;
            var prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets" });

            for (int i = 0; i < prefabGuids.Length; i++)
            {
                var path = AssetDatabase.GUIDToAssetPath(prefabGuids[i]);
                EditorUtility.DisplayProgressBar(
                    "프리팹 처리 중",
                    $"{i + 1}/{prefabGuids.Length}: {Path.GetFileName(path)}",
                    (float)i / prefabGuids.Length);

                var root = PrefabUtility.LoadPrefabContents(path);
                if (root == null)
                    continue;

                int before = changedCount;
                var texts = root.GetComponentsInChildren<TMP_Text>(true);

                foreach (var text in texts)
                {
                    if (text != null && text.font != targetFont)
                    {
                        text.font = targetFont;
                        EditorUtility.SetDirty(text);
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

        #endregion

        #region Helper Methods

        private void LoadSettingsFromFontAsset()
        {
            if (selectedFontAsset == null)
                return;

            var settings = selectedFontAsset.creationSettings;
            atlasWidth = settings.atlasWidth;
            atlasHeight = settings.atlasHeight;
            pointSize = settings.pointSize;
            padding = settings.padding;
            renderMode = settings.renderMode;

            useDynamicMode = selectedFontAsset.atlasPopulationMode == AtlasPopulationMode.Dynamic;

            if (selectedFontAsset.sourceFontFile != null)
            {
                sourceFontFile = selectedFontAsset.sourceFontFile;
            }
        }

        private string BuildCharacterSequence()
        {
            var sb = new StringBuilder();
            var addedChars = new HashSet<char>();

            // ASCII
            if (includeAscii)
            {
                foreach (char c in ASCII_CHARACTERS)
                {
                    if (addedChars.Add(c))
                        sb.Append(c);
                }
            }

            // 한글
            if (includeHangul)
            {
                for (int i = HANGUL_START; i <= HANGUL_END; i++)
                {
                    char c = (char)i;
                    if (addedChars.Add(c))
                        sb.Append(c);
                }
            }

            // 특수문자
            if (includeSpecialCharacters)
            {
                foreach (char c in SPECIAL_CHARACTERS)
                {
                    if (addedChars.Add(c))
                        sb.Append(c);
                }
            }

            // 커스텀 문자
            if (!string.IsNullOrEmpty(customCharacters))
            {
                foreach (char c in customCharacters)
                {
                    if (addedChars.Add(c))
                        sb.Append(c);
                }
            }

            return sb.ToString();
        }

        private int CalculateEstimatedCharacterCount()
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

        private int GetMaxCharactersForAtlas()
        {
            // 대략적인 추정치 (Point Size와 Atlas 크기에 따라 다름)
            float atlasArea = atlasWidth * atlasHeight;
            float charArea = pointSize * pointSize * 1.5f; // Padding 고려
            return (int)(atlasArea / charArea);
        }

        private string GetAtlasCapacityEstimate(int charCount)
        {
            int maxChars = GetMaxCharactersForAtlas();
            float percentage = (float)charCount / maxChars * 100f;

            if (percentage > 100)
                return $"경고: Atlas 용량 초과 예상 ({percentage:F1}%).\nAtlas 크기를 늘리거나 Point Size를 줄이세요.";
            else if (percentage > 80)
                return $"Atlas 사용률: {percentage:F1}% (여유 공간 부족, Dynamic Mode 권장)";
            else
                return $"Atlas 사용률: {percentage:F1}% (적정)";
        }

        #endregion
    }
}
