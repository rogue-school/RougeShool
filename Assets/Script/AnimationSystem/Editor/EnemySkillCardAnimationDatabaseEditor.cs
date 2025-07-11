using UnityEditor;
using UnityEngine;
using AnimationSystem.Data;
using System;
using System.Linq;
using System.Collections.Generic;
using AnimationSystem.Interface;

[CustomEditor(typeof(EnemySkillCardAnimationDatabase))]
public class EnemySkillCardAnimationDatabaseEditor : Editor
{
    private SerializedProperty skillCardAnimationsProp;

    private List<Type> spawnTypes;
    private string[] spawnTypeNames;
    private string[] spawnTypeFullNames;
    private List<Type> moveTypes;
    private string[] moveTypeNames;
    private string[] moveTypeFullNames;
    private List<Type> moveToCombatSlotTypes;
    private string[] moveToCombatSlotTypeNames;
    private string[] moveToCombatSlotTypeFullNames;
    private List<Type> useTypes;
    private string[] useTypeNames;
    private string[] useTypeFullNames;
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

        moveTypes = SortDefaultFirst(AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Where(t => typeof(ISkillCardMoveAnimationScript).IsAssignableFrom(t) && !t.IsAbstract && t.IsClass)
            .OrderBy(t => t.FullName).ToList());
        moveTypeNames = moveTypes.Select(t => t.Name).ToArray();
        moveTypeFullNames = moveTypes.Select(t => t.FullName).ToArray();

        moveToCombatSlotTypes = SortDefaultFirst(AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Where(t => typeof(ISkillCardCombatSlotMoveAnimationScript).IsAssignableFrom(t) && !t.IsAbstract && t.IsClass)
            .OrderBy(t => t.FullName).ToList());
        moveToCombatSlotTypeNames = moveToCombatSlotTypes.Select(t => t.Name).ToArray();
        moveToCombatSlotTypeFullNames = moveToCombatSlotTypes.Select(t => t.FullName).ToArray();

        useTypes = SortDefaultFirst(AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Where(t => typeof(ISkillCardUseAnimationScript).IsAssignableFrom(t) && !t.IsAbstract && t.IsClass)
            .OrderBy(t => t.FullName).ToList());
        useTypeNames = useTypes.Select(t => t.Name).ToArray();
        useTypeFullNames = useTypes.Select(t => t.FullName).ToArray();

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

        EditorGUILayout.LabelField("적 스킬카드 애니메이션 매핑", EditorStyles.boldLabel);
        if (skillCardAnimationsProp != null && skillCardAnimationsProp.isArray)
        {
            for (int i = 0; i < skillCardAnimationsProp.arraySize; i++)
            {
                var entryProp = skillCardAnimationsProp.GetArrayElementAtIndex(i);
                EditorGUILayout.BeginVertical(GUI.skin.box);
                EditorGUILayout.LabelField($"Element {i}", EditorStyles.boldLabel);

                var skillCardProp = entryProp.FindPropertyRelative("enemySkillCard");
                EditorGUILayout.PropertyField(skillCardProp, new GUIContent("적 스킬카드 SO"));

                var spawnAnimProp = entryProp.FindPropertyRelative("spawnAnimation");
                DrawAnimationSettingsDropdown(spawnAnimProp, "생성 애니메이션", spawnTypeNames, spawnTypeFullNames, spawnTypes);

                var moveAnimProp = entryProp.FindPropertyRelative("moveAnimation");
                DrawAnimationSettingsDropdown(moveAnimProp, "이동 애니메이션", moveTypeNames, moveTypeFullNames, moveTypes);

                var moveToCombatSlotAnimProp = entryProp.FindPropertyRelative("moveToCombatSlotAnimation");
                DrawAnimationSettingsDropdown(moveToCombatSlotAnimProp, "전투슬롯 이동 애니메이션", moveToCombatSlotTypeNames, moveToCombatSlotTypeFullNames, moveToCombatSlotTypes);

                var useAnimProp = entryProp.FindPropertyRelative("useAnimation");
                DrawAnimationSettingsDropdown(useAnimProp, "사용 애니메이션", useTypeNames, useTypeFullNames, useTypes);

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