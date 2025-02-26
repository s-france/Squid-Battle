using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;

public class GrowBehavior : ItemBehavior
{
    Rigidbody2D rb;

    float minSize = 1.5f;
    float maxSize = 3;
    float timeToGrow = .75f;
    
    float maxMoveSpeedMod = -.4f;
    float maxMoveTimeMod = 2.7f;
    float maxMovePowerMod = 2.2f;
    float maxHitstopMod = .5f;

    float maxPassiveArmorMod = 1.2f;
    float maxMoveArmorMod = 1.8f;




    Vector3 defaultScale;
    /*
    float defaultStrength;
    float defaultKnockback;
    float defaultMaxMoveSpeed;
    float defaultMaxMoveTime;
    float defaultMaxMovePower;
    float defaultMaxHitstop;
    */


    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();
    }

    public override string GetItemType()
    {
        return "Grow";
    }


    public override void UseItem(float chargeTime)
    {
        //pc = GetComponentInParent<PlayerController>();
        rb = pc.GetComponent<Rigidbody2D>();

        
        defaultScale = pc.defaultScale;
        /*
        defaultStrength = pc.defaultChargeStrength;

        defaultMaxMoveSpeed = pc.defaultMaxMoveSpeed;
        defaultMaxMoveTime = pc.defaultMaxMoveTime;
        defaultMaxMovePower = pc.defaultMaxMovePower;
        defaultMaxHitstop = pc.defaultMaxHitstop;
        */

        HideSprite();
        
        StartCoroutine(GrowPlayer(chargeTime));

    }

    public override void DestroyItem()
    {
        //pc = GetComponentInParent<PlayerController>();
        rb = pc.GetComponent<Rigidbody2D>();

        //failsafes
        //mod scale
        pc.transform.localScale = defaultScale;

        //mod chargestrength
        //pc.chargeStrength = defaultStrength;
        //mod knockback


        Debug.Log("player is now back to normal size - grow over");

        GameObject.Destroy(gameObject);
    }

    IEnumerator GrowPlayer(float chargeTime)
    {
        float timer = 0;
        float bigTime = Mathf.Clamp(6+((chargeTime/pc.maxChargeTime)*15),6,20);
        //float bigTime = Mathf.Clamp((1+(chargeTime/pc.maxChargeTime))*8, 6, 15);
        float sizeMod = Mathf.Clamp(1+((chargeTime/pc.maxChargeTime)*maxSize), minSize,maxSize);
        //float sizeMod = Mathf.Clamp(1+(chargeTime/pc.maxChargeTime), minSize, maxSize);

        //percent values to be added to default ratio
        //float maxMoveSpeedMod = -.5f;
        //float maxMoveTimeMod = .5f; 
        //float maxMovePowerMod = 1;
        //float maxHitstopMod = 1f;

        Vector3 finalSize = sizeMod * Vector2.one; /** pc.transform.localScale*/;

        //adjust aimLine scale
        ReticleController rc = pc.GetComponentInChildren<ReticleController>();
        rc.transform.localScale = new Vector2(1/finalSize.x, 1/finalSize.y);


        float t;

        pc.isGrown = true;
        //grow
        Debug.Log("initiating grow");
        while (timer <= timeToGrow)
        {
            t = timer/timeToGrow;

            //mod scale
            pc.transform.localScale = Vector3.Lerp(pc.transform.localScale, finalSize, t);
            //mod knockback

            timer +=Time.deltaTime;
            yield return null;
        }

        //set maxmovespeed, movetime, movepower, hitstop
        pc.maxMoveSpeed = pc.defaultMaxMoveSpeed *  (1 + ((chargeTime/pc.maxChargeTime) * maxMoveSpeedMod));
        pc.maxMoveTime = pc.defaultMaxMoveTime * (1 + ((chargeTime/pc.maxChargeTime) * maxMoveTimeMod));
        pc.maxMovePower = pc.defaultMaxMovePower * (1 + (chargeTime/pc.maxChargeTime) * maxMovePowerMod);
        pc.maxHitstop = pc.defaultMaxHitstop * (1 + (chargeTime/pc.maxChargeTime) * maxHitstopMod);
        
        //use addition because default = 0
        pc.maxPassiveArmor = pc.defaultMaxPassiveArmor + ((chargeTime/pc.maxChargeTime) * maxPassiveArmorMod);
        pc.maxMoveArmor = pc.defaultMaxMoveArmor * (1+ ((chargeTime/pc.maxChargeTime) * maxMoveArmorMod));

        Debug.Log("player is now big");
        Debug.Log("maxMoveSpeed: " + pc.maxMoveSpeed);
        Debug.Log("maxMoveTime: " + pc.maxMoveTime);
        Debug.Log("maxMovePower: " + pc.maxMovePower);
        Debug.Log("maxHitstop: " + pc.maxHitstop);

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

            timer +=Time.deltaTime;
            yield return null;
        }

        pc.isGrown = false;

        //reset maxmovespeed, movetime, movepower, hitstop to default values
        pc.maxMoveSpeed = pc.defaultMaxMoveSpeed;
        pc.maxMoveTime = pc.defaultMaxMoveTime;
        pc.maxMovePower = pc.defaultMaxMovePower;
        pc.maxHitstop = pc.defaultMaxHitstop;

        pc.maxPassiveArmor = pc.defaultMaxPassiveArmor;
        pc.maxMoveArmor = pc.defaultMaxMoveArmor;

        rc.transform.localScale = Vector2.one;

        DestroyItem();
    }

}
