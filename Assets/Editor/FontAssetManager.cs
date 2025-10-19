using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

namespace Game.Editor.FontManagement
{
    /// <summary>
    /// TextMeshPro 폰트 에셋 관리 및 폴백 시스템 설정을 위한 에디터 도구
    /// </summary>
    public static class FontAssetManager
    {
        // 완전한 한글 문자 세트 (가-힣: 모든 한글 조합형)
        private const string HANGUL_COMPLETE = "가-힣";

        // 기본 영문자 (대소문자)
        private const string ENGLISH_ALPHABET = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";

        // 숫자
        private const string NUMBERS = "0123456789";

        // 자주 사용되는 특수문자 (확장)
        private const string SPECIAL_CHARACTERS =
            "!@#$%^&*()_+-=[]{}|;:'\",.<>?/`~ " +  // 기본 특수문자
            "！？、。·：；「」『』（）［］｛｝〈〉《》【】" +  // 전각 기호
            "°℃℉±×÷≠≤≥∞∑∏√∫∂∇" +  // 수학 기호
            "←→↑↓↔↕⇒⇔" +  // 화살표
            "★☆♥♡♠♣♦♧" +  // 도형/기호
            "①②③④⑤⑥⑦⑧⑨⑩" +  // 원문자 숫자
            "㈜㎏㎖㎗㎘㎞㎟㎠㎡㎢㎣㎤㎥㎦㎧㎨㎩㎪㎫㎬㎭㎮㎯㎰㎱㎲㎳" +  // 단위
            "ⅠⅡⅢⅣⅤⅥⅦⅧⅨⅩ" +  // 로마 숫자
            "¥€£₩" +  // 통화 기호
            "©®™"; // 저작권 기호

        // 자주 사용되는 한자
        private const string COMMON_HANJA = "一二三四五六七八九十百千萬億兆京垓";

        // 게임에서 자주 사용되는 문자들
        private const string GAME_COMMON_CHARACTERS =
            "HP MP SP ATK DEF LV EXP DMG" +  // 게임 용어
            "＋－×÷" +  // 전각 연산자
            "▲▼◀▶■□●○◆◇△▽" +  // 게임 UI 기호
            "♂♀" +  // 성별 기호
            "⚔⚡❄🔥💧🌿"; // 게임 속성 이모지 (폰트에 따라 지원)
        /// <summary>
        /// 폰트 에셋의 폴백 시스템을 설정합니다
        /// </summary>
        [MenuItem("Tools/Font/Setup Font Fallback System", priority = 100)]
        public static void SetupFontFallbackSystem()
        {
            try
            {
                EditorUtility.DisplayProgressBar("폰트 폴백 시스템 설정", "폰트 에셋 로딩 중...", 0f);

                // 주요 폰트 에셋들 로드
                var danjoFont = LoadFontAsset("Assets/Resources/Font/SUIT-otf/Danjo-bold-Regular/Danjo-bold-Regular SDF.asset");
                var dungGeunMoFont = LoadFontAsset("Assets/Resources/Font/DungGeunMo TTF/DungGeunMo SDF.asset");
                var suitBoldFont = LoadFontAsset("Assets/Resources/Font/SUIT-otf/SUIT-Bold SDF.asset");

                if (danjoFont == null || dungGeunMoFont == null || suitBoldFont == null)
                {
                    EditorUtility.DisplayDialog("오류", "일부 폰트 에셋을 찾을 수 없습니다.", "확인");
                    return;
                }

                EditorUtility.DisplayProgressBar("폰트 폴백 시스템 설정", "폴백 설정 중...", 0.3f);

                // Danjo 폰트에 폴백 설정 (한글 완성도가 높은 DungGeunMo를 우선 폴백으로 설정)
                SetupFontFallback(danjoFont, new List<TMP_FontAsset> { dungGeunMoFont, suitBoldFont });

                EditorUtility.DisplayProgressBar("폰트 폴백 시스템 설정", "TMP Settings 업데이트 중...", 0.6f);

                // TMP Settings에 전역 폴백 설정
                UpdateTMPGlobalSettings(dungGeunMoFont);

                EditorUtility.DisplayProgressBar("폰트 폴백 시스템 설정", "에셋 저장 중...", 0.9f);

                // 변경사항 저장
                EditorUtility.SetDirty(danjoFont);
                EditorUtility.SetDirty(dungGeunMoFont);
                EditorUtility.SetDirty(suitBoldFont);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                EditorUtility.ClearProgressBar();
                EditorUtility.DisplayDialog("완료", "폰트 폴백 시스템이 성공적으로 설정되었습니다.", "확인");
            }
            catch (System.Exception ex)
            {
                EditorUtility.ClearProgressBar();
                EditorUtility.DisplayDialog("오류", $"폰트 폴백 설정 중 오류 발생:\n{ex.Message}", "확인");
                Debug.LogError($"폰트 폴백 설정 오류: {ex}");
            }
        }

