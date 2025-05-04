using UnityEngine;
using Game.Characters;

namespace Game.Battle.Initialization
{
    /// <summary>
    /// 플레이어와 적 캐릭터들을 찾아 초기화합니다.
    /// </summary>
    public static class CharacterInitializer
    {
        public static void SetupCharacters()
        {
            var characters = Object.FindObjectsOfType<CharacterBase>();
            foreach (var character in characters)
            {
                character.Initialize(20); // 여기 20을 직접 넣어주세요!
            }

            Debug.Log("[CharacterInitializer] 캐릭터 초기화 완료");
        }
    }
}
