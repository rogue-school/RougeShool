using System.Collections.Generic;
using UnityEngine;
using AnimationSystem.Interface;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.UI;

namespace AnimationSystem.Service
{
    /// <summary>
    /// 스킬카드 찾기 서비스 구현체
    /// 단일 책임 원칙에 따라 스킬카드 찾기 로직만 담당
    /// </summary>
    public class SkillCardFinder : MonoBehaviour, ISkillCardFinder
    {
        private const string COMPONENT_NAME = "SkillCardFinder";
        private const string PLAYER_PREFIX = "Player";
        private const string ENEMY_PREFIX = "Enemy";

        public List<GameObject> FindCharacterSkillCards(string characterName, bool isPlayerCharacter)
        {
            var skillCards = new List<GameObject>();
            
            Debug.Log($"[{COMPONENT_NAME}] 캐릭터 스킬카드 검색 시작: {characterName}, 플레이어: {isPlayerCharacter}");
            
            var cardSlots = FindObjectsByType<SkillCardUI>(FindObjectsSortMode.None);
            
            foreach (var cardSlot in cardSlots)
            {
                if (cardSlot?.GetCard() == null) continue;
                
                if (IsSkillCardBelongsToCharacter(cardSlot.GetCard(), characterName, isPlayerCharacter))
                {
                    skillCards.Add(cardSlot.gameObject);
                    Debug.Log($"[{COMPONENT_NAME}] 스킬카드 발견: {cardSlot.gameObject.name}");
                }
            }
            
            Debug.Log($"[{COMPONENT_NAME}] 찾은 스킬카드 수: {skillCards.Count}");
            return skillCards;
        }
        
        public bool IsSkillCardBelongsToCharacter(ISkillCard skillCard, string characterName, bool isPlayerCharacter)
        {
            if (skillCard == null) return false;
            
            var cardName = skillCard.GetCardName();
            var prefix = isPlayerCharacter ? PLAYER_PREFIX : ENEMY_PREFIX;
            
            return cardName.Contains(prefix) || cardName.Contains(characterName);
        }
    }
} 