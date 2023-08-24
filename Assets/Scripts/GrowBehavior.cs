using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class GrowBehavior : MonoBehaviour, IItemBehavior
{
    ItemManager im;
    PlayerController pc;
    Rigidbody2D rb;

    float minSize = 1.5f;
    float maxSize = 4;
    float timeToGrow = .75f;
    float minStrength = 1.2f;
    float maxStrength = 2.5f;
    float minMass = 1.2f;
    float maxMass = 2.5f;

    Vector3 defaultScale;
    float defaultStrength;
    float defaultKnockback;
    float defaultMass;

    // Start is called before the first frame update
    void Start()
    {
        im = GameObject.Find("ItemManager").GetComponent<ItemManager>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UseItem(float chargeTime)
    {
        pc = GetComponentInParent<PlayerController>();
        rb = pc.GetComponent<Rigidbody2D>();

        defaultScale = pc.defaultScale;
        defaultStrength = pc.defaultChargeStrength;
        defaultKnockback = pc.defaultKnockbackMultiplier;
        defaultMass = pc.defaultMass;

        StartCoroutine(GrowPlayer(chargeTime));

    }

    public void DestroyItem()
    {
        pc = GetComponentInParent<PlayerController>();
        rb = pc.GetComponent<Rigidbody2D>();

        //failsafes
        //mod scale
        pc.transform.localScale = defaultScale;
        //mod mass
        rb.mass = defaultMass;
        //mod chargestrength
        pc.chargeStrength = defaultStrength;
        //mod knockback


        Debug.Log("player is now back to normal size - grow over");

        GameObject.Destroy(gameObject);
    }

    IEnumerator GrowPlayer(float chargeTime)
    {
        float timer = 0;
        float bigTime = Mathf.Clamp(chargeTime*7, 5, 20);
        float sizeMod = Mathf.Clamp(chargeTime+1, minSize, maxSize);
        float massMod = Mathf.Clamp(chargeTime/2+1, minMass, maxMass);
        float strengthMod = Mathf.Clamp(chargeTime/2+1, minStrength, maxStrength);

        Vector3 finalSize = sizeMod * Vector2.one; /** pc.transform.localScale*/;

        float t;

        //grow
        Debug.Log("initiating grow");
        while (timer <= timeToGrow)
        {
            t = timer/timeToGrow;

            //mod scale
            pc.transform.localScale = Vector3.Lerp(pc.transform.localScale, finalSize, t);
            //mod mass
            rb.mass = Mathf.Lerp(rb.mass, defaultMass*massMod, t);
            //mod chargestrength
            pc.chargeStrength = Mathf.Lerp(pc.chargeStrength, defaultStrength*strengthMod, t);
            //mod knockback

            timer +=Time.deltaTime;
            yield return null;
        }

        Debug.Log("player is now big");

        //stay big for bigTime
        while (timer <= bigTime)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        timer = 0;

        //revert changes (shrink)

        Debug.Log("initiating shrink");

        while(timer <= timeToGrow)
        {
            t = timer/timeToGrow;

            //mod scale
            pc.transform.localScale = Vector3.Lerp(pc.transform.localScale, defaultScale, t);
            //mod mass
            rb.mass = Mathf.Lerp(rb.mass, defaultMass, t);
            //mod chargestrength
            

            //mod knockback

            timer +=Time.deltaTime;
            yield return null;
        }

        DestroyItem();
    }

}
