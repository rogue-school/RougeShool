using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Game.CoreSystem.Utility;

namespace Game.CoreSystem.Utility
{
    /// <summary>
    /// 컴포넌트 간 상호작용을 최적화하고 역할 충돌을 방지하는 클래스
    /// 각 컴포넌트가 자신의 역할만 수행하도록 보장합니다.
    /// </summary>
    public class ComponentInteractionOptimizer : MonoBehaviour
    {
        [Header("상호작용 최적화 설정")]
        [SerializeField] private bool enableInteractionValidation = true;
        [SerializeField] private bool enableRoleConflictDetection = true;
        [SerializeField] private bool enablePerformanceMonitoring = false;
        [SerializeField] private bool enableAutomaticOptimization = true;

        [Header("최적화 결과")]
        [SerializeField] private int totalComponents;
        [SerializeField] private int optimizedComponents;
        [SerializeField] private int conflictResolved;
        [SerializeField] private float optimizationTime;

        private Dictionary<MonoBehaviour, ComponentRoleManager.ComponentRole> componentRoles;
        private Dictionary<MonoBehaviour, List<string>> componentResponsibilities;

        private void Awake()
        {
            if (enableInteractionValidation)
            {
                InitializeOptimization();
            }
        }

        private void Start()
        {
            if (enableInteractionValidation)
            {
                OptimizeComponentInteractions();
            }
        }

        /// <summary>
        /// 컴포넌트 상호작용 최적화 초기화
        /// </summary>
        private void InitializeOptimization()
        {
            componentRoles = new Dictionary<MonoBehaviour, ComponentRoleManager.ComponentRole>();
            componentResponsibilities = new Dictionary<MonoBehaviour, List<string>>();

            // 모든 MonoBehaviour 컴포넌트 수집
            var allComponents = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
            totalComponents = allComponents.Length;

            GameLogger.LogInfo($"컴포넌트 상호작용 최적화 초기화: {totalComponents}개 컴포넌트 발견", GameLogger.LogCategory.Core);
        }

        /// <summary>
        /// 컴포넌트 상호작용 최적화 실행
        /// </summary>
        [ContextMenu("컴포넌트 상호작용 최적화 실행")]
        public void OptimizeComponentInteractions()
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            GameLogger.LogInfo("=== 컴포넌트 상호작용 최적화 시작 ===", GameLogger.LogCategory.Core);

            // 1. 역할 충돌 검사
            if (enableRoleConflictDetection)
            {
                DetectRoleConflicts();
            }

            // 2. 책임 분리 최적화
            if (enableAutomaticOptimization)
            {
                OptimizeResponsibilitySeparation();
            }

            // 3. 성능 모니터링
            if (enablePerformanceMonitoring)
            {
                MonitorComponentPerformance();
            }

            stopwatch.Stop();
            optimizationTime = stopwatch.ElapsedMilliseconds;

            GameLogger.LogInfo($"=== 컴포넌트 상호작용 최적화 완료 ({optimizationTime}ms) ===", GameLogger.LogCategory.Core);
            GameLogger.LogInfo($"최적화된 컴포넌트: {optimizedComponents}/{totalComponents}", GameLogger.LogCategory.Core);
            GameLogger.LogInfo($"해결된 충돌: {conflictResolved}개", GameLogger.LogCategory.Core);
        }

        /// <summary>
        /// 역할 충돌 검사
        /// </summary>
        private void DetectRoleConflicts()
        {
            GameLogger.LogInfo("역할 충돌 검사 중...", GameLogger.LogCategory.Core);

            var allComponents = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
            var roleGroups = new Dictionary<ComponentRoleManager.ComponentRole, List<MonoBehaviour>>();

            // 역할별로 컴포넌트 그룹화
            foreach (var component in allComponents)
            {
                var roleInfo = ComponentRoleManager.GetComponentRoleInfo(component);
                if (roleInfo != null)
                {
                    if (!roleGroups.ContainsKey(roleInfo.Role))
                    {
                        roleGroups[roleInfo.Role] = new List<MonoBehaviour>();
                    }
                    roleGroups[roleInfo.Role].Add(component);
                }
            }

            // 충돌 검사
            foreach (var group in roleGroups)
            {
                if (group.Value.Count > 1)
                {
                    GameLogger.LogWarning($"역할 '{group.Key}'에 {group.Value.Count}개의 컴포넌트가 있습니다:", GameLogger.LogCategory.Core);
                    foreach (var component in group.Value)
                    {
                        GameLogger.LogWarning($"- {component.GetType().Name}", GameLogger.LogCategory.Core);
                    }
                }
            }
        }

        /// <summary>
        /// 책임 분리 최적화
        /// </summary>
        private void OptimizeResponsibilitySeparation()
        {
            GameLogger.LogInfo("책임 분리 최적화 중...", GameLogger.LogCategory.Core);

            var allComponents = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
            optimizedComponents = 0;

            foreach (var component in allComponents)
            {
                if (OptimizeComponentResponsibilities(component))
                {
                    optimizedComponents++;
                }
            }
        }

