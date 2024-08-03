using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InkBehavior : ItemBehavior
{
    GameObject InkPrefab;

    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();

        InkPrefab = im.ItemObjs[1]; //ink prefab in slot 1
    }


    public override string GetItemType()
    {
        return "Ink";
    }



     public override void UseItem(float chargeTime)
    {
        //MUST CHANGE SPAWN DISTANCE
      
        //Vector2 InkSpawn = transform.parent.position - 1.2f*transform.parent.up;

        GameObject ink = Instantiate(InkPrefab, pc.transform.Find("ItemSpawn").position, pc.transform.Find("ItemSpawn").rotation);
        ink.GetComponent<InkObj>().Deploy(chargeTime);
        DestroyItem();
    }

    public override void DestroyItem()
    {
        base.DestroyItem();
    }
}
