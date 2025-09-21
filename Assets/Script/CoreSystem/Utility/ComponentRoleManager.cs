using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Game.CoreSystem.Utility;
using Game.CharacterSystem.Manager;
using Game.CombatSystem.Manager;
using Game.CombatSystem.Core;
using Game.CharacterSystem.UI;
using Game.SkillCardSystem.Manager;
using Game.CharacterSystem.Initialization;

namespace Game.CoreSystem.Utility
{
    /// <summary>
    /// 컴포넌트들의 역할을 명확히 분리하고 관리하는 유틸리티 클래스
    /// 각 컴포넌트가 자신의 역할만 수행하도록 보장합니다.
    /// </summary>
    public static class ComponentRoleManager
    {
        #region 역할 정의

        /// <summary>
        /// 컴포넌트 역할 열거형
        /// </summary>
        public enum ComponentRole
        {
            /// <summary>DI 바인딩 전용</summary>
            DI_BINDING,
            /// <summary>매니저 (상태 관리)</summary>
            MANAGER,
            /// <summary>UI 컨트롤러</summary>
            UI_CONTROLLER,
            /// <summary>데이터 관리</summary>
            DATA_MANAGER,
            /// <summary>팩토리 (객체 생성)</summary>
            FACTORY,
            /// <summary>서비스 (비즈니스 로직)</summary>
            SERVICE,
            /// <summary>유틸리티 (도우미)</summary>
            UTILITY,
            /// <summary>초기화 전용</summary>
            INITIALIZER
        }

        /// <summary>
        /// 컴포넌트 역할 정보
        /// </summary>
        public class ComponentRoleInfo
        {
            public MonoBehaviour Component { get; set; }
            public ComponentRole Role { get; set; }
            public string Description { get; set; }
            public List<string> AllowedResponsibilities { get; set; }
            public List<string> ForbiddenResponsibilities { get; set; }

            public ComponentRoleInfo(MonoBehaviour component, ComponentRole role, string description)
            {
                Component = component;
                Role = role;
                Description = description;
                AllowedResponsibilities = new List<string>();
                ForbiddenResponsibilities = new List<string>();
            }
        }

        #endregion

        #region 역할 검증

        /// <summary>
        /// 컴포넌트의 역할이 올바른지 검증
        /// </summary>
        public static bool ValidateComponentRole(MonoBehaviour component, ComponentRole expectedRole)
        {
            var roleInfo = GetComponentRoleInfo(component);
            if (roleInfo == null)
            {
                GameLogger.LogWarning($"컴포넌트 {component.GetType().Name}의 역할 정보를 찾을 수 없습니다.", GameLogger.LogCategory.Core);
                return false;
            }

            bool isValid = roleInfo.Role == expectedRole;
            if (!isValid)
            {
                GameLogger.LogWarning($"컴포넌트 {component.GetType().Name}의 역할이 예상과 다릅니다. 예상: {expectedRole}, 실제: {roleInfo.Role}", GameLogger.LogCategory.Core);
            }

            return isValid;
        }

        /// <summary>
        /// 컴포넌트가 금지된 책임을 수행하고 있는지 검사
        /// </summary>
        public static bool CheckForbiddenResponsibilities(MonoBehaviour component)
        {
            var roleInfo = GetComponentRoleInfo(component);
            if (roleInfo == null) return false;

            bool hasViolations = false;
            foreach (var forbidden in roleInfo.ForbiddenResponsibilities)
            {
                if (HasResponsibility(component, forbidden))
                {
                    GameLogger.LogWarning($"컴포넌트 {component.GetType().Name}이 금지된 책임 '{forbidden}'을 수행하고 있습니다.", GameLogger.LogCategory.Core);
                    hasViolations = true;
                }
            }

            return hasViolations;
        }

        #endregion

        #region 역할 정보 관리

        private static readonly Dictionary<System.Type, ComponentRoleInfo> roleRegistry = new Dictionary<System.Type, ComponentRoleInfo>();

        /// <summary>
        /// 컴포넌트 역할 정보 등록
        /// </summary>
        public static void RegisterComponentRole<T>(ComponentRole role, string description) where T : MonoBehaviour
        {
            var roleInfo = new ComponentRoleInfo(null, role, description);
            roleRegistry[typeof(T)] = roleInfo;
        }

        /// <summary>
        /// 컴포넌트의 허용된 책임 추가
        /// </summary>
        public static void AddAllowedResponsibility<T>(string responsibility) where T : MonoBehaviour
        {
            if (roleRegistry.ContainsKey(typeof(T)))
            {
                roleRegistry[typeof(T)].AllowedResponsibilities.Add(responsibility);
            }
        }

        /// <summary>
        /// 컴포넌트의 금지된 책임 추가
        /// </summary>
        public static void AddForbiddenResponsibility<T>(string responsibility) where T : MonoBehaviour
        {
            if (roleRegistry.ContainsKey(typeof(T)))
            {
                roleRegistry[typeof(T)].ForbiddenResponsibilities.Add(responsibility);
            }
        }

        /// <summary>
        /// 컴포넌트 역할 정보 가져오기
        /// </summary>
        public static ComponentRoleInfo GetComponentRoleInfo(MonoBehaviour component)
        {
            if (roleRegistry.ContainsKey(component.GetType()))
            {
                var info = roleRegistry[component.GetType()];
                info.Component = component;
                return info;
            }
            return null;
        }

