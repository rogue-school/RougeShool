using UnityEngine;
using Game.UI;

namespace Game.Battle.Initialization
{
    /// <summary>
    /// UI 요소(캐릭터 카드 등)를 초기화합니다.
    /// </summary>
    public static class UIInitializer
    {
        public static void SetupCharacterUI()
        {
            var ui = Object.FindObjectOfType<CharacterCardUI>();
            if (ui != null)
                ui.UpdateUI();

            Debug.Log("[UIInitializer] 캐릭터 UI 초기화 완료");
        }
    }
}
