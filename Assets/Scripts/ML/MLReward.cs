using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MLReward : MonoBehaviour
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
        if (LayerMask.LayerToName(col.gameObject.layer) == "Players")
        {
            Debug.Log("+1 reward received!");
            
            MLAgent mla = col.GetComponent<MLAgent>();
            mla.SetReward(1);
            mla.EndEpisode();
        }
    }

}