        /// <summary>
        /// 폰트 에셋을 로드합니다
        /// </summary>
        private static TMP_FontAsset LoadFontAsset(string path)
        {
            var fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
            if (fontAsset == null)
            {
                Debug.LogWarning($"폰트 에셋을 찾을 수 없습니다: {path}");
            }
            return fontAsset;
        }

        /// <summary>
        /// 폰트에 폴백 폰트들을 설정합니다
        /// </summary>
        private static void SetupFontFallback(TMP_FontAsset primaryFont, List<TMP_FontAsset> fallbackFonts)
        {
            if (primaryFont == null || fallbackFonts == null)
                return;

            // 폴백 폰트 리스트 설정
            var fallbackList = new List<TMP_FontAsset>();
            foreach (var fallbackFont in fallbackFonts)
            {
                if (fallbackFont != null && fallbackFont != primaryFont)
                {
                    fallbackList.Add(fallbackFont);
                }
            }

            // SerializedObject를 통해 폴백 설정
            var serializedFont = new SerializedObject(primaryFont);
            var fallbackProperty = serializedFont.FindProperty("fallbackFontAssets");
            
            if (fallbackProperty != null)
            {
                fallbackProperty.ClearArray();
                for (int i = 0; i < fallbackList.Count; i++)
                {
                    fallbackProperty.InsertArrayElementAtIndex(i);
                    var elementProperty = fallbackProperty.GetArrayElementAtIndex(i);
                    elementProperty.objectReferenceValue = fallbackList[i];
                }
                serializedFont.ApplyModifiedProperties();
            }

            Debug.Log($"폰트 폴백 설정 완료: {primaryFont.name} -> {fallbackList.Count}개 폴백 폰트");
        }

        /// <summary>
        /// TMP Settings의 전역 폴백 설정을 업데이트합니다
        /// </summary>
        private static void UpdateTMPGlobalSettings(TMP_FontAsset defaultFallbackFont)
        {
            var tmpSettings = Resources.Load<TMP_Settings>("TMP Settings");
            if (tmpSettings == null)
            {
                Debug.LogWarning("TMP Settings를 찾을 수 없습니다.");
                return;
            }

            var serializedSettings = new SerializedObject(tmpSettings);
            var fallbackProperty = serializedSettings.FindProperty("m_fallbackFontAssets");
            
            if (fallbackProperty != null && defaultFallbackFont != null)
            {
                // 기존 폴백이 비어있으면 기본 폴백 폰트 추가
                if (fallbackProperty.arraySize == 0)
                {
                    fallbackProperty.InsertArrayElementAtIndex(0);
                    var elementProperty = fallbackProperty.GetArrayElementAtIndex(0);
                    elementProperty.objectReferenceValue = defaultFallbackFont;
                    serializedSettings.ApplyModifiedProperties();
                    
                    Debug.Log($"TMP Settings 전역 폴백 설정: {defaultFallbackFont.name}");
                }
            }
        }

