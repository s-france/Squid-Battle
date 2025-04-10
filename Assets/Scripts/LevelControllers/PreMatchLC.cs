using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem.UI;

public class PreMatchLC : LevelController
{
    [SerializeField] InputSystemUIInputModule[] UIInputModules;

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

        StartLevel();
        
    }


    public override void StartLevel()
    {
        //basic menuLC stuff
        if(pm == null)
        {
            pm = FindFirstObjectByType<PlayerManager>();
        }

        if (pm != null && pm.PlayerList.Any())
        {
            foreach (PlayerConfig p in pm.PlayerList)
            {
                pm.DeactivatePlayer(p.playerIndex);

                pm.UnReadyPlayer(p.playerIndex);

                Transform spawn = this.gameObject.transform.GetChild(p.playerIndex);
                p.input.gameObject.transform.position = spawn.position;
                //}
                //else
                //{
                //    ui.HidePlayerUI(p.playerIndex);
                //}
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

        UIInputModules[idx].gameObject.SetActive(true);
        UIInputModules[idx].actionsAsset = pm.PlayerList[pm.PlayerList.FindIndex(p => p.playerIndex == idx)].input.actions;

        pm.PlayerList[pm.PlayerList.FindIndex(p => p.playerIndex == idx)].input.uiInputModule = UIInputModules[idx];

        SpawnPlayer(idx);
    }


    public override void SpawnPlayer(int idx)
    {
        //move player to spawnpoint
        pm.PlayerList[pm.PlayerList.FindIndex(p => p.playerIndex == idx)].input.gameObject.transform.position = SpawnPoints[idx].position;
    }


    public override int GetLevelType()
    {
        //MainMenu type = 0
        return 0;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
