using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class ArenaBoundary : MonoBehaviour
{
    GameManager gm;
    InputManager im;
    PlayerManager pm;


    //List<int> PlayerCollisions; //rework this to List<PlayerController>
    List<PlayerController> PlayerCollisions;

    bool started;

    // Start is called before the first frame update
    void Start()
    {
        gm = GameObject.Find("GameManager").GetComponent<GameManager>();
        im = GameObject.Find("PlayerInputManager").GetComponent<InputManager>();
        pm = gm.GetComponentInChildren<PlayerManager>();

        PlayerCollisions = new List<PlayerController>();

        StartCoroutine(DelayStart());
    }

    IEnumerator DelayStart()
    {
        int delay = 0;
        WaitForFixedUpdate fuWait = new WaitForFixedUpdate();
        
        while(delay < 4)
        {
            delay++;
            yield return fuWait;
        }

        started = true;
    }

    void OnTriggerExit2D(Collider2D col)
    {
        Debug.Log("Arena TriggerExit!: " + col.gameObject.name);

        if(started)
        {
            if(col.gameObject.layer == LayerMask.NameToLayer("Players"))
            {
                PlayerController player = col.GetComponentInParent<PlayerController>();
                int idx = player.idx;

                //remove one instance of player idx from collision tracking
                PlayerCollisions.Remove(player);

                //if player is not inside any arena colliders
                if(!PlayerCollisions.Contains(player))
                {
                    //set player out of bounds
                    //pm.playerList[idx].isInBounds = false;
                    //pm.playerList[idx].playerScript.isInBounds = false;
                    player.isInBounds = false;

                    //band aid fix for grow stats
                    if(!player.isGrown)
                    {
                        //alter player's stats offstage
                        player.maxMoveSpeed *= .5f;
                        player.maxMoveTime *= 1.5f;
                        player.maxMovePower *= .6f;
                        player.maxChargeTime *= 1.2f;
                    }
                    
                    //StartCoroutine(pm.PlayerKillClock(idx, 3.0f));
                }

            } else if (col.gameObject.layer == LayerMask.NameToLayer("ItemObjs"))
            {
                StartCoroutine(KillClock(col.gameObject, 2));
            }

        }
        
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        Debug.Log("Arena TriggerEnter!: " + col.gameObject.name);

        if(col.gameObject.layer == LayerMask.NameToLayer("Players"))
        {
            PlayerController player = col.GetComponentInParent<PlayerController>();
            int idx = player.idx;
            //PlayerCollisions.Add(idx);
            PlayerCollisions.Add(player);
            //pm.playerList[idx].isInBounds = true;
            //pm.playerList[idx].playerScript.isInBounds = true;
            player.isInBounds = true;

            //restore player's default stats
            //pm.playerList[idx].playerScript.ResetDefaultStats();
            
            //band aid fix for grow stats
            if(!player.isGrown)
            {
                player.maxMoveSpeed = player.defaultMaxMoveSpeed;
                player.maxMoveTime = player.defaultMaxMoveTime;
                player.maxMovePower = player.defaultMaxMovePower;
                player.maxChargeTime = player.defaultMaxChargeTime;
            }

            
        }

    }

    IEnumerator KillClock(GameObject obj, float timer)
    {
        yield return new WaitForSeconds(timer);
        Destroy(obj);
    }


}
