using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class ClickEventTest : MonoBehaviour
{
    public GameObject inventoryPanel;
    bool activeInventory = false;
    private void Start()
    {
        inventoryPanel.SetActive(activeInventory);
    }

   
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            Debug.Log("클릭해서 인벤 활성화.");

        }
    }

    public void OnButtonClicked()
    {
        Debug.Log("인벤토리 활성화");
        activeInventory = !activeInventory;
        inventoryPanel.SetActive(activeInventory);
    }
}


