using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;


public class ModeSelectLC : LevelController
{
    [SerializeField] InputSystemUIInputModule SharedUI;
    [SerializeField] PlayerInput menuPI;
    Transform Players;

    [SerializeField] Transform[] Descs;

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

        Players = gm.transform.Find("Players");

        StartLevel();
        
    }


    // Update is called once per frame
    void Update()
    {
        
    }


    public override void StartLevel()
    {
        AudioManager am = FindFirstObjectByType<AudioManager>();
        am.Init();
        if(!am.SoundsPlaying.Contains("Menu"))
        {
            am.Play("Menu");
        }

        //this works...
        if(pm != null)
        {
            GameObject.Destroy(pm.gameObject);
        }

        if(Players.childCount > 0)
        {
            foreach(Transform child in Players)
            {
                GameObject.Destroy(child.gameObject);
            }
        }

        menuPI.enabled = true;
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
        pm.PlayerList[pm.PlayerList.FindIndex(p => p.playerIndex == idx)].input.gameObject.transform.position = SpawnPoints[idx].position;
    }


    public override int GetLevelType()
    {
        //ModeSelect type = 2
        return 2;
    }

    public void SelectMode(int modeID)
    {
        FindFirstObjectByType<AudioManager>().Play("UINav1");

        SceneLoader sl = gm.GetComponentInChildren<SceneLoader>();

        switch(modeID)
        {
            case 0: //classic
                Debug.Log("classic battle");
                gm.gameMode = 0;
                sl.LoadScene("CharacterSelect");
                break;
            case 1: //teams
                Debug.Log("team battle");
                gm.gameMode = 1;
                sl.LoadScene("TeamCharacterSelect");
                break;
            case 2: //soccer
                Debug.Log("soccer");
                gm.gameMode = 2;
                sl.LoadScene("TeamCharacterSelect");
                break;
            case 3: //coin
                //INSERT LOAD HERE

                break;
            default:
                break;
        }


    }

    public void OnBack(InputAction.CallbackContext ctx)
    {
        if(ctx.performed)
        {
            gm.sl.LoadScene("MainMenu");
        }
        
    }


}
