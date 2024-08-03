using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class CharacterSelectLC : LevelController
{
    Transform Players;

    Transform[][] PlayersUI;
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
        pm = FindFirstObjectByType<PlayerManager>();
        //pm = GetGameManager().GetComponentInChildren<PlayerManager>();
        //pm = GameObject.Find("PlayerManager").GetComponent<PlayerManager>();

        SpawnPoints = new List<Transform>();

        SpawnPoints.Add(transform.GetChild(0));
        SpawnPoints.Add(transform.GetChild(1));
        SpawnPoints.Add(transform.GetChild(2));
        SpawnPoints.Add(transform.GetChild(3));

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
        //reactivate players
        Players.gameObject.SetActive(true);

        //basic menuLC stuff
        if(pm == null)
        {
            gm.CreatePlayerManager();

            pm = FindFirstObjectByType<PlayerManager>();
        }

        if (pm != null && pm.playerList.Any())
        {
            foreach (PlayerConfig p in pm.playerList)
            {
                pm.DeactivatePlayer(p.playerIndex);

                pm.UnReadyPlayer(p.playerIndex);

                Transform spawn = this.gameObject.transform.GetChild(p.playerIndex);
                p.input.gameObject.transform.position = spawn.position;


                p.input.SwitchCurrentActionMap("Menu");

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
        //run any kind of outro/transition stuff here
        Debug.Log("ending main menu");
    }


    public override void ResetLevel()
    {


    }


    public override void OnPlayerJoin(int idx)
    {
        Debug.Log("PLAYER" + idx + " JOINED FROM LC");

        SpawnPlayer(idx);

        pm.playerList[idx].input.SwitchCurrentActionMap("Menu");
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
        pm.playerList[pm.playerList.FindIndex(p => p.playerIndex == idx)].input.gameObject.transform.position = SpawnPoints[idx].position;
    }


    public override void ReadyPlayer(int idx)
    {
        //do ui stuff
        PlayersUI[idx][0].GetComponent<Image>().sprite = pm.playerSprites[pm.playerList[idx].color][2];
        PlayersUI[idx][2].gameObject.SetActive(false);
        PlayersUI[idx][5].gameObject.SetActive(false);
        PlayersUI[idx][6].gameObject.SetActive(true);


        //allow starting game if all active players are ready && more than 1 player
        if(pm.playerList.TrueForAll(p => (p.isReady || !p.isActive)) && pm.playerList.Count(p => p.isActive) > 1)
        {
            //show ready to start UI
            ready2StartUI.gameObject.SetActive(true);

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


    void ActivatePlayerUI(int idx)
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
        PlayerUI[0].GetComponent<Image>().sprite = pm.playerSprites[pm.playerList[idx].color][0];

        foreach (PlayerConfig pc in pm.playerList)
        {
            idx = pc.playerIndex;

            PlayerUI = PlayersUI[idx];
            

            PlayerUI[3].GetComponent<Image>().sprite = pm.playerSprites[pm.FindFirstAvailableColorID(pm.playerList[idx].color -1 , -1)][0];
            PlayerUI[4].GetComponent<Image>().sprite = pm.playerSprites[pm.FindFirstAvailableColorID(pm.playerList[idx].color + 1, 1)][0];
        }
    }

    

}
