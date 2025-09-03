using UnityEngine;

public class play : MonoBehaviour
{
    public GameObject weaponSelectionImage;

    public void ShowWeaponSelection()
    {
        if (weaponSelectionImage != null)
        {
            weaponSelectionImage.SetActive(true);
        }
    }
}
