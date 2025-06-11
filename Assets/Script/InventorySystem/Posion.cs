using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using Game.CharacterSystem.Core;

public class ButtonListener : MonoBehaviour
{
    public int Posion = 3;
    private int prevMulyak;
    private bool canUse = true;
    public PlayerCharacter playerCharacter;

    public TextMeshProUGUI infoText;
    public GameObject targetObjectToToggle;

    public Button usePotionButton;
    public TextMeshProUGUI buttonText;
    public Image cooldownFillImage;
    public TextMeshProUGUI cooldownText; // ← 추가: 남은 쿨타임 텍스트

    private void Start()
    {
        StartCoroutine(WaitForPlayerCharacter());
    }

    private IEnumerator WaitForPlayerCharacter()
    {
        while (playerCharacter == null)
        {
            GameObject playerObj = GameObject.FindWithTag("Player");

            if (playerObj != null)
            {
                playerCharacter = playerObj.GetComponent<PlayerCharacter>();
            }

            yield return null; // 다음 프레임까지 대기
        }

        Debug.Log("[ButtonListener] PlayerCharacter 연결됨!");

        // 초기화 계속 진행
        prevMulyak = Posion;
        UpdateMulyakText();
        UpdateButtonVisual();

        if (cooldownFillImage != null)
            cooldownFillImage.fillAmount = 1f;

        if (cooldownText != null)
            cooldownText.text = "";
    }



    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.W))
        {
            Posion += 1;
        }

        if (Posion != prevMulyak)
        {
            UpdateMulyakText();
            prevMulyak = Posion;
        }
    }

    public void OnButtonClicked()
    {
        if (!canUse || Posion <= 0)
            return;

        if (playerCharacter != null && playerCharacter.GetCurrentHP() < playerCharacter.GetMaxHP())
        {
            playerCharacter.Heal(1);
            Posion -= 1;
            StartCoroutine(PotionCooldown());
        }
    }

    private IEnumerator PotionCooldown()
    {
        canUse = false;
        usePotionButton.interactable = false;
        usePotionButton.image.color = Color.gray;

        float cooldown = 1f;
        float timer = 0f;

        if (cooldownFillImage != null)
            cooldownFillImage.fillAmount = 0f;

        if (cooldownText != null)
            cooldownText.text = $"{cooldown:0.0}s";

        while (timer < cooldown)
        {
            timer += Time.deltaTime;

            if (cooldownFillImage != null)
                cooldownFillImage.fillAmount = timer / cooldown;

            if (cooldownText != null)
                cooldownText.text = $"{(cooldown - timer):0.0}s";

            yield return null;
        }

        canUse = true;

        if (cooldownText != null)
            cooldownText.text = "";

        UpdateButtonVisual();
    }

    private void UpdateMulyakText()
    {
        if (Posion > 0)
        {
            infoText.text = $"체력포션 X {Posion}";
            if (targetObjectToToggle != null)
                targetObjectToToggle.SetActive(true);
        }
        else
        {
            infoText.text = "None Posion";
            if (targetObjectToToggle != null)
                targetObjectToToggle.SetActive(false);
        }


        UpdateButtonVisual();
    }

    private void UpdateButtonVisual()
    {
        if (usePotionButton == null || buttonText == null) return;

        if (!canUse || Posion <= 0)
        {
            usePotionButton.interactable = false;
            usePotionButton.image.color = Color.gray;

            if (canUse)
                buttonText.text = "None Posion";
        }
        else
        {
            usePotionButton.interactable = true;
            usePotionButton.image.color = Color.white;
            buttonText.text = $"체력포션 X {Posion}";

            if (cooldownFillImage != null)
                cooldownFillImage.fillAmount = 1f;
        }
    }
}