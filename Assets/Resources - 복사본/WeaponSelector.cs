using UnityEngine;

public class WeaponSelector : MonoBehaviour
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
