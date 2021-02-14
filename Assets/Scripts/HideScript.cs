using UnityEngine;

public class HideScript : MonoBehaviour
{
    public GameObject currentobject;
    public void Hide()
    {
        currentobject.SetActive(false);
    }
}
