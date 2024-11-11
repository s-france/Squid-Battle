using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Portal : MonoBehaviour
{
    [SerializeField] Portal otherPortal;
    [SerializeField] float defaultCoolDown;

    [SerializeField] Color activeColor;
    [SerializeField] Color inactiveColor;

    SpriteRenderer sr;
    [HideInInspector] public List<Collider2D> warpObjs;

    Collider2D collision;

    float coolDown = 0;

    // Start is called before the first frame update
    void Start()
    {
        warpObjs = new List<Collider2D>();
        collision = GetComponent<Collider2D>();
        sr = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (coolDown > 0)
        {
            sr.color = inactiveColor;
            coolDown -= Time.deltaTime;
        } else
        {
            sr.color = activeColor;
            coolDown = 0;
        }
        
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (coolDown <= 0)
        {
            //warpObjs.Add(col);
            Warp(col.transform);
        }

    }

    void OnTriggerExit2D(Collider2D col)
    {
        //warpObjs.Remove(col);
    }


    void Warp(Transform obj)
    {
        coolDown = defaultCoolDown;
        otherPortal.coolDown = otherPortal.defaultCoolDown;

        obj.transform.position = otherPortal.transform.position;

        PlayerController pc = obj.GetComponent<PlayerController>();
        if(pc != null)
        {
            pc.moveTime *= 1.5f;
        }

    }

}
