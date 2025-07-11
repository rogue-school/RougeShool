using UnityEditor;
using UnityEngine;
using AnimationSystem.Data;
using System;
using System.Linq;
using System.Collections.Generic;
using AnimationSystem.Interface;

[CustomEditor(typeof(PlayerSkillCardAnimationDatabase))]
public class PlayerSkillCardAnimationDatabaseEditor : Editor
{
    private SerializedProperty skillCardAnimationsProp;

    private List<Type> spawnTypes;
    private string[] spawnTypeNames;
    private string[] spawnTypeFullNames;
    private List<Type> useTypes;
    private string[] useTypeNames;
    private string[] useTypeFullNames;
    private List<Type> dragTypes;
    private string[] dragTypeNames;
    private string[] dragTypeFullNames;
    private List<Type> dropTypes;
    private string[] dropTypeNames;
    private string[] dropTypeFullNames;
    private List<Type> vanishTypes;
    private string[] vanishTypeNames;
    private string[] vanishTypeFullNames;

    private List<Type> SortDefaultFirst(List<Type> types)
    {
        var defaultType = types.FirstOrDefault(t => t.Name.Contains("Default"));
        if (defaultType != null)
        {
            return new List<Type> { defaultType }
                .Concat(types.Where(t => t != defaultType)).ToList();
        }
        return types;
    }

    private void OnEnable()
    {
        skillCardAnimationsProp = serializedObject.FindProperty("skillCardAnimations");
        spawnTypes = SortDefaultFirst(AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Where(t => typeof(ISkillCardSpawnAnimationScript).IsAssignableFrom(t) && !t.IsAbstract && t.IsClass)
            .OrderBy(t => t.FullName).ToList());
        spawnTypeNames = spawnTypes.Select(t => t.Name).ToArray();
        spawnTypeFullNames = spawnTypes.Select(t => t.FullName).ToArray();

        useTypes = SortDefaultFirst(AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Where(t => typeof(ISkillCardUseAnimationScript).IsAssignableFrom(t) && !t.IsAbstract && t.IsClass)
            .OrderBy(t => t.FullName).ToList());
        useTypeNames = useTypes.Select(t => t.Name).ToArray();
        useTypeFullNames = useTypes.Select(t => t.FullName).ToArray();

        dragTypes = SortDefaultFirst(AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Where(t => typeof(ISkillCardDragAnimationScript).IsAssignableFrom(t) && !t.IsAbstract && t.IsClass)
            .OrderBy(t => t.FullName).ToList());
        dragTypeNames = dragTypes.Select(t => t.Name).ToArray();
        dragTypeFullNames = dragTypes.Select(t => t.FullName).ToArray();

        dropTypes = SortDefaultFirst(AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Where(t => typeof(ISkillCardDropAnimationScript).IsAssignableFrom(t) && !t.IsAbstract && t.IsClass)
            .OrderBy(t => t.FullName).ToList());
        dropTypeNames = dropTypes.Select(t => t.Name).ToArray();
        dropTypeFullNames = dropTypes.Select(t => t.FullName).ToArray();

        vanishTypes = SortDefaultFirst(AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Where(t => typeof(ISkillCardDeathAnimationScript).IsAssignableFrom(t) && !t.IsAbstract && t.IsClass)
            .OrderBy(t => t.FullName).ToList());
        vanishTypeNames = vanishTypes.Select(t => t.Name).ToArray();
        vanishTypeFullNames = vanishTypes.Select(t => t.FullName).ToArray();
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.LabelField("플레이어 스킬카드 애니메이션 매핑", EditorStyles.boldLabel);
        if (skillCardAnimationsProp != null && skillCardAnimationsProp.isArray)
        {
            for (int i = 0; i < skillCardAnimationsProp.arraySize; i++)
            {
                var entryProp = skillCardAnimationsProp.GetArrayElementAtIndex(i);
                EditorGUILayout.BeginVertical(GUI.skin.box);
                EditorGUILayout.LabelField($"Element {i}", EditorStyles.boldLabel);

                var skillCardProp = entryProp.FindPropertyRelative("playerSkillCard");
                EditorGUILayout.PropertyField(skillCardProp, new GUIContent("플레이어 스킬카드 SO"));

                var spawnAnimProp = entryProp.FindPropertyRelative("spawnAnimation");
                DrawAnimationSettingsDropdown(spawnAnimProp, "생성 애니메이션", spawnTypeNames, spawnTypeFullNames, spawnTypes);

                var useAnimProp = entryProp.FindPropertyRelative("useAnimation");
                DrawAnimationSettingsDropdown(useAnimProp, "사용 애니메이션", useTypeNames, useTypeFullNames, useTypes);

                var dragAnimProp = entryProp.FindPropertyRelative("dragAnimation");
                DrawAnimationSettingsDropdown(dragAnimProp, "드래그 애니메이션", dragTypeNames, dragTypeFullNames, dragTypes);

                var dropAnimProp = entryProp.FindPropertyRelative("dropAnimation");
                DrawAnimationSettingsDropdown(dropAnimProp, "드롭 애니메이션", dropTypeNames, dropTypeFullNames, dropTypes);

                var vanishAnimProp = entryProp.FindPropertyRelative("vanishAnimation");
                DrawAnimationSettingsDropdown(vanishAnimProp, "소멸 애니메이션", vanishTypeNames, vanishTypeFullNames, vanishTypes);

                EditorGUILayout.EndVertical();
            }
        }
        if (GUILayout.Button("+", GUILayout.Width(30)))
        {
            skillCardAnimationsProp.arraySize++;
        }
        if (skillCardAnimationsProp.arraySize > 0 && GUILayout.Button("-", GUILayout.Width(30)))
        {
            skillCardAnimationsProp.arraySize--;
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawAnimationSettingsDropdown(SerializedProperty animSettingsProp, string label, string[] typeNames, string[] typeFullNames, List<Type> typeList = null)
    {
        EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
        var animScriptTypeProp = animSettingsProp.FindPropertyRelative("animationScriptType");
        int selected = Mathf.Max(0, Array.IndexOf(typeFullNames, animScriptTypeProp.stringValue));
        int newSelected = EditorGUILayout.Popup("애니메이션 스크립트 타입", selected, typeNames);
        if (typeList == null) return;
        if (newSelected != selected && newSelected >= 0 && newSelected < typeList.Count)
        {
            animScriptTypeProp.stringValue = typeList[newSelected].AssemblyQualifiedName;
        }
    }
} 