using System.Collections.Generic;
using UnityEngine;
using Game.SkillCardSystem.Data;
using Game.CoreSystem.Utility;

namespace Game.SkillCardSystem.Service
{
    /// <summary>
    /// 스킬 카드 정의(SkillCardDefinition)의 런타임 레지스트리입니다.
    /// Addressables/Resources/수동 주입 등 다양한 공급원을 통합해 id→SO 매핑을 제공합니다.
    /// </summary>
    public class SkillCardRegistry : MonoBehaviour
    {
        [Header("직접 참조할 카드 정의(선택)")]
        [SerializeField] private List<SkillCardDefinition> definitions = new();

        private readonly Dictionary<string, SkillCardDefinition> idToDefinition = new();

        private void Awake()
        {
            BuildIndex();
        }

        public void BuildIndex()
        {
            idToDefinition.Clear();
            foreach (var def in definitions)
            {
                if (def == null || string.IsNullOrEmpty(def.cardId)) continue;
                if (!idToDefinition.ContainsKey(def.cardId)) idToDefinition.Add(def.cardId, def);
                else Debug.LogWarning($"[SkillCardRegistry] 중복 id 감지: {def.cardId}");
            }
        }

        public bool TryGet(string id, out SkillCardDefinition definition)
        {
            if (string.IsNullOrEmpty(id))
            {
                definition = null;
                return false;
            }
            return idToDefinition.TryGetValue(id, out definition);
        }

        public void Add(SkillCardDefinition definition)
        {
            if (definition == null || string.IsNullOrEmpty(definition.cardId)) return;
            idToDefinition[definition.cardId] = definition;
        }

        /// <summary>
        /// 모든 스킬카드의 스택을 초기화합니다.
        /// </summary>
        public void ResetAllSkillCardStacks()
        {
            GameLogger.LogInfo("[SkillCardRegistry] 모든 스킬카드 스택 초기화 시작", GameLogger.LogCategory.SkillCard);
            
            int resetCount = 0;
            foreach (var definition in idToDefinition.Values)
            {
                if (definition != null)
                {
                    definition.ResetAttackPowerStacks();
                    resetCount++;
                }
            }
            
            GameLogger.LogInfo($"[SkillCardRegistry] {resetCount}개의 스킬카드 스택 초기화 완료", GameLogger.LogCategory.SkillCard);
        }

        public void Remove(string id)
        {
            if (string.IsNullOrEmpty(id)) return;
            idToDefinition.Remove(id);
        }
    }
}


