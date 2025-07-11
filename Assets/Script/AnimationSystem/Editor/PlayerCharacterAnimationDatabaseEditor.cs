using UnityEditor;
using UnityEngine;
using AnimationSystem.Data;
using System;
using System.Linq;
using System.Collections.Generic;
using AnimationSystem.Interface;

[CustomEditor(typeof(PlayerCharacterAnimationDatabase))]
public class PlayerCharacterAnimationDatabaseEditor : Editor
{
    private SerializedProperty characterAnimationsProp;

    private List<Type> spawnTypes;
    private string[] spawnTypeNames;
    private string[] spawnTypeFullNames;
    private List<Type> deathTypes;
    private string[] deathTypeNames;
    private string[] deathTypeFullNames;

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
        characterAnimationsProp = serializedObject.FindProperty("characterAnimations");
        spawnTypes = SortDefaultFirst(AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Where(t => typeof(ICharacterSpawnAnimationScript).IsAssignableFrom(t) && !t.IsAbstract && t.IsClass)
            .OrderBy(t => t.FullName).ToList());
        spawnTypeNames = spawnTypes.Select(t => t.Name).ToArray();
        spawnTypeFullNames = spawnTypes.Select(t => t.FullName).ToArray();

        deathTypes = SortDefaultFirst(AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Where(t => typeof(ICharacterDeathAnimationScript).IsAssignableFrom(t) && !t.IsAbstract && t.IsClass)
            .OrderBy(t => t.FullName).ToList());
        deathTypeNames = deathTypes.Select(t => t.Name).ToArray();
        deathTypeFullNames = deathTypes.Select(t => t.FullName).ToArray();
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.LabelField("플레이어 캐릭터 애니메이션 매핑", EditorStyles.boldLabel);
        if (characterAnimationsProp != null && characterAnimationsProp.isArray)
        {
            for (int i = 0; i < characterAnimationsProp.arraySize; i++)
            {
                var entryProp = characterAnimationsProp.GetArrayElementAtIndex(i);
                EditorGUILayout.BeginVertical(GUI.skin.box);
                EditorGUILayout.LabelField($"Element {i}", EditorStyles.boldLabel);

                var playerCharProp = entryProp.FindPropertyRelative("playerCharacter");
                EditorGUILayout.PropertyField(playerCharProp, new GUIContent("플레이어 캐릭터 데이터"));

                var spawnAnimProp = entryProp.FindPropertyRelative("spawnAnimation");
                DrawAnimationSettingsDropdown(spawnAnimProp, "생성 애니메이션", spawnTypeNames, spawnTypeFullNames, spawnTypes);

                var deathAnimProp = entryProp.FindPropertyRelative("deathAnimation");
                DrawAnimationSettingsDropdown(deathAnimProp, "사망 애니메이션", deathTypeNames, deathTypeFullNames, deathTypes);

                EditorGUILayout.EndVertical();
            }
        }
        if (GUILayout.Button("+", GUILayout.Width(30)))
        {
            characterAnimationsProp.arraySize++;
        }
        if (characterAnimationsProp.arraySize > 0 && GUILayout.Button("-", GUILayout.Width(30)))
        {
            characterAnimationsProp.arraySize--;
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