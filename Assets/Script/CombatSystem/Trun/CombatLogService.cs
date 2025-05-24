using Game.CombatSystem.Interface;
using UnityEngine;

namespace Game.CombatSystem.Turn
{
    public class CombatLogService : ICombatLogService
    {
        public void Log(string message)
        {
            Debug.Log("[Combat] " + message);
        }
    }
}
