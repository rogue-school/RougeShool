using UnityEngine;
using Game.CoreSystem.Utility;

namespace Game.UISystem
{
    /// <summary>
    /// UI 패널을 관리하는 매니저 클래스
    /// 패널 표시/숨김 및 전환 기능을 제공합니다.
    /// </summary>
    public class PanelManager : BaseUIController
    {
        #region PanelManager 전용 설정

        [Header("패널 관리 설정")]
        [Tooltip("표시할 패널")]
        [SerializeField] private GameObject panelA;

        [Tooltip("비활성화할 패널")]
        [SerializeField] private GameObject panelToDisable;

        [Tooltip("비활성화할 텍스트")]
        [SerializeField] private GameObject defeatTextToDisable;

        [Tooltip("패널 전환 애니메이션")]
        [SerializeField] private bool enablePanelTransition = true;

        #endregion

        #region 베이스 클래스 구현

        protected override System.Collections.IEnumerator OnInitialize()
        {
            // 참조 검증
            ValidateReferences();

            // UI 상태 로깅
            LogUIState();

            yield return null;
        }

        public override void Reset()
        {
            // 모든 패널 비활성화
            if (panelA != null)
                panelA.SetActive(false);

            if (panelToDisable != null)
                panelToDisable.SetActive(false);

            if (defeatTextToDisable != null)
                defeatTextToDisable.SetActive(false);

            if (enableDebugLogging)
            {
                GameLogger.LogInfo("PanelManager 리셋 완료", GameLogger.LogCategory.UI);
            }
        }

        #endregion

        #region 패널 관리

        /// <summary>
        /// 패널 A를 표시합니다.
        /// </summary>
        public void ShowPanelA()
        {
            if (panelA == null)
            {
                GameLogger.LogWarning("표시할 패널이 할당되지 않았습니다.", GameLogger.LogCategory.UI);
                return;
            }

            // 패널 표시
            panelA.SetActive(true);

            // 다른 패널들 비활성화
            if (panelToDisable != null)
                panelToDisable.SetActive(false);

            if (defeatTextToDisable != null)
                defeatTextToDisable.SetActive(false);

            // 애니메이션 적용
            if (enablePanelTransition)
            {
                FadeIn();
            }

            if (enableDebugLogging)
            {
                GameLogger.LogInfo("패널 A 표시 완료", GameLogger.LogCategory.UI);
            }
        }

        /// <summary>
        /// 패널 A를 숨깁니다.
        /// </summary>
        public void HidePanelA()
        {
            if (panelA == null)
            {
                return;
            }

            // 애니메이션 적용
            if (enablePanelTransition)
            {
                FadeOut();
            }

            // 패널 숨김
            panelA.SetActive(false);

            if (enableDebugLogging)
            {
                GameLogger.LogInfo("패널 A 숨김 완료", GameLogger.LogCategory.UI);
            }
        }

        /// <summary>
        /// 모든 패널을 숨깁니다.
        /// </summary>
        public void HideAllPanels()
        {
            if (panelA != null)
                panelA.SetActive(false);

            if (panelToDisable != null)
                panelToDisable.SetActive(false);

            if (defeatTextToDisable != null)
                defeatTextToDisable.SetActive(false);

            if (enableDebugLogging)
            {
                GameLogger.LogInfo("모든 패널 숨김 완료", GameLogger.LogCategory.UI);
            }
        }

        #endregion

        #region 유틸리티

        /// <summary>
        /// 패널 상태를 확인합니다.
        /// </summary>
        public bool IsPanelAActive()
        {
            return panelA != null && panelA.activeInHierarchy;
        }

        /// <summary>
        /// 패널 참조를 설정합니다.
        /// </summary>
        public void SetPanelA(GameObject panel)
        {
            panelA = panel;
        }

        /// <summary>
        /// 비활성화할 패널을 설정합니다.
        /// </summary>
        public void SetPanelToDisable(GameObject panel)
        {
            panelToDisable = panel;
        }

        #endregion
    }
}
