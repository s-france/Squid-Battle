using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework.Constraints;
//using UnityEditor.Callbacks;

//using Unity.MLAgents.SideChannels;
using UnityEngine;

public class RewindBehavior : ItemBehavior
{
    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();

        soundFX = "Item_Rewind";
    }


    public override string GetItemType()
    {
        return "Rewind";
    }


    public override void UseItem(float charge)
    {
        HideSprite();

        //play soundfx
        FindFirstObjectByType<AudioManager>().Play(soundFX);


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

        pc.SolidCol.enabled = false;


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
                pc.movePower = Mathf.Clamp(ps.movePower, pc.maxMovePower/5, 999);
                pc.movePriority = 3;

                if(ps.velocity.magnitude != 0)
                {
                    pc.RotateSprite(ps.velocity.normalized);

                }
                
                //should this really be inside here???
                timer += Time.fixedDeltaTime;

                
            }
            yield return fuwait;
        }

        pc.SolidCol.enabled = true;
        pc.isRewind = false;

        DestroyItem();
    }

}
