using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemManager : MonoBehaviour
{
    InputManager im;

    public GameObject ItemPrefab;
    public List<GameObject> ItemObjs;

    public int itemTypesCount;
    [HideInInspector] public int spawnedItemsCount;
    [HideInInspector] public int[] spawnedItems;

    // Start is called before the first frame update
    void Start()
    {
        im = GameObject.Find("PlayerInputManager").GetComponent<InputManager>();

        //SpawnItem(0, this.transform);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //spawns (creates) an item-pickup of type at position pos
    void SpawnItem(int type, Transform pos)
    {
        Debug.Log("spawning item");
        
        //instantiate new ItemPrefab
        GameObject newItem = Instantiate(ItemPrefab, pos.position, pos.rotation);

        //set item type
        switch (type)
        {
            case 0: //shot
                newItem.GetComponent<ItemController>().ib = newItem.AddComponent<ShotBehavior>() as ShotBehavior;
                Debug.Log("spawned shot at " + pos.position);
                break;

            case 1: //ink
                newItem.GetComponent<ItemController>().ib = newItem.AddComponent<InkBehavior>() as InkBehavior;
                Debug.Log("spawned ink at " + pos.position);
                break;
            
            default:
                Debug.Log("ERROR: invalid item type!!");
                break;
        }


        spawnedItemsCount++;
        //spawnedItems[spawnedItemsCount] = 

    }

    public IEnumerator RandomSpawns(float spawnRate)
    {
        float timer = 0;
        int type;
        Transform pos = new GameObject().transform;

        while(im.gameStarted)
        {
            //if(!im.gameStarted) {break;}
            timer += Time.deltaTime;

            if(timer >= spawnRate)
            {
                type = Random.Range(0,itemTypesCount);
                pos.position = Random.insideUnitCircle * 7;

                SpawnItem(type, pos);

                timer = 0;
            }
            
            yield return new WaitForEndOfFrame();
        }

        Destroy(pos.gameObject);
    }

    void DestroyAllItemObjs()
    {
        
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
