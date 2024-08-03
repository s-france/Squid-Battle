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
        GameObject shot = Instantiate(ShotPrefab, pc.transform.Find("ItemSpawn").position, pc.transform.Find("ItemSpawn").rotation);
        shot.GetComponent<ShotObj>().Shoot(chargeTime);
        DestroyItem();
    }

    public override void DestroyItem()
    {
        base.DestroyItem();
    }

}
