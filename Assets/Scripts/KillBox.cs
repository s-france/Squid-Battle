using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillBox : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        Debug.Log("KillBox collision!");
        //kill player on contact
        if(col.gameObject.layer == LayerMask.NameToLayer("Players"))
        {
            Debug.Log("Player entered KillBox!");

            //kill this player instance
            col.GetComponent<PlayerController>().KillPlayer();
            //check if all clones dead then do pm.killplayer
            col.GetComponent<PlayerController3>().CheckIfAlive();
        }

    }
}
