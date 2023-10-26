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

    float activeTimer;
    float upTime;

    // Start is called before the first frame update
    void Start()
    {
        gm = GameObject.Find("GameManager").GetComponent<GameManager>();

        activeTimer = 0;
        upTime = maxUpTime;
    }

    // Update is called once per frame
    void Update()
    {
        if(!pc.isCoolingDown)
        {
            tr.emitting = false;
        }

        if(tr.positionCount == 0)
        {
            tr.emitting = false;
            Destroy(gameObject);
        }

        SetColliderPointsFromTrail(tr, col);
    }


    public void Deploy(float chargetime, PlayerController pc)
    {
        //TWEAK THIS
        //upTime = Mathf.Clamp(chargetime * 10, 13, 25);
        tr = pc.GetComponentInChildren<TrailRenderer>();
        tr.emitting = true;

        while(pc.isCoolingDown)
        {
            
        }

        tr.emitting = false;
    }


    void SetColliderPointsFromTrail(TrailRenderer tr, EdgeCollider2D col)
    {
        List<Vector2> points = new List<Vector2>();
        Vector3[] positions;

        for(int pos = 0; pos < tr.positionCount; pos++)
        {
            points.Add(tr.GetPosition(pos));
            
        }
        //tr.GetVisiblePositions(positions)

        col.SetPoints(points);
    }
}
