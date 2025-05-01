using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.SocialPlatforms;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance {get; private set;}
    [SerializeField] GameObject pmPrefab;
    [SerializeField] AudioManager am;
    

    PlayerManager pm;
    [HideInInspector] public LevelController lc;
    [HideInInspector] public SceneLoader sl;
    [HideInInspector] public MatchSettings ms;
    [HideInInspector] public bool battleStarted;
    [HideInInspector] public int gameMode;

    // Start is called before the first frame update
    void Awake()
    {
        if (Instance != null)
        {
            Debug.Log("ERROR: trying to make more than one GameManager!");
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(Instance);
            

            sl = GetComponentInChildren<SceneLoader>();
            lc = GameObject.Find("LevelController").GetComponent<LevelController>();
            ms = GetComponent<MatchSettings>();

            am = GetComponentInChildren<AudioManager>();

            battleStarted = false;

        }

        



    }

    // Update is called once per frame
    void Update()
    {
        
    }


    //resets accumulated Match stats (player points, rounds played, etc)
    public void ResetGame()
    {
        ms.roundsPlayed = 1;

        foreach(PlayerConfig p in pm.PlayerList)
        {
            p.score = 0;
        }

        foreach(Team t in pm.TeamList)
        {
            t.score = 0;
        }
    }
    

    //Instantiates pm
    public void CreatePlayerManager()
    {
        GameObject PlayerManager = Instantiate(pmPrefab, transform.position, transform.rotation);
        PlayerManager.transform.parent = this.transform;
        pm = PlayerManager.GetComponent<PlayerManager>();
        pm.Init();
    }

    //starts a match on the inputted map
    public void StartMatch(int mapID)
    {
        am.StopAll();

        ms.ClearMapPool();

        //number of players
        int pcount = pm.PlayerList.Count(p => p.isActive);

        string sceneName = "MainMenu";
        if(gameMode == 0 || gameMode == 1)
        {
            sceneName = "BattleArena" + mapID;
        } else if(gameMode == 2)
        {
            sceneName = "SoccerArena" + mapID;
        } else
        {
            Debug.Log("ERROR: invalid gamemode!");
        }

        Debug.Log("StartMAtch scene: " + sceneName);

        sl.LoadScene(sceneName);        

    }

    
}