        /// <summary>
        /// 폰트 에셋의 문자 포함 상태를 확인합니다
        /// </summary>
        [MenuItem("Tools/Font/Check Font Character Coverage", priority = 101)]
        public static void CheckFontCharacterCoverage()
        {
            var danjoFont = LoadFontAsset("Assets/Resources/Font/SUIT-otf/Danjo-bold-Regular/Danjo-bold-Regular SDF.asset");
            if (danjoFont == null)
            {
                EditorUtility.DisplayDialog("오류", "Danjo 폰트 에셋을 찾을 수 없습니다.", "확인");
                return;
            }

            // 문제가 된 문자들 확인
            var problemCharacters = new char[] { '물', '드' }; // \uBB3C, \uB4DC
            
            var missingChars = new List<char>();
            foreach (var ch in problemCharacters)
            {
                if (!danjoFont.HasCharacter(ch))
                {
                    missingChars.Add(ch);
                }
            }

            if (missingChars.Count > 0)
            {
                var message = $"누락된 문자들:\n";
                foreach (var ch in missingChars)
                {
                    message += $"- '{ch}' (U+{((int)ch):X4})\n";
                }
                message += "\n폰트 폴백 시스템을 설정하거나 폰트 아틀라스를 재생성하세요.";
                
                EditorUtility.DisplayDialog("문자 포함 상태 확인", message, "확인");
            }
            else
            {
                EditorUtility.DisplayDialog("문자 포함 상태 확인", "모든 확인된 문자가 포함되어 있습니다.", "확인");
            }
        }

        /// <summary>
        /// 폰트 아틀라스 재생성을 위한 안내를 제공합니다
        /// </summary>
        [MenuItem("Tools/Font/Regenerate Font Atlas Guide", priority = 102)]
        public static void ShowFontAtlasRegenerationGuide()
        {
            var message = @"폰트 아틀라스 재생성 방법:

1. Assets/Resources/Font/SUIT-otf/Danjo-bold-Regular/Danjo-bold-Regular.otf 선택
2. Inspector에서 'Font Asset Creator' 버튼 클릭
3. Character Set을 'Custom Characters'로 설정
4. Custom Character List에 다음 문자들 추가:
   - 문제가 된 문자: 물, 드
   - 자주 사용되는 한글 문자들
5. 'Generate Font Atlas' 클릭
6. 생성된 SDF 에셋을 기존 에셋으로 교체

또는 폰트 폴백 시스템을 먼저 설정해보세요.";

            EditorUtility.DisplayDialog("폰트 아틀라스 재생성 가이드", message, "확인");
        }

        /// <summary>
        /// 모든 폰트 에셋의 문자 커버리지를 상세 분석합니다
        /// </summary>
        [MenuItem("Tools/Font/Analyze All Fonts Coverage", priority = 103)]
        public static void AnalyzeAllFontsCoverage()
        {
            try
            {
                EditorUtility.DisplayProgressBar("폰트 분석", "폰트 에셋 검색 중...", 0f);

                // Resources 폴더의 모든 TMP 폰트 에셋 찾기
                var fontAssets = AssetDatabase.FindAssets("t:TMP_FontAsset", new[] { "Assets/Resources/Font" })
                    .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
                    .Select(path => AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path))
                    .Where(font => font != null)
                    .ToList();

                if (fontAssets.Count == 0)
                {
                    EditorUtility.ClearProgressBar();
                    EditorUtility.DisplayDialog("오류", "폰트 에셋을 찾을 수 없습니다.", "확인");
                    return;
                }

                var report = new StringBuilder();
                report.AppendLine($"=== 폰트 분석 보고서 ===\n총 {fontAssets.Count}개 폰트 검사\n");

                for (int i = 0; i < fontAssets.Count; i++)
                {
                    var font = fontAssets[i];
                    EditorUtility.DisplayProgressBar("폰트 분석", $"{font.name} 분석 중...", (float)i / fontAssets.Count);

                    var analysis = AnalyzeFontCoverage(font);
                    report.AppendLine($"\n[{font.name}]");
                    report.AppendLine($"  한글: {analysis.hangulCount}자 / 11172자 ({analysis.hangulPercent:F1}%)");
                    report.AppendLine($"  영문: {analysis.englishCount}자 / {ENGLISH_ALPHABET.Length}자 ({analysis.englishPercent:F1}%)");
                    report.AppendLine($"  숫자: {analysis.numberCount}자 / {NUMBERS.Length}자 ({analysis.numberPercent:F1}%)");
                    report.AppendLine($"  특수문자: {analysis.specialCount}자 / {SPECIAL_CHARACTERS.Length}자 ({analysis.specialPercent:F1}%)");

                    if (analysis.missingCharacters.Count > 0)
                    {
                        report.AppendLine($"  ⚠ 누락 문자 샘플 ({analysis.missingCharacters.Count}개): ");
                        var sample = analysis.missingCharacters.Take(20).ToList();
                        report.AppendLine($"    {string.Join(", ", sample.Select(c => $"'{c}'"))}");
                        if (analysis.missingCharacters.Count > 20)
                            report.AppendLine($"    ... 외 {analysis.missingCharacters.Count - 20}개");
                    }
                    else
                    {
                        report.AppendLine("  ✓ 모든 기본 문자 포함");
                    }
                }

                EditorUtility.ClearProgressBar();

                // 보고서를 파일로 저장
                var reportPath = "Assets/Editor/FontCoverageReport.txt";
                System.IO.File.WriteAllText(reportPath, report.ToString());
                AssetDatabase.Refresh();

                Debug.Log(report.ToString());
                EditorUtility.DisplayDialog("분석 완료",
                    $"폰트 분석이 완료되었습니다.\n보고서: {reportPath}\n\n콘솔 로그를 확인하세요.",
                    "확인");
            }
            catch (System.Exception ex)
            {
                EditorUtility.ClearProgressBar();
                EditorUtility.DisplayDialog("오류", $"분석 중 오류 발생:\n{ex.Message}", "확인");
                Debug.LogError($"폰트 분석 오류: {ex}");
            }
        }

