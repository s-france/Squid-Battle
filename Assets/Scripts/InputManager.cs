using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class InputManager : MonoBehaviour
{
    CamController camCon;
    //UIController ui;
    ItemManager itemMan;
    PlayerManager pm;
    GameManager gm;
    [SerializeField] public PlayerInputManager pim;


    private void Awake()
    {
        
        
    }

    public void Init()
    {
        //REWORK
        //camCon = GameObject.Find("VCams").GetComponent<CamController>();
        //itemMan = GameObject.Find("ItemManager").GetComponent<ItemManager>();
        pm = GetComponentInParent<PlayerManager>();
        gm = GetComponentInParent<GameManager>();
        pim = GetComponent<PlayerInputManager>();

        //ui.HideAllPlayers();
    }


    //handle player joining here
    public void OnPlayerJoin(PlayerInput pi)
    {
        Debug.Log("Player joined!");
        Debug.Log("Index: " + pi.playerIndex);
        
        //make sure player index is not already added (not that that should ever happen)
        if(!pm.playerList.Any(p => p.playerIndex == pi.playerIndex))
        {
            pm.playerList.Add(new PlayerConfig(pi));

            //move player to correct spawn location (based on idx)
            Transform spawn = gm.lc.GetSpawnPoints()[pi.playerIndex];
            pi.gameObject.transform.position = spawn.position;

            //must initialize heldItems list before calling DeactivatePlayer()
            pm.playerList[pi.playerIndex].playerScript.Init();

            pm.DeactivatePlayer(pi.playerIndex);
            
            //only show new player if on Start Menu
            /*
            if(SceneManager.GetActiveScene().name == "StartMenu")
            {
                pm.ReactivatePlayer(pi.playerIndex);
            }
            */

            gm.lc.OnPlayerJoin(pi.playerIndex);
        
        }
    }

    //handle player leaving/disconnecting
    public void OnPlayerLeave(PlayerInput pi)
    {
        pm.playerList[pi.playerIndex].isActive = false;
        pm.DeactivatePlayer(pi.playerIndex);
        Debug.Log("P" + pi.playerIndex + " left!");
    }


    //ran when disconnected player controller reconnects
    public void OnPlayerReconnect(PlayerInput pi)
    {
        pm.playerList[pi.playerIndex].isActive = true;
        //REWORK: use OnStartMenu - not battlestarted
        if (gm.lc.GetLevelType() == 1)
        {
            pm.ReactivatePlayer(pi.playerIndex);
        }
        Debug.Log("player " + pi.playerIndex + " reconnected!");
    }


    void Update()
    {
        
    }

}

    


