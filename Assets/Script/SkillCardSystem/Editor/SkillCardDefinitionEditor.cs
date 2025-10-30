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
            EditorGUI.indentLevel--;
            
            EditorGUILayout.Space();
            
            // 비주얼 이펙트 섹션
            EditorGUILayout.LabelField("비주얼 이펙트", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            presentation.visualEffectPrefab = (GameObject)EditorGUILayout.ObjectField("Effect Prefab", presentation.visualEffectPrefab, typeof(GameObject), false);
            EditorGUI.indentLevel--;
            
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("연출 타이밍은 각 시스템(AudioSystem, DOTween Pro)에서 관리됩니다.", MessageType.Info);
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

            // 리소스 구성 토글
            configuration.hasResource = EditorGUILayout.Toggle("Has Resource", configuration.hasResource);
            if (configuration.hasResource)
            {
                EditorGUI.indentLevel++;
                DrawResourceConfiguration(configuration.resourceConfig);
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
            config.baseDamage = EditorGUILayout.IntField("기본 데미지", config.baseDamage);
            config.hits = EditorGUILayout.IntField("공격 횟수", config.hits);
            config.ignoreGuard = EditorGUILayout.Toggle("방어 무효화", config.ignoreGuard);
            config.ignoreCounter = EditorGUILayout.Toggle("반격 무효화", config.ignoreCounter);
            EditorGUI.indentLevel--;
        }

        private void DrawResourceConfiguration(ResourceConfiguration config)
        {
            EditorGUILayout.LabelField("자원 설정", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            config.cost = EditorGUILayout.IntField("Cost", config.cost);
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
                settings.damageAmount = EditorGUILayout.IntField("데미지량", settings.damageAmount);
                settings.damageHits = EditorGUILayout.IntField("공격 횟수", settings.damageHits);
                settings.ignoreGuard = EditorGUILayout.Toggle("방어 무효화", settings.ignoreGuard);
                settings.ignoreCounter = EditorGUILayout.Toggle("반격 무효화", settings.ignoreCounter);
            }
            else if (effectSO is BleedEffectSO)
            {
                EditorGUILayout.LabelField("출혈 효과 설정", EditorStyles.boldLabel);
                settings.bleedAmount = EditorGUILayout.IntField("출혈량", settings.bleedAmount);
                settings.bleedDuration = EditorGUILayout.IntField("출혈 지속 시간", settings.bleedDuration);
            }
            else if (effectSO is CounterEffectSO)
            {
                EditorGUILayout.LabelField("반격 효과 설정", EditorStyles.boldLabel);
                settings.counterDuration = EditorGUILayout.IntField("반격 지속 턴 수", settings.counterDuration);
            }
            else if (effectSO is GuardEffectSO)
            {
                EditorGUILayout.LabelField("가드 효과 설정", EditorStyles.boldLabel);
                settings.guardDuration = EditorGUILayout.IntField("가드 지속 턴 수", settings.guardDuration);
            }
            else if (effectSO is StunEffectSO)
            {
                EditorGUILayout.LabelField("스턴 효과 설정", EditorStyles.boldLabel);
                settings.stunDuration = EditorGUILayout.IntField("스턴 지속 턴 수", settings.stunDuration);
            }
            else if (effectSO is CardUseStackEffectSO)
            {
                EditorGUILayout.LabelField("카드 사용 스택 효과 설정", EditorStyles.boldLabel);
                settings.stackIncreasePerUse = EditorGUILayout.IntField("카드 사용 시 증가할 스택 수", settings.stackIncreasePerUse);
                settings.maxStacks = EditorGUILayout.IntField("최대 스택 수 (0 = 무제한)", settings.maxStacks);
            }
            else if (effectSO is HealEffectSO)
            {
                EditorGUILayout.LabelField("치유 효과 설정", EditorStyles.boldLabel);
                settings.healAmount = EditorGUILayout.IntField("치유량", settings.healAmount);
            }
            else if (effectSO is ResourceGainEffectSO)
            {
                EditorGUILayout.LabelField("자원 획득 효과 설정", EditorStyles.boldLabel);
                settings.resourceDelta = EditorGUILayout.IntField("자원 획득량", Mathf.Max(0, settings.resourceDelta));
            }
            // TODO: 향후 추가될 EffectSO 타입들을 위한 예약 공간
            // else if (effectSO is DrawEffectSO)
            // {
            //     EditorGUILayout.LabelField("드로우 효과 설정", EditorStyles.boldLabel);
            //     settings.drawCount = EditorGUILayout.IntField("드로우 수", settings.drawCount);
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
                         $"Has Resource: {card.configuration.hasResource}\n" +
                         (card.configuration.hasResource ? $"Resource Cost: {card.configuration.resourceConfig.cost}\n" : string.Empty) +
                         $"Has Effects: {card.configuration.hasEffects}\n" +
                         $"Effect Count: {card.configuration.effects.Count}";
            
            EditorGUILayout.HelpBox(summary, MessageType.Info);
        }
        
        private void ValidateCard()
        {
            var audioManager = UnityEngine.Object.FindFirstObjectByType<Game.CoreSystem.Audio.AudioManager>();
            var factory = new Game.SkillCardSystem.Factory.SkillCardFactory(audioManager);
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
