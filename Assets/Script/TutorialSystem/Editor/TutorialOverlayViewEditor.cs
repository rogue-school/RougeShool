using UnityEditorInternal;
using UnityEditor;
using UnityEngine;

namespace Game.TutorialSystem.Editor
{
    [CustomEditor(typeof(TutorialOverlayView))]
    public class TutorialOverlayViewEditor : UnityEditor.Editor
    {
        private ReorderableList _pagesList;

        private void OnEnable()
        {
            var pagesProp = serializedObject.FindProperty("pages");
            _pagesList = new ReorderableList(serializedObject, pagesProp, true, true, true, true);
            _pagesList.drawHeaderCallback = rect => EditorGUI.LabelField(rect, "Pages (1:1 매핑, 드래그로 순서 변경)");
            _pagesList.drawElementCallback = (rect, index, active, focused) =>
            {
                var element = pagesProp.GetArrayElementAtIndex(index);
                rect.height = EditorGUIUtility.singleLineHeight;
                EditorGUI.PropertyField(rect, element, new GUIContent($"[{index}] Page"));
            };
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // Draw core fields
            EditorGUILayout.LabelField("오버레이 구성", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("nextButton"), new GUIContent("Next Button"));

            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("옵션", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("tweenTime"), new GUIContent("Tween Time"));

            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("페이지 구성", EditorStyles.boldLabel);
            var pagesRootProp = serializedObject.FindProperty("pagesRoot");
            EditorGUILayout.PropertyField(pagesRootProp, new GUIContent("Pages Root (옵션)"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("initialPageIndex"), new GUIContent("Initial Page Index"));

            EditorGUILayout.Space(6);
            _pagesList.DoLayoutList();
            EditorGUILayout.HelpBox("Pages 리스트가 비어있지 않으면 이 순서를 사용합니다. 비어있으면 Pages Root의 자식 순서를 사용합니다.", MessageType.None);

            serializedObject.ApplyModifiedProperties();
        }
    }
}


