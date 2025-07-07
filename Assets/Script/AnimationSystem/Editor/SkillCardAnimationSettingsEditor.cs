using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.Collections.Generic;
using AnimationSystem.Data;
using AnimationSystem.Interface;

namespace AnimationSystem.Editor
{
    [CustomPropertyDrawer(typeof(SkillCardAnimationSettings))]
public class SkillCardAnimationSettingsEditor : PropertyDrawer
{
    private List<Type> monoBehaviourTypes;
    private string[] typeNames;
    private Dictionary<string, string[]> methodCache = new();

    public SkillCardAnimationSettingsEditor()
    {
        // IAnimationScript를 상속한 MonoBehaviour만 필터링
        var animationScriptType = typeof(AnimationSystem.Interface.IAnimationScript);
        monoBehaviourTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Where(t => typeof(MonoBehaviour).IsAssignableFrom(t)
                && animationScriptType.IsAssignableFrom(t)
                && !t.IsAbstract && t.IsPublic)
            .OrderBy(t => t.FullName)
            .ToList();
        // 'None' 항목 추가
        typeNames = new[] { "None" }.Concat(monoBehaviourTypes.Select(t => t.FullName)).ToArray();
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);
        var indent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;

        float y = position.y;
        float lineHeight = EditorGUIUtility.singleLineHeight + 2;
        float labelWidth = 140f;
        float fieldWidth = position.width - labelWidth - 8;

        // 명확한 라벨 표시 (한글+영어 병기)
        string lowerLabel = label.text.ToLower();
        string contextLabel = lowerLabel.Contains("cast") ? "시전 애니메이션 (Cast Animation)"
            : lowerLabel.Contains("slotmove") ? "슬롯 이동 애니메이션 (Slot Move Animation)"
            : lowerLabel.Contains("battleslotplace") ? "전투 슬롯 배치 애니메이션 (Battle Slot Place Animation)"
            : lowerLabel.Contains("carduse") ? "카드 사용 애니메이션 (Card Use Animation)"
            : lowerLabel.Contains("playercardhover") ? "플레이어 카드 호버 애니메이션 (Player Card Hover Animation)"
            : label.text;
        EditorGUI.LabelField(new Rect(position.x, y, position.width, lineHeight), contextLabel, EditorStyles.miniBoldLabel);
        y += lineHeight;

        // Animation Script Type
        var scriptTypeProp = property.FindPropertyRelative("animationScriptType");
        if (typeNames == null || typeNames.Length == 0)
            typeNames = new[] { "None" };
        int selectedTypeIdx = Array.IndexOf(typeNames, string.IsNullOrEmpty(scriptTypeProp.stringValue) ? "None" : scriptTypeProp.stringValue);
        if (selectedTypeIdx < 0) selectedTypeIdx = 0;
        EditorGUI.LabelField(new Rect(position.x, y, labelWidth, lineHeight), "Animation Script Type");
        int newTypeIdx = EditorGUI.Popup(new Rect(position.x + labelWidth, y, fieldWidth, lineHeight), selectedTypeIdx, typeNames);
        if (newTypeIdx == 0)
        {
            scriptTypeProp.stringValue = string.Empty;
        }
        else if (typeNames.Length > 0 && newTypeIdx >= 0 && newTypeIdx < typeNames.Length)
        {
            scriptTypeProp.stringValue = typeNames[newTypeIdx];
        }
        y += lineHeight;

        EditorGUI.indentLevel = indent;
        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float h = EditorGUIUtility.singleLineHeight * 3 + 12;
        return h;
    }
}
} 