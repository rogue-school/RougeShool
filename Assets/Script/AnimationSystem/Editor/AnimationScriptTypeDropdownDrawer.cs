using UnityEditor;
using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;
using AnimationSystem.Data;
using AnimationSystem.Interface;

[CustomPropertyDrawer(typeof(CharacterAnimationSettings))]
[CustomPropertyDrawer(typeof(SkillCardAnimationSettings))]
public class AnimationScriptTypeDropdownDrawer : PropertyDrawer
{
    private static List<Type> animationScriptTypes;
    private static string[] animationScriptTypeNames;
    private static bool initialized = false;

    private void Init()
    {
        if (initialized) return;
        animationScriptTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Where(t => typeof(IAnimationScript).IsAssignableFrom(t) && !t.IsAbstract && t.IsClass)
            .OrderBy(t => t.FullName)
            .ToList();
        animationScriptTypeNames = animationScriptTypes.Select(t => t.FullName).ToArray();
        initialized = true;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        Init();
        EditorGUI.BeginProperty(position, label, property);

        // animationScriptType 필드만 드롭다운으로 커스텀
        var animScriptTypeProp = property.FindPropertyRelative("animationScriptType");
        Rect dropdownRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
        int selected = Mathf.Max(0, Array.IndexOf(animationScriptTypeNames, animScriptTypeProp.stringValue));
        int newSelected = EditorGUI.Popup(
            dropdownRect,
            "애니메이션 스크립트 타입",
            selected,
            animationScriptTypeNames
        );
        if (newSelected != selected)
        {
            animScriptTypeProp.stringValue = animationScriptTypeNames[newSelected];
        }

        // 나머지 필드는 Unity 기본 Inspector로 한 번에 그리기
        float y = position.y + EditorGUIUtility.singleLineHeight + 2;
        Rect restRect = new Rect(position.x, y, position.width, EditorGUI.GetPropertyHeight(property, label, true) - EditorGUIUtility.singleLineHeight - 2);
        EditorGUI.PropertyField(restRect, property, true);

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        // 전체 높이에서 드롭다운 한 줄만큼만 추가
        return EditorGUI.GetPropertyHeight(property, label, true) + EditorGUIUtility.singleLineHeight + 2;
    }
} 