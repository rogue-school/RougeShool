using UnityEngine;
using Game.UI;
using Game.Characters;

namespace Game.Managers
{
    public class UIInitializer : MonoBehaviour
    {
        public static UIInitializer Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
        }

        public void SetupCharacterUI()
        {
            var uiCards = GameObject.FindObjectsOfType<CharacterCardUI>();

            foreach (var ui in uiCards)
            {
                var character = ui.GetComponentInParent<CharacterBase>();
                if (character == null)
                {
                    Debug.LogWarning($"[UIInitializer] {ui.name} 주변에 CharacterBase를 찾을 수 없습니다.");
                    continue;
                }
                ui.Initialize(character);
            }
        }
    }
}
