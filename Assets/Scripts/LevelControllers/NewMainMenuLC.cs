using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewMainMenuLC : LevelController
{

    // Start is called before the first frame update
    void Awake()
    {
        //stupid emergency workaround
        FindFirstObjectByType<AudioManager>().Init();

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

        StartLevel();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void StartLevel()
    {
        AudioManager am = FindFirstObjectByType<AudioManager>();
        if(!am.SoundsPlaying.Contains("Menu"))
        {
            am.Play("Menu");
        }
        
        
        
        if(pm == null)
        {
            pm = FindFirstObjectByType<PlayerManager>();
        }

        if(pm != null)
        {
            foreach (PlayerConfig p in pm.PlayerList)
            {
                pm.DeactivatePlayer(p.playerIndex);

                pm.UnReadyPlayer(p.playerIndex);

                Transform spawn = this.gameObject.transform.GetChild(p.playerIndex);
                p.input.gameObject.transform.position = spawn.position;
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
        pm.PlayerList[pm.PlayerList.FindIndex(p => p.playerIndex == idx)].input.gameObject.transform.position = SpawnPoints[idx].position;
    }

    public void LoadMenu(int type)
    {
        FindFirstObjectByType<AudioManager>().Play("UINav1");

        SceneLoader sl = gm.GetComponentInChildren<SceneLoader>();

        switch (type)
        {
            case 0: //Play
                sl.LoadScene("ModeSelect");
                break;
            case 1: //Options
                //LOAD OPTIONS MENU HERE!!
                break;
            case 2: //How to Play
                //LOAD HOW2PLAY HERE!!
                break;
            case 3: //Extras
                //LOAD EXTRAS/CREDITS HERE!!
                break;
            default:
                break;
        }
    }


    public override int GetLevelType()
    {
        //MainMenu type = 0
        return 0;
    }

}
