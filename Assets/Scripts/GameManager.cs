using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance {get; private set;}

    PlayerManager pm;
    [HideInInspector] public ILevelController lc;
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
        }



        pm = GetComponentInChildren<PlayerManager>();
        sl = GetComponentInChildren<SceneLoader>();
        lc = GameObject.Find("LevelController").GetComponent<ILevelController>();

        battleStarted = false;
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
        FindObjectOfType<AudioManager>().Stop("BattleTheme");


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


    //REWORK: move to LevelController.StartBattle
    void StartGame()
    {

        Debug.Log("playing music!!!");
        //play music
        FindObjectOfType<AudioManager>().Play("BattleTheme");

        //move players to spawn positions
        foreach (PlayerConfig p in pm.playerList.Where(p => p.isActive))
        {
            pm.DeactivatePlayer(p.playerIndex);
            pm.ReactivatePlayer(p.playerIndex);

            Transform start = this.gameObject.transform.GetChild(p.playerIndex + 4);
            p.input.gameObject.transform.position = start.position;

            p.isAlive = true;
        }

        //REWORK
        //camCon.GameCamOn();

        battleStarted = true;
        Debug.Log("game started!");

        //REWORK
        //StartCoroutine(itemMan.RandomSpawns(15));

        //ITEM TESTING
        //itemMan.SpawnItem(2, itemMan.transform);
    }

    
}
