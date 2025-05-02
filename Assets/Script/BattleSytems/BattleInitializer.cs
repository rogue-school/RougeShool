using Game.UI;
using Game.Units;
using UnityEngine;

public class BattleInitializer : MonoBehaviour
{
    public PlayerUnit player;
    public CharacterCardUI playerCardUI;

    private void Start()
    {
        var data = player.characterData;
        playerCardUI.Initialize(data, player.currentHP);
    }
}