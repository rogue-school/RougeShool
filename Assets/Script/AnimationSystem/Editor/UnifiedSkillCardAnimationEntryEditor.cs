using UnityEngine;
using UnityEditor;
using Game.AnimationSystem.Data;

namespace Game.AnimationSystem.Editor
{
    /// <summary>
    /// UnifiedSkillCardAnimationEntry의 커스텀 에디터
    /// 드롭다운을 통한 애니메이션 스크립트 타입 선택을 제공합니다.
    /// </summary>
    [CustomPropertyDrawer(typeof(UnifiedSkillCardAnimationEntry))]
    public class UnifiedSkillCardAnimationEntryEditor : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            // 기본 레이아웃 설정
            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            // 헤더 표시
            var headerRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            EditorGUI.LabelField(headerRect, label, EditorStyles.boldLabel);

            var yOffset = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            // 스킬카드 정의
            var skillCardDefinitionRect = new Rect(position.x, position.y + yOffset, position.width, EditorGUIUtility.singleLineHeight);
            var skillCardDefinitionProp = property.FindPropertyRelative("skillCardDefinition");
            EditorGUI.PropertyField(skillCardDefinitionRect, skillCardDefinitionProp, new GUIContent("스킬카드 정의"));

            yOffset += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            // 소유자 정책
            var ownerPolicyRect = new Rect(position.x, position.y + yOffset, position.width, EditorGUIUtility.singleLineHeight);
            var ownerPolicyProp = property.FindPropertyRelative("ownerPolicy");
            EditorGUI.PropertyField(ownerPolicyRect, ownerPolicyProp, new GUIContent("소유자 정책"));

            yOffset += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            // 애니메이션 섹션들 (그룹화)
            DrawAnimationGroup(property, position, ref yOffset, "공용 애니메이션", "Shared Animations", 
                new[] { ("spawnAnimation", "생성", "Spawn"), ("useAnimation", "사용", "Use"), ("vanishAnimation", "소멸", "Vanish") });
            
            DrawAnimationGroup(property, position, ref yOffset, "플레이어 전용 애니메이션", "Player Only Animations", 
                new[] { ("dragAnimation", "드래그", "Drag"), ("dropAnimation", "드랍", "Drop") });
            
            DrawAnimationGroup(property, position, ref yOffset, "적 전용 애니메이션", "Enemy Only Animations", 
                new[] { ("moveAnimation", "이동", "Move"), ("moveToCombatSlotAnimation", "전투슬롯 이동", "Move To Combat Slot") });

            EditorGUI.indentLevel = indent;
            EditorGUI.EndProperty();
        }

        private void DrawAnimationGroup(SerializedProperty property, Rect position, ref float yOffset, string koreanGroupName, string englishGroupName, (string propName, string koreanLabel, string englishLabel)[] animations)
        {
            // 그룹 헤더
            var groupHeaderRect = new Rect(position.x, position.y + yOffset, position.width, EditorGUIUtility.singleLineHeight);
            EditorGUI.LabelField(groupHeaderRect, $"{koreanGroupName} / {englishGroupName}", EditorStyles.boldLabel);
            yOffset += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            // 들여쓰기 시작
            EditorGUI.indentLevel++;
            
            foreach (var (propName, koreanLabel, englishLabel) in animations)
            {
                // 애니메이션 설정 프로퍼티
                var animationProp = property.FindPropertyRelative(propName);
                if (animationProp != null)
                {
                    // 애니메이션 스크립트 타입 드롭다운
                    var scriptTypeRect = new Rect(position.x, position.y + yOffset, position.width, EditorGUIUtility.singleLineHeight);
                    var scriptTypeProp = animationProp.FindPropertyRelative("animationScriptType");
                    
                    if (scriptTypeProp != null)
                    {
                        EditorGUI.PropertyField(scriptTypeRect, scriptTypeProp, new GUIContent($"{koreanLabel} / {englishLabel}"));
                    }
                    else
                    {
                        EditorGUI.LabelField(scriptTypeRect, $"{koreanLabel} / {englishLabel}", "프로퍼티를 찾을 수 없습니다.");
                    }
                    
                    yOffset += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                }
            }
            
            // 들여쓰기 종료
            EditorGUI.indentLevel--;
            
            // 그룹 간 간격
            yOffset += EditorGUIUtility.standardVerticalSpacing;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            // 기본 높이: 헤더 + 스킬카드 정의 + 소유자 정책 + 3개 그룹 (각각 헤더 + 애니메이션들)
            // 공용: 헤더 + 3개 애니메이션, 플레이어: 헤더 + 2개 애니메이션, 적: 헤더 + 2개 애니메이션
            int totalLines = 1 + 1 + 1 + 3 + 3 + 2 + 2; // 헤더 + 정의 + 정책 + 그룹헤더들 + 애니메이션들
            int totalSpacing = 2 + 3 + 2 + 2; // 기본 간격 + 그룹 간격들
            return EditorGUIUtility.singleLineHeight * totalLines + EditorGUIUtility.standardVerticalSpacing * totalSpacing;
        }
    }
}
