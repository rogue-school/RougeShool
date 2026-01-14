using UnityEngine;
using UnityEditor;
using Game.CoreSystem.Audio;
using Game.StageSystem.Data;

namespace Game.Editor
{
    [CustomPropertyDrawer(typeof(AudioManager.StageEnemyBGMConfig))]
    public class StageEnemyBGMConfigDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float height = EditorGUIUtility.singleLineHeight; // Stage Data field
            height += EditorGUIUtility.standardVerticalSpacing;
            
            var stageDataProp = property.FindPropertyRelative("stageData");
            StageData stageData = stageDataProp.objectReferenceValue as StageData;
            
            if (stageData != null && stageData.enemies != null)
            {
                height += EditorGUIUtility.standardVerticalSpacing;
                height += EditorGUIUtility.singleLineHeight; // Header
                height += stageData.enemies.Count * (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing);
            }
            
            return height;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            float yPos = position.y;
            float height = EditorGUIUtility.singleLineHeight;
            
            // Stage Data field
            var stageDataProp = property.FindPropertyRelative("stageData");
            Rect stageDataRect = new Rect(position.x, yPos, position.width, height);
            EditorGUI.PropertyField(stageDataRect, stageDataProp, new GUIContent("Stage Data"));
            yPos += height + EditorGUIUtility.standardVerticalSpacing;
            
            // If stageData is assigned, show enemy BGM settings
            StageData stageData = stageDataProp.objectReferenceValue as StageData;
            if (stageData != null && stageData.enemies != null && stageData.enemies.Count > 0)
            {
                // Get or create enemyBGMs list
                var enemyBGMsProp = property.FindPropertyRelative("enemyBGMs");
                
                // Resize list to match enemies
                if (enemyBGMsProp.arraySize != stageData.enemies.Count)
                {
                    enemyBGMsProp.arraySize = stageData.enemies.Count;
                }
                
                // Header
                yPos += EditorGUIUtility.standardVerticalSpacing;
                Rect headerRect = new Rect(position.x, yPos, position.width, height);
                EditorGUI.LabelField(headerRect, "적별 BGM 설정:", EditorStyles.boldLabel);
                yPos += height;
                
                // Show enemy BGM fields
                for (int i = 0; i < stageData.enemies.Count; i++)
                {
                    if (i >= enemyBGMsProp.arraySize)
                        break;
                    
                    var enemyBGMProp = enemyBGMsProp.GetArrayElementAtIndex(i);
                    var enemyProp = enemyBGMProp.FindPropertyRelative("enemy");
                    var bgmProp = enemyBGMProp.FindPropertyRelative("bgm");
                    
                    string enemyName = stageData.enemies[i]?.DisplayName ?? "None";
                    
                    // Readonly enemy field (showing which enemy this BGM is for)
                    Rect enemyLabelRect = new Rect(position.x, yPos, position.width / 2, height);
                    EditorGUI.LabelField(enemyLabelRect, enemyName, EditorStyles.helpBox);
                    
                    // BGM field
                    Rect bgmRect = new Rect(position.x + position.width / 2, yPos, position.width / 2, height);
                    EditorGUI.PropertyField(bgmRect, bgmProp, GUIContent.none);
                    
                    // Set enemy reference automatically
                    if (enemyProp.objectReferenceValue != stageData.enemies[i])
                    {
                        enemyProp.objectReferenceValue = stageData.enemies[i];
                    }
                    
                    yPos += height + EditorGUIUtility.standardVerticalSpacing;
                }
            }
            
            EditorGUI.EndProperty();
        }
    }
}

