using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UIController : MonoBehaviour
{
    PlayerManager pm;

    // Start is called before the first frame update
    void Start()
    {
        pm = GameObject.Find("GameManager").GetComponentInChildren<PlayerManager>();
        HighlightWinner(pm.placements.Last());

        ShowAll();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ShowAll()
    {
        gameObject.SetActive(true);
        //transform.Find("Canvas").Find("PlayersWin").gameObject.SetActive(false);
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
    public void HighlightWinner(int idx)
    {
        bool winnerFound = false;

        Debug.Log("winner idx: " + idx);
        int count = 0;

        //transform.Find("Canvas").Find("PlayersWin").gameObject.SetActive(true);
        GameObject.Find("PlayersWin").SetActive(true);

        foreach (Transform child in transform.Find("Canvas").Find("PlayersWin"))
        {
            if(count == idx)
            {
                winnerFound = true;
                Debug.Log("showing winner");
                child.gameObject.SetActive(true);
            } else
            {
                child.gameObject.SetActive(false);
            }

            count++;
        }

        if (!winnerFound)
        {
            GameObject.Find("PlayersWin").SetActive(false);
        }
    }

}
