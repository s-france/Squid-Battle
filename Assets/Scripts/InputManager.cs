using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    CamController camCon;
    UIController ui;
    ItemManager itemMan;

    public List<PlayerConfig> playerList {get; private set;}

    [HideInInspector] public int colorsCount;
    public Sprite[] blueSprites;
    public Sprite[] cyanSprites;
    public Sprite[] greenSprites;
    public Sprite[] orangeSprites;
    public Sprite[] pinkSprites;
    public Sprite[] purpSprites;
    public Sprite[] redSprites;
    public Sprite[] yellowSprites;
    public Sprite[][] playerSprites;
    
    [HideInInspector] public bool gameStarted;

    public static InputManager Instance {get; private set;}

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.Log("ERROR: trying to make more than one InputManager!");
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(Instance);
            playerList = new List<PlayerConfig>();
            gameStarted = false;
            
            playerSprites = new Sprite[8][];

            playerSprites[0] = blueSprites;
            playerSprites[1] = cyanSprites;
            playerSprites[2] = greenSprites;
            playerSprites[3] = orangeSprites;
            playerSprites[4] = pinkSprites;
            playerSprites[5] = purpSprites;
            playerSprites[6] = redSprites;
            playerSprites[7] = yellowSprites;

            /*playerSprites.Append(cyanSprites);
            playerSprites.Append(greenSprites);
            playerSprites.Append(orangeSprites);
            playerSprites.Append(pinkSprites);
            playerSprites.Append(purpSprites);
            playerSprites.Append(redSprites);
            playerSprites.Append(yellowSprites);*/

            colorsCount = playerSprites.Length - 1;

            ui = GameObject.Find("MenuUI").GetComponent<UIController>();
            camCon = GameObject.Find("VCams").GetComponent<CamController>();
            itemMan = GameObject.Find("ItemManager").GetComponent<ItemManager>();

            ui.HideAllPlayers();
        }
    }

    //sets player color
    public void SetPlayerColor(int idx, int color)
    {
        playerList[idx].color = color;
    }

    //handle player joining here
    public void OnPlayerJoin(PlayerInput pi)
    {
        Debug.Log("Player joined!");
        Debug.Log("Index: " + pi.playerIndex);
        

        //make sure player index is not already added (not that that should ever happen)
        if(!playerList.Any(p => p.playerIndex == pi.playerIndex))
        {
            playerList.Add(new PlayerConfig(pi));

            //move player to correct spawn location (based on idx)
            Transform spawn = this.gameObject.transform.GetChild(pi.playerIndex);
            pi.gameObject.transform.position = spawn.position;

            ui.ShowPlayerUI(pi.playerIndex);

            if(gameStarted)
            {
                DeactivatePlayer(pi.playerIndex);
            }
        }
    }

    //handle player leaving/disconnecting
    public void OnPlayerLeave(PlayerInput pi)
    {
        playerList[pi.playerIndex].isActive = false;
        DeactivatePlayer(pi.playerIndex);
        Debug.Log("P" + pi.playerIndex + " left!");
    }

    //ran when disconnected player controller reconnects
    public void OnPlayerReconnect(PlayerInput pi)
    {
        playerList[pi.playerIndex].isActive = true;
        if (!gameStarted)
        {
            ReactivatePlayer(pi.playerIndex);
        }
        Debug.Log("player " + pi.playerIndex + " reconnected!");
    }


    void Update()
    {
        /*
        if(playerList[0] != null)
        {
            Debug.Log("P" + playerList[0].playerIndex + " i_move val: " + playerList[0].playerScript.i_move);
        }
        if(playerList[1] != null)
        {
            Debug.Log("P" + playerList[1].playerIndex + " i_move val: " + playerList[1].playerScript.i_move);
        }
        */
    }

    public void ReadyPlayer(int idx)
    {
        ui.ReadyPlayer(idx);
        playerList[idx].isReady = true;
        Debug.Log("player" + playerList[idx].playerIndex + " is ready: " + playerList[idx].isReady);

        //start game if all active players are ready && more than 1 player
        if((playerList.TrueForAll(p => (p.isReady || !p.isActive))) && playerList.Count(p => p.isActive) > 1)
        {
            StartGame();
        }
    }

    public void UnReadyPlayer(int idx)
    {
        ui.UnReadyPlayer(idx);
        playerList[idx].isReady = false;
    }


    public void ReactivatePlayer(int idx)
    {
        playerList[idx].playerScript.Reactivate();
        playerList[idx].input.ActivateInput();
        playerList[idx].isAlive = true;

        ui.ShowPlayerUI(idx);

        Debug.Log("player" + idx + " reactivated!");
    }

    public void DeactivatePlayer(int idx)
    {
        //need to disable and hide gameobject
        playerList[idx].playerScript.Deactivate();
        UnReadyPlayer(idx);

        playerList[idx].input.DeactivateInput();
        playerList[idx].isReady = false;
        playerList[idx].isAlive = false;

        //hide player UI
        ui.HidePlayerUI(idx);

        Debug.Log("player " + idx + " deactivated!");
    }


    //kills player - ONLY CALL THIS WHEN gameStarted == true
    public void KillPlayer(int idx)
    {
        DeactivatePlayer(idx);

        //if only one remaining player alive end/reset the game
        if (playerList.Count(p => p.isAlive) == 1)
        {
            ResetGame(playerList.FindIndex(p => p.isAlive));
        }
    }


    //declare winner and end/reset game
    void ResetGame(int winnerIdx)
    {
        foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Despawns"))
        {
            Destroy(obj);
        }

        //turn start menu on
        ui.ShowAll();
        camCon.StartCamOn();

        gameStarted = false;

        //reactivate() all isactive players
        //move players to spawn points
        foreach (PlayerConfig p in playerList)
        {
            if(p.isActive)
            {
                DeactivatePlayer(p.playerIndex);
                ReactivatePlayer(p.playerIndex);
            }

            Transform spawn = this.gameObject.transform.GetChild(p.playerIndex);
            p.input.gameObject.transform.position = spawn.position;
        }

        




    }

    void StartGame()
    {

        //move players to spawn positions
        foreach (PlayerConfig p in playerList.Where(p => p.isActive))
        {
            DeactivatePlayer(p.playerIndex);
            ReactivatePlayer(p.playerIndex);

            Transform start = this.gameObject.transform.GetChild(p.playerIndex + 4);
            p.input.gameObject.transform.position = start.position;

            p.isAlive = true;
        }

        camCon.GameCamOn();
        ui.HideAll();

        gameStarted = true;
        Debug.Log("game started!");

        StartCoroutine(itemMan.RandomSpawns(15));

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