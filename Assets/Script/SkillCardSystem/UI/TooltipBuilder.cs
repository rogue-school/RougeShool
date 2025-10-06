using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace Game.SkillCardSystem.UI
{
    /// <summary>
    /// TooltipModel을 받아 기존 SkillCardTooltip 프리팹에 내용을 채웁니다.
    /// (현 단계: 제목/타입/아이콘/설명만 적용)
    /// </summary>
    public static class TooltipBuilder
    {
        public static void BuildMain(TooltipModel model, SkillCardTooltip view)
        {
            if (model == null || view == null) return;

            // 아이콘
            var iconImg = GetPrivateImage(view, "cardIconImage");
            if (iconImg != null)
            {
                iconImg.sprite = model.Icon;
                iconImg.color = Color.white;
            }

            // 제목
            var title = GetTMP(view, "cardNameText");
            if (title != null) title.text = model.Title ?? string.Empty;

            // 타입
            var type = GetTMP(view, "cardTypeText");
            if (type != null) type.text = model.CardType ?? string.Empty;

            // 설명
            var desc = GetTMP(view, "descriptionText");
            if (desc != null) desc.text = model.DescriptionRichText ?? string.Empty;

            // 효과 요약(간단) - effectsContainer가 연결되어 있으면 항목 텍스트로 렌더
            var effectsContainer = GetPrivateTransform(view, "effectsContainer");
            var effectItemPrefab = GetPrivateGO(view, "effectItemPrefab");
            if (effectsContainer != null && effectItemPrefab != null)
            {
                foreach (Transform c in effectsContainer)
                {
                    Object.Destroy(c.gameObject);
                }
                foreach (var e in model.Effects)
                {
                    var item = Object.Instantiate(effectItemPrefab, effectsContainer);
                    // 고급 효과 아이템(호버 지원)
                    var comp = item.GetComponent<SkillCardTooltip.EffectItemComponent>();
                    if (comp != null)
                    {
                        var data = new SkillCardTooltip.EffectData
                        {
                            name = e.Name,
                            description = e.Description,
                            iconColor = e.Color,
                            effectType = SkillCardTooltip.EffectType.Special
                        };
                        comp.SetupEffect(data, view);
                    }
                    else
                    {
                        // 폴백: 단일 TMP로 텍스트만 표시
                        var tmp = item.GetComponentInChildren<TextMeshProUGUI>();
                        if (tmp != null) tmp.text = $"<b>{e.Name}</b>  {e.Description}";
                    }
                }
            }
        }

        private static TextMeshProUGUI GetTMP(SkillCardTooltip view, string field)
        {
            var fi = typeof(SkillCardTooltip).GetField(field, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            return fi?.GetValue(view) as TextMeshProUGUI;
        }

        private static Image GetPrivateImage(SkillCardTooltip view, string field)
        {
            var fi = typeof(SkillCardTooltip).GetField(field, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            return fi?.GetValue(view) as Image;
        }

        private static Transform GetPrivateTransform(SkillCardTooltip view, string field)
        {
            var fi = typeof(SkillCardTooltip).GetField(field, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            return fi?.GetValue(view) as Transform;
        }

        private static GameObject GetPrivateGO(SkillCardTooltip view, string field)
        {
            var fi = typeof(SkillCardTooltip).GetField(field, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            return fi?.GetValue(view) as GameObject;
        }
        public static void BuildSub(SubTooltipModel model, SkillCardTooltip.SubTooltipComponent view)
        {
            if (model == null || view == null) return;

            var nameTMP = GetPrivateTMP(view, "effectNameText");
            if (nameTMP != null) nameTMP.text = model.Name ?? string.Empty;

            var descTMP = GetPrivateTMP(view, "effectDescriptionText");
            if (descTMP != null)
            {
                var txt = model.DescriptionRichText ?? string.Empty;
                if (model.ExtraPairs != null && model.ExtraPairs.Count > 0)
                {
                    for (int i = 0; i < model.ExtraPairs.Count; i++)
                    {
                        var (k, v) = model.ExtraPairs[i];
                        txt += $"\n<b><color=#A7D2FF>{k}</color></b> : {v}";
                    }
                }
                descTMP.text = txt;
            }
        }

        private static TextMeshProUGUI GetPrivateTMP(object obj, string field)
        {
            var fi = obj.GetType().GetField(field, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            return fi?.GetValue(obj) as TextMeshProUGUI;
        }
    }
}


