using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewMainMenuLC : LevelController
{

    // Start is called before the first frame update
    void Awake()
    {
        //stupid emergency workaround
        FindFirstObjectByType<AudioManager>().Awake();

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
        
        if(pm == null)
        {
            pm = FindFirstObjectByType<PlayerManager>();
        }

        if(pm != null)
        {
            foreach (PlayerConfig p in pm.playerList)
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
        SpawnPlayer(idx);
    }


    public override void SpawnPlayer(int idx)
    {
        //move player to spawnpoint
        pm.playerList[pm.playerList.FindIndex(p => p.playerIndex == idx)].input.gameObject.transform.position = SpawnPoints[idx].position;
    }


    public override int GetLevelType()
    {
        //MainMenu type = 0
        return 0;
    }

}
