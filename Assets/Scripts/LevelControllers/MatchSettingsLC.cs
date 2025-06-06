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
using UnityEngine.Rendering;

public class MatchSettingsLC : LevelController
{
    //game mode
    //items array
    //number of rounds
    //number of players
    //

    [SerializeField] MapVote mv;

    [SerializeField] Slider pointSlider;
    [SerializeField] Slider formatSlider;
    int[] pointIntervals = {10, 15, 20, 25, 30};
    [SerializeField] Text pointsNumber;
    [SerializeField] Text formatText;

    [SerializeField] InputSystemUIInputModule[] UIInputModules;

    //[SerializeField] GameObject ShotToggle;
    [SerializeField] Toggle[] ItemToggles;

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

        mv.InitMapVoteUI();
        mv.ActivateMapVoteMenu();


        //basic menuLC stuff
        if(pm == null)
        {
            pm = FindFirstObjectByType<PlayerManager>();
        }

        //disable players joining
        pm.im.pim.DisableJoining();

        foreach (PlayerConfig p in pm.PlayerList)
        {
            pm.DeactivatePlayer(p.playerIndex);

            pm.UnReadyPlayer(p.playerIndex);

            //move to spawn
            Transform spawn = this.gameObject.transform.GetChild(p.playerIndex);
            p.input.gameObject.transform.position = spawn.position;


            //assign players to UI input modules
            UIInputModules[p.playerIndex].gameObject.SetActive(true);
            UIInputModules[p.playerIndex].actionsAsset = pm.PlayerList[pm.PlayerList.FindIndex(player => player.playerIndex == p.playerIndex)].input.actions;
            pm.PlayerList[pm.PlayerList.FindIndex(player => player.playerIndex == p.playerIndex)].input.uiInputModule = UIInputModules[p.playerIndex];

            //set player UI colors
            SetUIColors(p.playerIndex);
        }

        //NOT doing this anymore
        //turn all items on
        //gm.ms.ResetItemSettings();

        //match item settings to menu toggles
        int idx = 0;
        foreach(Toggle tog in ItemToggles)
        {
            //enable/disable item
            gm.ms.ActiveItems[idx] = tog.isOn;

            //set default probability
            gm.ms.ItemProbs[idx] = gm.ms.DefaultItemProbs[idx];

            idx++;
        }

        //match points to win setting to menu point slider number
        SetPointsToWin();




        //set P1 icon color
        LeftUIElements[4].GetComponent<Image>().color = pm.PlayerList[0].playerScript.sr.color;

        //initialize p1 menu state tracking
        p1MenuState = 0;

        //deactivate P1 sprite until done with settings - OnDone()
        mv.TokenSprites[0].enabled = false;
    }


    public override void EndLevel()
    {
        //set player scores to 0    
        foreach(PlayerConfig p in pm.PlayerList)
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
            if(pm.PlayerList.TrueForAll(p => (p.isReady || !p.isActive)) && pm.PlayerList.Count(p => p.isActive) > 1)
            {
                FindFirstObjectByType<AudioManager>().Play("UINav1");

                //assign value randomly selected from gm.ms.MapPool
                int map = gm.ms.PickMap();

                gm.StartMatch(map);
            }



            if(!pm.PlayerList[playerID].isReady)
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
            if (pm.PlayerList[playerID].isReady)
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
                evSys.SetSelectedGameObject(ItemToggles[0].gameObject);

                //hide P1 map select token
                mv.TokenSprites[0].enabled = false;

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
        if(pm.PlayerList.TrueForAll(p => (p.isReady || !p.isActive)) && pm.PlayerList.Count(p => p.isActive) > 1)
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
        if(pm.PlayerList.TrueForAll(p => (p.isReady || !p.isActive)) && pm.PlayerList.Count(p => p.isActive) > 1)
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

        mv.SetUIColors(idx);
    }

    public void ToggleItem(int itemID)
    {
        gm.ms.ToggleItem(itemID);
    }


    public void SetPointsToWin()
    {
        gm.ms.pointsToWin = (int)pointSlider.value;
    }

    public void OnPointSliderValueChanged()
    {
        FindFirstObjectByType<AudioManager>().Play("UINav3");

        if(gm.gameMode != 2)
        {
            //update slider on intervals
            int idx = 0;
            foreach (int i in pointIntervals)
            {
                if(pointSlider.value == i-1)
                {
                    pointSlider.value = pointIntervals[idx-1];
                    break;
                } else if(pointSlider.value == i+1)
                {
                    pointSlider.value = pointIntervals[idx+1];
                    break;
                }

                idx++;
            }

        }
        

        SetPointsToWin();

        pointsNumber.text = pointSlider.value.ToString();
        
    }

    public void OnFormatSliderChanged()
    {
        FindFirstObjectByType<AudioManager>().Play("UINav3");

        switch (formatSlider.value)
        {
            case 0:
                formatText.text = "Headhunters";
                break;

            case 1:
                formatText.text = "Survival";
                break;
        }

        gm.ms.scoreFormat = (int)formatSlider.value;


    }


    public void OnDone()
    {
        //update p1 menu state
        p1MenuState = 1;

        //move P1 selection to map vote
        MultiplayerEventSystem evSys = UIInputModules[0].transform.GetComponent<MultiplayerEventSystem>();
        evSys.playerRoot = rightCanvas.gameObject;
        evSys.SetSelectedGameObject(mv.Map1Button);

        //show P1 map select token
        mv.TokenSprites[0].enabled = true;

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

        //show P1 icon
        LeftUIElements[4].gameObject.SetActive(true);
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

        //hide P1 icon
        LeftUIElements[4].gameObject.SetActive(false);
    }
    




}
