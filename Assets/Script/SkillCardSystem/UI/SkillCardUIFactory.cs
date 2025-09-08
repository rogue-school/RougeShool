using UnityEngine;
using Game.SkillCardSystem.Interface;
using Game.CombatSystem.DragDrop;
using Game.CombatSystem.Interface;

namespace Game.SkillCardSystem.UI
{
    /// <summary>
    /// 스킬 카드 UI 프리팹을 인스턴스화하고 초기화하는 정적 팩토리 클래스입니다.
    /// </summary>
    public static class SkillCardUIFactory
    {
        /// <summary>
        /// SkillCardUI 프리팹을 생성하고 카드 데이터 및 드래그 핸들러를 설정합니다.
        /// </summary>
        /// <param name="prefab">스킬 카드 UI 프리팹</param>
        /// <param name="parent">UI를 배치할 부모 트랜스폼</param>
        /// <param name="card">연결할 카드 데이터</param>
        /// <param name="flowCoordinator">카드 실행을 조정하는 흐름 제어자</param>
        /// <returns>초기화된 SkillCardUI 인스턴스</returns>
        public static SkillCardUI CreateUI(
            SkillCardUI prefab,
            Transform parent,
            ISkillCard card,
            ICombatFlowCoordinator flowCoordinator)
        {
            // === 유효성 검사 ===
            if (prefab == null || parent == null || card == null)
            {
                Debug.LogError("[SkillCardUIFactory] 카드 UI 생성 실패 - null 인자 존재");
                return null;
            }

            // === 프리팹 인스턴스 생성 ===
            var instance = Object.Instantiate(prefab, parent, false);
            if (instance == null)
            {
                Debug.LogError("[SkillCardUIFactory] 프리팹 인스턴스화 실패");
                return null;
            }

            // === 카드 데이터 설정 ===
            instance.SetCard(card);

            // === 기본 Transform 초기화 ===
            if (instance.TryGetComponent(out RectTransform rect))
            {
                rect.anchoredPosition = Vector2.zero;
                rect.localRotation = Quaternion.identity;
                rect.localScale = Vector3.one;
            }

            // === 필수 컴포넌트 보장 및 드래그 핸들러 연결 ===
            // CanvasGroup이 없으면 추가 (CardDragHandler가 사용)
            if (!instance.TryGetComponent<UnityEngine.CanvasGroup>(out var cg))
            {
                cg = instance.gameObject.AddComponent<UnityEngine.CanvasGroup>();
            }

            // CardDragHandler가 없으면 추가
            if (!instance.TryGetComponent<CardDragHandler>(out var dragHandler))
            {
                dragHandler = instance.gameObject.AddComponent<CardDragHandler>();
            }

            if (dragHandler != null)
            {
                if (flowCoordinator != null)
                {
                    dragHandler.Inject(flowCoordinator);
                }
                else
                {
                    Debug.LogWarning("[SkillCardUIFactory] flowCoordinator가 null입니다.");
                }
            }

            // === Raycast 설정: 기본적으로 클릭 가능해야 함 ===
            foreach (var img in instance.GetComponentsInChildren<UnityEngine.UI.Image>())
            {
                img.raycastTarget = true;
            }

            return instance;
        }
    }
}
