using UnityEngine;
using Game.IManager;

namespace Game.CombatSystem.Manager
{
    public class VictoryManager : MonoBehaviour, IVictoryManager
    {
        [SerializeField] private GameObject victoryUI;

        public void ShowVictoryUI()
        {
            Debug.Log("[VictoryManager] ÀüÅõ ½Â¸® UI È£ÃâµÊ");
            if (victoryUI != null)
                victoryUI.SetActive(true);
        }

        public void ProcessVictory()
        {
            Debug.Log("[VictoryManager] ½Â¸® Ã³¸® ½ÃÀÛ");
            ShowVictoryUI();
        }
    }
}
