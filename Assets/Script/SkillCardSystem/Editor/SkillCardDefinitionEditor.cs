using UnityEngine;
using UnityEditor;
using Game.SkillCardSystem.Data;
using Game.SkillCardSystem.Effect;

namespace Game.SkillCardSystem.Editor
{
    /// <summary>
    /// SkillCardDefinition의 커스텀 에디터입니다.
    /// 제작 편의성을 위한 통합 인터페이스를 제공합니다.
    /// </summary>
    [CustomEditor(typeof(SkillCardDefinition))]
    public class SkillCardDefinitionEditor : UnityEditor.Editor
    {
        private SkillCardDefinition card;
        private bool showPresentation = true;
        private bool showConfiguration = true;
        
        private void OnEnable()
        {
            card = (SkillCardDefinition)target;
        }
        
        public override void OnInspectorGUI()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("스킬카드 정의", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            // 필수 정보 섹션
            DrawRequiredInfo();
            
            EditorGUILayout.Space();
            
            // 연출 구성 섹션
            DrawPresentationSection();
            
            EditorGUILayout.Space();
            
            // 선택적 구성 섹션
            DrawConfigurationSection();
            
            EditorGUILayout.Space();
            
            // 검증 및 도구 섹션
            DrawValidationAndTools();
            
            if (GUI.changed)
            {
                EditorUtility.SetDirty(card);
            }
        }
        
