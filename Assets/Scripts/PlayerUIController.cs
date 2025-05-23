using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;
using System.Runtime.InteropServices;


public class PlayerUIController : MonoBehaviour
{
    [SerializeField] PlayerController pc;

    GameManager gm;
    PlayerManager pm;


    // Start is called before the first frame update
    void Start()
    {
        gm = GameObject.Find("GameManager").GetComponent<GameManager>();
        pm = gm.GetComponentInChildren<PlayerManager>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    
    public void OnJoin(InputAction.CallbackContext ctx)
    {
        if(ctx.performed /*&& !gm.battleStarted*/ && !pm.PlayerList[pc.idx].isActive && gm.lc.GetLevelType() == 1)
        {
            Debug.Log("Reconnect!!");
            pc.OnControllerReconnect(GetComponent<PlayerInput>());
            //^^this calls lc.OnPlayerReconnect
        }

    }


    public void OnConfirm(InputAction.CallbackContext ctx)
    {
        //readyUp if A pressed before game (on CSS)
        if(ctx.performed && gm.lc.GetLevelType() == 1)
        {
            if(pm.PlayerList[pc.idx].isActive && !pm.PlayerList[pc.idx].isReady)
            {
                FindFirstObjectByType<AudioManager>().Play("UINav1");

                pm.ReadyPlayer(pc.idx);
                //pc.ReadyUp();
            } else if (pm.PlayerList.TrueForAll(p => p.isReady || !p.isActive) && pm.PlayerList.Count(p => p.isActive) > 1)
            {
                //load map select

                if(gm.gameMode == 0 || gm.gameMode == 1)
                {
                    gm.sl.LoadScene("MatchSettings");

                } else if(gm.gameMode == 2)
                {
                    gm.sl.LoadScene("SoccerSettings");
                }

            }

        } else
        {
            gm.lc.OnConfirm(pc.idx, ctx);
        }

        

    }


    public void OnBack(InputAction.CallbackContext ctx)
    {
        //Debug.Log("Back action performed!!");

        Debug.Log("OnBack Called!");

        gm.lc.OnBack(pc.idx, ctx);        

    }

    public void OnSelectL(InputAction.CallbackContext ctx)
    {
        if(ctx.performed)
        {
            Debug.Log("Player" + pc.idx + " pressed L!");
        }

        //change color if on character select and not ready
        if(ctx.performed && gm.lc.GetLevelType() == 1 && !pm.PlayerList[pc.idx].isReady) //change color only before match
        {
            FindFirstObjectByType<AudioManager>().Play("UINav3");

            //Debug.Log("updating colors from L input!");

            int color = pm.FindFirstAvailableColorID(pc.colorID - 1, -1);

            //old stuff
            //colorID = (colorID - 1);
            //if(colorID < 0) {colorID = pm.colorsCount;}

            pc.ChangeColor(color);
        }

    }

    public void OnSelectR(InputAction.CallbackContext ctx)
    {
        //Debug.Log("R Pressed!");
        if(ctx.performed)
        {
            Debug.Log("Player" + pc.idx + " pressed R!");
        }

        //change color if on character select and not ready
        if(ctx.performed && gm.lc.GetLevelType() == 1 && !pm.PlayerList[pc.idx].isReady) //change color only before match
        {
            FindFirstObjectByType<AudioManager>().Play("UINav3");

            //Debug.Log("updating colors from R input!");

            int color = pm.FindFirstAvailableColorID(pc.colorID + 1, 1);


            pc.ChangeColor(color);
        }

    }


    public void OnSelectZL(InputAction.CallbackContext ctx)
    {
        //ZL pressed
        if(ctx.performed)
        {
            Debug.Log("Player" + pc.idx + " pressed ZL!");
        }

        //change teams
        //if on Teams CSS and not ready
        if(ctx.performed && (gm.gameMode == 1 || gm.gameMode == 2) && gm.lc.GetLevelType() == 1 && !pm.PlayerList[pc.idx].isReady)
        {
            FindFirstObjectByType<AudioManager>().Play("UINav3");

            //change team
            int newTeam = pm.FindFirstAvailableTeam(pm.PlayerList[pc.idx].team - 1, -1);

            pm.SetPlayerTeam(pc.idx, newTeam);
        }
    }

    public void OnSelectZR(InputAction.CallbackContext ctx)
    {
        //ZR pressed
        if(ctx.performed)
        {
            Debug.Log("Player" + pc.idx + " pressed ZR!");
        }

        //change teams
        //if on Teams CSS and not ready
        if(ctx.performed && gm.gameMode == 1 && gm.lc.GetLevelType() == 1 && !pm.PlayerList[pc.idx].isReady)
        {
            FindFirstObjectByType<AudioManager>().Play("UINav3");

            //change team
            int newTeam = pm.FindFirstAvailableTeam(pm.PlayerList[pc.idx].team + 1, 1);

            pm.SetPlayerTeam(pc.idx, newTeam);
        }
        
    }

}
