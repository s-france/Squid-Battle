using System.Collections;
using System.Collections.Generic;
using UnityEditor.U2D.Path;
using UnityEngine;

public class WallBehavior : MonoBehaviour, IItemBehavior
{
    ItemManager im;
    GameObject WallPrefab;

    // Start is called before the first frame update
    void Start()
    {
        im = GameObject.Find("ItemManager").GetComponent<ItemManager>();
        WallPrefab = im.ItemObjs[2]; //wall prefab in slot 2

    }


    // Update is called once per frame
    void Update()
    {
        
    }

    public string GetItemType()
    {
        return "Wall";
    }


    public void UseItem(float charge)
    {
        GameObject Wall = Instantiate(WallPrefab, Vector3.zero, Quaternion.identity);
        Wall.GetComponent<WallObj>().pc = GetComponentInParent<PlayerController>();
        Wall.GetComponent<WallObj>().tr = GetComponentInParent<PlayerController>().GetComponentInChildren<TrailRenderer>();
        Wall.GetComponent<WallObj>().tr.emitting = true;

        DestroyItem();
    }


    
    public void DestroyItem()
    {
        GameObject.Destroy(gameObject);
    }



}