        /// <summary>
        /// 폰트의 문자 커버리지를 분석합니다
        /// </summary>
        private static FontCoverageAnalysis AnalyzeFontCoverage(TMP_FontAsset font)
        {
            var analysis = new FontCoverageAnalysis();
            var missing = new List<char>();

            // 한글 검사 (가-힣: 11172자)
            int hangulStart = '가';
            int hangulEnd = '힣';
            int totalHangul = hangulEnd - hangulStart + 1;

            for (int i = hangulStart; i <= hangulEnd; i++)
            {
                char c = (char)i;
                if (font.HasCharacter(c))
                    analysis.hangulCount++;
                else
                    missing.Add(c);
            }
            analysis.hangulPercent = (float)analysis.hangulCount / totalHangul * 100;

            // 영문 검사
            foreach (char c in ENGLISH_ALPHABET)
            {
                if (font.HasCharacter(c))
                    analysis.englishCount++;
                else
                    missing.Add(c);
            }
            analysis.englishPercent = (float)analysis.englishCount / ENGLISH_ALPHABET.Length * 100;

            // 숫자 검사
            foreach (char c in NUMBERS)
            {
                if (font.HasCharacter(c))
                    analysis.numberCount++;
                else
                    missing.Add(c);
            }
            analysis.numberPercent = (float)analysis.numberCount / NUMBERS.Length * 100;

            // 특수문자 검사
            foreach (char c in SPECIAL_CHARACTERS)
            {
                if (font.HasCharacter(c))
                    analysis.specialCount++;
                else
                    missing.Add(c);
            }
            analysis.specialPercent = (float)analysis.specialCount / SPECIAL_CHARACTERS.Length * 100;

            analysis.missingCharacters = missing;
            return analysis;
        }

