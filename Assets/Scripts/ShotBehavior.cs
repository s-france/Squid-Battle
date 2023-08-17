using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ShotBehavior : MonoBehaviour, IItemBehavior
{
    ItemManager im;
    GameObject ShotPrefab;
    
    // Start is called before the first frame update
    void Start()
    {
        im = GameObject.Find("ItemManager").GetComponent<ItemManager>();
        ShotPrefab = im.ItemObjs[0];


        Debug.Log("ShotBehavior added!");
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void UseItem(float chargeTime)
    {
        GameObject shot = Instantiate(ShotPrefab, transform.position, transform.rotation);
        shot.GetComponent<ShotObj>().Shoot(chargeTime);
        DestroyItem();
    }

    public void DestroyItem()
    {
        GameObject.Destroy(gameObject);
    }

}
