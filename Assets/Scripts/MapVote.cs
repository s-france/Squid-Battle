using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem.UI;
using System.Runtime.InteropServices;

public class MapVote : MonoBehaviour
{
    GameManager gm;
    PlayerManager pm;

    [SerializeField] InputSystemUIInputModule[] UIInputModules;


    public Image[] TokenSprites;
    public GameObject Map1Button;
    [SerializeField] TextMeshProUGUI nextRoundText;


    // Start is called before the first frame update
    void Start()
    {   
        InitMapVoteUI();
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    //initializes menu
    public void InitMapVoteUI()
    {
        gm = FindFirstObjectByType<GameManager>();
        pm = FindFirstObjectByType<PlayerManager>();

        foreach(PlayerConfig p in pm.playerList)
        {
            //activate player token UI
            TokenSprites[p.playerIndex].enabled = true;
            //TokenSprites[p.playerIndex+6].enabled = false;
            //initialize token position to Map1
            TokenSprites[p.playerIndex].transform.position = Map1Button.GetComponent<ButtonMultiSelections>().positions[p.playerIndex].position;
            TokenSprites[p.playerIndex+6].transform.position = Map1Button.GetComponent<ButtonMultiSelections>().positions[p.playerIndex+6].position;
        }

        UITokenTracker token;
        foreach(InputSystemUIInputModule im in UIInputModules)
        {
            token = im.GetComponent<UITokenTracker>();

            token.tokenPos = TokenSprites[token.idx].transform;
            token.confPos = TokenSprites[token.idx + 6].transform;
        }

    }

    public void ActivatePlayerSelection(int idx)
    {

    }

    //activates menu with input
    public void ActivateMapVoteMenu()
    {
        if(gm == null)
        {
            gm = FindFirstObjectByType<GameManager>();
        }

        //update next round text

        if(gm.lc.GetLevelType() >= 10) //arenas only
        {
            nextRoundText.text = "Round " + ++gm.ms.roundsPlayed + " Incoming!";
        }

        gameObject.SetActive(true);

        foreach(PlayerConfig p in pm.playerList)
        {
            //switch to menu input action map
            p.input.SwitchCurrentActionMap("Menu");
            
            if(!(p.playerIndex == 0 && gm.lc.GetLevelType() == 3))
            {
                 UIInputModules[p.playerIndex].GetComponent<MultiplayerEventSystem>().SetSelectedGameObject(Map1Button);
            }

            //initialize token position to Map1
            TokenSprites[p.playerIndex].transform.position = Map1Button.GetComponent<ButtonMultiSelections>().positions[p.playerIndex].position;
            TokenSprites[p.playerIndex+6].transform.position = Map1Button.GetComponent<ButtonMultiSelections>().positions[p.playerIndex+6].position;
        }

    }

    public void DeactivateMapVoteMenu()
    {
        gameObject.SetActive(false);
    }


    public void SetUIColors(int idx)
    {
        Debug.Log("SETUICOLORS idx = " + idx);

        if(pm == null)
        {
            pm = FindFirstObjectByType<PlayerManager>();
        }

        //set token color
        TokenSprites[idx].sprite = pm.playerList[idx].playerScript.spriteSet[0];

        //set confirmed token colors
        TokenSprites[idx+6].sprite = pm.playerList[idx].playerScript.spriteSet[2];
    }




}
