using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ShotBehavior : ItemBehavior
{

    GameObject ShotPrefab;
    
    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();

        ShotPrefab = im.ItemObjs[0];
        Debug.Log("ShotBehavior added!");
    }


    public override string GetItemType()
    {
        return "Shot";
    }
    

    public override void UseItem(float chargeTime)
    {
        GameObject shot = Instantiate(ShotPrefab, pc.transform.position, pc.transform.rotation);
        shot.GetComponent<ShotObj>().parentID = pc.idx;
        shot.GetComponent<ShotObj>().Shoot(chargeTime);

        //pc.ApplyMove(0,);
        pc.ApplyMove(0, -pc.i_move, pc.specialMoveMod * Mathf.Clamp(pc.specialChargeTime, pc.minCharge, pc.maxChargeTime));

        DestroyItem();
    }

    public override void DestroyItem()
    {
        base.DestroyItem();
    }

}
