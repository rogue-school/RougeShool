using UnityEditor;
using UnityEngine;
using Game.ItemSystem.Data;

namespace Game.ItemSystem.Editor
{
    /// <summary>
    /// PassiveItemDefinition 전용 커스텀 인스펙터
    /// 보너스 타입에 따라 관련 필드만 노출하여 가독성을 높입니다.
    /// </summary>
    [CustomEditor(typeof(PassiveItemDefinition))]
    public class PassiveItemDefinitionEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // 먼저 기본 필드들을 그리되, 우리가 커스텀으로 그릴 항목은 제외합니다.
            // 이렇게 하면 ItemDefinition에 있는 공통 필드들(아이템 ID/이름/설명/아이콘 등)이 그대로 노출됩니다.
            UnityEditor.Editor.DrawPropertiesExcluding(
                serializedObject,
                new string[] {
                    "m_Script",
                    "bonusType",
                    "targetSkill",
                    "targetSkillId",
                    "enhancementIncrements",
                    "category"
                }
            );

            // 전용 필드들
            var bonusTypeProp = serializedObject.FindProperty("bonusType");
            var targetSkillProp = serializedObject.FindProperty("targetSkill");
            var incrementsProp = serializedObject.FindProperty("enhancementIncrements");
            var categoryProp = serializedObject.FindProperty("category");

            // 보너스 타입
            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("보너스 타입 설정", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(bonusTypeProp, new GUIContent("Bonus Type", "보너스 적용 타입"));

            // 타입별 상세 필드
            var type = (PassiveBonusType)bonusTypeProp.enumValueIndex;

            if (type == PassiveBonusType.SkillDamage)
            {
                EditorGUILayout.Space(6);
                EditorGUILayout.LabelField("스킬 데미지 보너스 설정", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(targetSkillProp, new GUIContent("Target Skill", "연결할 스킬 SO (권장)"));

                // 고정 보너스는 제거되었습니다. 강화 단계 배열을 사용하세요.
            }
            else if (type == PassiveBonusType.PlayerMaxHealth)
            {
                EditorGUILayout.Space(6);
                EditorGUILayout.LabelField("플레이어 최대 체력 보너스", EditorStyles.boldLabel);
                EditorGUILayout.HelpBox("강화 단계별 증가량이 MaxHP에 적용됩니다.", MessageType.Info);
            }

            // 공통: 강화 단계 증가량
            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("강화 단계별 보너스 설정", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("강화 보너스는 누적 합계로 적용됩니다. 예) [1,2,3] → 3단계 총 +6", MessageType.Info);
            EditorGUILayout.PropertyField(incrementsProp, new GUIContent("Enhancement Increments"), true);

            // 누적 합계 미리보기
            if (incrementsProp != null && incrementsProp.isArray)
            {
                int size = incrementsProp.arraySize;
                int running = 0;
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                for (int i = 0; i < size; i++)
                {
                    var el = incrementsProp.GetArrayElementAtIndex(i);
                    int v = el.intValue;
                    running += v;
                    if (i > 0) sb.Append("  ");
                    sb.Append($"L{i + 1}: +{running}");
                }
                if (size > 0)
                {
                    EditorGUILayout.LabelField("누적 합계 미리보기", sb.ToString());
                }
            }

            // 카테고리
            EditorGUILayout.Space(6);
            EditorGUILayout.PropertyField(categoryProp, new GUIContent("Category"));

            serializedObject.ApplyModifiedProperties();
        }

        // (helper 제거)
    }
}


