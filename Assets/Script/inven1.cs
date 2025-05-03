using UnityEngine;

public class inven1 : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
            Debug.Log("인벤토리 축소");
        GameObject yellowObject = GameObject.Find("노란색");
        GameObject lowObject = GameObject.Find("내리기");
        if (yellowObject != null)
        {
            // 월드 좌표로 이동
            yellowObject.transform.position = new Vector3(17f, 0f, 0f);
        }
        else
        {
            Debug.LogWarning("이름이 '노란색'인 오브젝트를 찾을 수 없습니다.");

        }

        if (Input.GetKeyDown(KeyCode.Q))
            if (lowObject != null)
            {
                // 월드 좌표로 이동
                lowObject.transform.position = new Vector3(12f, 4f, 0f);
            }
            else
            {
                Debug.LogWarning("이름이 '노란색'인 오브젝트를 찾을 수 없습니다.");

            }
    }
}
