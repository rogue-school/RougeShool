using UnityEngine;
using UnityEditor;
using Game.SkillCardSystem.Deck;
using Game.SkillCardSystem.Data;

namespace Game.SkillCardSystem.Editor
{
    /// <summary>
    /// EnemySkillDeck의 커스텀 에디터입니다.
    /// 적과 공용 스킬카드만 필터링하여 표시합니다.
    /// </summary>
    [CustomEditor(typeof(EnemySkillDeck))]
    public class EnemySkillDeckEditor : UnityEditor.Editor
    {
        private SerializedProperty cardsProperty;
        private bool showCards = true;

        private void OnEnable()
        {
            cardsProperty = serializedObject.FindProperty("cards");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // 헤더 정보
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("적 스킬 덱 설정", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("적과 공용 스킬카드만 선택할 수 있습니다.", MessageType.Info);
            EditorGUILayout.Space();

            // 카드 목록 표시
            showCards = EditorGUILayout.Foldout(showCards, "적용될 카드 목록 (확률 기반)", true);
            if (showCards)
            {
                EditorGUI.indentLevel++;
                
                if (cardsProperty.arraySize == 0)
                {
                    EditorGUILayout.HelpBox("카드가 없습니다. + 버튼을 눌러 카드를 추가하세요.", MessageType.Info);
                }
                else
                {
                    for (int i = 0; i < cardsProperty.arraySize; i++)
                    {
                        DrawCardEntry(i);
                    }
                }
                
                EditorGUI.indentLevel--;
            }

            // 카드 추가/제거 버튼
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("+ 카드 추가"))
            {
                AddNewCardEntry();
            }
            
            if (cardsProperty.arraySize > 0 && GUILayout.Button("- 마지막 카드 제거"))
            {
                RemoveLastCardEntry();
            }
            
            EditorGUILayout.EndHorizontal();

            // 확률 검증 및 경고
            ValidateProbabilities();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawCardEntry(int index)
        {
            var cardEntry = cardsProperty.GetArrayElementAtIndex(index);
            var definitionProperty = cardEntry.FindPropertyRelative("definition");
            var probabilityProperty = cardEntry.FindPropertyRelative("probability");

            EditorGUILayout.BeginVertical("box");

            // 카드 정의 선택 (필터링 적용)
            EditorGUILayout.LabelField($"카드 {index + 1}", EditorStyles.boldLabel);
            
            EditorGUI.BeginChangeCheck();
            definitionProperty.objectReferenceValue = EditorGUILayout.ObjectField(
                "스킬카드 정의", 
                definitionProperty.objectReferenceValue, 
                typeof(SkillCardDefinition), 
                false
            ) as SkillCardDefinition;
            
            if (EditorGUI.EndChangeCheck())
            {
                // 선택된 카드가 적 또는 공용인지 검증
                ValidateCardDefinition(definitionProperty.objectReferenceValue as SkillCardDefinition, index);
            }

            // 확률 설정
            EditorGUILayout.Slider(probabilityProperty, 0f, 1f, "등장 확률");

            // 카드 정보 표시
            if (definitionProperty.objectReferenceValue != null)
            {
                var cardDef = definitionProperty.objectReferenceValue as SkillCardDefinition;
                
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("카드명:", GUILayout.Width(60));
                EditorGUILayout.LabelField(cardDef.displayName, EditorStyles.boldLabel);
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("소유자:", GUILayout.Width(60));
                
                // 소유자 정책에 따른 색상 표시
                var originalColor = GUI.color;
                switch (cardDef.configuration.ownerPolicy)
                {
                    case OwnerPolicy.Enemy:
                        GUI.color = Color.red;
                        EditorGUILayout.LabelField("적 전용", EditorStyles.miniLabel);
                        break;
                    case OwnerPolicy.Shared:
                        GUI.color = Color.green;
                        EditorGUILayout.LabelField("공용", EditorStyles.miniLabel);
                        break;
                    case OwnerPolicy.Player:
                        GUI.color = Color.yellow;
                        EditorGUILayout.LabelField("플레이어 전용 (사용 불가)", EditorStyles.miniLabel);
                        break;
                }
                GUI.color = originalColor;
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                EditorGUILayout.HelpBox("스킬카드를 선택해주세요.", MessageType.Info);
            }

            // 개별 카드 제거 버튼
            EditorGUILayout.Space();
            if (GUILayout.Button($"카드 {index + 1} 제거", GUILayout.Height(20)))
            {
                RemoveCardEntry(index);
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }

        private void AddNewCardEntry()
        {
            cardsProperty.arraySize++;
            var newEntry = cardsProperty.GetArrayElementAtIndex(cardsProperty.arraySize - 1);
            newEntry.FindPropertyRelative("definition").objectReferenceValue = null;
            newEntry.FindPropertyRelative("probability").floatValue = 1.0f;
        }

        private void RemoveCardEntry(int index)
        {
            cardsProperty.DeleteArrayElementAtIndex(index);
        }

        private void RemoveLastCardEntry()
        {
            if (cardsProperty.arraySize > 0)
            {
                cardsProperty.arraySize--;
            }
        }

        private void ValidateCardDefinition(SkillCardDefinition cardDef, int index)
        {
            if (cardDef == null) return;

            // 플레이어 전용 카드인 경우 경고
            if (cardDef.configuration.ownerPolicy == OwnerPolicy.Player)
            {
                EditorUtility.DisplayDialog(
                    "경고", 
                    $"'{cardDef.displayName}'은 플레이어 전용 카드입니다.\n적이 사용할 수 없습니다.", 
                    "확인"
                );
            }
        }

        private void ValidateProbabilities()
        {
            if (cardsProperty.arraySize == 0) return;

            float totalProbability = 0f;
            for (int i = 0; i < cardsProperty.arraySize; i++)
            {
                var probability = cardsProperty.GetArrayElementAtIndex(i).FindPropertyRelative("probability").floatValue;
                totalProbability += probability;
            }

            // 확률 총합 검증
            if (totalProbability > 1.0f)
            {
                EditorGUILayout.HelpBox(
                    $"경고: 확률 총합이 1.0을 초과합니다 ({totalProbability:F2}). " +
                    "확률을 조정해주세요.", 
                    MessageType.Warning
                );
            }
            else if (totalProbability < 1.0f)
            {
                EditorGUILayout.HelpBox(
                    $"정보: 확률 총합이 1.0 미만입니다 ({totalProbability:F2}). " +
                    "일부 경우에 카드가 선택되지 않을 수 있습니다.", 
                    MessageType.Info
                );
            }
            else
            {
                EditorGUILayout.HelpBox("✓ 확률 총합이 정상입니다.", MessageType.None);
            }
        }
    }
}