        #endregion

        #region 책임 검사

        /// <summary>
        /// 컴포넌트가 특정 책임을 수행하고 있는지 검사
        /// </summary>
        private static bool HasResponsibility(MonoBehaviour component, string responsibility)
        {
            // 리플렉션을 사용하여 컴포넌트의 메서드와 필드를 검사
            var type = component.GetType();
            
            switch (responsibility.ToLower())
            {
                case "ui_management":
                    return HasUIManagementMethods(type);
                case "object_creation":
                    return HasObjectCreationMethods(type);
                case "data_persistence":
                    return HasDataPersistenceMethods(type);
                case "scene_management":
                    return HasSceneManagementMethods(type);
                case "audio_management":
                    return HasAudioManagementMethods(type);
                default:
                    return false;
            }
        }

        private static bool HasUIManagementMethods(System.Type type)
        {
            var methods = type.GetMethods().Select(m => m.Name.ToLower());
            return methods.Any(m => m.Contains("ui") || m.Contains("button") || m.Contains("panel"));
        }

        private static bool HasObjectCreationMethods(System.Type type)
        {
            var methods = type.GetMethods().Select(m => m.Name.ToLower());
            return methods.Any(m => m.Contains("create") || m.Contains("instantiate") || m.Contains("spawn"));
        }

        private static bool HasDataPersistenceMethods(System.Type type)
        {
            var methods = type.GetMethods().Select(m => m.Name.ToLower());
            return methods.Any(m => m.Contains("save") || m.Contains("load") || m.Contains("persist"));
        }

        private static bool HasSceneManagementMethods(System.Type type)
        {
            var methods = type.GetMethods().Select(m => m.Name.ToLower());
            return methods.Any(m => m.Contains("scene") || m.Contains("loadscene") || m.Contains("transition"));
        }

        private static bool HasAudioManagementMethods(System.Type type)
        {
            var methods = type.GetMethods().Select(m => m.Name.ToLower());
            return methods.Any(m => m.Contains("audio") || m.Contains("sound") || m.Contains("music"));
        }

        #endregion

        #region 역할별 컴포넌트 검색

        /// <summary>
        /// 특정 역할의 컴포넌트들을 찾기
        /// </summary>
        public static List<MonoBehaviour> FindComponentsByRole(ComponentRole role)
        {
            var components = new List<MonoBehaviour>();
            var allComponents = Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);

            foreach (var component in allComponents)
            {
                var roleInfo = GetComponentRoleInfo(component);
                if (roleInfo != null && roleInfo.Role == role)
                {
                    components.Add(component);
                }
            }

            return components;
        }

        /// <summary>
        /// 역할 충돌 검사
        /// </summary>
        public static void CheckRoleConflicts()
        {
            GameLogger.LogInfo("=== 컴포넌트 역할 충돌 검사 시작 ===", GameLogger.LogCategory.Core);

            var allComponents = Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
            var roleGroups = allComponents
                .Select(c => GetComponentRoleInfo(c))
                .Where(r => r != null)
                .GroupBy(r => r.Role)
                .ToList();

            foreach (var group in roleGroups)
            {
                if (group.Count() > 1)
                {
                    GameLogger.LogWarning($"역할 '{group.Key}'에 {group.Count()}개의 컴포넌트가 있습니다:", GameLogger.LogCategory.Core);
                    foreach (var roleInfo in group)
                    {
                        GameLogger.LogWarning($"- {roleInfo.Component.GetType().Name}", GameLogger.LogCategory.Core);
                    }
                }
            }

            // 금지된 책임 검사
            foreach (var component in allComponents)
            {
                CheckForbiddenResponsibilities(component);
            }

            GameLogger.LogInfo("=== 컴포넌트 역할 충돌 검사 완료 ===", GameLogger.LogCategory.Core);
        }

        #endregion

        #region 기본 역할 등록

        /// <summary>
        /// 기본 컴포넌트 역할들을 등록
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void RegisterDefaultRoles()
        {
            // DI Installer들
            RegisterComponentRole<CoreSystemInstaller>(ComponentRole.DI_BINDING, "CoreSystem DI 바인딩 전용");
            RegisterComponentRole<CombatInstaller>(ComponentRole.DI_BINDING, "CombatSystem DI 바인딩 전용");
            
            // 매니저들
            RegisterComponentRole<PlayerManager>(ComponentRole.MANAGER, "플레이어 캐릭터 관리 전용");
            RegisterComponentRole<CombatFlowManager>(ComponentRole.MANAGER, "전투 플로우 관리 전용");
            // CombatSlotManager 제거됨 - 슬롯 관리 기능을 CombatFlowManager로 통합
            
            // UI 컨트롤러들
            RegisterComponentRole<PlayerCharacterUIController>(ComponentRole.UI_CONTROLLER, "플레이어 UI 제어 전용");
            
            // 초기화자들
            RegisterComponentRole<PlayerSkillCardInitializer>(ComponentRole.INITIALIZER, "플레이어 스킬카드 초기화 전용");

            // 금지된 책임들 정의
            AddForbiddenResponsibility<CoreSystemInstaller>("object_creation");
            AddForbiddenResponsibility<CombatInstaller>("object_creation");
            AddForbiddenResponsibility<PlayerManager>("ui_management");
            AddForbiddenResponsibility<CombatFlowManager>("data_persistence");
        }

        #endregion
    }
}
