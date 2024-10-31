using System.Collections;
using System.Collections.Generic;
//using UnityEditor.U2D.Path;
using UnityEngine;

public class WallBehavior : ItemBehavior
{
    GameObject WallPrefab;

    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();

        WallPrefab = im.ItemObjs[2]; //wall prefab in slot 2
    }


    public override string GetItemType()
    {
        return "Wall";
    }


    public override void UseItem(float charge)
    {
        GameObject Wall = Instantiate(WallPrefab, Vector3.zero, Quaternion.identity);
        Wall.GetComponent<WallObj>().pc = pc;
        Wall.GetComponent<WallObj>().tr = pc.GetComponentInChildren<TrailRenderer>();
        Wall.GetComponent<WallObj>().tr.emitting = true;

        DestroyItem();
    }


    
    public override void DestroyItem()
    {
        base.DestroyItem();
    }



}
