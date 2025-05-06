using Game.Battle;
using Game.Player;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game Assets/Charater/PlayerCharacter", fileName = "NewPlayerCharacter")]
public class PlayerCharacterData : ScriptableObject
{
    public string characterName;
    public Sprite portrait;
    public int maxHP;

    [Header("배치 슬롯 위치")]
    public BattleSlotPosition slotPosition;

    [Header("사용 가능한 스킬 카드 목록")]
    public List<PlayerSkillCard> skillCards;

    public BattleSlotPosition battleSlotPosition;
}
