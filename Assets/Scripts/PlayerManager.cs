using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;




public class PlayerManager : MonoBehaviour
{
    GameManager gm;

    //REWORK!!!
    /*[HideinInspector]*/ public List<PlayerConfig> playerList {get; private set;}
    
    /*[HideInInspector]*/ public int colorsCount;
    public Sprite[] blueSprites;
    public Sprite[] cyanSprites;
    public Sprite[] greenSprites;
    public Sprite[] orangeSprites;
    public Sprite[] pinkSprites;
    public Sprite[] purpSprites;
    public Sprite[] redSprites;
    public Sprite[] yellowSprites;
    public Sprite[][] playerSprites;



    // Start is called before the first frame update
    void Awake()
    {
        
        
    }

    public void Init()
    {
        gm = GetComponentInParent<GameManager>();

        playerList = new List<PlayerConfig>();
        
        //build sprite array
        playerSprites = new Sprite[8][];
        //build sprite array
        playerSprites[0] = blueSprites;
        playerSprites[1] = cyanSprites;
        playerSprites[2] = greenSprites;
        playerSprites[3] = orangeSprites;
        playerSprites[4] = pinkSprites;
        playerSprites[5] = purpSprites;
        playerSprites[6] = redSprites;
        playerSprites[7] = yellowSprites;

        colorsCount = playerSprites.Length - 1;

        GetComponentInChildren<InputManager>().Init();
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    //sets player color
    public void SetPlayerColor(int idx, int color)
    {
        playerList[idx].color = color;
    }


    public void ReadyPlayer(int idx)
    {
        playerList[idx].isReady = true;
        Debug.Log("player" + playerList[idx].playerIndex + " is ready: " + playerList[idx].isReady);

        //start game if all active players are ready && more than 1 player
        if((playerList.TrueForAll(p => (p.isReady || !p.isActive))) && playerList.Count(p => p.isActive) > 1 && SceneManager.GetActiveScene().name == "StartMenu")
        {
            //load map select
            gm.sl.LoadScene("MapSelect");
            
        }
    }


    public void UnReadyPlayer(int idx)
    {
        playerList[idx].isReady = false;

        //start game if all active players are ready && more than 1 player
        if((playerList.TrueForAll(p => (p.isReady || !p.isActive))) && playerList.Count(p => p.isActive) > 1 && !gm.battleStarted)
        {
            //REWORK: load map select scene
            //StartGame();
        }
    }


    public void ReactivatePlayer(int idx)
    {
        playerList[idx].playerScript.Reactivate();
        playerList[idx].input.ActivateInput();
        playerList[idx].isAlive = true;


        Debug.Log("player" + idx + " reactivated!");
    }

    public void DeactivatePlayer(int idx)
    {
        //disable and hide gameobject
        playerList[idx].playerScript.Deactivate();
        UnReadyPlayer(idx);

        playerList[idx].input.DeactivateInput();
        playerList[idx].isReady = false;
        playerList[idx].isAlive = false;


        Debug.Log("player " + idx + " deactivated!");
    }


    //kills player - ONLY CALL THIS WHEN gameStarted == true
    public void KillPlayer(int idx)
    {
        DeactivatePlayer(idx);
        FindObjectOfType<AudioManager>().Play("Fall");


        //if only one remaining player alive end/reset the game
        if (playerList.Count(p => p.isAlive) == 1 && gm.battleStarted)
        {
            gm.lc.EndLevel();
        }
    }


    



}

public class PlayerConfig
{
    public PlayerConfig(PlayerInput pi)
    {
        input = pi;
        playerIndex = pi.playerIndex;
        playerScript = pi.GetComponentInParent<PlayerController>();
        isReady = false;
        isActive = true;
        isAlive = true;
        color = pi.playerIndex;
    }

    public PlayerController playerScript {get; set;} //player prefab object
    public PlayerInput input {get; set;}
    public int playerIndex {get; set;}
    public bool isReady {get; set;}
    public bool isActive {get; set;}
    public bool isAlive {get; set;}
    public int color {get; set;} //color idx
}