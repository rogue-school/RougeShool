using UnityEngine;
using UnityEngine.EventSystems;
using Game.CoreSystem.Interface;
using Game.CoreSystem.Utility;
using Game.SkillCardSystem.UI;
using System.Collections.Generic;
using Zenject;

namespace Game.SkillCardSystem.Manager
{
    /// <summary>
    /// 툴팁 클릭 관리를 담당하는 매니저입니다.
    /// 전역 좌클릭 감지로 고정된 툴팁들을 해제합니다.
    /// </summary>
    public class TooltipClickManager : MonoBehaviour, ICoreSystemInitializable
    {
        #region ICoreSystemInitializable 구현

        public bool IsInitialized { get; private set; } = false;

        public System.Collections.IEnumerator Initialize()
        {
            GameLogger.LogInfo($"{GetType().Name} 초기화 시작", GameLogger.LogCategory.UI);
            
            // 전역 클릭 감지 시스템 초기화
            InitializeGlobalClickDetection();
            
            IsInitialized = true;
            GameLogger.LogInfo($"{GetType().Name} 초기화 완료", GameLogger.LogCategory.UI);
            yield return null;
        }

        public void OnInitializationFailed()
        {
            GameLogger.LogError($"{GetType().Name} 초기화 실패", GameLogger.LogCategory.Error);
            IsInitialized = false;
        }

        #endregion

        #region 전역 클릭 감지

        private void Update()
        {
            // 좌클릭 감지
            if (Input.GetMouseButtonDown(0))
            {
                HandleGlobalLeftClick();
            }
        }

        /// <summary>
        /// 전역 좌클릭을 처리합니다.
        /// </summary>
        private void HandleGlobalLeftClick()
        {
            // 고정된 툴팁이 있는지 먼저 확인
            if (!HasFixedTooltips())
            {
                return; // 고정된 툴팁이 없으면 아무것도 하지 않음
            }

            // 클릭된 오브젝트 확인
            var clickedObject = GetClickedObject();
            
            // 스킬 카드나 툴팁이 아닌 곳을 클릭했으면 모든 고정된 툴팁 해제
            if (!IsSkillCardOrTooltip(clickedObject))
            {
                ReleaseAllFixedTooltips();
            }
        }

        /// <summary>
        /// 클릭된 오브젝트를 가져옵니다.
        /// </summary>
        private GameObject GetClickedObject()
        {
            var eventData = new PointerEventData(EventSystem.current)
            {
                position = Input.mousePosition
            };

            var results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, results);

            return results.Count > 0 ? results[0].gameObject : null;
        }

        /// <summary>
        /// 클릭된 오브젝트가 스킬 카드나 툴팁인지 확인합니다.
        /// </summary>
        private bool IsSkillCardOrTooltip(GameObject clickedObject)
        {
            if (clickedObject == null) return false;

            // SkillCardUI 컴포넌트 확인
            if (clickedObject.GetComponent<SkillCardUI>() != null) return true;

            // SkillCardTooltip 컴포넌트 확인
            if (clickedObject.GetComponent<SkillCardTooltip>() != null) return true;

            // 부모 오브젝트들도 확인 (툴팁의 자식 요소들)
            Transform parent = clickedObject.transform.parent;
            while (parent != null)
            {
                if (parent.GetComponent<SkillCardUI>() != null) return true;
                if (parent.GetComponent<SkillCardTooltip>() != null) return true;
                parent = parent.parent;
            }

            return false;
        }

        /// <summary>
        /// 고정된 툴팁이 있는지 확인합니다.
        /// </summary>
        private bool HasFixedTooltips()
        {
            // 모든 SkillCardUI 컴포넌트를 찾아서 고정된 툴팁이 있는지 확인
            var skillCardUIs = FindObjectsByType<SkillCardUI>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            
            foreach (var skillCardUI in skillCardUIs)
            {
                if (skillCardUI.IsTooltipFixed())
                {
                    return true; // 고정된 툴팁이 하나라도 있으면 true
                }
            }

            return false; // 고정된 툴팁이 없으면 false
        }

        /// <summary>
        /// 모든 고정된 툴팁을 해제합니다.
        /// </summary>
        private void ReleaseAllFixedTooltips()
        {
            // 모든 SkillCardUI 컴포넌트를 찾아서 고정된 툴팁 해제
            var skillCardUIs = FindObjectsByType<SkillCardUI>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            
            int releasedCount = 0;
            foreach (var skillCardUI in skillCardUIs)
            {
                if (skillCardUI.IsTooltipFixed())
                {
                    skillCardUI.ForceReleaseTooltip();
                    releasedCount++;
                }
            }

            if (releasedCount > 0)
            {
                GameLogger.LogInfo($"전역 클릭으로 {releasedCount}개의 고정된 툴팁 해제", GameLogger.LogCategory.UI);
            }
        }

        #endregion

        #region 초기화

        /// <summary>
        /// 전역 클릭 감지 시스템을 초기화합니다.
        /// </summary>
        private void InitializeGlobalClickDetection()
        {
            GameLogger.LogInfo("전역 클릭 감지 시스템 초기화 완료", GameLogger.LogCategory.UI);
        }

        #endregion
    }
}