        private void DrawRequiredInfo()
        {
            EditorGUILayout.LabelField("필수 정보", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            
            card.cardId = EditorGUILayout.TextField("Card ID", card.cardId);
            card.displayName = EditorGUILayout.TextField("Display Name", card.displayName);
            card.description = EditorGUILayout.TextField("Description", card.description);
            card.artwork = (Sprite)EditorGUILayout.ObjectField("Artwork", card.artwork, typeof(Sprite), false);
            
            EditorGUI.indentLevel--;
        }
        
        private void DrawPresentationSection()
        {
            showPresentation = EditorGUILayout.Foldout(showPresentation, "연출 구성", true);
            
            if (showPresentation)
            {
                EditorGUI.indentLevel++;
                DrawPresentationSettings(card.presentation);
                EditorGUI.indentLevel--;
            }
        }
        
        private void DrawPresentationSettings(CardPresentation presentation)
        {
            // 사운드 섹션
            EditorGUILayout.LabelField("사운드", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            presentation.sfxClip = (AudioClip)EditorGUILayout.ObjectField("SFX Clip", presentation.sfxClip, typeof(AudioClip), false);
            presentation.sfxVolume = EditorGUILayout.Slider("SFX Volume", presentation.sfxVolume, 0f, 1f);
            presentation.playOnStart = EditorGUILayout.Toggle("Play on Start", presentation.playOnStart);
            EditorGUI.indentLevel--;
            
            EditorGUILayout.Space();
            
            // 비주얼 이펙트 섹션
            EditorGUILayout.LabelField("비주얼 이펙트", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            presentation.visualEffectPrefab = (GameObject)EditorGUILayout.ObjectField("Effect Prefab", presentation.visualEffectPrefab, typeof(GameObject), false);
            presentation.effectDuration = EditorGUILayout.FloatField("Effect Duration", presentation.effectDuration);
            presentation.effectOffset = EditorGUILayout.Vector3Field("Effect Offset", presentation.effectOffset);
            presentation.followTarget = EditorGUILayout.Toggle("Follow Target", presentation.followTarget);
            EditorGUI.indentLevel--;
            
            EditorGUILayout.Space();
            
            // 애니메이션 섹션
            EditorGUILayout.LabelField("애니메이션", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            presentation.cardAnimation = (AnimationClip)EditorGUILayout.ObjectField("Card Animation", presentation.cardAnimation, typeof(AnimationClip), false);
            presentation.targetAnimation = (AnimationClip)EditorGUILayout.ObjectField("Target Animation", presentation.targetAnimation, typeof(AnimationClip), false);
            presentation.animationSpeed = EditorGUILayout.FloatField("Animation Speed", presentation.animationSpeed);
            EditorGUI.indentLevel--;
            
            EditorGUILayout.Space();
            
            // 연출 타이밍 섹션
            EditorGUILayout.LabelField("연출 타이밍", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            presentation.timing.sfxDelay = EditorGUILayout.FloatField("SFX Delay", presentation.timing.sfxDelay);
            presentation.timing.visualEffectDelay = EditorGUILayout.FloatField("Visual Effect Delay", presentation.timing.visualEffectDelay);
            presentation.timing.animationDelay = EditorGUILayout.FloatField("Animation Delay", presentation.timing.animationDelay);
            presentation.timing.waitForAnimation = EditorGUILayout.Toggle("Wait for Animation", presentation.timing.waitForAnimation);
            EditorGUI.indentLevel--;
        }
        
        private void DrawConfigurationSection()
        {
            showConfiguration = EditorGUILayout.Foldout(showConfiguration, "선택적 구성", true);
            
            if (showConfiguration)
            {
                EditorGUI.indentLevel++;
                DrawConfigurationSettings(card.configuration);
                EditorGUI.indentLevel--;
            }
        }
        
        private void DrawConfigurationSettings(CardConfiguration configuration)
        {
            // 데미지 구성 토글
            configuration.hasDamage = EditorGUILayout.Toggle("Has Damage", configuration.hasDamage);
            if (configuration.hasDamage)
            {
                EditorGUI.indentLevel++;
                DrawDamageConfiguration(configuration.damageConfig);
                EditorGUI.indentLevel--;
            }
            
            EditorGUILayout.Space();
            
            // 효과 구성 토글
            configuration.hasEffects = EditorGUILayout.Toggle("Has Effects", configuration.hasEffects);
            if (configuration.hasEffects)
            {
                EditorGUI.indentLevel++;
                DrawEffectsConfiguration(configuration.effects);
                EditorGUI.indentLevel--;
            }
            
            EditorGUILayout.Space();
            
            // 소유자 정책
            EditorGUILayout.LabelField("소유자 정책", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            configuration.ownerPolicy = (OwnerPolicy)EditorGUILayout.EnumPopup("Owner Policy", configuration.ownerPolicy);
            EditorGUI.indentLevel--;
        }
        
        private void DrawDamageConfiguration(DamageConfiguration config)
        {
            EditorGUILayout.LabelField("데미지 설정", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            config.baseDamage = EditorGUILayout.IntField("Base Damage", config.baseDamage);
            config.hits = EditorGUILayout.IntField("Hits", config.hits);
            config.pierceable = EditorGUILayout.Toggle("Pierceable", config.pierceable);
            config.critChance = EditorGUILayout.Slider("Crit Chance", config.critChance, 0f, 1f);
            EditorGUI.indentLevel--;
        }
        
        private void DrawEffectsConfiguration(System.Collections.Generic.List<EffectConfiguration> effects)
        {
            EditorGUILayout.LabelField("효과 설정", EditorStyles.boldLabel);
            
            for (int i = 0; i < effects.Count; i++)
            {
                var effect = effects[i];
                
                EditorGUILayout.BeginVertical("box");
                
                effect.effectSO = (SkillCardEffectSO)EditorGUILayout.ObjectField(
                    $"Effect {i + 1}", effect.effectSO, typeof(SkillCardEffectSO), false);
                
                if (effect.effectSO != null)
                {
                    effect.useCustomSettings = EditorGUILayout.Toggle("Use Custom Settings", effect.useCustomSettings);
                    if (effect.useCustomSettings)
                    {
                        DrawCustomSettings(effect.customSettings, effect.effectSO);
                    }
                    effect.executionOrder = EditorGUILayout.IntField("Execution Order", effect.executionOrder);
                }
                
                EditorGUILayout.EndVertical();
            }
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Add Effect"))
            {
                effects.Add(new EffectConfiguration());
            }
            if (GUILayout.Button("Remove Last Effect") && effects.Count > 0)
            {
                effects.RemoveAt(effects.Count - 1);
            }
            EditorGUILayout.EndHorizontal();
        }
        
        private void DrawCustomSettings(EffectCustomSettings settings, SkillCardEffectSO effectSO)
        {
            EditorGUI.indentLevel++;
            
            if (effectSO is DamageEffectSO)
            {
                EditorGUILayout.LabelField("데미지 효과 설정", EditorStyles.boldLabel);
                settings.damageAmount = EditorGUILayout.IntField("Damage Amount", settings.damageAmount);
                settings.damageHits = EditorGUILayout.IntField("Damage Hits", settings.damageHits);
                settings.pierceable = EditorGUILayout.Toggle("Pierceable", settings.pierceable);
                settings.critChance = EditorGUILayout.Slider("Crit Chance", settings.critChance, 0f, 1f);
            }
            else if (effectSO is BleedEffectSO)
            {
                EditorGUILayout.LabelField("출혈 효과 설정", EditorStyles.boldLabel);
                settings.bleedAmount = EditorGUILayout.IntField("Bleed Amount", settings.bleedAmount);
                settings.bleedDuration = EditorGUILayout.IntField("Bleed Duration", settings.bleedDuration);
            }
            else if (effectSO is GuardEffectSO)
            {
                EditorGUILayout.LabelField("가드 효과 설정", EditorStyles.boldLabel);
                settings.guardAmount = EditorGUILayout.IntField("Guard Amount", settings.guardAmount);
                settings.overflowToTempHP = EditorGUILayout.Toggle("Overflow to Temp HP", settings.overflowToTempHP);
            }
            // TODO: 향후 추가될 EffectSO 타입들을 위한 예약 공간
            // else if (effectSO is HealEffectSO)
            // {
            //     EditorGUILayout.LabelField("치유 효과 설정", EditorStyles.boldLabel);
            //     settings.healAmount = EditorGUILayout.IntField("Heal Amount", settings.healAmount);
            // }
            // else if (effectSO is DrawEffectSO)
            // {
            //     EditorGUILayout.LabelField("드로우 효과 설정", EditorStyles.boldLabel);
            //     settings.drawCount = EditorGUILayout.IntField("Draw Count", settings.drawCount);
            // }
            // else if (effectSO is ResourceEffectSO)
            // {
            //     EditorGUILayout.LabelField("리소스 효과 설정", EditorStyles.boldLabel);
            //     settings.resourceDelta = EditorGUILayout.IntField("Resource Delta", settings.resourceDelta);
            // }
            
            EditorGUI.indentLevel--;
        }
        
        private void DrawValidationAndTools()
        {
            EditorGUILayout.LabelField("검증 및 도구", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Validate Card"))
            {
                ValidateCard();
            }
            
            if (GUILayout.Button("Reset to Default"))
            {
                ResetToDefault();
            }
            
            EditorGUILayout.EndHorizontal();
            
            // 카드 정보 요약
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("카드 정보 요약", EditorStyles.boldLabel);
            
            var summary = $"ID: {card.cardId}\n" +
                         $"Name: {card.displayName}\n" +
                         $"Has Damage: {card.configuration.hasDamage}\n" +
                         $"Has Effects: {card.configuration.hasEffects}\n" +
                         $"Effect Count: {card.configuration.effects.Count}";
            
            EditorGUILayout.HelpBox(summary, MessageType.Info);
        }
        
        private void ValidateCard()
        {
            var factory = new Game.SkillCardSystem.Factory.SkillCardFactory();
            bool isValid = factory.ValidateDefinition(card);
            
            if (isValid)
            {
                EditorUtility.DisplayDialog("검증 결과", "카드 정의가 유효합니다.", "OK");
            }
            else
            {
                EditorUtility.DisplayDialog("검증 결과", "카드 정의에 오류가 있습니다. 콘솔을 확인하세요.", "OK");
            }
        }
        
        private void ResetToDefault()
        {
            if (EditorUtility.DisplayDialog("초기화 확인", "카드를 기본값으로 초기화하시겠습니까?", "Yes", "No"))
            {
                card.presentation = new CardPresentation();
                card.configuration = new CardConfiguration();
                EditorUtility.SetDirty(card);
            }
        }
    }
}
