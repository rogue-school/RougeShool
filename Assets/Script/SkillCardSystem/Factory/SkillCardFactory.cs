using UnityEngine;
using System.Collections.Generic;
using Game.SkillCardSystem.Data;
using Game.SkillCardSystem.Effect;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.Runtime;
using System.Linq;
using Game.CombatSystem.Data;

namespace Game.SkillCardSystem.Factory
{
    /// <summary>
    /// 스킬 카드 런타임 인스턴스를 생성하는 팩토리 클래스입니다.
    /// <para>SRP: 카드 인스턴스 생성만 담당</para>
    /// <para>DIP: SkillCardData와 Effect 데이터에 의존</para>
    /// </summary>
    public class SkillCardFactory : ISkillCardFactory
    {
        /// <summary>
        /// 적 캐릭터용 스킬 카드 런타임 인스턴스를 생성합니다.
        /// </summary>
        /// <param name="data">카드 데이터</param>
        /// <param name="effects">카드 효과 리스트</param>
        /// <returns>생성된 적 카드 런타임 인스턴스</returns>
        public ISkillCard CreateEnemyCard(SkillCardData data, List<SkillCardEffectSO> effects, string ownerCharacterName)
        {
            if (data == null)
            {
                Debug.LogError("[SkillCardFactory] Enemy SkillCardData가 null입니다.");
                return null;
            }
            if (effects == null)
            {
                Debug.LogWarning("[SkillCardFactory] EnemyCard 효과 리스트가 null입니다. 빈 리스트로 대체합니다.");
                effects = new List<SkillCardEffectSO>();
            }
            data.OwnerCharacterName = ownerCharacterName;
            return new EnemySkillCardRuntime(data, CloneEffects(effects));
        }
        public ISkillCard CreatePlayerCard(SkillCardData data, List<SkillCardEffectSO> effects, string ownerCharacterName)
        {
            if (data == null)
            {
                Debug.LogError("[SkillCardFactory] Player SkillCardData가 null입니다.");
                return null;
            }
            if (effects == null)
            {
                Debug.LogWarning("[SkillCardFactory] PlayerCard 효과 리스트가 null입니다. 빈 리스트로 대체합니다.");
                effects = new List<SkillCardEffectSO>();
            }
            data.OwnerCharacterName = ownerCharacterName;
            return new PlayerSkillCardRuntime(data, CloneEffects(effects));
        }
        // 기존 시그니처도 유지 (ownerCharacterName 없이)
        public ISkillCard CreateEnemyCard(SkillCardData data, List<SkillCardEffectSO> effects)
        {
            return CreateEnemyCard(data, effects, null);
        }
        public ISkillCard CreatePlayerCard(SkillCardData data, List<SkillCardEffectSO> effects)
        {
            return CreatePlayerCard(data, effects, null);
        }

        /// <summary>
        /// 효과 리스트를 복제합니다. 현재는 얕은 복사이며, 필요 시 깊은 복사로 확장 가능합니다.
        /// </summary>
        /// <param name="original">원본 효과 리스트</param>
        /// <returns>복제된 효과 리스트</returns>
        private List<SkillCardEffectSO> CloneEffects(List<SkillCardEffectSO> original)
        {
            return new List<SkillCardEffectSO>(original);
        }

        /// <summary>
        /// 공용 카드 정의와 소유자 정보를 기반으로 카드를 생성합니다.
        /// ownerPolicy가 허용하지 않으면 null을 반환합니다.
        /// </summary>
        public ISkillCard CreateFromDefinition(SkillCardDefinition definition, Owner owner, string ownerCharacterName = null)
        {
            if (definition == null)
            {
                Debug.LogError("[SkillCardFactory] SkillCardDefinition이 null입니다.");
                return null;
            }

            // 정책 확인
            if (definition.ownerPolicy == OwnerPolicy.PlayerOnly && owner != Owner.Player) return null;
            if (definition.ownerPolicy == OwnerPolicy.EnemyOnly && owner != Owner.Enemy) return null;

            // 기존 런타임 데이터로 매핑(점진적 마이그레이션)
            var data = new SkillCardData(definition.displayNameKO, definition.descriptionKO, definition.icon, 0, 0)
            {
                CardId = definition.id,
                Cost = definition.actionCost,
                OwnerCharacterName = ownerCharacterName
            };

            // EffectRef에서 SO만 추출하고 order 기준으로 정렬
            var effects = new List<SkillCardEffectSO>();
            foreach (var e in definition.effects)
            {
                if (e != null && e.effect != null)
                    effects.Add(e.effect);
            }
            // order 오름차순 정렬 적용
            if (definition.effects != null && definition.effects.Count > 0)
            {
                effects.Sort((a, b) =>
                {
                    int GetOrder(SkillCardEffectSO so)
                    {
                        var refA = definition.effects.Find(r => r != null && r.effect == so);
                        return refA != null ? refA.order : 0;
                    }
                    return GetOrder(a).CompareTo(GetOrder(b));
                });
            }

            // owner 보정 적용(간단히 파워 스케일 가정)
            float multiplier = 1f;
            var mod = definition.ownerModifiers?.FirstOrDefault(m => m.owner == owner);
            if (mod != null) multiplier = Mathf.Max(0f, mod.magnitudeMultiplier);

            var powerMap = new Dictionary<SkillCardEffectSO, int>();
            foreach (var so in effects)
            {
                // 기본 파워는 SkillCardData.Damage를 사용(향후 SO별 기본 값으로 확장 가능)
                int basePower = data.Damage;
                // per-card override 존재 시 반영
                var r = definition.effects.FirstOrDefault(x => x != null && x.effect == so);
                if (r != null && r.magnitudeOverride > 0f)
                    basePower = Mathf.RoundToInt(r.magnitudeOverride);
                basePower = Mathf.RoundToInt(basePower * multiplier);
                powerMap[so] = basePower;
            }

            var slotOwner = owner == Owner.Player ? SlotOwner.PLAYER : SlotOwner.ENEMY;
            var runtime = new RuntimeSkillCard(data, effects, slotOwner, powerMap);
            return runtime;
        }
    }
}
