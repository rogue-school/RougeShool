using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using Game.AnimationSystem.Data;
using Game.SkillCardSystem.Data;

namespace Game.AnimationSystem.Editor
{
    /// <summary>
    /// 통합 애니메이션 데이터베이스 관리 도구
    /// 기존 분리된 데이터베이스는 이미 삭제되었습니다.
    /// </summary>
    public class AnimationDatabaseMigrator : EditorWindow
    {
        [Header("통합 데이터베이스")]
        [SerializeField] private UnifiedSkillCardAnimationDatabase unifiedDatabase;
        
        [Header("설정")]
        [SerializeField] private string outputPath = "Assets/Resources/Data/Animation/Unified/";

        [MenuItem("Tools/Animation System/Manage Unified Animation Database")]
        public static void ShowWindow()
        {
            GetWindow<AnimationDatabaseMigrator>("Unified Animation Database Manager");
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("통합 애니메이션 데이터베이스 관리 도구", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            // 정보 메시지
            EditorGUILayout.HelpBox(
                "기존 분리된 데이터베이스(PlayerSkillCardAnimationDatabase, EnemySkillCardAnimationDatabase)는 이미 삭제되었습니다.\n" +
                "이제 통합 데이터베이스(UnifiedSkillCardAnimationDatabase)만 사용합니다.", 
                MessageType.Info);

            EditorGUILayout.Space();

            // 통합 데이터베이스 선택/생성
            EditorGUILayout.LabelField("통합 데이터베이스", EditorStyles.boldLabel);
            unifiedDatabase = (UnifiedSkillCardAnimationDatabase)EditorGUILayout.ObjectField(
                "통합 데이터베이스", unifiedDatabase, typeof(UnifiedSkillCardAnimationDatabase), false);

            EditorGUILayout.Space();

            // 버튼들
            GUI.enabled = unifiedDatabase == null;
            if (GUILayout.Button("새 통합 데이터베이스 생성"))
            {
                CreateUnifiedDatabase();
            }
            GUI.enabled = true;

            EditorGUILayout.Space();

            // 통계 정보
            EditorGUILayout.LabelField("통계 정보", EditorStyles.boldLabel);
            if (unifiedDatabase != null)
            {
                EditorGUILayout.LabelField($"통합 카드 수: {unifiedDatabase.SkillCardAnimations.Count}");
                
                EditorGUILayout.Space();
                EditorGUILayout.HelpBox("새로운 구조:\n" +
                                        "• 소유자 정책: Shared(공통), Player(플레이어), Enemy(적)\n" +
                                        "• 애니메이션 스크립트 타입: 드롭다운으로 선택\n" +
                                        "• 애니메이션 파라미터: 제거됨 (단순화)", MessageType.Info);
                
                // 카드 목록 표시
                if (unifiedDatabase.SkillCardAnimations.Count > 0)
                {
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("등록된 카드 목록:", EditorStyles.boldLabel);
                    foreach (var entry in unifiedDatabase.SkillCardAnimations)
                    {
                        if (entry.SkillCardDefinition != null)
                        {
                            EditorGUILayout.LabelField($"- {entry.SkillCardDefinition.displayName} ({entry.OwnerPolicy})");
                        }
                    }
                }
            }
            else
            {
                EditorGUILayout.LabelField("통합 데이터베이스가 설정되지 않았습니다.");
            }
        }

        private void CreateUnifiedDatabase()
        {
            if (!AssetDatabase.IsValidFolder(outputPath))
            {
                System.IO.Directory.CreateDirectory(outputPath);
            }

            string assetPath = $"{outputPath}UnifiedSkillCardAnimationDatabase.asset";
            unifiedDatabase = CreateInstance<UnifiedSkillCardAnimationDatabase>();
            AssetDatabase.CreateAsset(unifiedDatabase, assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"새 통합 데이터베이스 생성됨: {assetPath}");
            EditorUtility.DisplayDialog("생성 완료", $"새 통합 데이터베이스가 생성되었습니다:\n{assetPath}", "확인");
        }
    }
}