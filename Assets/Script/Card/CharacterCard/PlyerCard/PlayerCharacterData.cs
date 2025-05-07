using UnityEngine;

namespace Game.Data
{
    /// <summary>
    /// 플레이어 캐릭터의 기본 능력치를 저장하는 데이터
    /// </summary>
    [CreateAssetMenu(menuName = "Game/Character/Player Character Data")]
    public class PlayerCharacterData : ScriptableObject
    {
        public string displayName;
        public int maxHP;
        public Sprite portrait;
        public AudioClip voiceClip;

        // 추가 확장 가능 필드 예: public int baseAttack;
    }
}
