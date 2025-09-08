using UnityEngine;

namespace Game.CharacterSystem.Data
{
    /// <summary>
    /// 플레이어 캐릭터 타입별 특색과 설정을 관리하는 헬퍼 클래스입니다.
    /// </summary>
    public static class PlayerCharacterTypeHelper
    {
        /// <summary>
        /// 캐릭터 타입별 기본 리소스 설정을 반환합니다.
        /// </summary>
        /// <param name="characterType">캐릭터 타입</param>
        /// <returns>리소스 이름과 최대값</returns>
        public static (string resourceName, int maxResource) GetResourceInfo(PlayerCharacterType characterType)
        {
            return characterType switch
            {
                PlayerCharacterType.Sword => ("무리소스", 0),
                PlayerCharacterType.Bow => ("화살", 10),
                PlayerCharacterType.Staff => ("마나", 8),
                _ => ("무리소스", 0)
            };
        }

        /// <summary>
        /// 캐릭터 타입별 설명을 반환합니다.
        /// </summary>
        /// <param name="characterType">캐릭터 타입</param>
        /// <returns>캐릭터 설명</returns>
        public static string GetDescription(PlayerCharacterType characterType)
        {
            return characterType switch
            {
                PlayerCharacterType.Sword => "근접 공격 특화. 강력한 물리 공격과 방어 능력을 가집니다.",
                PlayerCharacterType.Bow => "원거리 공격 특화. 화살을 소모하여 정확한 공격을 합니다.",
                PlayerCharacterType.Staff => "마법 공격 특화. 마나를 소모하여 다양한 마법을 사용합니다.",
                _ => "알 수 없는 캐릭터 타입입니다."
            };
        }

        /// <summary>
        /// 캐릭터 타입별 스킬카드 특색을 반환합니다.
        /// </summary>
        /// <param name="characterType">캐릭터 타입</param>
        /// <returns>스킬카드 특색 설명</returns>
        public static string GetSkillCardConcept(PlayerCharacterType characterType)
        {
            return characterType switch
            {
                PlayerCharacterType.Sword => "강력한 근접 공격, 방어, 연속 공격",
                PlayerCharacterType.Bow => "원거리 공격, 화살 소모, 정확한 타격",
                PlayerCharacterType.Staff => "마법 공격, 마나 소모, 다양한 효과",
                _ => "알 수 없는 특색입니다."
            };
        }
    }
}
