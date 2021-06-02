using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
[Serializable]
public class VisibleSignList
{
    public List<VisibleSign> visibleSigns;
    public VisibleSignList() { visibleSigns = new List<VisibleSign>(); }
    public VisibleSignList(List<VisibleSign> visibleSigns)
    {
        this.visibleSigns = visibleSigns;
    }
    public void Add(VisibleSign visibleSign)
    {
        visibleSigns.Add(visibleSign);
    }
}
[Serializable]
public class VisibleSign
{
    public int id;
    public string name;
    public string readable_id;
    public VisibleSign() { }

    public VisibleSign(int id, string name, string readable_id)
    {
        this.id = id;
        this.name = name;
        this.readable_id = readable_id;
    }
}
public class VisibleSignsRequest : MonoBehaviour
{
    [SerializeField] private string url;
    public GameObject SomethigWrongPanel;
    private bool isServerRespond = false;
    public Text ErrorText;
    public SpriteRenderer scleraColor;
    public SpriteRenderer skinColor;
    public SpriteRenderer earsColor;
    public SpriteRenderer noseColor;

    public SpriteRenderer skinRash;
    public SpriteRenderer skinBlush;

    public SpriteRenderer scarfLine;
    public SpriteRenderer scarfColor;
    public static VisibleSignList visibleSignList;

    void Start()
    {
        StartCoroutine(routine: SendRequest());
    }
    private IEnumerator SendRequest()
    {
        UnityWebRequest request = UnityWebRequest.Get(StartGame.SERVERURL + url + StartGame.RightDiagnosis);
        yield return request.SendWebRequest();
        isServerRespond = !(request.isHttpError || request.isNetworkError);
        //Debug.Log(isServerRespond);
        if (!isServerRespond)
        {
            //Debug.Log(request.error);
            ErrorText.text = request.error;
            SomethigWrongPanel.SetActive(true);
        }
        else
        {
            visibleSignList = JsonUtility.FromJson<VisibleSignList>(request.downloadHandler.text);
            if (visibleSignList.visibleSigns.Count > 0)
                foreach (VisibleSign sign in visibleSignList.visibleSigns)
                    DisplayVisibleSign(sign.readable_id);
        }
    }

    void DisplayVisibleSign (string signReadableId)
    {
        if (signReadableId == "neck_scarf")
        { 
            neck_scarf();
            return; 
        }

    }

    void neck_scarf()
    {
        scarfColor.gameObject.SetActive(true);
        scarfLine.gameObject.SetActive(true);
        skinBlush.gameObject.SetActive(true);
        skinBlush.color = new Color(1f, 0.6958495f, 0.6839622f);
        scarfColor.color = new Color(0.635f, 0.78f, 0.8f);
        noseColor.color = new Color(1f, 0.6958495f, 0.6839622f);
    }
}
