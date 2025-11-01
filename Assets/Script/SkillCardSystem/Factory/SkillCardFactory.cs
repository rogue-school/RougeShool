using System;
using UnityEngine;
using Game.SkillCardSystem.Data;
using Game.SkillCardSystem.Runtime;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.Effect;
using Game.CombatSystem.Interface;
using Game.CoreSystem.Utility;
using Game.CoreSystem.Interface;
using Zenject;

namespace Game.SkillCardSystem.Factory
{
    /// <summary>
    /// 스킬카드 생성을 담당하는 팩토리 클래스입니다.
    /// 새로운 통합 구조를 사용하여 카드를 생성합니다.
    /// </summary>
    public class SkillCardFactory : ISkillCardFactory
    {
        private readonly IAudioManager audioManager;

        [Inject]
        public SkillCardFactory(IAudioManager audioManager)
        {
            this.audioManager = audioManager;
        }
        /// <summary>
        /// 카드 정의로부터 스킬카드를 생성합니다.
        /// </summary>
        /// <param name="definition">카드 정의</param>
        /// <param name="owner">소유자</param>
        /// <param name="ownerCharacterName">소유 캐릭터 이름</param>
        /// <returns>생성된 스킬카드</returns>
        public ISkillCard CreateFromDefinition(SkillCardDefinition definition, Owner owner, string ownerCharacterName = null)
        {
            return CreateFromDefinition(definition, owner, -1);
        }

        /// <summary>
        /// 카드 정의로부터 스킬카드를 생성합니다 (데미지 오버라이드 지원).
        /// </summary>
        /// <param name="definition">카드 정의</param>
        /// <param name="owner">소유자</param>
        /// <param name="damageOverride">데미지 오버라이드 (옵셔널, -1이면 기본값 사용)</param>
        /// <returns>생성된 스킬카드</returns>
        public ISkillCard CreateFromDefinition(SkillCardDefinition definition, Owner owner, int damageOverride)
        {
            if (definition == null)
            {
                GameLogger.LogError("[SkillCardFactory] SkillCardDefinition이 null입니다.", GameLogger.LogCategory.SkillCard);
                throw new ArgumentNullException(nameof(definition), "카드 정의는 null일 수 없습니다.");
            }
            
            // 정책 확인
            if (definition.configuration.ownerPolicy == OwnerPolicy.Player && owner != Owner.Player)
            {
                GameLogger.LogWarning($"[SkillCardFactory] 카드 '{definition.displayName}'은 플레이어 전용입니다.", GameLogger.LogCategory.SkillCard);
                throw new InvalidOperationException($"카드 '{definition.displayName}'은 플레이어 전용입니다. 현재 소유자: {owner}");
            }
            
            if (definition.configuration.ownerPolicy == OwnerPolicy.Enemy && owner != Owner.Enemy)
            {
                GameLogger.LogWarning($"[SkillCardFactory] 카드 '{definition.displayName}'은 적 전용입니다.", GameLogger.LogCategory.SkillCard);
                throw new InvalidOperationException($"카드 '{definition.displayName}'은 적 전용입니다. 현재 소유자: {owner}");
            }
            
            var skillCard = new SkillCard(definition, owner, audioManager, damageOverride);
            
            // GameLogger.LogInfo($"[SkillCardFactory] 카드 생성 완료: {definition.displayName} (Owner: {owner})", GameLogger.LogCategory.SkillCard);
            
            return skillCard;
        }
        
        /// <summary>
        /// 카드 ID로부터 스킬카드를 생성합니다.
        /// </summary>
        /// <param name="cardId">카드 ID</param>
        /// <param name="owner">소유자</param>
        /// <returns>생성된 스킬카드</returns>
        public ISkillCard CreateFromId(string cardId, Owner owner)
        {
            if (string.IsNullOrEmpty(cardId))
            {
                GameLogger.LogError("[SkillCardFactory] 카드 ID가 null이거나 비어있습니다.", GameLogger.LogCategory.SkillCard);
                throw new ArgumentNullException(nameof(cardId), "카드 ID는 null이거나 비어있을 수 없습니다.");
            }
            
            var definition = Resources.Load<SkillCardDefinition>($"SkillCards/{cardId}");
            
            if (definition == null)
            {
                GameLogger.LogError($"[SkillCardFactory] 카드 정의를 찾을 수 없습니다: {cardId}", GameLogger.LogCategory.SkillCard);
                throw new InvalidOperationException($"카드 ID '{cardId}'에 해당하는 정의를 찾을 수 없습니다.");
            }
            
            return CreateFromDefinition(definition, owner);
        }
        
