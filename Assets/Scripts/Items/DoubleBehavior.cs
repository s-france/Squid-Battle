using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public class DoubleBehavior : ItemBehavior
{
    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();
    }

    public override string GetItemType()
    {
        return "Double";
    }


    public override void UseItem(float chargeTime)
    {
        //spawn double
        CreateDouble(); 
       

        
        DestroyItem();
    }

    void CreateDouble()
    {
        //spawn dummy
        DummyPlayerController Dummy = Instantiate(pc.DummyPrefab).GetComponent<DummyPlayerController>();

        //add dummy to original player's clone list
        //find original parent pc
        PlayerController original = pc;
        while (original.isDummy)
        {
            original = ((DummyPlayerController)original).parent;
        }
        //add to clone list
        original.Clones.Add(Dummy);

        //set position
        Dummy.transform.position = pc.transform.position;

        //set parent
        Dummy.parent = original;
        //Init
        Dummy.Init();
        
        //add invol frames to prevent instant collision
        Dummy.SetPeerPriority(Dummy.IntangiblePeerPrioTable, pc, 5*Time.fixedDeltaTime);
        pc.SetPeerPriority(pc.IntangiblePeerPrioTable, Dummy, 5*Time.fixedDeltaTime);




        //add to target group (done in Reactivate())
        //activate pc
        Dummy.Reactivate();

        //split direction
        pc.ApplyMove(0, pc.i_move, .25f * Mathf.Clamp(pc.specialChargeTime, pc.minCharge, pc.maxChargeTime));
        Dummy.ApplyMove(0, -pc.i_move, .25f * Mathf.Clamp(pc.specialChargeTime, pc.minCharge, pc.maxChargeTime));

    }


    
    public override void DestroyItem()
    {
        Debug.Log("Double over");
        base.DestroyItem();
    }

}
