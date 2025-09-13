using UnityEngine;
using UnityEditor;
using Game.AnimationSystem.Data;

namespace Game.AnimationSystem.Editor
{
    /// <summary>
    /// SkillCardAnimationSettings의 커스텀 PropertyDrawer
    /// 애니메이션 타입별로 해당하는 스크립트만 표시합니다.
    /// </summary>
    [CustomPropertyDrawer(typeof(SkillCardAnimationSettings))]
    public class SkillCardAnimationSettingsDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            // AnimationScriptType 필드 찾기
            SerializedProperty animationScriptTypeProp = property.FindPropertyRelative("animationScriptType");

            // 애니메이션 타입에 따른 필터링된 옵션 생성
            var filteredOptions = GetFilteredOptions(label.text);
            
            if (filteredOptions.Length > 0)
            {
                // 필터링된 드롭다운 표시
                int currentIndex = GetCurrentIndex(animationScriptTypeProp.enumValueIndex, filteredOptions);
                int newIndex = EditorGUI.Popup(position, label.text, currentIndex, filteredOptions);
                
                if (newIndex != currentIndex && newIndex >= 0 && newIndex < filteredOptions.Length)
                {
                    animationScriptTypeProp.enumValueIndex = GetEnumIndexFromFilteredIndex(newIndex, filteredOptions);
                }
            }
            else
            {
                // 필터링된 옵션이 없으면 기본 드롭다운 표시
                EditorGUI.PropertyField(position, animationScriptTypeProp, label);
            }

            EditorGUI.EndProperty();
        }

        private string[] GetFilteredOptions(string animationTypeLabel)
        {
            // 라벨에서 애니메이션 타입 추출
            string lowerLabel = animationTypeLabel.ToLower();
            
            // 디버깅을 위한 로그
            Debug.Log($"[SkillCardAnimationSettingsDrawer] 라벨: '{animationTypeLabel}' -> 소문자: '{lowerLabel}'");
            
            if (lowerLabel.Contains("spawn") || lowerLabel.Contains("생성"))
            {
                Debug.Log("[SkillCardAnimationSettingsDrawer] 생성 애니메이션 필터링 적용");
                return new string[] { "Default Skill Card Spawn Animation" };
            }
            else if (lowerLabel.Contains("move") && !lowerLabel.Contains("movetocombatslot") || lowerLabel.Contains("이동") && !lowerLabel.Contains("전투슬롯"))
            {
                Debug.Log("[SkillCardAnimationSettingsDrawer] 이동 애니메이션 필터링 적용");
                return new string[] { "Default Skill Card Move Animation" };
            }
            else if (lowerLabel.Contains("movetocombatslot") || lowerLabel.Contains("전투슬롯"))
            {
                Debug.Log("[SkillCardAnimationSettingsDrawer] 전투슬롯 이동 애니메이션 필터링 적용");
                return new string[] { "Default Skill Card Move To Combat Slot Animation" };
            }
            else if (lowerLabel.Contains("use") || lowerLabel.Contains("사용"))
            {
                Debug.Log("[SkillCardAnimationSettingsDrawer] 사용 애니메이션 필터링 적용");
                return new string[] { "Default Skill Card Use Animation" };
            }
            else if (lowerLabel.Contains("drag") || lowerLabel.Contains("드래그"))
            {
                Debug.Log("[SkillCardAnimationSettingsDrawer] 드래그 애니메이션 필터링 적용");
                return new string[] { "Default Skill Card Drag Animation" };
            }
            else if (lowerLabel.Contains("drop") || lowerLabel.Contains("드랍"))
            {
                Debug.Log("[SkillCardAnimationSettingsDrawer] 드랍 애니메이션 필터링 적용");
                return new string[] { "Default Skill Card Drop Animation" };
            }
            else if (lowerLabel.Contains("vanish") || lowerLabel.Contains("소멸"))
            {
                Debug.Log("[SkillCardAnimationSettingsDrawer] 소멸 애니메이션 필터링 적용");
                return new string[] { "Default Skill Card Vanish Animation" };
            }
            
            Debug.Log("[SkillCardAnimationSettingsDrawer] 기본 옵션 반환 (필터링 실패)");
            // 기본적으로 모든 옵션 반환 (None 제거)
            return new string[] { "Default Skill Card Spawn Animation", "Default Skill Card Move Animation", 
                                "Default Skill Card Move To Combat Slot Animation", "Default Skill Card Use Animation",
                                "Default Skill Card Drag Animation", "Default Skill Card Drop Animation", 
                                "Default Skill Card Vanish Animation" };
        }

        private int GetCurrentIndex(int enumValueIndex, string[] filteredOptions)
        {
            string enumName = ((AnimationScriptType)enumValueIndex).ToString();
            
            for (int i = 0; i < filteredOptions.Length; i++)
            {
                if (filteredOptions[i].Replace(" ", "") == enumName)
                {
                    return i;
                }
            }
            
            return 0; // 첫 번째 옵션 (None이 없으므로)
        }

        private int GetEnumIndexFromFilteredIndex(int filteredIndex, string[] filteredOptions)
        {
            string selectedOption = filteredOptions[filteredIndex];
            
            return selectedOption switch
            {
                "Default Skill Card Spawn Animation" => 1,
                "Default Skill Card Move Animation" => 2,
                "Default Skill Card Move To Combat Slot Animation" => 3,
                "Default Skill Card Use Animation" => 4,
                "Default Skill Card Drag Animation" => 5,
                "Default Skill Card Drop Animation" => 6,
                "Default Skill Card Vanish Animation" => 7,
                _ => 1 // 기본값을 Spawn으로 설정
            };
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }
    }
}
