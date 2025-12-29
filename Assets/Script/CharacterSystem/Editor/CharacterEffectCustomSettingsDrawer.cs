using UnityEngine;
using UnityEditor;
using Game.CharacterSystem.Data;
using Game.CharacterSystem.Effect;
using Game.SkillCardSystem.Data;

namespace Game.CharacterSystem.Editor
{
    /// <summary>
    /// CharacterEffectCustomSettings의 커스텀 PropertyDrawer입니다.
    /// 현재 선택된 이펙트 타입에 따라 관련 설정만 표시합니다.
    /// </summary>
    [CustomPropertyDrawer(typeof(CharacterEffectCustomSettings))]
    public class CharacterEffectCustomSettingsDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float height = 0f;
            
            // 부모 CharacterEffectEntry에서 effectSO 찾기
            CharacterEffectSO effectSO = GetEffectSOFromParent(property);
            
            if (effectSO is SummonEffectSO)
            {
                // 소환 이펙트 설정: healthThreshold, summonTarget
                height += EditorGUIUtility.standardVerticalSpacing;
                height += EditorGUIUtility.singleLineHeight; // Header
                height += EditorGUIUtility.singleLineHeight; // healthThreshold
                height += EditorGUIUtility.singleLineHeight; // summonTarget
            }
            else if (effectSO is TriggerSkillOnHealthEffectSO)
            {
                // 스킬 발동 이펙트 설정
                height += EditorGUIUtility.standardVerticalSpacing;
                height += EditorGUIUtility.singleLineHeight; // Header
                height += EditorGUIUtility.singleLineHeight; // useSkillHealthRatio (먼저 선택)
                
                // useSkillHealthRatio 값에 따라 다른 필드 표시
                var useRatioProp = property.FindPropertyRelative("useSkillHealthRatio");
                bool useRatio = useRatioProp != null && useRatioProp.boolValue;
                
                if (useRatio)
                {
                    // 비율 기반: skillHealthThreshold (0~100 범위로 표시)
                    height += EditorGUIUtility.singleLineHeight; // skillHealthThreshold (비율)
                }
                else
                {
                    // 절대값 기반: skillHealthThreshold (정수)
                    height += EditorGUIUtility.singleLineHeight; // skillHealthThreshold (절대값)
                }
                
                height += EditorGUIUtility.singleLineHeight; // skillCardDefinition
                height += EditorGUIUtility.singleLineHeight; // skillCardId (레거시)
            }
            else if (effectSO != null)
            {
                // 알 수 없는 이펙트 타입
                height += EditorGUIUtility.standardVerticalSpacing;
                height += EditorGUIUtility.singleLineHeight * 2; // HelpBox
            }
            
            return height > 0 ? height : EditorGUIUtility.singleLineHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            float yPos = position.y;
            float lineHeight = EditorGUIUtility.singleLineHeight;
            float spacing = EditorGUIUtility.standardVerticalSpacing;

            // 부모 CharacterEffectEntry에서 effectSO 찾기
            CharacterEffectSO effectSO = GetEffectSOFromParent(property);

