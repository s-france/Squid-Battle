using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class CharacterSelectLC : LevelController
{
    Transform Players;

    [HideInInspector] public Transform[][] PlayersUI;
    [SerializeField] Transform[] P1UI;
    [SerializeField] Transform[] P2UI;
    [SerializeField] Transform[] P3UI;
    [SerializeField] Transform[] P4UI;
    [SerializeField] Transform[] P5UI;
    [SerializeField] Transform[] P6UI;

    [SerializeField] Transform ready2StartUI;

    // Start is called before the first frame update
    void Awake()
    {
        //for each player
        //assign pi.UIInputModule
        //

        gm = GameObject.Find("GameManager").GetComponent<GameManager>();
        gm.lc = this;
        gm.battleStarted = false; //lol
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

        PlayersUI = new Transform[6][];
        PlayersUI[0] = P1UI;
        PlayersUI[1] = P2UI;
        PlayersUI[2] = P3UI;
        PlayersUI[3] = P4UI;
        PlayersUI[4] = P5UI;
        PlayersUI[5] = P6UI;

        Players = gm.transform.Find("Players");

        StartLevel();
        
    }


    public override void StartLevel()
    {
        AudioManager am = FindFirstObjectByType<AudioManager>();
        am.Init();
        if(!am.SoundsPlaying.Contains("Menu"))
        {
            am.Play("Menu");
        }

        //reactivate players
        Players.gameObject.SetActive(true);

        //basic menuLC stuff
        if(pm == null)
        {
            gm.CreatePlayerManager();

            pm = FindFirstObjectByType<PlayerManager>();
        }

        if (pm != null && pm.PlayerList.Any())
        {
            foreach (PlayerConfig p in pm.PlayerList)
            {
                //pm.DeactivatePlayer(p.playerIndex);

                pm.UnReadyPlayer(p.playerIndex);

                Transform spawn = this.gameObject.transform.GetChild(p.playerIndex);
                p.input.gameObject.transform.position = spawn.position;


                p.input.SwitchCurrentActionMap("Menu");

                //set colors
                SetUIColors(p.playerIndex);

                //set UI to be active
                ActivatePlayerUI(p.playerIndex);

                //Debug.Log("Action Map: " + p.input.currentActionMap);
                Debug.Log("P" + p.playerIndex + " devices: " + p.input.devices);

                //p.input.ActivateInput();
            }
        }

        //enable players joining
        pm.im.pim.EnableJoining();


    }


    public override void EndLevel()
    {
        //stop players from joining outside of this menu
        pm.im.pim.DisableJoining();

        //run any kind of outro/transition stuff here
        Debug.Log("character select");
    }


    public override void ResetLevel()
    {


    }


    public override void OnPlayerJoin(int idx)
    {
        Debug.Log("PLAYER" + idx + " JOINED FROM LC");

        SpawnPlayer(idx);

        pm.PlayerList[idx].input.SwitchCurrentActionMap("Menu");
        //Debug.Log("Action Map: " + pm.playerList[idx].input.currentActionMap);

        //Character Select UI stuff

        //set colors
        SetUIColors(idx);


        //set UI to be active
        ActivatePlayerUI(idx);

    }


    public override void OnPlayerLeave(int idx)
    {
        //disable player's UI
        DeactivatePlayerUI(idx);
    }

    public override void OnPlayerReconnect(int idx)
    {
        //reenable playerUI
        ActivatePlayerUI(idx);

    }


    public override void SpawnPlayer(int idx)
    {
        //move player to spawnpoint
        pm.PlayerList[pm.PlayerList.FindIndex(p => p.playerIndex == idx)].input.gameObject.transform.position = SpawnPoints[idx].position;
    }


    public override void ReadyPlayer(int idx)
    {
        //do ui stuff
        PlayersUI[idx][0].GetComponent<Image>().sprite = pm.playerSprites[2];
        PlayersUI[idx][2].gameObject.SetActive(false);
        PlayersUI[idx][5].gameObject.SetActive(false);
        PlayersUI[idx][6].gameObject.SetActive(true);


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
        //base.UnReadyPlayer(idx);
        
        //do ui stuff
        PlayersUI[idx][0].GetComponent<Image>().sprite = pm.playerSprites[0]; //neutral sprite
        PlayersUI[idx][2].gameObject.SetActive(true);
        PlayersUI[idx][5].gameObject.SetActive(true);
        PlayersUI[idx][6].gameObject.SetActive(false);

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


    public override int GetLevelType()
    {
        //type = 1
        return 1;
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public virtual void ActivatePlayerUI(int idx)
    {
        Transform[] PlayerUI = PlayersUI[idx];


        PlayerUI[0].gameObject.SetActive(true);
        PlayerUI[1].gameObject.SetActive(false);
        PlayerUI[2].gameObject.SetActive(true);
        PlayerUI[5].gameObject.SetActive(true);
        PlayerUI[7].gameObject.SetActive(true);
    }

    void DeactivatePlayerUI(int idx)
    {
        Transform[] PlayerUI = PlayersUI[idx];

        PlayerUI[0].gameObject.SetActive(false);
        PlayerUI[1].gameObject.SetActive(true);
        PlayerUI[2].gameObject.SetActive(false);
        PlayerUI[5].gameObject.SetActive(false);
        PlayerUI[6].gameObject.SetActive(false);
        PlayerUI[7].gameObject.SetActive(false);

    }


    public override void SetUIColors(int idx)
    {
        Transform[] PlayerUI = PlayersUI[idx];


        //set colors
        PlayerUI[0].GetComponent<Image>().color = pm.playerColors[pm.PlayerList[idx].color];

        foreach (PlayerConfig pc in pm.PlayerList)
        {
            idx = pc.playerIndex;

            PlayerUI = PlayersUI[idx];
            

            PlayerUI[3].GetComponent<Image>().color = pm.playerColors[pm.FindFirstAvailableColorID(pm.PlayerList[idx].color -1 , -1)];
            PlayerUI[4].GetComponent<Image>().color = pm.playerColors[pm.FindFirstAvailableColorID(pm.PlayerList[idx].color + 1, 1)];
        }
    }

    public override void OnBack(int playerID, InputAction.CallbackContext ctx)
    {
        //base.OnBack(ctx);
        if(ctx.performed)
        {
            FindFirstObjectByType<AudioManager>().Play("UINav2");

            if(pm.PlayerList[playerID].isActive)
            {
                if(pm.PlayerList[playerID].isReady)
                {
                    //unReady player
                    pm.UnReadyPlayer(playerID);
                } else //drop out if held for 1 secs before game
                {
                    pm.PlayerList[playerID].playerScript.OnControllerDisconnect(pm.PlayerList[playerID].input);
                    //(^^this calls lc.OnPlayerLeave)
                }

                
            } else if(!pm.PlayerList[playerID].isActive)
            {
                //go to previous screen (mode select)

                //TRY destroy playermanager here
                //GameObject.Destroy(pm.gameObject);
                pm.im.pim.DisableJoining();

                gm.sl.LoadScene("ModeSelect");

            }
        }

        
        

    }



}
