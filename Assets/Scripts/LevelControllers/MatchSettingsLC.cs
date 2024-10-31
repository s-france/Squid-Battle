using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.InputSystem.UI;
using Unity.VisualScripting.Antlr3.Runtime;
using System;
using TMPro;

public class MatchSettingsLC : LevelController
{
    //game mode
    //items array
    //number of rounds
    //number of players
    //

    [SerializeField] Slider slider1;
    [SerializeField] SliderButton SliderButton1;

    [SerializeField] InputSystemUIInputModule[] UIInputModules;

    [SerializeField] Image[] TokenSprites;

    [SerializeField] GameObject Map1Button;
    [SerializeField] GameObject ShotToggle;

    [SerializeField] Canvas leftCanvas;
    [SerializeField] Canvas rightCanvas;
    [SerializeField] Transform ready2StartUI;


    //stuff to show/hide when switching from the settings menu
    [SerializeField] Transform[] LeftUIElements;

    int p1MenuState; //0 = settings, 1 = mapvote




    // Start is called before the first frame update
    void Awake()
    {
        gm = GameObject.Find("GameManager").GetComponent<GameManager>();
        gm.lc = this;
        pm = FindFirstObjectByType<PlayerManager>();
        //pm = GetGameManager().GetComponentInChildren<PlayerManager>();
        //pm = GameObject.Find("PlayerManager").GetComponent<PlayerManager>();

        SpawnPoints = new List<Transform>();

        SpawnPoints.Add(transform.GetChild(0));
        SpawnPoints.Add(transform.GetChild(1));
        SpawnPoints.Add(transform.GetChild(2));
        SpawnPoints.Add(transform.GetChild(3));
        SpawnPoints.Add(transform.GetChild(4));
        SpawnPoints.Add(transform.GetChild(5));



        StartLevel();
        
    }


    // Update is called once per frame
    void Update()
    {
        
    }


    public override void StartLevel()
    {
        //play menu music
        AudioManager am = FindFirstObjectByType<AudioManager>();
        am.Init();
        if(!am.SoundsPlaying.Contains("Menu"))
        {
            am.Play("Menu");
        }

        //basic menuLC stuff
        if(pm == null)
        {
            pm = FindFirstObjectByType<PlayerManager>();
        }

        //disable players joining
        pm.im.pim.DisableJoining();

        foreach (PlayerConfig p in pm.playerList)
        {
            pm.DeactivatePlayer(p.playerIndex);

            pm.UnReadyPlayer(p.playerIndex);

            Transform spawn = this.gameObject.transform.GetChild(p.playerIndex);
            p.input.gameObject.transform.position = spawn.position;


            //assign players to UI input modules
            UIInputModules[p.playerIndex].gameObject.SetActive(true);
            UIInputModules[p.playerIndex].actionsAsset = pm.playerList[pm.playerList.FindIndex(player => player.playerIndex == p.playerIndex)].input.actions;
            pm.playerList[pm.playerList.FindIndex(player => player.playerIndex == p.playerIndex)].input.uiInputModule = UIInputModules[p.playerIndex];

            //activate player token UI
            TokenSprites[p.playerIndex].enabled = true;
            //TokenSprites[p.playerIndex+6].enabled = false;
            //initialize token position to Map1
            TokenSprites[p.playerIndex].transform.position = Map1Button.GetComponent<ButtonMultiSelections>().positions[p.playerIndex].position;
            TokenSprites[p.playerIndex+6].transform.position = Map1Button.GetComponent<ButtonMultiSelections>().positions[p.playerIndex+6].position;

            //set player UI colors
            SetUIColors(p.playerIndex);
        }

        //initialize p1 menu state tracking
        p1MenuState = 0;

        //deactivate P1 sprite until done with settings - OnDone()
        TokenSprites[0].enabled = false;
        

    }


    public override void EndLevel()
    {
        //set player scores to 0    
        foreach(PlayerConfig p in pm.playerList)
        {
            p.score = 0;
        }

        //clear map vote
        int idx = 0;
        foreach(int m in gm.ms.MapPool)
        {
            gm.ms.MapPool[idx] = 0;
            idx++;
        }

        //run any kind of outro/transition stuff here
        Debug.Log("ending match settings menu");
    }


    public override void ResetLevel()
    {


    }


    public override int GetLevelType()
    {
        //MatchSettings type = 3
        return 3;
    }


    public override void OnConfirm(int playerID, InputAction.CallbackContext ctx)
    {
        base.OnConfirm(playerID, ctx);

        if(ctx.performed)
        {
            //start game if all players ready
            if(pm.playerList.TrueForAll(p => (p.isReady || !p.isActive)) && pm.playerList.Count(p => p.isActive) > 1)
            {
                FindFirstObjectByType<AudioManager>().Play("UINav1");

                //assign value randomly selected from gm.ms.MapPool
                int map = gm.ms.PickMap();

                gm.StartMatch(map);
            }



            if(!pm.playerList[playerID].isReady)
            {
                //FindFirstObjectByType<AudioManager>().Play("UINav1");

                if(playerID != 0)
                {
                    pm.ReadyPlayer(playerID); //calls lc.ReadyPlayer
                } else if(p1MenuState == 1)
                {
                    pm.ReadyPlayer(playerID); //calls lc.ReadyPlayer
                }
            }
        }

        
        

    }


