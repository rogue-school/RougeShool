using UnityEngine;
using Game.CombatSystem.Interface;
using Game.CombatSystem.Slot;

public class SlotIdentityDebugger : MonoBehaviour
{
    void Start()
    {
        string owner = "Unknown";
        string type = "Unknown";

        if (TryGetComponent<ICombatCardSlot>(out var combatSlot))
        {
            var combatPos = combatSlot.GetCombatPosition();
            type = $"CombatSlot ({combatPos})";

            // 위치 기반 추론 (선택사항)
            owner = combatPos switch
            {
                CombatFieldSlotPosition.FIELD_LEFT => "Likely ENEMY",
                CombatFieldSlotPosition.FIELD_RIGHT => "Likely PLAYER",
                _ => "Unknown"
            };
        }
        else
        {
            owner = "Possibly HandSlot";
            type = "Not a CombatCardSlot";
        }

        Debug.Log($"[SlotIdentityDebugger] {gameObject.name} → {type}, Owner: {owner}");
    }
}
