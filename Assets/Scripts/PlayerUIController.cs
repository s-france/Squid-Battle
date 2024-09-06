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
        if(ctx.performed /*&& !gm.battleStarted*/ && !pm.playerList[pc.idx].isActive && gm.lc.GetLevelType() == 1)
        {
            Debug.Log("Reconnect!!");
            pc.OnControllerReconnect(GetComponent<PlayerInput>());
            //^^this calls lc.OnPlayerReconnect
        }

    }


    public void OnConfirm(InputAction.CallbackContext ctx)
    {
        //readyUp if A pressed before game (on CSS)
        if(ctx.performed && gm.lc.GetLevelType() == 1 && !gm.battleStarted)
        {
            if(pm.playerList[pc.idx].isActive && !pm.playerList[pc.idx].isReady)
            {
                pm.ReadyPlayer(pc.idx);
                //pc.ReadyUp();
            } else if (pm.playerList.TrueForAll(p => p.isReady || !p.isActive) && pm.playerList.Count(p => p.isActive) > 1)
            {
                //load map select
                gm.sl.LoadScene("MatchSettings");
            }

        }

    }


    public void OnBack(InputAction.CallbackContext ctx)
    {
        //Debug.Log("Back action performed!!");

        Debug.Log("OnBack Called!");
        

        if(ctx.performed)
        {
            Debug.Log("Back Performed!");

            switch(gm.lc.GetLevelType())
            {
                case 1:
                    if(pc.pm.playerList[pc.idx].isActive && !gm.battleStarted)
                    {
                        if(pm.playerList[pc.idx].isReady)
                        {
                            //unReady player
                            pm.UnReadyPlayer(pc.idx);
                        } else //drop out if held for 1 secs before game
                        {
                            pc.OnControllerDisconnect(gameObject.GetComponent<PlayerInput>());
                            //(^^this calls lc.OnPlayerLeave)
                        }

                        
                    } else if(!pc.pm.playerList[pc.idx].isActive)
                    {
                        //go to previous screen (mode select)

                        //TRY destroy playermanager here
                        //GameObject.Destroy(pm.gameObject);
                        pm.im.pim.DisableJoining();

                        gm.sl.LoadScene("ModeSelect");

                    }

                    break;

                case 2:
                    //go to previous screen (main menu)
                    gm.sl.LoadScene("MainMenu");
                    break;

                case 3:
                    gm.lc.OnBack();
                    break;

                default:
                    break;


            }

        }
        

        

    }

    public void OnSelectL(InputAction.CallbackContext ctx)
    {
        if(ctx.performed)
        {
            Debug.Log("Player" + pc.idx + " pressed L!");
        }

        //change color if on character select and not ready
        if(ctx.performed && gm.lc.GetLevelType() == 1 && !pm.playerList[pc.idx].isReady) //change color only before match
        {
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
        if(ctx.performed && gm.lc.GetLevelType() == 1 && !pm.playerList[pc.idx].isReady) //change color only before match
        {
            //Debug.Log("updating colors from R input!");

            int color = pm.FindFirstAvailableColorID(pc.colorID + 1, 1);


            pc.ChangeColor(color);
        }

    }

}
