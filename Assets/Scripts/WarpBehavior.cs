using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

public class WarpBehavior : MonoBehaviour, IItemBehavior
{
    PlayerController pc;
    [HideInInspector] public Vector2 warpPoint;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public string GetItemType()
    {
        return "Warp";
    }

    public void UseItem(float chargeTime)
    {
        Debug.Log("Warping!!");
        pc = GetComponentInParent<PlayerController>();
        StartCoroutine(Warp(chargeTime));
    }

    public void DestroyItem()
    {
        GameObject.Destroy(gameObject);
    }

    IEnumerator Warp(float chargeTime)
    {
        CircleCollider2D playerCol = pc.GetComponent<CircleCollider2D>();
        CircleCollider2D warpCol = pc.transform.Find("WarpPoint").GetComponent<CircleCollider2D>();

        float timer = 0; //warp routine timer
        float totalTime = .5f; //total warp time from start -> finish

        Color change = new Color(0,0,0, -1/(totalTime/3));

        //player disappear:
        ///disable collision
        playerCol.enabled = false;

        ///play warp-out animation
        while(timer < totalTime/3)
        {
            //pc.sr.color += change;


            timer += Time.deltaTime;
            yield return null;
        }
        pc.sr.enabled = false;

        pc.transform.position = warpPoint;

        //wait a few frames
        while(timer < 2*totalTime/3)
        {
            //do nothing
            timer += Time.deltaTime;
            yield return null;
        }

        //player reappear at warpPoint:
        ///activate shockwave
        warpCol.enabled = true;

        ///play warp-in animation
        
        pc.sr.enabled = true;
        while (timer < totalTime)
        {
            //wait a moment
            //activate shockwave
            
            timer += Time.deltaTime;
            yield return null;
        }

        //deactivate shockwave
        warpCol.enabled = false;

        ///enable collision
        playerCol.enabled = true;
        
        //destroy
        DestroyItem();
    }

}
