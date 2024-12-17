using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallObj : MonoBehaviour
{
    [SerializeField] EdgeCollider2D col;

    GameManager gm;
    [HideInInspector] public TrailRenderer tr;
    [HideInInspector] public PlayerController pc; //set by WallBehavior script

    [SerializeField] float maxUpTime;

    Vector3[] positions = new Vector3[100];

    float activeTimer;
    float upTime;

    // Start is called before the first frame update
    void Start()
    {
        gm = GameObject.Find("GameManager").GetComponent<GameManager>();

        activeTimer = 0;
        upTime = maxUpTime;
    }


    void Update()
    {
        if(!pc.isMoving || (pc.moveTimer/pc.moveTime) > .9)
        {
            tr.emitting = false;
            if(tr.GetVisiblePositions(positions) == 0)
            {
                tr.emitting = false;
                Destroy(gameObject);
            }
        }

        SetColliderPointsFromTrail(tr, col);
    }

    void FixedUpdate()
    {
        



    }


    public void Deploy(float chargetime, PlayerController pc)
    {
        //TWEAK THIS
        //upTime = Mathf.Clamp(chargetime * 10, 13, 25);
        tr = pc.GetComponentInChildren<TrailRenderer>();
        tr.emitting = true;

        while(pc.isCoolingDown)
        {
            if(pc.GetComponent<Collider2D>().IsTouchingLayers(LayerMask.GetMask("Players")))
            {
                Debug.Log("COLLISISON -> CANCELLING TRAILRENDER");
                tr.emitting = false;
                break;
            }
        }

        tr.emitting = false;
    }


    void SetColliderPointsFromTrail(TrailRenderer tr, EdgeCollider2D col)
    {
        List<Vector2> points = new List<Vector2>();

        for(int pos = 0; pos < tr.GetVisiblePositions(positions); pos++)
        {
            points.Add(positions[pos]);
        }
        //tr.GetVisiblePositions(positions)

        col.SetPoints(points);
    }
}
