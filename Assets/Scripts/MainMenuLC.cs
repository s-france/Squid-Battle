using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MainMenuLC : MonoBehaviour, ILevelController
{
    GameManager gm;
    UIController ui;
    PlayerManager pm;
    List<Transform> SpawnPoints;

    // Start is called before the first frame update
    void Awake()
    {
        gm = GameObject.Find("GameManager").GetComponent<GameManager>();
        gm.lc = this;
        ui = GameObject.Find("MenuUI").GetComponent<UIController>();
        pm = FindObjectOfType<PlayerManager>();
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


    public void StartLevel()
    {
        ui.HideAllPlayers();

        //ADD THIS if(pm.playerList != null)
        foreach (PlayerConfig p in pm.playerList)
        {
            //if(p.isActive)
            //{
            pm.DeactivatePlayer(p.playerIndex);
            pm.ReactivatePlayer(p.playerIndex);

            pm.UnReadyPlayer(p.playerIndex);
            ui.ShowPlayerUI(p.playerIndex);

            Transform spawn = this.gameObject.transform.GetChild(p.playerIndex);
            p.input.gameObject.transform.position = spawn.position;
            //}
            //else
            //{
            //    ui.HidePlayerUI(p.playerIndex);
            //}
        }
    }


    public void EndLevel()
    {
        //run any kind of outro/transition stuff here
        Debug.Log("ending main menu");
    }


    public void ResetLevel()
    {
        transform.Find("Canvas").Find("PlayersWin").gameObject.SetActive(false);

        foreach (PlayerConfig p in pm.playerList)
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


    public void OnPlayerJoin(int idx)
    {
        ui.ShowPlayerUI(idx);
        SpawnPlayer(idx);
    }

    public void SpawnPlayer(int idx)
    {
        //move player to spawnpoint
        pm.playerList[pm.playerList.FindIndex(p => p.playerIndex == idx)].input.gameObject.transform.position = SpawnPoints[idx].position;
    }

    public List<Transform> GetSpawnPoints()
    {
        return SpawnPoints;
    }


    public int GetLevelType()
    {
        //MainMenu type = 0
        return 0;
    }

    public GameObject GetGameManager()
    {
        return GameObject.Find("GameManager");
    }
}
