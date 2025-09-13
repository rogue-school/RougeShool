using UnityEngine;
using UnityEditor;
using Game.AnimationSystem.Data;
using Game.SkillCardSystem.Data;

namespace Game.AnimationSystem.Editor
{
    /// <summary>
    /// UnifiedSkillCardAnimationDatabase의 커스텀 에디터
    /// 그룹화된 애니메이션 설정 UI를 제공합니다.
    /// </summary>
    [CustomEditor(typeof(UnifiedSkillCardAnimationDatabase))]
    public class UnifiedSkillCardAnimationDatabaseEditor : UnityEditor.Editor
    {
        private SerializedProperty skillCardAnimationsProp;
        private bool showAnimations = true;

        private void OnEnable()
        {
            skillCardAnimationsProp = serializedObject.FindProperty("skillCardAnimations");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // 헤더
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("통합 스킬카드 애니메이션 매핑", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            // 애니메이션 목록 토글
            showAnimations = EditorGUILayout.Foldout(showAnimations, 
                $"Skill Card Animations ({skillCardAnimationsProp.arraySize})", true);

            if (showAnimations)
            {
                EditorGUI.indentLevel++;
                
                // 각 애니메이션 엔트리 표시
                for (int i = 0; i < skillCardAnimationsProp.arraySize; i++)
                {
                    DrawAnimationEntry(i);
                }
                
                EditorGUI.indentLevel--;
            }

            // 추가/제거 버튼
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("+", GUILayout.Width(30)))
            {
                skillCardAnimationsProp.arraySize++;
                var newElement = skillCardAnimationsProp.GetArrayElementAtIndex(skillCardAnimationsProp.arraySize - 1);
                // 기본값 설정
                newElement.FindPropertyRelative("ownerPolicy").enumValueIndex = 0; // Shared
            }
            
            if (GUILayout.Button("-", GUILayout.Width(30)) && skillCardAnimationsProp.arraySize > 0)
            {
                skillCardAnimationsProp.arraySize--;
            }
            
            EditorGUILayout.EndHorizontal();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawAnimationEntry(int index)
        {
            var elementProp = skillCardAnimationsProp.GetArrayElementAtIndex(index);
            
            EditorGUILayout.BeginVertical("box");
            
            // Element 헤더
            EditorGUILayout.LabelField($"Element {index}", EditorStyles.boldLabel);
            
            // 스킬카드 정의
            var skillCardDefinitionProp = elementProp.FindPropertyRelative("skillCardDefinition");
            EditorGUILayout.PropertyField(skillCardDefinitionProp, new GUIContent("스킬카드 정의"));
            
            // 소유자 정책
            var ownerPolicyProp = elementProp.FindPropertyRelative("ownerPolicy");
            EditorGUILayout.PropertyField(ownerPolicyProp, new GUIContent("소유자 정책"));
            
            EditorGUILayout.Space();
            
            // 애니메이션 그룹들
            DrawAnimationGroup(elementProp, "공용 애니메이션", "Shared Animations", 
                new[] { ("spawnAnimation", "생성", "Spawn"), ("useAnimation", "사용", "Use"), ("vanishAnimation", "소멸", "Vanish") });
            
            DrawAnimationGroup(elementProp, "플레이어 전용 애니메이션", "Player Only Animations", 
                new[] { ("dragAnimation", "드래그", "Drag"), ("dropAnimation", "드랍", "Drop") });
            
            DrawAnimationGroup(elementProp, "적 전용 애니메이션", "Enemy Only Animations", 
                new[] { ("moveAnimation", "이동", "Move"), ("moveToCombatSlotAnimation", "전투슬롯 이동", "Move To Combat Slot") });
            
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }

        private void DrawAnimationGroup(SerializedProperty elementProp, string koreanGroupName, string englishGroupName, (string propName, string koreanLabel, string englishLabel)[] animations)
        {
            // 그룹 헤더
            EditorGUILayout.LabelField($"{koreanGroupName} / {englishGroupName}", EditorStyles.boldLabel);
            
            EditorGUI.indentLevel++;
            
            foreach (var (propName, koreanLabel, englishLabel) in animations)
            {
                var animationProp = elementProp.FindPropertyRelative(propName);
                if (animationProp != null)
                {
                    var scriptTypeProp = animationProp.FindPropertyRelative("animationScriptType");
                    if (scriptTypeProp != null)
                    {
                        // 필터링된 드롭다운 표시
                        DrawFilteredAnimationDropdown(scriptTypeProp, $"{koreanLabel} / {englishLabel}", propName);
                    }
                }
            }
            
            EditorGUI.indentLevel--;
            EditorGUILayout.Space();
        }

        private void DrawFilteredAnimationDropdown(SerializedProperty scriptTypeProp, string label, string animationType)
        {
            // 애니메이션 타입별 필터링된 옵션 생성
            var filteredOptions = GetFilteredOptionsForType(animationType);
            var filteredLabels = GetFilteredLabelsForType(animationType);
            
            // 현재 선택된 값의 인덱스 찾기
            int currentIndex = GetCurrentIndexForFilteredOptions(scriptTypeProp.enumValueIndex, filteredOptions);
            
            // 필터링된 드롭다운 표시
            int newIndex = EditorGUILayout.Popup(label, currentIndex, filteredLabels);
            
            if (newIndex != currentIndex && newIndex >= 0 && newIndex < filteredOptions.Length)
            {
                scriptTypeProp.enumValueIndex = filteredOptions[newIndex];
            }
        }

        private int[] GetFilteredOptionsForType(string animationType)
        {
            return animationType switch
            {
                "spawnAnimation" => new int[] { 1 }, // DefaultSkillCardSpawnAnimation
                "moveAnimation" => new int[] { 2 }, // DefaultSkillCardMoveAnimation
                "moveToCombatSlotAnimation" => new int[] { 3 }, // DefaultSkillCardMoveToCombatSlotAnimation
                "useAnimation" => new int[] { 4 }, // DefaultSkillCardUseAnimation
                "dragAnimation" => new int[] { 5 }, // DefaultSkillCardDragAnimation
                "dropAnimation" => new int[] { 6 }, // DefaultSkillCardDropAnimation
                "vanishAnimation" => new int[] { 7 }, // DefaultSkillCardVanishAnimation
                _ => new int[] { 1 } // 기본값
            };
        }

        private string[] GetFilteredLabelsForType(string animationType)
        {
            return animationType switch
            {
                "spawnAnimation" => new string[] { "Default Skill Card Spawn Animation" },
                "moveAnimation" => new string[] { "Default Skill Card Move Animation" },
                "moveToCombatSlotAnimation" => new string[] { "Default Skill Card Move To Combat Slot Animation" },
                "useAnimation" => new string[] { "Default Skill Card Use Animation" },
                "dragAnimation" => new string[] { "Default Skill Card Drag Animation" },
                "dropAnimation" => new string[] { "Default Skill Card Drop Animation" },
                "vanishAnimation" => new string[] { "Default Skill Card Vanish Animation" },
                _ => new string[] { "Default Skill Card Spawn Animation" }
            };
        }

        private int GetCurrentIndexForFilteredOptions(int enumValueIndex, int[] filteredOptions)
        {
            for (int i = 0; i < filteredOptions.Length; i++)
            {
                if (filteredOptions[i] == enumValueIndex)
                {
                    return i;
                }
            }
            return 0; // 첫 번째 옵션
        }
    }
}
