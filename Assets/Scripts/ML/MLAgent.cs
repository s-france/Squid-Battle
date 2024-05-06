using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
//using UnityEditorInternal;
using System.Net.Sockets;
//using System.Numerics;
//using System.Numerics;

public class MLAgent : Agent
{
    [SerializeField] public AgentController ac;

    [SerializeField] public Transform targetTransform;
    [SerializeField] public Vector3 startPos;
    //Vector3 rewardPos;

    AgentController otherAC;



    // Start is called before the first frame update
    void Start()
    {
        otherAC = targetTransform.GetComponent<AgentController>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void FixedUpdate()
    {
        
        if(ac.charging)
        {
            //incentivize charging
            AddReward(0.01f + (Mathf.Clamp(ac.chargeTime/ac.maxChargeTime, 0, 1) * .075f));

        } else
        {
            AddReward(-0.01f);
        }

        //punish going out of bounds
        if(!ac.isInBounds)
        {
            AddReward(-.2f);
        }

        if(!otherAC.isInBounds)
        {
            AddReward(.2f);
        }
        

        //incentivize shorter episodes -> /aggressive play
        //AddReward(-0.000125f);
    }

    public override void OnEpisodeBegin()
    {
        //enable random positions
        startPos = Random.insideUnitCircle * 6.5f;

        //set start position
        ac.isInBounds = true;
        transform.localPosition = startPos;
        //UNCOMMENT FOR DUMMY TESTS
        targetTransform.localPosition = Random.insideUnitCircle *6.5f;

        ac.i_charge = 0;
        ac.chargeTime = 0;

        ac.StopAllCoroutines();
        ac.rc.DeactivateReticle();
        ac.isMoving = false;
        ac.isKnockback = false;
        ac.rb.velocity = Vector2.zero;

        //UNCOMMENT FOR DUMMY TESTS
        targetTransform.GetComponent<AgentController>().isMoving = false;
        targetTransform.GetComponent<AgentController>().isKnockback = false;
        targetTransform.GetComponent<AgentController>().rb.velocity = Vector2.zero;
    }


    public virtual void KillAgent()
    {
        Debug.Log("player" + ac.idx + " losses! -200 reward");
        //targetTransform.GetComponent<MLAgent>().AddReward(2);
        AddReward(-200);

        //targetTransform.GetComponent<MLAgent>().EndEpisode();
        EndEpisode();
    }


    public override void CollectObservations(VectorSensor sensor)
    {
        //agent's position
        sensor.AddObservation((Vector2)transform.localPosition);
        //opponent's position
        sensor.AddObservation((Vector2)targetTransform.localPosition);
        
        //agent's current "moveVector" = velocity direction * movePower
        sensor.AddObservation((Vector2)(ac.rb.velocity.normalized * ac.movePower));
        //opponent's moveVector
        sensor.AddObservation((Vector2)(otherAC.rb.velocity.normalized * otherAC.movePower));

        //agent's moveTimer = remaining time to spend moving
        sensor.AddObservation(ac.moveTimer);
        //opponent's moveTime
        sensor.AddObservation(otherAC.moveTimer);

        
        //agent's charge level
        sensor.AddObservation(ac.chargeTime / ac.maxChargeTime);
        //opponent's charge level
        sensor.AddObservation(otherAC.chargeTime / otherAC.maxChargeTime);

        //agent's chargeTime
        sensor.AddObservation(ac.chargeTime);
        //opponent's chargeTime
        sensor.AddObservation(otherAC.chargeTime);

        //add charging bool?
        sensor.AddObservation(ac.charging);
        //opponent's charging
        sensor.AddObservation(otherAC.charging);

        //add agents' coolDown status??
        sensor.AddObservation(ac.isCoolingDown);
        //opponent's coolDown
        sensor.AddObservation(otherAC.isCoolingDown);


        //add isMoving?
        sensor.AddObservation(ac.isMoving);
        //opponent's isMoving
        sensor.AddObservation(otherAC.isMoving);

        //add isKnockback?
        sensor.AddObservation(ac.isKnockback);
        //opponent isKnockback
        sensor.AddObservation(otherAC.isKnockback);
        
        //add isInBounds?
        sensor.AddObservation(ac.isInBounds);
        //opponent isinBounds
        sensor.AddObservation(otherAC.isInBounds);

        //add trajectory - predicted moveTo position based on charge/aim
        sensor.AddObservation((Vector2)transform.localPosition + (ac.CalcMoveForce(0) * .575f));
        //Debug.Log("trajectory: " + ((Vector2)transform.localPosition + (ac.CalcMoveForce(0) * .575f)));


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

    //called in ac.ApplyKnockback
    public void KnockbackReward(float strengthDiff)
    {
        //Debug.Log("strengthDiff: " + strengthDiff);

        //(max strengthDiff = 2.75)
        AddReward(strengthDiff * 8);;
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
