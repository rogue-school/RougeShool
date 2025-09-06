using UnityEditor;
using UnityEngine;
using Game.AnimationSystem.Data;
using System;
using System.Linq;
using System.Collections.Generic;
using Game.AnimationSystem.Interface;

[CustomEditor(typeof(EnemyCharacterAnimationDatabase))]
public class EnemyCharacterAnimationDatabaseEditor : Editor
{
    private SerializedProperty characterAnimationsProp;

    // 드롭다운용 타입 캐시
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
        // 드롭다운용 타입 캐싱 (성능 최적화)
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

        EditorGUILayout.LabelField("적 캐릭터 애니메이션 매핑", EditorStyles.boldLabel);
        if (characterAnimationsProp != null && characterAnimationsProp.isArray)
        {
            for (int i = 0; i < characterAnimationsProp.arraySize; i++)
            {
                var entryProp = characterAnimationsProp.GetArrayElementAtIndex(i);
                EditorGUILayout.BeginVertical(GUI.skin.box);
                EditorGUILayout.LabelField($"Element {i}", EditorStyles.boldLabel);

                // 적 캐릭터 데이터(SO 참조)
                var enemyCharProp = entryProp.FindPropertyRelative("enemyCharacter");
                EditorGUILayout.PropertyField(enemyCharProp, new GUIContent("적 캐릭터 데이터"));

                // 생성 애니메이션 드롭다운
                var spawnAnimProp = entryProp.FindPropertyRelative("spawnAnimation");
                DrawAnimationSettingsDropdown(spawnAnimProp, "생성 애니메이션", spawnTypeNames, spawnTypeFullNames, spawnTypes);

                // 사망 애니메이션 드롭다운
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

    // 드롭다운 그리기 (성능 최적화)
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