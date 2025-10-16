using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Game.ItemSystem.Data;
using Game.CoreSystem.Utility;

namespace Game.ItemSystem.Utility
{
    /// <summary>
    /// UI 업데이트 헬퍼 클래스
    /// 중복된 UI 업데이트 로직을 통합하여 코드 중복을 제거합니다.
    /// </summary>
    public static class UIUpdateHelper
    {
        /// <summary>
        /// 아이템 슬롯 UI를 업데이트합니다.
        /// </summary>
        /// <param name="itemIcon">아이템 아이콘 이미지</param>
        /// <param name="itemNameText">아이템 이름 텍스트</param>
        /// <param name="itemDescriptionText">아이템 설명 텍스트</param>
        /// <param name="item">아이템 정의</param>
        /// <param name="iconColor">아이콘 색상 (기본값: 흰색)</param>
        public static void UpdateItemSlotUI(Image itemIcon, TextMeshProUGUI itemNameText, 
            TextMeshProUGUI itemDescriptionText, ActiveItemDefinition item, Color? iconColor = null)
        {
            if (itemIcon != null)
            {
                itemIcon.sprite = item?.Icon;
                itemIcon.color = iconColor ?? Color.white;
            }
            
            if (itemNameText != null)
                itemNameText.text = item?.DisplayName ?? "";
                
            if (itemDescriptionText != null)
                itemDescriptionText.text = item?.Description ?? "";
        }

        /// <summary>
        /// 슬롯을 빈 상태로 설정합니다.
        /// </summary>
        /// <param name="itemIcon">아이템 아이콘 이미지</param>
        /// <param name="itemNameText">아이템 이름 텍스트</param>
        /// <param name="itemDescriptionText">아이템 설명 텍스트</param>
        /// <param name="emptyColor">빈 슬롯 색상</param>
        public static void SetEmptySlot(Image itemIcon, TextMeshProUGUI itemNameText, 
            TextMeshProUGUI itemDescriptionText, Color emptyColor)
        {
            if (itemIcon != null)
            {
                itemIcon.sprite = null;
                itemIcon.color = emptyColor;
            }
            
            if (itemNameText != null)
                itemNameText.text = "";
                
            if (itemDescriptionText != null)
                itemDescriptionText.text = "";
        }

        /// <summary>
        /// 슬롯을 빈 상태로 설정합니다 (기본 회색).
        /// </summary>
        /// <param name="itemIcon">아이템 아이콘 이미지</param>
        /// <param name="itemNameText">아이템 이름 텍스트</param>
        /// <param name="itemDescriptionText">아이템 설명 텍스트</param>
        public static void SetEmptySlot(Image itemIcon, TextMeshProUGUI itemNameText, 
            TextMeshProUGUI itemDescriptionText)
        {
            SetEmptySlot(itemIcon, itemNameText, itemDescriptionText, Color.gray);
        }

        /// <summary>
        /// 버튼의 상호작용 가능 여부를 설정합니다.
        /// </summary>
        /// <param name="button">설정할 버튼</param>
        /// <param name="interactable">상호작용 가능 여부</param>
        /// <param name="disabledColor">비활성화 색상 (기본값: 회색)</param>
        public static void SetButtonInteractable(Button button, bool interactable, Color? disabledColor = null)
        {
            if (button != null)
            {
                button.interactable = interactable;
                
                // 버튼이 비활성화되면 색상 변경
                if (!interactable && disabledColor.HasValue)
                {
                    var colors = button.colors;
                    colors.normalColor = disabledColor.Value;
                    button.colors = colors;
                }
            }
        }

        /// <summary>
        /// 아이템 정보를 문자열로 생성합니다.
        /// </summary>
        /// <param name="item">아이템 정의</param>
        /// <param name="includeEffects">효과 정보 포함 여부</param>
        /// <returns>아이템 정보 문자열</returns>
        public static string GenerateItemInfo(ActiveItemDefinition item, bool includeEffects = true)
        {
            if (item == null)
                return "[빈 아이템]";

            var info = new System.Text.StringBuilder();
            info.AppendLine($"=== {item.DisplayName} ===");
            info.AppendLine($"ID: {item.ItemId}");
            info.AppendLine($"설명: {item.Description}");
            info.AppendLine($"타입: {item.Type}");
            info.AppendLine($"아이콘: {(item.Icon != null ? item.Icon.name : "없음")}");
            
            if (includeEffects)
            {
                info.AppendLine();
                info.AppendLine("효과:");
                
                foreach (var effectConfig in item.EffectConfiguration.effects)
                {
                    if (effectConfig.effectSO != null)
                    {
                        info.AppendLine($"- {effectConfig.effectSO.name}");
                        
                        if (effectConfig.useCustomSettings && effectConfig.customSettings != null)
                        {
                            AppendCustomSettingsInfo(info, effectConfig.customSettings);
                        }
                    }
                }
            }
            
            return info.ToString();
        }

        /// <summary>
        /// 커스텀 설정 정보를 추가합니다.
        /// </summary>
        /// <param name="info">정보 문자열 빌더</param>
        /// <param name="settings">커스텀 설정</param>
        private static void AppendCustomSettingsInfo(System.Text.StringBuilder info, Data.ItemEffectCustomSettings settings)
        {
            switch (settings)
            {
                case Data.HealEffectCustomSettings healSettings:
                    info.AppendLine($"  회복량: {healSettings.healAmount}");
                    break;
                case Data.AttackBuffEffectCustomSettings buffSettings:
                    info.AppendLine($"  버프량: {buffSettings.buffAmount}, 지속시간: {buffSettings.duration}");
                    break;
                case Data.TimeStopEffectCustomSettings timeSettings:
                    info.AppendLine($"  봉인 수: {timeSettings.sealCount}, 지속시간: {timeSettings.duration}");
                    break;
                case Data.DiceOfFateEffectCustomSettings diceSettings:
                    info.AppendLine($"  변경 수: {diceSettings.changeCount}, 지속시간: {diceSettings.duration}");
                    break;
                case Data.ClownPotionEffectCustomSettings clownSettings:
                    info.AppendLine($"  회복확률: {clownSettings.healChance}%, 회복량: {clownSettings.healAmount}, 데미지: {clownSettings.damageAmount}");
                    break;
                case Data.RerollEffectCustomSettings rerollSettings:
                    info.AppendLine($"  리롤 수: {rerollSettings.rerollCount}");
                    break;
                case Data.ShieldBreakerEffectCustomSettings shieldSettings:
                    info.AppendLine($"  지속시간: {shieldSettings.duration}");
                    break;
            }
        }
    }
}
