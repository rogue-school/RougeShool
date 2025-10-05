using UnityEngine;
using Game.SkillCardSystem.Interface;
using Game.CoreSystem.Utility;
using Game.CombatSystem.DragDrop;
using Game.CombatSystem.Manager;
using Game.VFXSystem.Manager;

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
        /// <param name="animationFacade">애니메이션 파사드</param>
        /// <param name="vfxManager">VFX 매니저 (선택적, Object Pooling용)</param>
        /// <returns>초기화된 SkillCardUI 인스턴스</returns>
        public static SkillCardUI CreateUI(
            SkillCardUI prefab,
            Transform parent,
            ISkillCard card,
            object animationFacade,
            VFXManager vfxManager = null)
        {
            // === 유효성 검사 ===
            if (parent == null || card == null)
            {
                GameLogger.LogError("[SkillCardUIFactory] 카드 UI 생성 실패 - null 인자 존재", GameLogger.LogCategory.SkillCard);
                return null;
            }

            // === 프리팹 인스턴스 생성 (VFXManager 풀링 우선) ===
            SkillCardUI instance = null;
            if (vfxManager != null)
            {
                instance = vfxManager.GetSkillCardUI(parent);
                if (instance != null)
                {
                    GameLogger.LogInfo("[SkillCardUIFactory] VFXManager 풀에서 카드 UI 재사용", GameLogger.LogCategory.SkillCard);
                }
            }

            // Fallback: VFXManager가 없거나 풀이 비었으면 기존 방식 사용
            if (instance == null)
            {
                if (prefab == null)
                {
                    GameLogger.LogError("[SkillCardUIFactory] 프리팹과 VFXManager 모두 null입니다.", GameLogger.LogCategory.SkillCard);
                    return null;
                }

                instance = Object.Instantiate(prefab, parent, false);
                if (instance == null)
                {
                    GameLogger.LogError("[SkillCardUIFactory] 프리팹 인스턴스화 실패", GameLogger.LogCategory.SkillCard);
                    return null;
                }
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

            // CardDragHandler는 플레이어 카드에만 추가 (적 카드는 드래그 불가)
            if (card.IsFromPlayer())
            {
                if (!instance.TryGetComponent<CardDragHandler>(out var dragHandler))
                {
                    dragHandler = instance.gameObject.AddComponent<CardDragHandler>();
                }

                if (dragHandler != null)
                {
                    // CombatSlotManager 제거됨 - Inject 메서드 호출 제거
                    dragHandler.Inject();
                }
            }
            else
            {
                // 적 카드의 경우 기존 CardDragHandler 제거 (있다면)
                var existingDragHandler = instance.GetComponent<CardDragHandler>();
                if (existingDragHandler != null)
                {
                    Object.DestroyImmediate(existingDragHandler);
                }
            }

            // === Raycast 설정: 플레이어 카드는 클릭 가능, 적 카드는 클릭 불가 ===
            foreach (var img in instance.GetComponentsInChildren<UnityEngine.UI.Image>())
            {
                img.raycastTarget = card.IsFromPlayer();
            }

            return instance;
        }
    }
}
