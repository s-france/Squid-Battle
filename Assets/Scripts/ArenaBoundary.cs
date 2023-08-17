using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArenaBoundary : MonoBehaviour
{
    InputManager im;

    // Start is called before the first frame update
    void Start()
    {
        im = GameObject.Find("PlayerInputManager").GetComponent<InputManager>();   
    }

    void OnTriggerExit2D(Collider2D col)
    {
        if(col.gameObject.layer == LayerMask.NameToLayer("Players"))
        {
            im.KillPlayer(col.GetComponentInParent<PlayerController>().idx);
        }
    }

}
