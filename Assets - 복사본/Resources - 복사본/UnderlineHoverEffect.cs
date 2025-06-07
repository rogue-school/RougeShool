using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class UnderlineHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public RectTransform underline; // πÿ¡Ÿ ¿ÃπÃ¡ˆ
    public float animationDuration = 0.4f;

    private Coroutine animCoroutine;

    private void Start()
    {
        ResetUnderline();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (animCoroutine != null) StopCoroutine(animCoroutine);
        animCoroutine = StartCoroutine(AnimateUnderline(underline.localScale.x, 1f));
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (animCoroutine != null) StopCoroutine(animCoroutine);
        animCoroutine = StartCoroutine(AnimateUnderline(underline.localScale.x, 0f));
    }

    private IEnumerator AnimateUnderline(float from, float to)
    {
        float elapsed = 0f;
        underline.pivot = new Vector2(0f, 0.5f); // øﬁ¬  ±‚¡ÿ

        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / animationDuration;
            float scale = Mathf.SmoothStep(from, to, t);
            underline.localScale = new Vector3(scale, 1f, 1f);
            yield return null;
        }

        underline.localScale = new Vector3(to, 1f, 1f);
    }

    private void ResetUnderline()
    {
        underline.pivot = new Vector2(0f, 0.5f);
        underline.localScale = new Vector3(0f, 1f, 1f);
    }
}