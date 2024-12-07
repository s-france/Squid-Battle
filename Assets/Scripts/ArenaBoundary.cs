using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class ArenaBoundary : MonoBehaviour
{
    GameManager gm;
    InputManager im;
    PlayerManager pm;

    bool started;

    // Start is called before the first frame update
    void Start()
    {
        gm = GameObject.Find("GameManager").GetComponent<GameManager>();
        im = GameObject.Find("PlayerInputManager").GetComponent<InputManager>();
        pm = gm.GetComponentInChildren<PlayerManager>();

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
                int idx = col.GetComponentInParent<PlayerController>().idx;

                pm.playerList[idx].isInBounds = false;
                pm.playerList[idx].playerScript.isInBounds = false;

                StartCoroutine(pm.PlayerKillClock(idx, 3.0f));

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
            int idx = col.GetComponentInParent<PlayerController>().idx;
            pm.playerList[idx].isInBounds = true;
            pm.playerList[idx].playerScript.isInBounds = true;
        }

    }

    IEnumerator KillClock(GameObject obj, float timer)
    {
        yield return new WaitForSeconds(timer);
        Destroy(obj);
    }


}