            // 이펙트 타입에 따라 관련 설정만 표시
            if (effectSO is SummonEffectSO)
            {
                // 소환 이펙트 설정
                yPos += spacing;
                Rect headerRect = new Rect(position.x, yPos, position.width, lineHeight);
                EditorGUI.LabelField(headerRect, "소환 이펙트 설정", EditorStyles.boldLabel);
                yPos += lineHeight + spacing;

                var healthThresholdProp = property.FindPropertyRelative("healthThreshold");
                Rect healthRect = new Rect(position.x, yPos, position.width, lineHeight);
                EditorGUI.PropertyField(healthRect, healthThresholdProp, new GUIContent("Health Threshold", "소환이 발동되는 체력 비율 (0.5 = 50%)"));
                yPos += lineHeight + spacing;

                var summonTargetProp = property.FindPropertyRelative("summonTarget");
                Rect targetRect = new Rect(position.x, yPos, position.width, lineHeight);
                EditorGUI.PropertyField(targetRect, summonTargetProp, new GUIContent("Summon Target", "소환할 적 캐릭터 데이터"));
            }
            else if (effectSO is TriggerSkillOnHealthEffectSO)
            {
                // 스킬 발동 이펙트 설정
                yPos += spacing;
                Rect headerRect = new Rect(position.x, yPos, position.width, lineHeight);
                EditorGUI.LabelField(headerRect, "스킬 발동 이펙트 설정", EditorStyles.boldLabel);
                yPos += lineHeight + spacing;

                // 먼저 체력 비율 사용 여부 선택
                var useSkillHealthRatioProp = property.FindPropertyRelative("useSkillHealthRatio");
                Rect ratioRect = new Rect(position.x, yPos, position.width, lineHeight);
                EditorGUI.PropertyField(ratioRect, useSkillHealthRatioProp, new GUIContent("Use Skill Health Ratio", "체력 비율 기반 사용 여부 (true면 비율, false면 절대값)"));
                yPos += lineHeight + spacing;

                // useSkillHealthRatio 값에 따라 다른 필드 표시
                bool useRatio = useSkillHealthRatioProp.boolValue;
                var skillHealthThresholdProp = property.FindPropertyRelative("skillHealthThreshold");
                
                if (useRatio)
                {
                    // 비율 기반: 0~100 범위로 표시
                    Rect thresholdRect = new Rect(position.x, yPos, position.width, lineHeight);
                    int thresholdValue = skillHealthThresholdProp.intValue;
                    thresholdValue = EditorGUI.IntSlider(thresholdRect, new GUIContent("Skill Health Threshold (%)", "스킬이 발동되는 체력 비율 (0~100%)"), thresholdValue, 0, 100);
                    skillHealthThresholdProp.intValue = thresholdValue;
                    yPos += lineHeight + spacing;
                }
                else
                {
                    // 절대값 기반: 정수 입력
                    Rect thresholdRect = new Rect(position.x, yPos, position.width, lineHeight);
                    EditorGUI.PropertyField(thresholdRect, skillHealthThresholdProp, new GUIContent("Skill Health Threshold", "스킬이 발동되는 체력 임계값 (절대값, 예: 30)"));
                    yPos += lineHeight + spacing;
                }

                var skillCardDefinitionProp = property.FindPropertyRelative("skillCardDefinition");
                Rect cardDefRect = new Rect(position.x, yPos, position.width, lineHeight);
                EditorGUI.PropertyField(cardDefRect, skillCardDefinitionProp, new GUIContent("Skill Card Definition", "발동할 스킬 카드 정의"));
                yPos += lineHeight + spacing;

                var skillCardIdProp = property.FindPropertyRelative("skillCardId");
                Rect cardIdRect = new Rect(position.x, yPos, position.width, lineHeight);
                EditorGUI.PropertyField(cardIdRect, skillCardIdProp, new GUIContent("Skill Card Id (레거시)", "발동할 스킬 카드 ID (레거시 호환용, skillCardDefinition이 우선)"));
            }
            else if (effectSO != null)
            {
                // 알 수 없는 이펙트 타입이거나 커스텀 설정이 없는 경우
                yPos += spacing;
                Rect infoRect = new Rect(position.x, yPos, position.width, lineHeight * 2);
                EditorGUI.HelpBox(infoRect, $"이 이펙트 타입 ({effectSO.GetType().Name})은 커스텀 설정을 지원하지 않습니다.", MessageType.Info);
            }

            EditorGUI.EndProperty();
        }

        /// <summary>
        /// 부모 CharacterEffectEntry에서 effectSO를 찾아 반환합니다.
        /// </summary>
        private CharacterEffectSO GetEffectSOFromParent(SerializedProperty property)
        {
            // propertyPath 예시: "phaseEffects.Array.data[0].customSettings" 또는 "characterEffects.Array.data[1].customSettings"
            // 부모 경로를 찾기 위해 "customSettings"를 제거
            var parentPath = property.propertyPath;
            var customSettingsIndex = parentPath.LastIndexOf(".customSettings");
            
            if (customSettingsIndex < 0)
            {
                // 다른 경로 형식일 수 있음
                var lastDotIndex = parentPath.LastIndexOf('.');
                if (lastDotIndex >= 0)
                {
                    parentPath = parentPath.Substring(0, lastDotIndex);
                }
            }
            else
            {
                parentPath = parentPath.Substring(0, customSettingsIndex);
            }

            // 부모 Property 찾기
            var parentProperty = property.serializedObject.FindProperty(parentPath);
            if (parentProperty != null)
            {
                var effectSOProp = parentProperty.FindPropertyRelative("effectSO");
                if (effectSOProp != null && effectSOProp.objectReferenceValue != null)
                {
                    return effectSOProp.objectReferenceValue as CharacterEffectSO;
                }
            }

            return null;
        }
    }
}

