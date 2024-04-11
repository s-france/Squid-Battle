using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEditorInternal;
//using System.Numerics;

public class MLAgent : Agent
{
    [SerializeField] public AgentController ac;

    [SerializeField] public Transform targetTransform;
    [SerializeField] public Vector3 startPos;
    //Vector3 rewardPos;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void OnEpisodeBegin()
    {
        //enable random positions
        startPos = Random.insideUnitCircle * 6.5f;

        //set start position
        ac.isInBounds = true;
        transform.localPosition = startPos;
        targetTransform.localPosition = Random.insideUnitCircle *6.5f;

        ac.StopAllCoroutines();
        ac.rc.DeactivateReticle();
        ac.isMoving = false;
        ac.isKnockback = false;
        ac.rb.velocity = Vector2.zero;

        targetTransform.GetComponent<AgentController>().isMoving = false;
        targetTransform.GetComponent<AgentController>().isKnockback = false;
        targetTransform.GetComponent<AgentController>().rb.velocity = Vector2.zero;
    }


    public virtual void KillAgent()
    {
        Debug.Log("-1 reward received");
        //targetTransform.GetComponent<MLAgent>().SetReward(1);
        SetReward(-1);
        EndEpisode();
    }


    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation((Vector2)transform.localPosition);
        sensor.AddObservation((Vector2)targetTransform.localPosition);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float moveX = actions.ContinuousActions[0];
        float moveY = actions.ContinuousActions[1];

        int isCharging = actions.DiscreteActions[0];

        Vector2 Vmove = new Vector2(moveX, moveY);
        ac.OnMove(Vmove);

        ac.OnCharge(isCharging);

        //Debug.Log("moveX: " + moveX);
        //Debug.Log("moveY: " + moveY);
        //Debug.Log("isCharging: " + isCharging);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> continuousActions = actionsOut.ContinuousActions;
        continuousActions[0] = ac.v_move.x;
        continuousActions[1] = ac.v_move.y;

        ActionSegment<int> discreteActions = actionsOut.DiscreteActions;
        discreteActions[0] = ac.i_charge;
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        //contact reward:
        /*
        if (LayerMask.LayerToName(col.gameObject.layer) == "Players")
        {
            Debug.Log("+1 reward received!");
            
            SetReward(1);
            EndEpisode();
        }
        */
    }

}
