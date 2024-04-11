using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MLArena : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    
    void OnTriggerExit2D(Collider2D col)
    {
        if(col.gameObject.layer == LayerMask.NameToLayer("Players"))
        {
            AgentController ac = col.GetComponent<AgentController>();

            ac.isInBounds = false;
            StartCoroutine(ac.PlayerKillClock(1.0f));

        }
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if(col.gameObject.layer == LayerMask.NameToLayer("Players"))
        {
            AgentController ac = col.GetComponent<AgentController>();
            
            ac.isInBounds = true;

        }
    }
}
