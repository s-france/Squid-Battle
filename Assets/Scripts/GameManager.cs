using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance {get; private set;}
    [SerializeField] GameObject pmPrefab;

    PlayerManager pm;
    [HideInInspector] public LevelController lc;
    [HideInInspector] public SceneLoader sl;
    [HideInInspector] public bool battleStarted;

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

            battleStarted = false;

        }

        



    }

    // Update is called once per frame
    void Update()
    {
        
    }


    //REWORK:  LevelController.EndBattle should call this -> leads back to start menu
    //declare winner and end/reset game
    public void ResetGame(int winnerIdx)
    {
        Debug.Log("stopping music!!!");
        //turn off battle music
        FindFirstObjectByType<AudioManager>().Stop("BattleTheme");


        foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Despawns"))
        {
            Destroy(obj);
        }

        //REWORK
        //camCon.StartCamOn();

        battleStarted = false;

        //reactivate() all isactive players
        //move players to spawn points
        foreach (PlayerConfig p in pm.playerList)
        {
            if(p.isActive)
            {
                pm.DeactivatePlayer(p.playerIndex);
                pm.ReactivatePlayer(p.playerIndex);
            }

            Transform spawn = this.gameObject.transform.GetChild(p.playerIndex);
            p.input.gameObject.transform.position = spawn.position;
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

    
}
