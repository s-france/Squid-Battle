using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemManager : MonoBehaviour
{
    InputManager im;
    GameManager gm;

    public GameObject ItemPrefab;
    public List<GameObject> ItemObjs;
    [SerializeField] Sprite[] ItemPickupSprites;

    [SerializeField] Transform[] ItemSpawners;

    public int itemTypesCount;
    [HideInInspector] public int spawnedItemsCount;
    [HideInInspector] public int[] spawnedItems;


    // Start is called before the first frame update
    void Awake()
    {
        im = GameObject.Find("PlayerInputManager").GetComponent<InputManager>();
        gm = GameObject.Find("GameManager").GetComponent<GameManager>();

        //TEST TEST TEST - for testing purposes
        //uncomment for testing items
        //SpawnItem(3, this.transform);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //spawns (creates) an item-pickup of type at position pos
    public void SpawnItem(int type, Transform pos)
    {
        Debug.Log("spawning item");
        
        //instantiate new ItemPrefab
        GameObject newItem = Instantiate(ItemPrefab, pos.position, pos.rotation);

        //Random Item
        if (type == -1)
        {
            float rand = Random.value;
            if (rand <= .9f) //9/10 chance for shot+ink+wall
            {
                type = Random.Range(0,3);
            } else //1/10 chance for grow
            {
                type = 3;
            }
        }
        


        //set item type
        switch (type)
        {
            case 0: //shot
                newItem.GetComponent<ItemController>().ib = newItem.AddComponent<ShotBehavior>() as ShotBehavior;
                newItem.GetComponent<SpriteRenderer>().sprite = ItemPickupSprites[0];
                Debug.Log("spawned shot at " + pos.position);
                break;

            case 1: //ink
                newItem.GetComponent<ItemController>().ib = newItem.AddComponent<InkBehavior>() as InkBehavior;
                newItem.GetComponent<SpriteRenderer>().sprite = ItemPickupSprites[1];
                Debug.Log("spawned ink at " + pos.position);
                break;

            case 2: //wall
                newItem.GetComponent<ItemController>().ib = newItem.AddComponent<WallBehavior>() as WallBehavior;
                newItem.GetComponent<SpriteRenderer>().sprite = ItemPickupSprites[3];
                Debug.Log("spawned wall at " + pos.position);
                break;
            
            case 3: //grow
                newItem.GetComponent<ItemController>().ib = newItem.AddComponent<GrowBehavior>() as GrowBehavior;
                newItem.GetComponent<SpriteRenderer>().sprite = ItemPickupSprites[2];
                Debug.Log("spawned grow at " + pos.position);
                break;
            
            default:
                Debug.Log("ERROR: invalid item type!!");
                break;
        }

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
            //if(!im.gameStarted) {break;}
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

    /*
    public IEnumerator RandomSpawns(float spawnRate)
    {
        float rand;

        float timer = 0;
        int type;
        Transform pos = new GameObject().transform;

        //spawn at start of match
        rand = Random.value;
        if (rand <= .9f) //9/10 chance for shot+ink+wall
        {
            type = Random.Range(0,3);
        } else //1/10 chance for grow
        {
            type = 3;
        }

        pos.position = Random.insideUnitCircle * 7;
        SpawnItem(type, pos);

        while(gm.battleStarted)
        {
            //if(!im.gameStarted) {break;}
            timer += Time.deltaTime;

            if(timer >= spawnRate)
            {
                rand = Random.value;
                if (rand <= .9f) //9/10 chance for shot+ink
                {
                    type = Random.Range(0,2);
                } else //1/10 chance for grow
                {
                    type = 2;
                }
                pos.position = Random.insideUnitCircle * 7;

                SpawnItem(type, pos);

                timer = 0;
            }
            
            yield return null;
        }

        Destroy(pos.gameObject);
    }
    */


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
