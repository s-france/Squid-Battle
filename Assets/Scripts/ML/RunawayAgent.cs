using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class RunawayAgent : MLAgent
{
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
        base.OnEpisodeBegin();

        StopAllCoroutines();
        StartCoroutine(SurviveRewards());
    }

    IEnumerator SurviveRewards()
    {
        while(ac.isInBounds)
        {
            AddReward(Time.deltaTime);
            yield return null;
        }

    }


    public override void KillAgent()
    {
        Debug.Log("-50 reward received");
        //targetTransform.GetComponent<MLAgent>().SetReward(1);
        AddReward(-50);
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
