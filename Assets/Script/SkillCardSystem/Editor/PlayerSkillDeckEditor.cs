using UnityEngine;
using UnityEditor;
using Game.SkillCardSystem.Deck;

namespace Game.SkillCardSystem.Editor
{
    /// <summary>
    /// PlayerSkillDeck을 위한 커스텀 에디터입니다.
    /// 덱 구성과 카드 수량을 시각적으로 관리할 수 있습니다.
    /// </summary>
    [CustomEditor(typeof(PlayerSkillDeck))]
    public class PlayerSkillDeckEditor : UnityEditor.Editor
    {
        private PlayerSkillDeck deck;
        private bool showCardEntries = true;
        private bool showValidation = true;

        private void OnEnable()
        {
            deck = target as PlayerSkillDeck;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawDeckHeader();
            DrawCardEntries();
            DrawValidation();
            DrawTools();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawDeckHeader()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("플레이어 스킬 덱", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            // 덱 기본 정보
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("덱 정보", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"덱 이름: {deck.name}");
            EditorGUILayout.LabelField($"총 카드 수: {deck.TotalCardCount}");
            EditorGUILayout.LabelField($"카드 엔트리 수: {deck.CardEntries.Count}");
            EditorGUILayout.EndVertical();
        }

        private void DrawCardEntries()
        {
            EditorGUILayout.Space();
            showCardEntries = EditorGUILayout.Foldout(showCardEntries, "카드 엔트리", true);
            
            if (showCardEntries)
            {
                EditorGUILayout.BeginVertical("box");
                
                var cardEntriesProperty = serializedObject.FindProperty("cardEntries");
                if (cardEntriesProperty != null)
                {
                    EditorGUILayout.PropertyField(cardEntriesProperty, new GUIContent("카드 엔트리 목록"), true);
                }
                else
                {
                    EditorGUILayout.HelpBox("카드 엔트리 프로퍼티를 찾을 수 없습니다.", MessageType.Error);
                }
                
                EditorGUILayout.EndVertical();
            }
        }

        private void DrawValidation()
        {
            EditorGUILayout.Space();
            showValidation = EditorGUILayout.Foldout(showValidation, "덱 검증", true);
            
            if (showValidation)
            {
                EditorGUILayout.BeginVertical("box");
                
                bool isValid = deck.IsValidDeck();
                MessageType messageType = isValid ? MessageType.Info : MessageType.Warning;
                string message = isValid ? "덱 구성이 유효합니다." : "덱 구성에 문제가 있습니다.";
                
                EditorGUILayout.HelpBox(message, messageType);
                
                if (!isValid)
                {
                    EditorGUILayout.HelpBox("각 카드 엔트리는 유효한 SkillCardDefinition과 1 이상의 수량을 가져야 합니다.", MessageType.Info);
                }
                
                EditorGUILayout.EndVertical();
            }
        }

        private void DrawTools()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("도구", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginVertical("box");
            
            if (GUILayout.Button("덱 정보 출력"))
            {
                deck.LogDeckInfo();
            }
            
            if (GUILayout.Button("모든 카드 목록 출력"))
            {
                var allCards = deck.GetAllCards();
                Debug.Log($"[PlayerSkillDeckEditor] 모든 카드 목록 ({allCards.Count}장):");
                for (int i = 0; i < allCards.Count; i++)
                {
                    Debug.Log($"  {i + 1}. {allCards[i].displayName}");
                }
            }
            
            if (GUILayout.Button("카드 수량 통계"))
            {
                LogCardQuantityStats();
            }
            
            EditorGUILayout.EndVertical();
        }

        private void LogCardQuantityStats()
        {
            Debug.Log($"[PlayerSkillDeckEditor] 카드 수량 통계:");
            Debug.Log($"총 카드 수: {deck.TotalCardCount}");
            
            foreach (var entry in deck.CardEntries)
            {
                if (entry.IsValid())
                {
                    Debug.Log($"  - {entry.cardDefinition.displayName}: {entry.quantity}장");
                }
            }
        }
    }
}
