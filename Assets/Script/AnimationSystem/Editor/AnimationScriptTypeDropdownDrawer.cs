using UnityEditor;
using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;
using AnimationSystem.Data;
using AnimationSystem.Interface;

namespace AnimationSystem.Editor {

[CustomPropertyDrawer(typeof(CharacterAnimationSettings))]
[CustomPropertyDrawer(typeof(SkillCardAnimationSettings))]
public class AnimationScriptTypeDropdownDrawer : PropertyDrawer
{
    private static List<Type> animationScriptTypes;
    private static string[] animationScriptTypeNames; // 드롭다운에 표시될 이름 (Name)
    private static string[] animationScriptTypeFullNames; // 실제 값 (FullName)

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        string fieldName = property.propertyPath.Split('.').Last().ToLower();
        string parentPath = property.propertyPath;

        Type filterInterface = null;
        if (fieldName.Contains("spawn")) {
            if (parentPath.Contains("character") || parentPath.Contains("Character")) {
                filterInterface = typeof(ICharacterSpawnAnimationScript);
            }
            else if (parentPath.Contains("skillcard") || parentPath.Contains("SkillCard")) {
                filterInterface = typeof(ISkillCardSpawnAnimationScript);
            }
        }
        else if (fieldName.Contains("death")) {
            if (parentPath.Contains("character") || parentPath.Contains("Character")) {
                filterInterface = typeof(ICharacterDeathAnimationScript);
            }
            else if (parentPath.Contains("skillcard") || parentPath.Contains("SkillCard")) {
                filterInterface = typeof(ISkillCardDeathAnimationScript);
            }
        }
        else if (fieldName.Contains("movetocombatslot")) {
            if (parentPath.Contains("character") || parentPath.Contains("Character")) {
                filterInterface = typeof(ICharacterCombatSlotMoveAnimationScript);
            }
            else if (parentPath.Contains("skillcard") || parentPath.Contains("SkillCard")) {
                filterInterface = typeof(ISkillCardCombatSlotMoveAnimationScript);
            }
        }
        else if (fieldName.Contains("move")) {
            if (parentPath.Contains("character") || parentPath.Contains("Character")) {
                filterInterface = typeof(ICharacterMoveAnimationScript);
            }
            else if (parentPath.Contains("skillcard") || parentPath.Contains("SkillCard")) {
                filterInterface = typeof(ISkillCardMoveAnimationScript);
            }
        }

        if (filterInterface == null) {
            animationScriptTypes = new List<Type>();
            animationScriptTypeNames = new string[0];
            animationScriptTypeFullNames = new string[0];
        } else {
            var allTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t => !t.IsAbstract && t.IsClass)
                .ToList();
            animationScriptTypes = allTypes
                .Where(t => filterInterface.IsAssignableFrom(t))
                .OrderBy(t => t.FullName)
                .ToList();
            animationScriptTypeNames = animationScriptTypes.Select(t => t.Name).ToArray(); // Name만 표시
            animationScriptTypeFullNames = animationScriptTypes.Select(t => t.FullName).ToArray(); // 실제 값
        }

        EditorGUI.BeginProperty(position, label, property);
        var animScriptTypeProp = property.FindPropertyRelative("animationScriptType");
        Rect dropdownRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
        // 현재 값이 FullName이므로, FullName 배열에서 인덱스 찾기
        int selected = Mathf.Max(0, Array.IndexOf(animationScriptTypeFullNames, animScriptTypeProp.stringValue));
        int newSelected = EditorGUI.Popup(
            dropdownRect,
            "애니메이션 스크립트 타입",
            selected,
            animationScriptTypeNames
        );
        if (newSelected != selected && newSelected >= 0 && newSelected < animationScriptTypeFullNames.Length)
        {
            animScriptTypeProp.stringValue = animationScriptTypeFullNames[newSelected]; // 실제 값은 FullName으로 저장
        }

        float y = position.y + EditorGUIUtility.singleLineHeight + 2;
        Rect restRect = new Rect(position.x, y, position.width, EditorGUI.GetPropertyHeight(property, label, true) - EditorGUIUtility.singleLineHeight - 2);
        EditorGUI.PropertyField(restRect, property, true);

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUI.GetPropertyHeight(property, label, true) + EditorGUIUtility.singleLineHeight + 2;
    }
}
} 