        /// <summary>
        /// 플레이어 카드를 생성합니다.
        /// </summary>
        /// <param name="definition">카드 정의</param>
        /// <param name="ownerCharacterName">소유 캐릭터 이름</param>
        /// <returns>생성된 플레이어 스킬카드</returns>
        public ISkillCard CreatePlayerCard(SkillCardDefinition definition, string ownerCharacterName = null)
        {
            var card = CreateFromDefinition(definition, Owner.Player);
            
            if (card != null && !string.IsNullOrEmpty(ownerCharacterName))
            {
                GameLogger.LogInfo($"[SkillCardFactory] 플레이어 카드 생성: {definition.displayName} (Character: {ownerCharacterName})", GameLogger.LogCategory.SkillCard);
            }
            
            return card;
        }
        
        /// <summary>
        /// 적 카드를 생성합니다.
        /// </summary>
        /// <param name="definition">카드 정의</param>
        /// <param name="ownerCharacterName">소유 캐릭터 이름</param>
        /// <param name="damageOverride">데미지 오버라이드 (옵셔널, -1이면 기본값 사용)</param>
        /// <returns>생성된 적 스킬카드</returns>
        public ISkillCard CreateEnemyCard(SkillCardDefinition definition, string ownerCharacterName = null, int damageOverride = -1)
        {
            var card = CreateFromDefinition(definition, Owner.Enemy, damageOverride);
            
            if (card != null && !string.IsNullOrEmpty(ownerCharacterName))
            {
                if (damageOverride >= 0)
                {
                    GameLogger.LogInfo($"[SkillCardFactory] 적 카드 생성 (데미지 오버라이드): {definition.displayName} (Character: {ownerCharacterName}, Damage: {damageOverride})", GameLogger.LogCategory.SkillCard);
                }
                else
                {
                    // GameLogger.LogInfo($"[SkillCardFactory] 적 카드 생성: {definition.displayName} (Character: {ownerCharacterName})", GameLogger.LogCategory.SkillCard);
                }
            }
            
            return card;
        }
        
        /// <summary>
        /// 카드 정의의 유효성을 검증합니다.
        /// </summary>
        /// <param name="definition">카드 정의</param>
        /// <returns>유효성 여부</returns>
        public bool ValidateDefinition(SkillCardDefinition definition)
        {
            if (definition == null)
            {
                GameLogger.LogError("[SkillCardFactory] 카드 정의가 null입니다.", GameLogger.LogCategory.SkillCard);
                return false;
            }
            
            if (string.IsNullOrEmpty(definition.cardId))
            {
                GameLogger.LogError("[SkillCardFactory] 카드 ID가 비어있습니다.", GameLogger.LogCategory.SkillCard);
                return false;
            }
            
            if (string.IsNullOrEmpty(definition.displayName))
            {
                GameLogger.LogError("[SkillCardFactory] 카드 표시 이름이 비어있습니다.", GameLogger.LogCategory.SkillCard);
                return false;
            }
            
            if (definition.artwork == null)
            {
                GameLogger.LogWarning($"[SkillCardFactory] 카드 '{definition.displayName}'의 아트워크가 없습니다.", GameLogger.LogCategory.SkillCard);
            }
            
            // 데미지와 효과가 모두 없는 경우 경고
            if (!definition.configuration.hasDamage && !definition.configuration.hasEffects)
            {
                GameLogger.LogWarning($"[SkillCardFactory] 카드 '{definition.displayName}'에 데미지나 효과가 없습니다.", GameLogger.LogCategory.SkillCard);
            }
            
            // 효과 구성 검증
            if (definition.configuration.hasEffects)
            {
                foreach (var effectConfig in definition.configuration.effects)
                {
                    if (effectConfig.effectSO == null)
                    {
                        GameLogger.LogError($"[SkillCardFactory] 카드 '{definition.displayName}'에 null 효과가 있습니다.", GameLogger.LogCategory.SkillCard);
                        return false;
                    }
                }
            }
            
            return true;
        }
        
        /// <summary>
        /// 카드 정의를 로드합니다.
        /// </summary>
        /// <param name="cardId">카드 ID</param>
        /// <returns>카드 정의</returns>
        public SkillCardDefinition LoadDefinition(string cardId)
        {
            var definition = Resources.Load<SkillCardDefinition>($"SkillCards/{cardId}");
            
            if (definition == null)
            {
                GameLogger.LogError($"[SkillCardFactory] 카드 정의를 로드할 수 없습니다: {cardId}", GameLogger.LogCategory.SkillCard);
                return null;
            }
            
            return definition;
        }
        
        /// <summary>
        /// 모든 카드 정의를 로드합니다.
        /// </summary>
        /// <returns>카드 정의 배열</returns>
        public SkillCardDefinition[] LoadAllDefinitions()
        {
            var definitions = Resources.LoadAll<SkillCardDefinition>("SkillCards");
            
            GameLogger.LogInfo($"[SkillCardFactory] 총 {definitions.Length}개의 카드 정의를 로드했습니다.", GameLogger.LogCategory.SkillCard);
            
            return definitions;
        }
    }
}