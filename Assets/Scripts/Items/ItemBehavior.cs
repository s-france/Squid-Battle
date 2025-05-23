using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

//parent class for item behaviors
public class ItemBehavior : MonoBehaviour
{
    [HideInInspector] public ItemManager im;
    [HideInInspector] public PlayerController pc;
    [HideInInspector] public SpriteRenderer[] Sprites;

    [HideInInspector] public String soundFX;

    public virtual void Start()
    {
        Sprites = GetComponentsInChildren<SpriteRenderer>();

        im = GameObject.Find("ItemManager").GetComponent<ItemManager>();
    }

    public virtual string GetItemType()
    {
        Debug.Log("ERROR: no item type assigned!");
        return null;
    }
    public virtual void UseItem(float charge)
    {
        Debug.Log("ERROR: no item type assigned!");
        DestroyItem();
    }

    public virtual void HideSprite()
    {
        foreach(SpriteRenderer sr in Sprites)
        {
            sr.enabled = false;
        }
    }
    
    public virtual void DestroyItem()
    {
        HideSprite();
        GameObject.Destroy(gameObject);
    }
}

