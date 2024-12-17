using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework.Constraints;
using UnityEditor.Callbacks;

//using Unity.MLAgents.SideChannels;
using UnityEngine;

public class RewindBehavior : ItemBehavior
{
    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();
    }


    public override string GetItemType()
    {
        return "Rewind";
    }


    public override void UseItem(float charge)
    {
        HideSprite();
        StartCoroutine(Rewind(charge));
        
    }

    public override void DestroyItem()
    {
        base.DestroyItem();
    }

    //rewinds the player back in time
    IEnumerator Rewind(float charge)
    {
        WaitForFixedUpdate fuwait = new WaitForFixedUpdate();
        Vector2 pos = Vector2.zero;
        PlayerState ps;
        pc.isRewind = true;


        float rewindTime = (charge/pc.maxChargeTime) * Time.fixedDeltaTime * pc.rewindSize;

        //convert queue to stack
        Stack<PlayerState> states = new Stack<PlayerState>(pc.prevStates.ToArray());

        float timer = 0;

        while (timer < rewindTime && states.Count > 0)
        {
            if(!pc.isHitStop)
            {
                ps = states.Pop();
                pos.x = ps.xPos;
                pos.y = ps.yPos;

                //pc.transform.position = pos;
                pc.rb.MovePosition(pos);
                pc.storedVelocity = -ps.velocity;
                pc.rb.velocity = -ps.velocity;
                pc.movePower = ps.movePower;
                pc.movePriority = 3;

                if(ps.velocity.magnitude != 0)
                {
                    pc.RotateSprite(ps.velocity.normalized);
                }

                //TRY THIS: move timer tick into above if statement^^
                timer += Time.fixedDeltaTime;
            }
            yield return fuwait;
        }

        pc.isRewind = false;

        DestroyItem();
    }

}
