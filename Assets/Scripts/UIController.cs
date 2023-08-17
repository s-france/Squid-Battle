using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIController : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        ShowAll();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ShowAll()
    {
        gameObject.SetActive(true);
    }
    public void HideAll()
    {
        gameObject.SetActive(false);
    }

    public void ShowPlayerUI(int idx)
    {
        transform.Find("Canvas").Find("PlayersReady").GetChild(idx).gameObject.SetActive(true);
    }

    public void HidePlayerUI(int idx)
    {
        UnReadyPlayer(idx);
        transform.Find("Canvas").Find("PlayersReady").GetChild(idx).gameObject.SetActive(false);
    }

    public void HideAllPlayers()
    {
        for (int i=0; i<4; i++)
        {
            HidePlayerUI(i);
        }
    }

    public void ReadyPlayer(int idx)
    {
        transform.Find("Canvas").Find("PlayersReady").GetChild(idx).Find("Waiting").gameObject.SetActive(false);
        transform.Find("Canvas").Find("PlayersReady").GetChild(idx).Find("Ready").gameObject.SetActive(true);
    }

    public void UnReadyPlayer(int idx)
    {
        transform.Find("Canvas").Find("PlayersReady").GetChild(idx).Find("Waiting").gameObject.SetActive(true);
        transform.Find("Canvas").Find("PlayersReady").GetChild(idx).Find("Ready").gameObject.SetActive(false);  
    }

}
