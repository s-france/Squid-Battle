using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ItemManager : MonoBehaviour
{
    [HideInInspector] public GameManager gm;

    public GameObject ItemPrefab;
    public List<GameObject> ItemObjs;
    [SerializeField] Sprite[] ItemPickupSprites;
    [SerializeField] Color[] ItemPickupColors;

    [SerializeField] Transform[] ItemSpawners;

    //public int itemTypesCount;
    [HideInInspector] public int spawnedItemsCount;
    [HideInInspector] public int[] spawnedItems;

    //Default item spawn probabilities - MOVED TO MATCHSETTINGS
    //[SerializeField] int[] ItemProbs;


    // Start is called before the first frame update
    void Awake()
    {
        gm = GameObject.Find("GameManager").GetComponent<GameManager>();

        

        //TEST TEST TEST - for testing purposes
        //uncomment for testing items
        //SpawnItem(3, this.transform);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //picks random item using MatchSettings.ItemProbs
    int pickItem()
    {
        int sum = 0;
        foreach (int i in gm.ms.ItemProbs)
        {
            sum += i;
        }

        if(sum == 0)
        {
            Debug.Log("No Items enabled.");
            return -1;
        }


        //pick random value in votes
        float r = Random.value * sum;
        float bottom = 0;
        float top = 0;

        int itemID = 0;

        //find which item corresponds to r value
        foreach (int i in gm.ms.ItemProbs)
        {
            top += i;

            if(bottom <= r && r <= top)
            {
                return itemID;
            }
            
            itemID += 1;
            bottom += i;
        }

        return itemID;
    }

    //spawns an item-pickup of type at position pos
    //type == -1 => random item
    public void SpawnItem(int type, Transform pos)
    {
        //Debug.Log("spawning item");

        //exit if no items enabled
        if(gm.ms.ItemProbs.Max() <= 0)
        {
            Debug.Log("no items enabled");
            return;
        }
        
        //instantiate new ItemPrefab
        GameObject newItem = Instantiate(ItemPrefab, pos.position, pos.rotation);
        ItemController ic = newItem.GetComponent<ItemController>();

        newItem.transform.parent = pos;

        //Random Item
        if (type == -1)
        {
            type = pickItem();
        }
        
        //set item type
        switch (type)
        {
            case 0: //shot
                ic.ib = newItem.AddComponent<ShotBehavior>() as ShotBehavior;
                Debug.Log("spawned shot at " + pos.position);
                break;

            case 1: //ink
                ic.ib = newItem.AddComponent<InkBehavior>() as InkBehavior;
                //ic.itemSprite.sprite = ItemPickupSprites[1];
                Debug.Log("spawned ink at " + pos.position);
                break;

            case 2: //wall
                ic.ib = newItem.AddComponent<WallBehavior>() as WallBehavior;
                //ic.itemSprite.sprite = ItemPickupSprites[3];
                Debug.Log("spawned wall at " + pos.position);
                break;
            
            case 3: //warp
                ic.ib = newItem.AddComponent<WarpBehavior>() as WarpBehavior;
                //ic.itemSprite.sprite = ItemPickupSprites[4];
                Debug.Log("spawned warp at " + pos.position);
                break;
            
            case 4: //rewind
                ic.ib = newItem.AddComponent<RewindBehavior>() as RewindBehavior;
                //ic.itemSprite.sprite = ItemPickupSprites[5];
                Debug.Log("spawned rewind at " + pos.position);
                break;
            
            case 5: //grow
                ic.ib = newItem.AddComponent<GrowBehavior>() as GrowBehavior;
                //ic.itemSprite.sprite = ItemPickupSprites[2];
                Debug.Log("spawned grow at " + pos.position);
                break;
            case 6: //double
                ic.ib = newItem.AddComponent<DoubleBehavior>() as DoubleBehavior;
                Debug.Log("spawned double at " + pos.position);
                break;
                
            default:
                Debug.Log("ERROR: invalid item type!!");
                break;
        }

        ic.itemSprite.sprite = ItemPickupSprites[type];

        ic.colorSprites[0].color = ItemPickupColors[type*2];
        ic.colorSprites[1].color = ItemPickupColors[(type*2)+1];

        newItem.GetComponent<ItemController>().spawn = pos;
        pos.GetComponent<ItemSpawn>().isFull = true;


        spawnedItemsCount++;
        //spawnedItems[spawnedItemsCount] = 

    }

    public IEnumerator RandomSpawns(float spawnRate)
    {

        float timer = 0;

        while(gm.battleStarted)
        {
            timer += Time.deltaTime;

            if(timer >= spawnRate)
            {
                for (int i = 0; i < ItemSpawners.Length; i++)
                {
                    if(ItemSpawners[i].GetComponent<ItemSpawn>().isFull == false)
                    {
                        SpawnItem(-1, ItemSpawners[i]);
                    }
                }

                timer = 0;
            }
            
            yield return null;
        }

    }

    void DestroyAllItemObjs()
    {
        
    }

}


public class ItemConfig
{
    public ItemConfig(ItemBehavior item, int idx, int spawn)
    {
        itemObj = item;
        playerIndex = -1;
        isActive = true;
    }

    ItemBehavior itemObj {get; set;}
    public int itemType {get; set;}
    public int itemIndex {get; set;} //MANAGER MUST ASSIGN ON CREATION
    public int playerIndex {get; set;} //assigned player -> -1 for unassigned
    public bool isActive {get; set;}
}
