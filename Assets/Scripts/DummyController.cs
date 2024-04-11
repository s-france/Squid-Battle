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
        Debug.Log("+2 reward received");
        agentTransform.GetComponent<MLAgent>().SetReward(2);
        agentTransform.GetComponent<MLAgent>().EndEpisode();
    }



}
