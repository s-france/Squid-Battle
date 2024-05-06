using System.Collections;
using System.Collections.Generic;
//using System.Numerics;
using UnityEngine;
using UnityEngine.InputSystem;

public class DummyController : AgentController
{
    public Transform agentTransform;

    // Start is called before the first frame update
    void Start()
    {
        i_move = Vector2.zero;
        
    }

    public override void OnMove(InputAction.CallbackContext ctx)
    {
        return;
    }
    public override void OnMove(Vector2 Vmove)
    {
        return;
    }

    public override void OnCharge(InputAction.CallbackContext ctx)
    {
        return;
    }
    public override void OnCharge(int Icharge)
    {
        return;
    }



    public override IEnumerator PlayerKillClock(float timer)
    {
        float clock = 0;
        
        while(clock < timer && !isInBounds)
        {
            //add dying animation here
            
            
            clock += Time.deltaTime;
            yield return null;
        }

        if(!isInBounds)
        {
            KillDummy();
        }

        

    }

    public void KillDummy()
    {
        Debug.Log("+200 reward received");
        agentTransform.GetComponent<MLAgent>().AddReward(200);
        agentTransform.GetComponent<MLAgent>().EndEpisode();
    }

     public override IEnumerator ApplyKnockback(float otherPower, Rigidbody2D otherRB)
     {
        return base.ApplyKnockback(otherPower, otherRB);
     }



}
