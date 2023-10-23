using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class ArenaBoundary : MonoBehaviour
{
    GameManager gm;
    InputManager im;
    PlayerManager pm;

    // Start is called before the first frame update
    void Start()
    {
        gm = GameObject.Find("GameManager").GetComponent<GameManager>();
        im = GameObject.Find("PlayerInputManager").GetComponent<InputManager>();
        pm = gm.GetComponentInChildren<PlayerManager>();  
    }

    void OnTriggerExit2D(Collider2D col)
    {
        if(col.gameObject.layer == LayerMask.NameToLayer("Players"))
        {
            pm.KillPlayer(col.GetComponentInParent<PlayerController>().idx);
        } else if (col.gameObject.layer == LayerMask.NameToLayer("ItemObjs"))
        {
            StartCoroutine(KillClock(col.gameObject, 2));
        }

    }

    IEnumerator KillClock(GameObject obj, float timer)
    {
        yield return new WaitForSeconds(timer);
        Destroy(obj);
    }

}
