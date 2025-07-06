using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System.Collections;
using Game.CharacterSystem.Core;

[RequireComponent(typeof(AudioSource))]
public class PotionManager : MonoBehaviour, IPointerClickHandler
{
    [Header("포션 수량")]
    public int Posion = 3;
    private int prevPosion;
    private bool canUse = true;

    [Header("플레이어 연결")]
    public PlayerCharacter playerCharacter;

    [Header("UI 연결")]
    public TextMeshProUGUI infoText;
    public GameObject panelToActivate;
    public Button usePotionButton;
    public TextMeshProUGUI buttonText;
    public Image cooldownFillImage;
    public TextMeshProUGUI cooldownText;

    [Header("포션 이동/이름 표시")]
    public Transform inventoryParent;
    public Vector3 targetPosition;
    public Text nameText;
    public string customPotionName = "이름 없는 포션";
    public Vector3 nameTextPosition;

    [Header("사운드")]
    [SerializeField] private AudioClip potionUseSound;
    private AudioSource audioSource;

    private void Start()
    {
        StartCoroutine(WaitForPlayerCharacter());

        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;

        prevPosion = Posion;
        UpdatePotionUI();

        if (cooldownFillImage != null)
            cooldownFillImage.fillAmount = 1f;

        if (cooldownText != null)
            cooldownText.text = "";
    }

    private void Update()
    {
        // 디버깅용 포션 +1
        if (Input.GetKeyDown(KeyCode.W))
        {
            Posion += 1;
        }

        if (Posion != prevPosion)
        {
            UpdatePotionUI();
            prevPosion = Posion;
        }
    }

    private IEnumerator WaitForPlayerCharacter()
    {
        while (playerCharacter == null)
        {
            GameObject playerObj = GameObject.FindWithTag("Player");

            if (playerObj != null)
                playerCharacter = playerObj.GetComponent<PlayerCharacter>();

            yield return null;
        }

        Debug.Log("[PotionManager] PlayerCharacter 연결됨!");
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // 1. 포션 UI 위치 이동
        if (inventoryParent != null)
        {
            transform.SetParent(inventoryParent);
            transform.localPosition = targetPosition;
            transform.localScale = Vector3.one;
            Debug.Log("포션이 원하는 위치에 들어갔습니다.");
        }

        // 2. 패널 활성화
        if (panelToActivate != null)
            panelToActivate.SetActive(true);

        // 3. 이름 표시
        if (nameText != null)
        {
            nameText.text = customPotionName;
            nameText.rectTransform.localPosition = nameTextPosition;
        }
        else
        {
            Debug.LogWarning("nameText가 설정되지 않았습니다.");
        }

        // 4. 포션 사용 시도
        TryUsePotion();
    }

    private void TryUsePotion()
    {
        if (!canUse || Posion <= 0 || playerCharacter == null)
            return;

        if (playerCharacter.GetCurrentHP() < playerCharacter.GetMaxHP())
        {
            playerCharacter.Heal(3);
            Posion--;

            if (audioSource != null && potionUseSound != null)
                audioSource.PlayOneShot(potionUseSound);

            StartCoroutine(PotionCooldown());
        }
    }

    private IEnumerator PotionCooldown()
    {
        canUse = false;

        if (usePotionButton != null)
        {
            usePotionButton.interactable = false;
            usePotionButton.image.color = Color.gray;
        }

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

        UpdatePotionUI();
    }

    private void UpdatePotionUI()
    {
        // 텍스트 업데이트
        if (infoText != null)
        {
            if (Posion > 0)
                infoText.text = $"체력포션 X {Posion}";
            else
                infoText.text = "None Posion";
        }

        // 버튼 상태
        if (usePotionButton != null && buttonText != null)
        {
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
}