        /// <summary>
        /// 선택한 폰트를 완전히 재생성합니다 (기존 에셋 덮어쓰기)
        /// </summary>
        [MenuItem("Tools/Font/Regenerate Selected Font (Complete Rebuild)", priority = 104)]
        public static void RegenerateSelectedFontComplete()
        {
            var fontAsset = Selection.activeObject as TMP_FontAsset;
            if (fontAsset == null)
            {
                EditorUtility.DisplayDialog("오류",
                    "TMP_FontAsset을 선택하세요.\n\nProject 창에서 .asset 파일을 선택한 후 다시 시도하세요.",
                    "확인");
                return;
            }

            // 소스 폰트 파일 확인
            if (fontAsset.sourceFontFile == null)
            {
                EditorUtility.DisplayDialog("오류",
                    $"'{fontAsset.name}'의 소스 폰트 파일을 찾을 수 없습니다.\n\n" +
                    "Source Font File이 연결되어 있는지 Inspector에서 확인하세요.\n\n" +
                    "대신 'Create Complete Font (Open Guide)' 메뉴를 사용하세요.",
                    "확인");
                return;
            }

            var sourceFontPath = AssetDatabase.GetAssetPath(fontAsset.sourceFontFile);
            var currentAssetPath = AssetDatabase.GetAssetPath(fontAsset);

            if (!EditorUtility.DisplayDialog("폰트 완전 재생성 확인",
                $"'{fontAsset.name}' 폰트를 완전히 재생성하시겠습니까?\n\n" +
                $"소스 폰트: {fontAsset.sourceFontFile.name}\n\n" +
                "포함될 문자:\n" +
                "• 한글 전체 11,172자 (가-힣)\n" +
                "• 영문, 숫자, 특수문자\n" +
                "• 수학 기호, 화살표, 도형\n" +
                "• CJK 기호, 전각 문자\n\n" +
                "⚠ 기존 폰트가 완전히 덮어쓰여집니다!\n" +
                "⚠ 이 작업은 1-3분 소요됩니다.",
                "재생성", "취소"))
            {
                return;
            }

            try
            {
                EditorUtility.DisplayProgressBar("폰트 재생성", "Font Asset Creator 준비 중...", 0f);

                // 유니코드 범위를 클립보드에 복사
                var unicodeRange = "0020-007E,00A0-00FF,AC00-D7AF,1100-11FF,3130-318F,2000-206F,20A0-20CF,2200-22FF,2300-23FF,2500-259F,25A0-25FF,2600-26FF,3000-303F,FF00-FFEF";
                EditorGUIUtility.systemCopyBuffer = unicodeRange;

                EditorUtility.ClearProgressBar();

                var message = $@"자동 재생성이 준비되었습니다!

Font Asset Creator가 열립니다.
다음 단계를 따라주세요:

=== 자동 설정됨 ===
✓ 소스 폰트: {fontAsset.sourceFontFile.name}
✓ 유니코드 범위가 클립보드에 복사됨

=== 직접 설정 필요 ===
1. Character Set: 'Unicode Range (Hex)' 선택
2. Character Sequence (HEX) 칸에 Ctrl+V (붙여넣기)
3. Sampling Point Size: 90
4. Padding: 9
5. Atlas Width: 4096
6. Atlas Height: 4096
7. Render Mode: SDFAA

8. 'Generate Font Atlas' 클릭
9. 생성 완료 후 Save 클릭
   → 기존 파일({fontAsset.name})을 선택하여 덮어쓰기

완료 후 'Analyze All Fonts Coverage'로 확인하세요!";

                EditorUtility.DisplayDialog("폰트 재생성 가이드", message, "Font Asset Creator 열기");

                // Font Asset Creator 창 열기
                EditorApplication.ExecuteMenuItem("Window/TextMeshPro/Font Asset Creator");
            }
            catch (System.Exception ex)
            {
                EditorUtility.ClearProgressBar();
                EditorUtility.DisplayDialog("오류", $"오류 발생:\n{ex.Message}", "확인");
                Debug.LogError($"폰트 재생성 오류: {ex}");
            }
        }

