using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemManager : MonoBehaviour
{
    public GameObject ItemPrefab;
    public List<GameObject> ItemObjs;

    public int itemTypesCount;
    [HideInInspector] public int spawnedItemsCount;
    [HideInInspector] public int[] spawnedItems;

    // Start is called before the first frame update
    void Start()
    {
        SpawnItem(0, this.transform);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //spawns (creates) an item of type at position pos
    void SpawnItem(int type, Transform pos)
    {
        Debug.Log("spawning item");
        //instantiate new ItemPrefab
        GameObject newItem = Instantiate(ItemPrefab, pos.position, pos.rotation);
        //set item type
        //MUST ADD OTHER ITEM TYPES BASED ON int type
        newItem.GetComponent<ItemController>().ib = newItem.AddComponent<ShotBehavior>() as ShotBehavior;
        
        Debug.Log("new behavior assigned!");

        spawnedItemsCount++;
        //spawnedItems[spawnedItemsCount] = 


    }

}


public class ItemConfig
{
    public ItemConfig(IItemBehavior item, int idx, int spawn)
    {
        itemObj = item;
        playerIndex = -1;
        isActive = true;
    }

    IItemBehavior itemObj {get; set;}
    public int itemType {get; set;}
    public int itemIndex {get; set;} //MANAGER MUST ASSIGN ON CREATION
    public int playerIndex {get; set;} //assigned player -> -1 for unassigned
    public bool isActive {get; set;}
}
