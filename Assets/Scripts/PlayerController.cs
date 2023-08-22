using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System;
using Cinemachine;
using Unity.Mathematics;

public class PlayerController : MonoBehaviour
{
    InputManager im;
    ReticleController rc;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private SpriteRenderer sr;
    private Sprite[] spriteSet;
    [SerializeField] CinemachineTargetGroup tg;

    [HideInInspector] public Vector2 i_move;
    Vector3 rotation;

    [HideInInspector] public int idx;
    [HideInInspector] public int colorID;
    [HideInInspector] public List<IItemBehavior> heldItems;
    int selectedItemIdx;
    [HideInInspector] public bool charging;
    [HideInInspector] public float chargeTime;
    [HideInInspector] public bool specialCharging;
    [HideInInspector] public float specialChargeTime;
    [HideInInspector] public bool isCoolingDown;


    [SerializeField] public int inventorySize; //amount of items that can be held at once
    [SerializeField] float chargeStrength;
    [SerializeField] float maxChargeTime;
    [SerializeField] float knockbackMultiplier;
    [SerializeField] float coolDownFactor;
    [SerializeField] float moveCoolDown;
    [SerializeField] float coolDownVelocity;

    void Start()
    {
        rb = this.gameObject.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            Debug.Log("got RB!!");
        }

        im = GameObject.Find("PlayerInputManager").GetComponent<InputManager>();
        if (im != null)
        {
            Debug.Log("got IM!!");
        }

        idx = im.playerList.FindIndex(i => i.input == this.gameObject.GetComponent<PlayerInput>());
        colorID = idx;
        Debug.Log("player idx (from playercontroller): " + idx);

        tg = GameObject.Find("TargetGroup1").GetComponent<CinemachineTargetGroup>();
        if (tg != null)
        {
            Debug.Log("got TG!");
        }
        tg.AddMember(this.transform, 1, 1);

        sr = this.gameObject.GetComponent<SpriteRenderer>();
        if(sr != null)
        {
            Debug.Log("got SR!!");
        }
        rc = this.GetComponentInChildren<ReticleController>();

        heldItems = new List<IItemBehavior>();

        ChangeColor(colorID);