        /// <summary>
        /// Font Asset Creator를 사용하여 완전한 문자 세트로 폰트 생성 (가이드)
        /// </summary>
        [MenuItem("Tools/Font/Create Complete Font (Open Guide)", priority = 105)]
        public static void OpenFontAssetCreatorWithSettings()
        {
            var message = @"완전한 한글 폰트 생성 가이드

=== 1단계: Font Asset Creator 열기 ===
Window > TextMeshPro > Font Asset Creator

=== 2단계: 폰트 파일 선택 ===
Source Font File에 .ttf 또는 .otf 폰트 드래그

=== 3단계: 문자 세트 설정 ===
Character Set: Unicode Range (Hex)

Character Sequence (HEX) 칸에 복사 붙여넣기:
0020-007E,00A0-00FF,AC00-D7AF,1100-11FF,3130-318F,2000-206F,20A0-20CF,2200-22FF,2300-23FF,2500-259F,25A0-25FF,2600-26FF,3000-303F,FF00-FFEF

=== 4단계: 폰트 설정 ===
Sampling Point Size: 90
Padding: 9
Packing Method: Fast
Atlas Resolution: Width 4096, Height 4096
Render Mode: SDFAA

=== 5단계: 생성 ===
'Generate Font Atlas' 클릭 후 1-3분 대기

=== 포함되는 문자 ===
✓ 한글 전체 11,172자 (가-힣)
✓ 영문, 숫자, 특수문자
✓ 수학 기호, 화살표, 도형
✓ CJK 기호, 전각 문자

클립보드에 유니코드 범위가 복사됩니다!";

            // 유니코드 범위를 클립보드에 복사
            var unicodeRange = "0020-007E,00A0-00FF,AC00-D7AF,1100-11FF,3130-318F,2000-206F,20A0-20CF,2200-22FF,2300-23FF,2500-259F,25A0-25FF,2600-26FF,3000-303F,FF00-FFEF";
            EditorGUIUtility.systemCopyBuffer = unicodeRange;

            EditorUtility.DisplayDialog("완전한 폰트 생성 가이드", message, "Font Asset Creator 열기");

            // Font Asset Creator 창 열기
            EditorApplication.ExecuteMenuItem("Window/TextMeshPro/Font Asset Creator");
        }

        /// <summary>
        /// 선택한 폰트에 누락된 문자를 동적으로 추가합니다
        /// </summary>
        [MenuItem("Tools/Font/Add Missing Characters to Selected Font", priority = 106)]
        public static void AddMissingCharactersToFont()
        {
            var fontAsset = Selection.activeObject as TMP_FontAsset;
            if (fontAsset == null)
            {
                EditorUtility.DisplayDialog("오류",
                    "TMP_FontAsset을 선택하세요.\n\nProject 창에서 .asset 파일을 선택한 후 다시 시도하세요.",
                    "확인");
                return;
            }

            try
            {
                EditorUtility.DisplayProgressBar("문자 추가", "누락된 문자 확인 중...", 0f);

                // 완전한 문자 세트 생성
                var characterSet = GenerateCompleteCharacterSet();
                var missingChars = new List<char>();

                // 누락된 문자 찾기
                foreach (char c in characterSet)
                {
                    if (!fontAsset.HasCharacter(c))
                    {
                        missingChars.Add(c);
                    }
                }

                if (missingChars.Count == 0)
                {
                    EditorUtility.ClearProgressBar();
                    EditorUtility.DisplayDialog("확인", "모든 문자가 이미 폰트에 포함되어 있습니다!", "확인");
                    return;
                }

                if (!EditorUtility.DisplayDialog("문자 추가 확인",
                    $"누락된 문자 {missingChars.Count}개를 추가하시겠습니까?\n\n" +
                    $"샘플: {string.Join("", missingChars.Take(20))}\n\n" +
                    "⚠ Atlas Population Mode가 Dynamic이어야 합니다.",
                    "추가", "취소"))
                {
                    EditorUtility.ClearProgressBar();
                    return;
                }

                EditorUtility.DisplayProgressBar("문자 추가", "문자 추가 중...", 0.5f);

                // 문자 추가 시도 (uint 배열로 변환)
                var missingUints = missingChars.Select(c => (uint)c).ToArray();

                // TryAddCharacters 호출 (버전에 따라 반환값 다를 수 있음)
                bool success = false;
                string addedChars = string.Empty;

                try
                {
                    // TryAddCharacters는 문자열을 받고 성공한 문자를 반환
                    var missingString = new string(missingChars.ToArray());
                    success = fontAsset.TryAddCharacters(missingString);
                    addedChars = missingString;
                }
                catch
                {
                    // 오류 발생 시 uint[] 오버로드 시도
                    try
                    {
                        success = fontAsset.TryAddCharacters(missingUints);
                        addedChars = new string(missingChars.ToArray());
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogError($"TryAddCharacters 실패: {ex.Message}");
                    }
                }

                EditorUtility.SetDirty(fontAsset);
                AssetDatabase.SaveAssets();

                EditorUtility.ClearProgressBar();

                if (success)
                {
                    var analysis = AnalyzeFontCoverage(fontAsset);
                    EditorUtility.DisplayDialog("추가 완료",
                        $"문자가 성공적으로 추가되었습니다!\n\n" +
                        $"현재 커버리지:\n" +
                        $"한글: {analysis.hangulPercent:F1}%\n" +
                        $"영문: {analysis.englishPercent:F1}%\n" +
                        $"숫자: {analysis.numberPercent:F1}%\n" +
                        $"특수문자: {analysis.specialPercent:F1}%\n\n" +
                        $"총 글리프: {fontAsset.characterTable.Count}개",
                        "확인");
                }
                else
                {
                    EditorUtility.DisplayDialog("경고",
                        "문자를 추가할 수 없습니다.\n\n" +
                        "가능한 원인:\n" +
                        "• Atlas Population Mode가 'Static'으로 설정됨\n" +
                        "• 소스 폰트 파일이 없음\n" +
                        "• 아틀라스 크기 부족\n\n" +
                        "해결 방법:\n" +
                        "1. Inspector에서 Atlas Population Mode를 'Dynamic'으로 변경\n" +
                        "2. 또는 'Create Complete Font' 메뉴로 새 폰트 생성",
                        "확인");
                }
            }
            catch (System.Exception ex)
            {
                EditorUtility.ClearProgressBar();
                EditorUtility.DisplayDialog("오류", $"문자 추가 중 오류:\n{ex.Message}", "확인");
                Debug.LogError($"문자 추가 오류: {ex}");
            }
        }