    public override void OnBack(int playerID, InputAction.CallbackContext ctx)
    {
        //base.OnBack(ctx);
        //if slider selected -> exit slider
        //else -> go to previous menu

        if(ctx.started)
        {
            if(SliderButton1.sliderSelected)
            {
                FindFirstObjectByType<AudioManager>().Play("UINav3");

                SliderButton1.sliderSelected = false;
                slider1.interactable = false;
                SliderButton1.Select();
            } else if (pm.playerList[playerID].isReady)
            {
                FindFirstObjectByType<AudioManager>().Play("UINav2");
                pm.UnReadyPlayer(playerID);
            } else if (playerID == 0 && p1MenuState == 1)
            {
                FindFirstObjectByType<AudioManager>().Play("UINav2");

                ActivateSettingsMenu();

                //transition from mapselect panel to matchsettings panel
                //update p1 menu state
                p1MenuState = 0;

                //move P1 selection to match settings
                MultiplayerEventSystem evSys = UIInputModules[0].transform.GetComponent<MultiplayerEventSystem>();
                evSys.playerRoot = leftCanvas.gameObject;
                evSys.SetSelectedGameObject(ShotToggle);

                //hide P1 map select token
                TokenSprites[0].enabled = false;

            }
        }
        
        if (ctx.performed)
        {
            FindFirstObjectByType<AudioManager>().Play("UINav2");
            gm.sl.LoadScene("CharacterSelect");
        }
    }

    public override void ReadyPlayer(int idx)
    {
        base.ReadyPlayer(idx);

        //allow starting game if all active players are ready && more than 1 player
        if(pm.playerList.TrueForAll(p => (p.isReady || !p.isActive)) && pm.playerList.Count(p => p.isActive) > 1)
        {
            //show ready to start UI
            ready2StartUI.gameObject.SetActive(true);

        }else
        {
            ready2StartUI.gameObject.SetActive(false);
        }


    }

    public override void UnReadyPlayer(int idx)
    {
        base.UnReadyPlayer(idx);

        //reactivate player's mapselect token
        UITokenTracker token = UIInputModules[idx].GetComponent<UITokenTracker>();

        //cancel map vote
        gm.ms.UnVoteMap(token.selectionID);
        
        //deactivate confirmed icon
        token.confPos.GetComponent<Image>().enabled = false;
        //selections.positions[6 + token.idx].gameObject.SetActive(true);

        //reactivate selecting icon
        token.tokenPos.GetComponent<Image>().enabled = true;

        //reenable player selection
        UIInputModules[idx].gameObject.SetActive(true);


        //allow starting game if all active players are ready && more than 1 player
        if(pm.playerList.TrueForAll(p => (p.isReady || !p.isActive)) && pm.playerList.Count(p => p.isActive) > 1)
        {
            //show ready to start UI
            ready2StartUI.gameObject.SetActive(true);

        }else
        {
            ready2StartUI.gameObject.SetActive(false);
        }
    }

    public override void SetUIColors(int idx)
    {
        //base.SetUIColors(idx);

        //set token color
        TokenSprites[idx].sprite = pm.playerList[idx].playerScript.spriteSet[0];

        //set confirmed token colors
        TokenSprites[idx+6].sprite = pm.playerList[idx].playerScript.spriteSet[2];



    }

    public void ToggleItem(int itemID)
    {
        gm.ms.ToggleItem(itemID);
    }

    public void SetPointsToWin()
    {
        gm.ms.pointsToWin = (int)slider1.value;
    }


    public void OnDone()
    {
        //update p1 menu state
        p1MenuState = 1;

        //move P1 selection to map vote
        MultiplayerEventSystem evSys = UIInputModules[0].transform.GetComponent<MultiplayerEventSystem>();
        evSys.playerRoot = rightCanvas.gameObject;
        evSys.SetSelectedGameObject(Map1Button);

        //show P1 map select token
        TokenSprites[0].enabled = true;


        //ADD THIS!!!!:
        //disable settings menu
        DeactivateSettingsMenu();
    }

    //shows settings menu visuals
    void ActivateSettingsMenu()
    {
        //show player1 text
        LeftUIElements[0].gameObject.SetActive(true);

        //replace game rules text
        LeftUIElements[1].GetComponent<TextMeshProUGUI>().text = "Set Game Rules";

        //show done button
        LeftUIElements[2].gameObject.SetActive(true);

        //hide shade cover
        LeftUIElements[3].gameObject.SetActive(false);
    }

    //hides settings menu panel
    void DeactivateSettingsMenu()
    {
        //hide player1 text
        LeftUIElements[0].gameObject.SetActive(false);

        //replace game rules text
        LeftUIElements[1].GetComponent<TextMeshProUGUI>().text = "Game Rules:";

        //hide done button
        LeftUIElements[2].gameObject.SetActive(false);

        //shade out
        LeftUIElements[3].gameObject.SetActive(true);
    }
    




}
