using UnityEngine;

public class Xbotton : MonoBehaviour
{
    public GameObject targetToHide;

    public void HideTarget()
    {
        if (targetToHide != null)
        {
            targetToHide.SetActive(false);
        }
    }
}