        isCoolingDown = false;
        i_move = Vector2.zero;
        rotation = Vector3.zero;


    }

    // Update is called once per frame
    void Update()
    {
        CoolDown();
    }

    void FixedUpdate()
    {

    }

    //handle players colliding
    void OnCollisionEnter2D(Collision2D col)
    {
        Debug.Log("Player colliding! Collision LayerMask name: " + LayerMask.LayerToName(col.gameObject.layer));

        if (LayerMask.LayerToName(col.gameObject.layer) == "Players")
        {
            Debug.Log("Player" + idx + " collided with Player " + col.gameObject.GetComponent<PlayerController>().idx);

            //REPLACE THIS WITH STUNNED SPRITE!!!!!!!!!!!!
            sr.sprite = spriteSet[0];

            StartCoroutine(Knockback(col.rigidbody));
        } /*else if(LayerMask.LayerToName(col.gameObject.layer) == "Items")
        {

        }*/

    }


    IEnumerator Knockback(Rigidbody2D col)
    {
        var fuWait = new WaitForFixedUpdate();

        if (col.velocity.magnitude < rb.velocity.magnitude)
        {

            Debug.Log("player" + idx + " velocity: " + rb.velocity.magnitude);
            Debug.Log(col.gameObject.name + " velocity: " + col.velocity.magnitude);

            yield return fuWait;
            rb.velocity *= knockbackMultiplier;

            Debug.Log("player" + idx + "knocked!!");

        }
    }

    public void Deactivate()
    {
        rb.angularVelocity = 0;
        transform.rotation = quaternion.identity;

        rb.velocity = Vector2.zero;
        i_move = Vector2.zero;
        chargeTime = 0;
        charging = false;
        specialCharging = false;
        ClearInventory();
        this.GetComponent<CircleCollider2D>().enabled = false;
        this.GetComponent<SpriteRenderer>().enabled = false;
        tg.RemoveMember(this.transform);
    }

    public void Reactivate()
    {
        rb.angularVelocity = 0;
        transform.rotation = quaternion.identity;

        i_move= Vector2.zero;
        chargeTime = 0;
        charging = false;
        specialCharging = false;
        
        this.GetComponent<CircleCollider2D>().enabled = true;
        this.GetComponent<SpriteRenderer>().enabled = true;
        tg.AddMember(this.transform, 1, 1);
    }

    //handle player leaving in conjunction with im.OnPlayerLeave()
    public void OnControllerDisconnect(PlayerInput pi)
    {
        im.OnPlayerLeave(pi);
    }

    //runs when d/c controller reconnected
    public void OnControllerReconnect(PlayerInput pi)
    {
        im.OnPlayerReconnect(pi);
    }

    //called when move (stick) input received
    public void OnMove(InputAction.CallbackContext ctx)
    {
        //save move input
        if(ctx.ReadValue<Vector2>().magnitude != 0 && im.gameStarted)
        {
            i_move = ctx.ReadValue<Vector2>();
        }
        //Debug.Log(i_move);

        //rotate sprite
        if(!isCoolingDown)
        {
            RotateSprite(i_move);
        }
    }

    void RotateSprite(Vector2 dir)
    {
        rb.angularVelocity = 0;
        float angle = Vector2.SignedAngle(Vector2.up, dir);
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    //called when charge (button) input received
    public void OnCharge(InputAction.CallbackContext ctx)
    {
        //Debug.Log(ctx.phase);
        if(ctx.performed) //charging
        {
            chargeTime = 0;
            charging = true;
            StartCoroutine(Charge());

        } else if (ctx.canceled) //released
        {
            charging = false;
        }
    }

    //called when OnCharge() performed - handles player charging
    IEnumerator Charge()
    {
        if(im.gameStarted)
        {
            StartCoroutine(rc.RenderReticle());
        }

        //Charging
        while (charging)
        {
            if (!isCoolingDown)
            {
                sr.sprite = spriteSet[2]; //charging sprite
                chargeTime += Time.deltaTime;
            } else
            {
                //failsafe prevents chargehold while stunned
                //MIGHT CAUSE PROBLEMS idk
                chargeTime = 0;
            }
            

            //readyUp if held for 2 secs before game
            if(chargeTime > 1 && !im.gameStarted && !im.playerList[idx].isReady)
            {
                ReadyUp();
            }
            yield return null;
        }

        //After charging
        if(im.gameStarted && !isCoolingDown /*&& !specialCharging*/) //perform movement during match
        {
            Move(0);
        } else if (!im.gameStarted && !im.playerList[idx].isReady)
        {
            sr.sprite = spriteSet[0];
        }

        //reset chargeTime
        chargeTime = 0;
    }

    //called in OnCharge - handles launch movement
    //type of movement - 0:normal 1:special
    //^^stupid workaround for demo sprint MUST FIX LATER
    void Move(int type)
    {
        rb.velocity = Vector2.zero;
        Vector2 moveForce = calcMoveForce(type);
        Debug.Log("Moving Force: " + moveForce.magnitude);

        rb.AddForce(moveForce, ForceMode2D.Impulse);

        sr.sprite = spriteSet[1];
        RotateSprite(i_move);
    }

    //type of movement - 0:normal 1:special
    //^^stupid workaround for demo sprint MUST FIX LATER
    public Vector2 calcMoveForce(int type)
    {
        float charge;
        switch (type)
        {
            case 0:
                charge = chargeTime;
                break;
            case 1:
                charge = specialChargeTime;
                break;
            default:
                charge = chargeTime;
                break;
        }

        float xmod = .4f;
        float ymod = 1.37f;

        float chargeCurve = (MathF.Log10(maxChargeTime) + 1);
        float chargeScale = 3; //max charge
        float chargeFactor = chargeScale / chargeCurve;
        float minCharge = .5f;

        Vector2 moveForce = i_move * chargeStrength * Math.Clamp(ymod*chargeFactor * (MathF.Log10(xmod*Math.Clamp(charge, 0, maxChargeTime)) + 1), minCharge, 100);
        
        if (type == 1) //total bullshit
        {
            float idk = moveForce.magnitude*.25f + 7;

            //return moveForce * .33f;
            return moveForce.normalized * idk;

        } else
        {
            return moveForce;
        }

        
    }


    void CoolDown()
    {
        if(rb.velocity.magnitude > coolDownVelocity)
        {
            isCoolingDown = true;
        } else
        {
            isCoolingDown = false;

            //reset sprite after cooldown
            if(!charging)
            {
                sr.sprite = spriteSet[0];
            }
        }
    }

    public void OnSpecial(InputAction.CallbackContext ctx)
    {
        if(ctx.performed) //special charging
        {
            specialChargeTime = 0;
            specialCharging = true;
            StartCoroutine(SpecialCharge());
            
        } else if (ctx.canceled) //released
        {
            specialCharging = false;
        }
    }

    IEnumerator SpecialCharge()
    {
        if(im.gameStarted)
        {
            StartCoroutine(rc.RenderReticle());
        }


        //Special Charging
        while (specialCharging)
        {
            if (!isCoolingDown)
            {
                sr.sprite = spriteSet[2]; //charging sprite
                specialChargeTime += Time.deltaTime;
            } else
            {
                specialChargeTime = 0;
            }

            //readyUp if held for 2 secs before game
            if(specialChargeTime > 1 && !im.gameStarted && !im.playerList[idx].isReady)
            {
                ReadyUp();
            }

            yield return null;
        }

        //After special charging
        if(im.gameStarted && !isCoolingDown) //use item during match
        {
            Move(1);
            UseItem(selectedItemIdx);
        } else if (!im.gameStarted && !im.playerList[idx].isReady)
        {
            sr.sprite = spriteSet[0];
        }

        //reset chargeTime
        specialChargeTime = 0;
    }

    public void GainItem(IItemBehavior item)
    {
        heldItems.Add(item);
        Debug.Log("player" + idx + " gained an item!");
        
    }

    //uses item in heldItems[idx]
    void UseItem(int idx)
    {
        heldItems[idx].UseItem(specialChargeTime);
        heldItems.RemoveAt(idx);
    }

    void ClearInventory()
    {
        foreach (IItemBehavior item in heldItems)
        {
            item.DestroyItem();
        }
        heldItems.Clear();
    }

    public void OnSelectL(InputAction.CallbackContext ctx)
    {
        if(ctx.performed)
        {
            if(!im.gameStarted) //change color only before match
            {
                colorID = (colorID - 1);
                if(colorID < 0) {colorID = im.colorsCount;}
                ChangeColor(colorID);
            } else
            {
                //whatever L does during gameplay
                //maybe swaps between items idk
            }
        }
    }

    public void OnSelectR(InputAction.CallbackContext ctx)
    {
        if(ctx.performed)
        {
            if(!im.gameStarted) //change color only before match
            {
                colorID = (colorID + 1);
                if(colorID > im.colorsCount) {colorID = 0;}
                ChangeColor(colorID);
            } else
            {
                //whatever R does during gameplay
                //maybe swaps between items idk
            }
        }
    }

    //change color - updates sprite
    public void ChangeColor(int color)
    {
        spriteSet = im.playerSprites[color];
        sr.sprite = spriteSet[0];

        im.SetPlayerColor(idx, color);
    }

    public void ReadyUp()
    {
        sr.sprite = spriteSet[2]; //charging sprite doubles as ready sprite
        im.ReadyPlayer(idx);
    }
}
