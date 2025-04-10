using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MainMenuLC : LevelController
{
    UIController ui;
    

    // Start is called before the first frame update
    void Awake()
    {
        //stupid emergency workaround
        FindFirstObjectByType<AudioManager>().Awake();

        gm = GameObject.Find("GameManager").GetComponent<GameManager>();
        gm.lc = this;
        ui = GameObject.Find("MenuUI").GetComponent<UIController>();
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

    // Update is called once per frame
    void Update()
    {
        
    }


    public override void StartLevel()
    {
        if (FindAnyObjectByType<AudioManager>() != null)
        {
            FindAnyObjectByType<AudioManager>().Play("Menu");
        }
        ui.HideAllPlayers();

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

        ui.HighlightWinner(pm.placements.Last());
    }


    public override void EndLevel()
    {
        //run any kind of outro/transition stuff here
        Debug.Log("ending main menu");
    }


    public override void ResetLevel()
    {
        transform.Find("Canvas").Find("PlayersWin").gameObject.SetActive(false);

        foreach (PlayerConfig p in pm.PlayerList)
        {
            if(p.isActive)
            {
                pm.UnReadyPlayer(p.playerIndex);
                ui.ShowPlayerUI(p.playerIndex);
            }
            else
            {
                ui.HidePlayerUI(p.playerIndex);
            }
        }
    }


    public override void OnPlayerJoin(int idx)
    {
        ui.ShowPlayerUI(idx);
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

    /*
    public GameObject GetGameManager()
    {
        return GameObject.Find("GameManager");
    }

    public override List<Transform> GetSpawnPoints()
    {
        return base.GetSpawnPoints();
    }
    */
}
