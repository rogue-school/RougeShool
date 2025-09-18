using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using Game.CharacterSystem.Interface;
using Game.SkillCardSystem.Interface;

namespace Game.CharacterSystem.UI
{
    /// <summary>
    /// 적 전용 캐릭터 UI 컨트롤러.
    /// 캐릭터 이미지 하단에 HP, 버프/디버프를 간결하게 표시한다.
    /// </summary>
    public class EnemyCharacterUIController : MonoBehaviour
    {
        [Header("바 표시")]
        [SerializeField] private Slider hpSlider;

        [Header("버프/디버프")]
        [SerializeField] private Transform buffContainer;
        [SerializeField] private GameObject buffIconPrefab; // 선택 사항

        private ICharacter target;

        private void OnDisable()
        {
            Unsubscribe();
        }

        public void SetTarget(ICharacter character)
        {
            Unsubscribe();
            target = character;
            Subscribe();
            if (target != null)
            {
                UpdateHP(target.GetCurrentHP(), target.GetMaxHP());
                UpdateBuffs(target.GetBuffs());
            }
        }

        private void Subscribe()
        {
            if (target == null) return;
            target.OnHPChanged += UpdateHP;
            target.OnBuffsChanged += UpdateBuffs;
        }

        private void Unsubscribe()
        {
            if (target == null) return;
            target.OnHPChanged -= UpdateHP;
            target.OnBuffsChanged -= UpdateBuffs;
        }

        private void UpdateHP(int current, int max)
        {
            if (hpSlider == null) return;
            hpSlider.maxValue = Mathf.Max(1, max);
            hpSlider.value = Mathf.Clamp(current, 0, hpSlider.maxValue);
        }

        private void UpdateBuffs(IReadOnlyList<IPerTurnEffect> effects)
        {
            if (buffContainer == null) return;

            for (int i = buffContainer.childCount - 1; i >= 0; i--)
            {
                var child = buffContainer.GetChild(i);
                if (Application.isPlaying) Destroy(child.gameObject); else DestroyImmediate(child.gameObject);
            }

            if (effects == null || effects.Count == 0) return;
            if (buffIconPrefab == null) return;

            foreach (var _ in effects)
            {
                Instantiate(buffIconPrefab, buffContainer);
            }
        }
    }
}


