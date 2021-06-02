using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenWebForm : MonoBehaviour
{
    public string path;
    // Start is called before the first frame update
    public void Open()
    {
        Application.OpenURL(StartGame.SERVERURL + path);
    }
}
