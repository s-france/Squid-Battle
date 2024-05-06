using System.Collections;
using System.Collections.Generic;
//using System.Numerics;
using Unity.VisualScripting;
using UnityEngine;

public class ShockWave : MonoBehaviour
{
    [SerializeField] float shockPower;
    [SerializeField] float hitstopRatio;

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
            //Debug.Log("Player" + idx + " collided with Player " + col.gameObject.GetComponent<PlayerController>().idx);
            PlayerController pc = col.gameObject.GetComponent<PlayerController>();

            Vector2 direction = (col.transform.position - transform.position).normalized;

            //FINISH THIS:  NEED TO UPDATE pc.ApplyKnockback for only one player rb
            pc.StartCoroutine(pc.HitStop(pc.maxHitstop * hitstopRatio));
            pc.ApplyMove(1, direction, shockPower);

        }
    }

}
