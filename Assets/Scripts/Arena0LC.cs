using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Arena0LC : MonoBehaviour, ILevelController
{
    GameManager gm;
    PlayerManager pm;
    List<Transform> SpawnPoints;

    // Start is called before the first frame update
    void Awake()
    {
        gm = GameObject.Find("GameManager").GetComponent<GameManager>();
        gm.lc = this;
        pm = GameObject.Find("PlayerManager").GetComponent<PlayerManager>();

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
        Debug.Log("Starting LEvel arena0");
        if(pm == null)
        {
            Debug.Log("PlayerManager NOT FOUND!!");
        }

        //move players to spawnpoints
        //enable player control
        //begin item spawns
        //battleStarted = true;


        Debug.Log("playing music!!!");
        //play music
        FindObjectOfType<AudioManager>().Play("BattleTheme");

        //move players to spawn positions
        foreach (PlayerConfig p in pm.playerList.Where(p => p.isActive))
        {
            pm.DeactivatePlayer(p.playerIndex);
            pm.ReactivatePlayer(p.playerIndex);

            Transform start = GetSpawnPoints()[p.playerIndex];
            p.input.gameObject.transform.position = start.position;

            p.isAlive = true;
        }

        //REWORK
        //camCon.GameCamOn();

        gm.battleStarted = true;
        Debug.Log("game started!");

        //REWORK
        //StartCoroutine(itemMan.RandomSpawns(15));

        //ITEM TESTING
        //itemMan.SpawnItem(2, itemMan.transform);
    

    

    }


    public void EndLevel()
    {
        //deactivate all players
        //declare winner somewhere
        //load start menu
    }


    public void ResetLevel()
    {
        //idk
    }


    public void OnPlayerJoin(int idx)
    {
        //deactivate until next round
    }


    //moves player to spawnpoint
    public void SpawnPlayer(int idx)
    {
        pm.playerList[pm.playerList.FindIndex(p => p.playerIndex == idx)].input.gameObject.transform.position = SpawnPoints[idx].position;
    }

    
    public GameObject GetGameManager()
    {
        return GameObject.Find("GameManager");
    }

    public List<Transform> GetSpawnPoints()
    {
        return SpawnPoints;
    }

    public int GetLevelType()
    {
        //battle arenas type = 2
        return 2;
    }
}
