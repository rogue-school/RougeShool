using UnityEngine;

namespace Game.SkillCardSystem.Data
{
    /// <summary>
    /// 스킬 카드의 정적 정보를 담는 구조체입니다.
    /// </summary>
    [System.Serializable]
    public struct SkillCardData
    {
        public string Name;
        public string Description;
        public Sprite Artwork;
        public int CoolTime;
        public int Damage;

        public SkillCardData(string name, string description, Sprite artwork, int coolTime, int damage)
        {
            Name = name;
            Description = description;
            Artwork = artwork;
            CoolTime = coolTime;
            Damage = damage;
        }
    }
}
