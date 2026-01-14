#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using Game.ItemSystem.Data.Reward;
using Game.ItemSystem.Data;

namespace Game.ItemSystem.Editor
{
    /// <summary>
    /// RewardPool 인스펙터에 각 항목의 현재 확률(%)을 가시화합니다.
    /// 확률 = 항목 가중치 / 유효 가중치 합 × 100
    /// </summary>
    [CustomEditor(typeof(RewardPool))]
    public class RewardPoolEditor : UnityEditor.Editor
    {
        private SerializedProperty _entriesProp;

        private void OnEnable()
        {
            _entriesProp = serializedObject.FindProperty("entries");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // 유효 가중치 합 계산(0 이하는 제외)
            float totalWeight = 0f;
            if (_entriesProp != null && _entriesProp.isArray)
            {
                for (int i = 0; i < _entriesProp.arraySize; i++)
                {
                    var element = _entriesProp.GetArrayElementAtIndex(i);
                    if (element == null) continue;
                    var weightProp = element.FindPropertyRelative("weight");
                    if (weightProp != null)
                    {
                        int w = Mathf.Max(0, weightProp.intValue);
                        totalWeight += w;
                    }
                }
            }

            // 헤더
            EditorGUILayout.LabelField("보상 후보 목록", EditorStyles.boldLabel);
            using (new EditorGUI.IndentLevelScope(1))
            {
                // 리스트 수동 렌더링: 항목별 확률 표시
                for (int i = 0; i < _entriesProp.arraySize; i++)
                {
                    var element = _entriesProp.GetArrayElementAtIndex(i);
                    if (element == null)
                    {
                        continue;
                    }

                    var itemProp = element.FindPropertyRelative("item");
                    var weightProp = element.FindPropertyRelative("weight");
                    var tagsProp = element.FindPropertyRelative("tags");
                    var minStageProp = element.FindPropertyRelative("minStage");
                    var maxStageProp = element.FindPropertyRelative("maxStage");
                    var uniqueProp = element.FindPropertyRelative("uniquePerRun");

                    EditorGUILayout.BeginVertical("box");

                    EditorGUILayout.PropertyField(itemProp);

                    // weight만 편집 (확률은 하단 요약에서 표시)
                    EditorGUILayout.PropertyField(weightProp, new GUIContent("weight"));

                    EditorGUILayout.PropertyField(tagsProp, true);

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(minStageProp);
                    EditorGUILayout.PropertyField(maxStageProp);
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.PropertyField(uniqueProp);

                    EditorGUILayout.EndVertical();
                }

                // 리스트 크기/추가 제거 기본 컨트롤
                EditorGUILayout.Space(4);
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("항목 추가"))
                {
                    _entriesProp.InsertArrayElementAtIndex(_entriesProp.arraySize);
                }
                if (GUILayout.Button("마지막 항목 제거"))
                {
                    if (_entriesProp.arraySize > 0)
                        _entriesProp.DeleteArrayElementAtIndex(_entriesProp.arraySize - 1);
                }
                EditorGUILayout.EndHorizontal();
            }

            // 총 가중치/정규화 안내 + 항목별 확률 요약
            EditorGUILayout.Space(6);
            var sb = new System.Text.StringBuilder();
            sb.Append($"유효 가중치 합: {totalWeight:0}");
            // 패시브 카테고리 집계 (검/활/지팡이/공용)
            var categoryWeights = new System.Collections.Generic.Dictionary<PassiveItemCategory, int>();
            if (totalWeight > 0f && _entriesProp != null && _entriesProp.isArray)
            {
                sb.Append('\n');
                for (int i = 0; i < _entriesProp.arraySize; i++)
                {
                    var element = _entriesProp.GetArrayElementAtIndex(i);
                    if (element == null) continue;
                    var itemProp = element.FindPropertyRelative("item");
                    var weightProp = element.FindPropertyRelative("weight");
                    int w = Mathf.Max(0, weightProp != null ? weightProp.intValue : 0);
                    float pct = (w / totalWeight) * 100f;
                    string itemName = "(미할당)";
                    if (itemProp != null && itemProp.objectReferenceValue != null)
                    {
                        var obj = itemProp.objectReferenceValue;
                        itemName = obj != null ? obj.name : itemName;

                        // 패시브 카테고리 가중치 누적
                        if (obj is PassiveItemDefinition passive)
                        {
                            if (!categoryWeights.ContainsKey(passive.Category)) categoryWeights[passive.Category] = 0;
                            categoryWeights[passive.Category] += w;
                        }
                    }
                    sb.AppendLine($"- {itemName}: {pct:0.##}%");
                }
            }
            // 캐릭터별(검/활/지팡이) 서브풀: [해당 카테고리 + 공용] 확률표
            if (totalWeight > 0f && _entriesProp != null && _entriesProp.isArray)
            {
                // 집계 컨테이너
                var subsets = new System.Collections.Generic.Dictionary<PassiveItemCategory, System.Collections.Generic.List<(string name, int w)>>();
                var subsetTotals = new System.Collections.Generic.Dictionary<PassiveItemCategory, int>();

                void AddToSubset(PassiveItemCategory key, string name, int w)
                {
                    if (!subsets.ContainsKey(key)) subsets[key] = new System.Collections.Generic.List<(string, int)>();
                    if (!subsetTotals.ContainsKey(key)) subsetTotals[key] = 0;
                    subsets[key].Add((name, w));
                    subsetTotals[key] += w;
                }

                for (int i = 0; i < _entriesProp.arraySize; i++)
                {
                    var element = _entriesProp.GetArrayElementAtIndex(i);
                    if (element == null) continue;
                    var itemProp = element.FindPropertyRelative("item");
                    var weightProp = element.FindPropertyRelative("weight");
                    int w = Mathf.Max(0, weightProp != null ? weightProp.intValue : 0);
                    if (w == 0) continue;

                    if (itemProp != null && itemProp.objectReferenceValue is PassiveItemDefinition passive)
                    {
                        string nm = passive.name;
                        // 각 캐릭터 서브풀: 자신의 카테고리 + 공용
                        if (passive.Category == PassiveItemCategory.Sword || passive.Category == PassiveItemCategory.Common)
                            AddToSubset(PassiveItemCategory.Sword, nm, w);
                        if (passive.Category == PassiveItemCategory.Bow || passive.Category == PassiveItemCategory.Common)
                            AddToSubset(PassiveItemCategory.Bow, nm, w);
                        if (passive.Category == PassiveItemCategory.Staff || passive.Category == PassiveItemCategory.Common)
                            AddToSubset(PassiveItemCategory.Staff, nm, w);
                    }
                }

                // 출력
                if (subsets.Count > 0)
                {
                    sb.AppendLine("\n캐릭터별 서브풀 확률 (해당 카테고리 + 공용, 서브풀 정규화):");
                    foreach (var key in new[]{ PassiveItemCategory.Sword, PassiveItemCategory.Bow, PassiveItemCategory.Staff })
                    {
                        if (!subsetTotals.ContainsKey(key) || subsetTotals[key] == 0) continue;
                        int tot = subsetTotals[key];
                        sb.AppendLine($"- {key} (합계 {tot}):");
                        foreach (var (name, w) in subsets[key])
                        {
                            float p = (w / (float)tot) * 100f;
                            sb.AppendLine($"    · {name}: {p:0.##}%");
                        }
                    }
                }
            }
            EditorGUILayout.HelpBox(sb.ToString(), MessageType.Info);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif


