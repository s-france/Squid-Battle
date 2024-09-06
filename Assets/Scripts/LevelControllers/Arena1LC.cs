using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.VisualScripting.FullSerializer;

public class Arena1LC : LevelController
{
    [SerializeField] List<Vector2> ArenaShrinks;
    [SerializeField] float shrinkSpeed;
    Transform arena;


    // Start is called before the first frame update
    void Awake()
    {
        gm = GameObject.Find("GameManager").GetComponent<GameManager>();
        gm.lc = this;
        pm = gm.GetComponentInChildren<PlayerManager>();
        im = GameObject.Find("ItemManager").GetComponent<ItemManager>();

        im.gm = gm;

        SpawnPoints = new List<Transform>();

        SpawnPoints.Add(transform.GetChild(0));
        SpawnPoints.Add(transform.GetChild(1));
        SpawnPoints.Add(transform.GetChild(2));
        SpawnPoints.Add(transform.GetChild(3));

        arena = GameObject.Find("Arena").transform;
        
        StartLevel();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator ShrinkClock()
    {
        float timer = 0;
        Debug.Log("COROUTINE STARTED");
        
        while(arena.localScale.magnitude > ArenaShrinks[2].magnitude)
        {
            if (timer >= 35 && arena.localScale.magnitude > ArenaShrinks[0].magnitude)
            {
                //Debug.Log("SHRINKING 0");
                arena.localScale = Vector2.MoveTowards(arena.localScale, ArenaShrinks[0], shrinkSpeed * Time.deltaTime);
            } else if (timer >= 70 && arena.localScale.magnitude > ArenaShrinks[1].magnitude)
            {
                //Debug.Log("SHRINKING 1");
                arena.localScale = Vector2.MoveTowards(arena.localScale, ArenaShrinks[1], shrinkSpeed * Time.deltaTime);
            } else if (timer >= 105 && arena.localScale.magnitude > ArenaShrinks[2].magnitude)
            {
                //Debug.Log("SHRINKING 2");
                arena.localScale = Vector2.MoveTowards(arena.localScale, ArenaShrinks[2], shrinkSpeed * Time.deltaTime);
            }
            timer = timer + Time.deltaTime;
            yield return null;
        }
        Debug.Log("ENDING SHRINK COROUTINE");
        
    }


    public override void StartLevel()
    {
        //FindObjectOfType<AudioManager>().Stop("MenuTheme1");

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
        FindFirstObjectByType<AudioManager>().PlayRandom("BattleTheme");

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
        StartCoroutine(im.RandomSpawns(15));

        StartCoroutine(ShrinkClock());



        //ITEM TESTING
        //itemMan.SpawnItem(2, itemMan.transform);
    

    

    }


    public override void EndLevel()
    {
        //deactivate all players
        //declare winner somewhere
        //load start menu

        Debug.Log("stopping music!!!");
        //turn off battle music
        FindFirstObjectByType<AudioManager>().Stop("BattleTheme1");
        FindFirstObjectByType<AudioManager>().Stop("BattleTheme2");
        FindFirstObjectByType<AudioManager>().Stop("BattleTheme3");

        //not really needed anymore
        /*
        foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Despawns"))
        {
            Destroy(obj);
        }
        */

        gm.battleStarted = false;

        gm.sl.LoadScene("StartMenu");

    }


    //REWORK:  LevelController.EndBattle should call this -> leads back to start menu
    //declare winner and end/reset game
    public void ResetGame(int winnerIdx)
    {
        


        


        

        //reactivate() all isactive players
        //move players to spawn points
        
    }


    public override void ResetLevel()
    {
        //idk
    }


    public override void OnPlayerJoin(int idx)
    {
        //deactivate until next round
    }


    //moves player to spawnpoint
    public override void SpawnPlayer(int idx)
    {
        pm.playerList[pm.playerList.FindIndex(p => p.playerIndex == idx)].input.gameObject.transform.position = SpawnPoints[idx].position;
    }

    /*
    public GameObject GetGameManager()
    {
        return GameObject.Find("GameManager");
    }

    public List<Transform> GetSpawnPoints()
    {
        return SpawnPoints;
    }
    */

    public override int GetLevelType()
    {
        //battle arenas type = 10
        return 10;
    }
}