using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InkBehavior : MonoBehaviour, IItemBehavior
{
    ItemManager im;
    GameObject InkPrefab;

    // Start is called before the first frame update
    void Start()
    {
        im = GameObject.Find("ItemManager").GetComponent<ItemManager>();
        InkPrefab = im.ItemObjs[1]; //ink prefab in slot 1

    }

    // Update is called once per frame
    void Update()
    {
        
    }


     public void UseItem(float chargeTime)
    {
        //MUST CHANGE SPAWN DISTANCE
        Vector2 InkSpawn = transform.parent.position - 1.2f*transform.parent.up;

        GameObject ink = Instantiate(InkPrefab, InkSpawn, transform.parent.rotation);
        ink.GetComponent<InkObj>().Deploy(chargeTime);
        DestroyItem();
    }

    public void DestroyItem()
    {
        GameObject.Destroy(gameObject);
    }
}
