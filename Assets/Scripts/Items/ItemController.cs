using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemController : MonoBehaviour
{
    [HideInInspector] public Transform spawn;

    SpriteRenderer sr;
    public ItemBehavior ib;
    public SpriteRenderer itemSprite;
    public SpriteRenderer[] colorSprites;

    ItemManager im;
    [HideInInspector] public PlayerController pc; //player that owns this item -> null until picked up

    [SerializeField] float followSpeed;
    [SerializeField] float accelRate;

    int itemType;
    int idx;
    float activeTimer;

    // Start is called before the first frame update
    void Start()
    {
        sr = gameObject.GetComponent<SpriteRenderer>();

        im = GameObject.Find("ItemManager").GetComponent<ItemManager>();
        pc = null;

        //ib assigned by ItemManager
        if(ib == null)
        {
            Debug.Log("ERROR: IItemBehavior not assigned!");
        }

        //itemType = Random.Range(0, im.itemTypesCount);
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void AssignPlayer(Transform player)
    {
        PlayerController playerPC = player.gameObject.GetComponentInParent<PlayerController>();

        //Debug.Log("CHECK heldItems.Length = " + playerPC.heldItems.Length);
        //Debug.Log("CHECK inventorySize = " + playerPC.inventorySize);

        //if player has open space in inventory -> assign item
        if (playerPC.heldItems.Count < playerPC.inventorySize)
        {
            spawn.GetComponent<ItemSpawn>().isFull = false;

            //transform.SetParent(player);

            pc = playerPC;
            ib.pc = playerPC;
            //sr.enabled = false; //hide sprite after pickup
            transform.SetParent(null);
            transform.localScale = new Vector3(.3f, .3f, 1);
            GetComponent<Collider2D>().enabled = false; //disable future collision
            
            pc.GainItem(ib);

            StartCoroutine(Follow());

            Debug.Log("Item assigned to player" + pc.idx);
        } else
        {
            Debug.Log("Player" + playerPC + " inventory is full! Item NOT assigned.");
        }

        
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        Debug.Log("Item colliding! Collision LayerMask name: " + LayerMask.LayerToName(col.gameObject.layer));

        if (LayerMask.LayerToName(col.gameObject.layer) == "Players" && pc == null)
        {
            AssignPlayer(col.gameObject.transform);
            Debug.Log("Item" + idx + " collided with Player " + col.gameObject.GetComponentInParent<PlayerController>().idx);
        }

    }


    IEnumerator Follow()
    {
        float acceleration;
        while (true)
        {
            acceleration = accelRate * (transform.position - pc.transform.Find("FollowPoint").position).magnitude;

            transform.position = Vector2.MoveTowards(transform.position, pc.transform.Find("FollowPoint").position, followSpeed * acceleration * Time.deltaTime);
            yield return null;
        }
    }

}