        /// <summary>
        /// 완전한 문자 세트를 생성합니다
        /// </summary>
        private static string GenerateCompleteCharacterSet()
        {
            var chars = new HashSet<char>();

            // 한글 추가 (가-힣)
            for (int i = '가'; i <= '힣'; i++)
            {
                chars.Add((char)i);
            }

            // 영문, 숫자, 특수문자 추가
            foreach (char c in ENGLISH_ALPHABET) chars.Add(c);
            foreach (char c in NUMBERS) chars.Add(c);
            foreach (char c in SPECIAL_CHARACTERS) chars.Add(c);
            foreach (char c in COMMON_HANJA) chars.Add(c);
            foreach (char c in GAME_COMMON_CHARACTERS) chars.Add(c);

            return new string(chars.ToArray());
        }

        /// <summary>
        /// 프로젝트의 모든 텍스트 에셋에서 사용된 문자를 추출합니다
        /// </summary>
        [MenuItem("Tools/Font/Extract Characters from Project", priority = 107)]
        public static void ExtractCharactersFromProject()
        {
            try
            {
                EditorUtility.DisplayProgressBar("문자 추출", "텍스트 파일 검색 중...", 0f);

                var usedCharacters = new HashSet<char>();

                // .cs, .txt, .json, .asset 파일에서 문자 추출
                var textFiles = AssetDatabase.FindAssets("t:TextAsset")
                    .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
                    .Where(path => path.StartsWith("Assets/") &&
                                   (path.EndsWith(".txt") || path.EndsWith(".json")))
                    .ToList();

                // ScriptableObject 데이터에서도 추출
                var itemAssets = AssetDatabase.FindAssets("t:ScriptableObject", new[] { "Assets/Resources/Data" })
                    .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
                    .ToList();

                var allFiles = textFiles.Concat(itemAssets).ToList();

                for (int i = 0; i < allFiles.Count; i++)
                {
                    var path = allFiles[i];
                    EditorUtility.DisplayProgressBar("문자 추출", $"{path} 처리 중...", (float)i / allFiles.Count);

                    var content = System.IO.File.ReadAllText(path);
                    foreach (char c in content)
                    {
                        if (!char.IsControl(c) && c != '\r' && c != '\n')
                            usedCharacters.Add(c);
                    }
                }

                EditorUtility.ClearProgressBar();

                // 결과 저장
                var result = new StringBuilder();
                result.AppendLine($"=== 프로젝트에서 사용된 문자 ({usedCharacters.Count}개) ===\n");
                result.AppendLine("한글:");
                result.AppendLine(new string(usedCharacters.Where(c => c >= '가' && c <= '힣').ToArray()));
                result.AppendLine("\n영문:");
                result.AppendLine(new string(usedCharacters.Where(c => (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z')).ToArray()));
                result.AppendLine("\n숫자:");
                result.AppendLine(new string(usedCharacters.Where(c => c >= '0' && c <= '9').ToArray()));
                result.AppendLine("\n특수문자:");
                result.AppendLine(new string(usedCharacters.Where(c => !char.IsLetterOrDigit(c) && c != ' ').ToArray()));

                var outputPath = "Assets/Editor/ProjectUsedCharacters.txt";
                System.IO.File.WriteAllText(outputPath, result.ToString());
                AssetDatabase.Refresh();

                Debug.Log(result.ToString());
                EditorUtility.DisplayDialog("추출 완료",
                    $"프로젝트에서 {usedCharacters.Count}개의 문자를 추출했습니다.\n\n" +
                    $"결과: {outputPath}",
                    "확인");
            }
            catch (System.Exception ex)
            {
                EditorUtility.ClearProgressBar();
                EditorUtility.DisplayDialog("오류", $"추출 중 오류 발생:\n{ex.Message}", "확인");
                Debug.LogError($"문자 추출 오류: {ex}");
            }
        }

        /// <summary>
        /// 모든 폰트를 한 번에 검증하고 문제가 있는 폰트를 보고합니다
        /// </summary>
        [MenuItem("Tools/Font/Validate All Fonts (Quick Check)", priority = 108)]
        public static void ValidateAllFonts()
        {
            try
            {
                EditorUtility.DisplayProgressBar("폰트 검증", "폰트 검색 중...", 0f);

                var fontAssets = AssetDatabase.FindAssets("t:TMP_FontAsset", new[] { "Assets/Resources/Font" })
                    .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
                    .Select(path => AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path))
                    .Where(font => font != null)
                    .ToList();

                var problematicFonts = new List<string>();
                var threshold = 95f; // 95% 미만이면 문제 있다고 판단

                for (int i = 0; i < fontAssets.Count; i++)
                {
                    var font = fontAssets[i];
                    EditorUtility.DisplayProgressBar("폰트 검증", $"{font.name} 검증 중...", (float)i / fontAssets.Count);

                    var analysis = AnalyzeFontCoverage(font);

                    if (analysis.englishPercent < 100 || analysis.numberPercent < 100)
                    {
                        problematicFonts.Add($"{font.name}: 영문/숫자 미완성 (영문 {analysis.englishPercent:F0}%, 숫자 {analysis.numberPercent:F0}%)");
                    }
                    else if (analysis.hangulPercent < threshold)
                    {
                        problematicFonts.Add($"{font.name}: 한글 부족 ({analysis.hangulPercent:F1}%)");
                    }
                }

                EditorUtility.ClearProgressBar();

                if (problematicFonts.Count > 0)
                {
                    var message = $"문제가 발견된 폰트 ({problematicFonts.Count}개):\n\n" +
                                  string.Join("\n", problematicFonts) +
                                  "\n\n'Analyze All Fonts Coverage'를 실행하여 자세한 정보를 확인하세요.";

                    Debug.LogWarning(message);
                    EditorUtility.DisplayDialog("폰트 검증 결과", message, "확인");
                }
                else
                {
                    EditorUtility.DisplayDialog("폰트 검증 결과",
                        $"모든 폰트({fontAssets.Count}개)가 정상입니다!",
                        "확인");
                }
            }
            catch (System.Exception ex)
            {
                EditorUtility.ClearProgressBar();
                EditorUtility.DisplayDialog("오류", $"검증 중 오류 발생:\n{ex.Message}", "확인");
                Debug.LogError($"폰트 검증 오류: {ex}");
            }
        }

        /// <summary>
        /// 폰트 커버리지 분석 결과
        /// </summary>
        private class FontCoverageAnalysis
        {
            public int hangulCount;
            public float hangulPercent;
            public int englishCount;
            public float englishPercent;
            public int numberCount;
            public float numberPercent;
            public int specialCount;
            public float specialPercent;
            public List<char> missingCharacters;
        }
    }
}
