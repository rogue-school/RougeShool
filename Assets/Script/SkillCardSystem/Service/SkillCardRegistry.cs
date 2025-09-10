using System.Collections.Generic;
using UnityEngine;
using Game.SkillCardSystem.Data;

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
                if (def == null || string.IsNullOrEmpty(def.id)) continue;
                if (!idToDefinition.ContainsKey(def.id)) idToDefinition.Add(def.id, def);
                else Debug.LogWarning($"[SkillCardRegistry] 중복 id 감지: {def.id}");
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
            if (definition == null || string.IsNullOrEmpty(definition.id)) return;
            idToDefinition[definition.id] = definition;
        }

        public void Remove(string id)
        {
            if (string.IsNullOrEmpty(id)) return;
            idToDefinition.Remove(id);
        }
    }
}


