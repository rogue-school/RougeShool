using UnityEngine;
using Game.CoreSystem.Utility;
using Game.SkillCardSystem.Data;

namespace Game.CharacterSystem.Data
{
    /// <summary>
    /// 메인 로비 및 씬 간 공유를 위한 선택 캐릭터 요약 데이터(SO).
    /// 엠블럼, 설명, 대표 스킬 3개만 저장합니다.
    /// </summary>
    [CreateAssetMenu(menuName = "CharacterSystem/SelectedCharacter", fileName = "SelectedCharacterSO")]
    public class SelectedCharacterSO : ScriptableObject
    {
        [Header("엠블럼/설명")]
        [Tooltip("선택된 캐릭터의 엠블럼 이미지")]
        [SerializeField] private Sprite emblem;
        
        [Tooltip("선택된 캐릭터의 설명 텍스트")]
        [TextArea(2, 4)]
        [SerializeField] private string description;
        
        [Header("대표 스킬(최대 3개)")]
        [Tooltip("실제 사용할 스킬 카드 정의 3개")]
        [SerializeField] private SkillCardDefinition[] skillCards = new SkillCardDefinition[3];

        /// <summary>
        /// 엠블럼 스프라이트
        /// </summary>
        public Sprite Emblem => emblem;
        
        /// <summary>
        /// 캐릭터 설명
        /// </summary>
        public string Description => description;
        
        /// <summary>
        /// 대표 스킬 카드 정의들(길이 3 보장 아님)
        /// </summary>
        public SkillCardDefinition[] SkillCards => skillCards;

        /// <summary>
        /// 선택 캐릭터 요약 정보를 설정합니다.
        /// </summary>
        public void Set(Sprite emblemSprite, string desc, SkillCardDefinition[] skills)
        {
            emblem = emblemSprite;
            description = desc ?? string.Empty;
            if (skills == null || skills.Length == 0)
            {
                skillCards = new SkillCardDefinition[0];
            }
            else
            {
                int count = Mathf.Min(3, skills.Length);
                if (skillCards == null || skillCards.Length != count)
                    skillCards = new SkillCardDefinition[count];
                for (int i = 0; i < count; i++)
                {
                    skillCards[i] = skills[i];
                }
            }
            GameLogger.LogInfo("[SelectedCharacterSO] 선택 캐릭터 요약 정보 설정 완료", GameLogger.LogCategory.UI);
        }

        /// <summary>
        /// 선택 정보를 초기화합니다.
        /// </summary>
        public void Clear()
        {
            emblem = null;
            description = string.Empty;
            skillCards = new SkillCardDefinition[0];
            GameLogger.LogInfo("[SelectedCharacterSO] 선택 캐릭터 요약 초기화", GameLogger.LogCategory.UI);
        }
    }
}


