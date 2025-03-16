using System.Collections;
using System.Collections.Generic;
//using System.Numerics;
using Unity.VisualScripting;
using UnityEngine;

public class ShockWave : MonoBehaviour
{
    [SerializeField] CircleCollider2D shockWaveCollider;
    [SerializeField] float shockPower;
    [SerializeField] float hitstopRatio;

    int ownerID = -1;

    // Start is called before the first frame update
    void Start()
    {
        ownerID = GetComponentInParent<PlayerController>().idx;
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //warp hitbox: radius1, pos
    //player colliding hitbox: radius2, otherpos, otherprev
    Vector2 EstimatePlayerTriggerCollision(float radius1, float radius2, Vector2 pos, Vector2 otherPos, Vector2 otherPrev)
    {
        //exit condition
        if(((radius1 + radius2)+.001f >= (pos-otherPrev).magnitude) && ((pos-otherPrev).magnitude >= (radius1 + radius2)-.001f))
        {
            return otherPrev;
        }


        //recursive divide + conquer loop
        //Vector2 dist = pos - prev; //dist points toward pos
        Vector2 otherDist = otherPos - otherPrev;

        //Vector2 newPrev;
        //Vector2 otherNewPrev;

        //plus 1/2 or -1/2 for closer or farther
        if((pos-otherPrev).magnitude > radius1 + radius2)
        {
            //prev += (.5f * dist);
            otherPrev += (.5f * otherDist);
        }else if((pos-otherPrev).magnitude < radius1 + radius2)
        {
            //prev -= (.5f * dist);
            otherPrev -= (.5f * otherDist);
        }

        //repeat loop
        return EstimatePlayerTriggerCollision(radius1, radius2, pos, otherPos, otherPrev);
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (LayerMask.LayerToName(col.gameObject.layer) == "Players")
        {
            //Debug.Log("Player" + idx + " collided with Player " + col.gameObject.GetComponent<PlayerController>().idx);
            PlayerController pc = col.gameObject.GetComponentInParent<PlayerController>();

            //trigger collision correction
            //col.transform.root.position = EstimatePlayerTriggerCollision(shockWaveCollider.radius, ((CircleCollider2D)col).radius, transform.position, col.transform.root.position, pc.prevPos[1]);


            Vector2 direction = (col.transform.root.position - transform.position).normalized;

            //FINISH THIS:  NEED TO UPDATE pc.ApplyKnockback for only one player rb
            pc.StartCoroutine(pc.HitStop(pc.maxHitstop * hitstopRatio));
            pc.ApplyMove(1, direction, shockPower);

            //assign killcredit
            pc.killCredit = ownerID;
            pc.killCreditTimer = 0;
        }
    }

}
