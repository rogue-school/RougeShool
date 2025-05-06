using Game.Slots;
using Game.Enemy;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game Assets/Charater/EnemyCharacter", fileName = "EnemyCharacter")]
public class EnemyCharacterData : ScriptableObject
{
    public string characterName;
    public Sprite portrait;
    public int maxHP;

    [Header("배치 슬롯 위치")]
    public BattleSlotPosition slotPosition;

    [Header("전투 시 사용할 스킬 카드")]
    public List<EnemySkillCard> initialDeck;

    [Header("패시브 효과 카드 (전투 전 자동 적용)")]
    public List<EnemySkillCard> passiveSkills;

    public BattleSlotPosition battleSlotPosition;
}
