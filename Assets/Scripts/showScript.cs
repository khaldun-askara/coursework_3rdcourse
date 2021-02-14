using UnityEngine;

public class showScript : MonoBehaviour
{
    public GameObject currentobject;
    public void Show()
    {
        currentobject.SetActive(true);
    }
}