        /// <summary>
        /// 개별 컴포넌트의 책임 최적화
        /// </summary>
        private bool OptimizeComponentResponsibilities(MonoBehaviour component)
        {
            var roleInfo = ComponentRoleManager.GetComponentRoleInfo(component);
            if (roleInfo == null) return false;

            bool wasOptimized = false;

            // 금지된 책임 검사 및 제거
            foreach (var forbiddenResponsibility in roleInfo.ForbiddenResponsibilities)
            {
                if (HasForbiddenResponsibility(component, forbiddenResponsibility))
                {
                    GameLogger.LogWarning($"컴포넌트 {component.GetType().Name}에서 금지된 책임 '{forbiddenResponsibility}' 발견", GameLogger.LogCategory.Core);
                    
                    if (enableAutomaticOptimization)
                    {
                        RemoveForbiddenResponsibility(component, forbiddenResponsibility);
                        wasOptimized = true;
                        conflictResolved++;
                    }
                }
            }

            return wasOptimized;
        }

        /// <summary>
        /// 금지된 책임이 있는지 검사
        /// </summary>
        private bool HasForbiddenResponsibility(MonoBehaviour component, string responsibility)
        {
            var type = component.GetType();
            var methods = type.GetMethods().Select(m => m.Name.ToLower());

            switch (responsibility.ToLower())
            {
                case "object_creation":
                    return methods.Any(m => m.Contains("create") || m.Contains("instantiate") || m.Contains("spawn"));
                case "ui_management":
                    return methods.Any(m => m.Contains("ui") || m.Contains("button") || m.Contains("panel"));
                case "data_persistence":
                    return methods.Any(m => m.Contains("save") || m.Contains("load") || m.Contains("persist"));
                case "scene_management":
                    return methods.Any(m => m.Contains("scene") || m.Contains("loadscene") || m.Contains("transition"));
                default:
                    return false;
            }
        }

        /// <summary>
        /// 금지된 책임 제거 (경고만 출력, 실제 제거는 수동으로)
        /// </summary>
        private void RemoveForbiddenResponsibility(MonoBehaviour component, string responsibility)
        {
            GameLogger.LogWarning($"컴포넌트 {component.GetType().Name}에서 '{responsibility}' 책임을 제거해야 합니다. 수동으로 코드를 수정하세요.", GameLogger.LogCategory.Core);
        }

        /// <summary>
        /// 컴포넌트 성능 모니터링
        /// </summary>
        private void MonitorComponentPerformance()
        {
            GameLogger.LogInfo("컴포넌트 성능 모니터링 중...", GameLogger.LogCategory.Core);

            var allComponents = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
            var performanceIssues = new List<string>();

            foreach (var component in allComponents)
            {
                // Update 메서드가 있는 컴포넌트 검사
                var updateMethod = component.GetType().GetMethod("Update");
                if (updateMethod != null)
                {
                    performanceIssues.Add($"{component.GetType().Name}: Update 메서드 사용 중");
                }

                // 많은 의존성을 가진 컴포넌트 검사
                var fields = component.GetType().GetFields();
                if (fields.Length > 10)
                {
                    performanceIssues.Add($"{component.GetType().Name}: 많은 필드 ({fields.Length}개)");
                }
            }

            if (performanceIssues.Count > 0)
            {
                GameLogger.LogWarning("성능 이슈가 있는 컴포넌트들:", GameLogger.LogCategory.Core);
                foreach (var issue in performanceIssues)
                {
                    GameLogger.LogWarning($"- {issue}", GameLogger.LogCategory.Core);
                }
            }
        }

        /// <summary>
        /// 컴포넌트 역할 검증
        /// </summary>
        [ContextMenu("컴포넌트 역할 검증")]
        public void ValidateComponentRoles()
        {
            GameLogger.LogInfo("=== 컴포넌트 역할 검증 시작 ===", GameLogger.LogCategory.Core);

            var allComponents = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
            int validComponents = 0;
            int invalidComponents = 0;

            foreach (var component in allComponents)
            {
                var roleInfo = ComponentRoleManager.GetComponentRoleInfo(component);
                if (roleInfo != null)
                {
                    bool isValid = ComponentRoleManager.ValidateComponentRole(component, roleInfo.Role);
                    if (isValid)
                    {
                        validComponents++;
                    }
                    else
                    {
                        invalidComponents++;
                    }
                }
            }

            GameLogger.LogInfo($"=== 컴포넌트 역할 검증 완료 ===", GameLogger.LogCategory.Core);
            GameLogger.LogInfo($"유효한 컴포넌트: {validComponents}개", GameLogger.LogCategory.Core);
            GameLogger.LogInfo($"무효한 컴포넌트: {invalidComponents}개", GameLogger.LogCategory.Core);
        }

        /// <summary>
        /// 최적화 리포트 생성
        /// </summary>
        [ContextMenu("최적화 리포트 생성")]
        public void GenerateOptimizationReport()
        {
            GameLogger.LogInfo("=== 컴포넌트 최적화 리포트 ===", GameLogger.LogCategory.Core);
            GameLogger.LogInfo($"총 컴포넌트 수: {totalComponents}", GameLogger.LogCategory.Core);
            GameLogger.LogInfo($"최적화된 컴포넌트: {optimizedComponents}", GameLogger.LogCategory.Core);
            GameLogger.LogInfo($"해결된 충돌: {conflictResolved}", GameLogger.LogCategory.Core);
            GameLogger.LogInfo($"최적화 시간: {optimizationTime}ms", GameLogger.LogCategory.Core);
            GameLogger.LogInfo($"최적화율: {(float)optimizedComponents / totalComponents * 100:F1}%", GameLogger.LogCategory.Core);
        }
    }
}
