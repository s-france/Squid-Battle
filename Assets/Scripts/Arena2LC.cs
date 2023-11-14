using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Numerics;
using UnityEngine.UIElements;
using Unity.Mathematics;

public class Arena2LC : MonoBehaviour, ILevelController
{
    GameManager gm;
    PlayerManager pm;
    ItemManager im;
    List<Transform> SpawnPoints;

    Transform Box1;
    Transform Box2;

    [SerializeField] float moveDistance;
    [SerializeField] float moveSpeed;
    int moveDirection1;
    int moveDirection2;
    float arenaPosition;
    bool forward;
    UnityEngine.Vector2 movePos;


    [SerializeField] List<UnityEngine.Vector2> ArenaShrinks;
    [SerializeField] float shrinkSpeed;
    Transform arena;

    // Start is called before the first frame update
    void Awake()
    {
        gm = GameObject.Find("GameManager").GetComponent<GameManager>();
        gm.lc = this;
        pm = gm.GetComponentInChildren<PlayerManager>();
        im = GameObject.Find("ItemManager").GetComponent<ItemManager>();

        SpawnPoints = new List<Transform>();

        SpawnPoints.Add(transform.GetChild(0));
        SpawnPoints.Add(transform.GetChild(1));
        SpawnPoints.Add(transform.GetChild(2));
        SpawnPoints.Add(transform.GetChild(3));
        
        Box1 = GameObject.Find("Box1").transform;
        Box2 = GameObject.Find("Box2").transform;
        arenaPosition = 0;
        moveDirection1 = 1;
        moveDirection2 = -1;
        forward = true;
        
        arena = GameObject.Find("Arena").transform;
        
        StartLevel();
    }

    // Update is called once per frame
    void Update()
    {
        MoveBox(1);
        MoveBox(2);
    }

    void MoveBox(int boxNum)
    {
        Transform box = null;
        int direction = 0;
        UnityEngine.Vector2 target = UnityEngine.Vector2.zero;
        UnityEngine.Vector2 start = UnityEngine.Vector2.zero;

        if(boxNum == 1)
        {
            box = Box1;
            direction = moveDirection1;
        } else if(boxNum == 2)
        {
            box = Box2;
            direction = moveDirection2;
        }

        start = new UnityEngine.Vector2(0, box.localPosition.y);
        target = new UnityEngine.Vector2(moveDistance*direction, box.localPosition.y);
        
        if(Mathf.Abs(box.localPosition.x) == Mathf.Abs(target.x) /*-.01*/)
        {
            forward = false;
        } else if(Mathf.Abs(box.localPosition.x) == Mathf.Abs(start.x))
        {
            forward = true;
        }

        if(forward)
        {
            box.localPosition = UnityEngine.Vector2.MoveTowards(box.localPosition, target, moveSpeed*Time.deltaTime);
        } else
        {
            box.localPosition = UnityEngine.Vector2.MoveTowards(box.localPosition, start, moveSpeed*Time.deltaTime);
        }
        
    }


    IEnumerator ShrinkClock()
    {
        float timer = 0;
        Debug.Log("COROUTINE STARTED");
        
        while(arena.localScale.magnitude > ArenaShrinks[2].magnitude)
        {
            if (timer >= 35 && arena.localScale.magnitude > ArenaShrinks[0].magnitude)
            {
                Debug.Log("SHRINKING 0");
                arena.localScale = UnityEngine.Vector2.MoveTowards(arena.localScale, ArenaShrinks[0], shrinkSpeed * Time.deltaTime);
            } else if (timer >= 70 && arena.localScale.magnitude > ArenaShrinks[1].magnitude)
            {
                Debug.Log("SHRINKING 1");
                arena.localScale = UnityEngine.Vector2.MoveTowards(arena.localScale, ArenaShrinks[1], shrinkSpeed * Time.deltaTime);
            } else if (timer >= 105 && arena.localScale.magnitude > ArenaShrinks[2].magnitude)
            {
                Debug.Log("SHRINKING 2");
                arena.localScale = UnityEngine.Vector2.MoveTowards(arena.localScale, ArenaShrinks[2], shrinkSpeed * Time.deltaTime);
            }
            timer = timer + Time.deltaTime;
            yield return null;
        }
        Debug.Log("ENDING SHRINK COROUTINE");
        
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
        FindObjectOfType<AudioManager>().PlayRandom("BattleTheme");

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


    public void EndLevel()
    {
        //deactivate all players
        //declare winner somewhere
        //load start menu

        Debug.Log("stopping music!!!");
        //turn off battle music
        FindObjectOfType<AudioManager>().Stop("BattleTheme1");
        FindObjectOfType<AudioManager>().Stop("BattleTheme2");


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
