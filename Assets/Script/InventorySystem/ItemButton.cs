using UnityEngine;
using UnityEngine.UI;

public class ItemButton : MonoBehaviour
{
    public string itemId;
    public Sprite itemIcon;

    [System.Obsolete]
    private void Start()
    {
        var inventoryManager = FindObjectOfType<InventoryManager>();
        GetComponent<Button>().onClick.AddListener(() =>
        {
            inventoryManager.OnItemClicked(itemId, itemIcon);
        });
    }
}