using UnityEngine;
using TMPro;

namespace Game.CombatSystem.UI
{
    /// <summary>
    /// 전투 중 데미지/회복 등 수치를 떠오르게 표시하는 UI
    /// </summary>
    public class DamageTextUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI damageText;
        [SerializeField] private float floatSpeed = 50f;
        [SerializeField] private float duration = 1.0f;

        private RectTransform rectTransform;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
        }

        /// <summary>
        /// 데미지 또는 회복 수치를 표시합니다.
        /// </summary>
        /// <param name="amount">표시할 수치</param>
        /// <param name="color">텍스트 색상</param>
        /// <param name="prefix">접두사 (+ 또는 -)</param>
        public void Show(int amount, Color color, string prefix = "")
        {
            // GameObject가 비활성화 상태면 Coroutine 시작 불가
            if (!gameObject.activeInHierarchy)
            {
                Destroy(gameObject);
                return;
            }

            if (damageText != null)
            {
                damageText.text = $"{prefix}{amount}";
                damageText.color = color;
            }

            rectTransform.anchoredPosition = new Vector2(30f, 40f);
            StopAllCoroutines();
            StartCoroutine(FloatAndFade(color));
        }

        /// <summary>
        /// 위로 떠오르면서 알파값이 점점 줄어듭니다.
        /// </summary>
        private System.Collections.IEnumerator FloatAndFade(Color startColor)
        {
            float elapsed = 0f;
            Vector2 startPos = rectTransform.anchoredPosition;

            while (elapsed < duration)
            {
                float delta = Time.deltaTime;
                elapsed += delta;

                // 상승
                rectTransform.anchoredPosition = startPos + Vector2.up * floatSpeed * (elapsed / duration);

                // 점점 투명하게
                if (damageText != null)
                {
                    Color color = startColor;
                    color.a = Mathf.Lerp(1f, 0f, elapsed / duration);
                    damageText.color = color;
                }

                yield return null;
            }

            Destroy(gameObject);
        }
    }
}
