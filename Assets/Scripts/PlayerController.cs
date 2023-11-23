using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System;
using Cinemachine;
using Unity.Mathematics;

public class PlayerController : MonoBehaviour
{
    GameManager gm;
    [HideInInspector] public PlayerManager pm;
    InputManager im;
    ReticleController rc;
    public Rigidbody2D rb;
    [SerializeField] private SpriteRenderer sr;
    private Sprite[] spriteSet;
    CinemachineTargetGroup tg;

    [HideInInspector] public Vector2 i_move;
    Vector3 rotation;

    [HideInInspector] public int idx;
    [HideInInspector] public int colorID;
    [HideInInspector] public List<IItemBehavior> heldItems;
    [HideInInspector] public int selectedItemIdx;
    [HideInInspector] public bool charging;
    [HideInInspector] public float chargeTime;
    [HideInInspector] public bool specialCharging;
    [HideInInspector] public float specialChargeTime;

    //NEW movement system
    float moveTime; //time to spend moving
    float moveSpeed; //speed of movement
    float movePower; //knockback to apply on collision


    [HideInInspector] public bool isCoolingDown;
    [HideInInspector] public bool isGrown;


    [SerializeField] public int inventorySize; //amount of items that can be held at once
    [SerializeField] public float chargeStrength;
    [SerializeField] float maxChargeTime;
    [SerializeField] public float knockbackMultiplier;
    [SerializeField] float coolDownFactor;
    [SerializeField] float moveCoolDown;
    [SerializeField] float coolDownVelocity;
    [SerializeField] float DIStrength;

    [HideInInspector] public Vector3 defaultScale;
    [HideInInspector] public float defaultMass;
    [HideInInspector] public float defaultChargeStrength;
    [HideInInspector] public float defaultKnockbackMultiplier;


    void Awake()
    {
        
    }

    //use this instead of Awake()
    public void Init()
    {
        DontDestroyOnLoad(this);

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

        gm = GameObject.Find("GameManager").GetComponent<GameManager>();
        pm = gm.GetComponentInChildren<PlayerManager>(); 
        
        transform.parent = gm.transform.GetChild(0);

        idx = pm.playerList.FindIndex(i => i.input == this.gameObject.GetComponent<PlayerInput>());
        colorID = idx;
        Debug.Log("player idx (from playercontroller): " + idx);


        sr = this.gameObject.GetComponent<SpriteRenderer>();
        if(sr != null)
        {
            Debug.Log("got SR!!");
        }
        rc = this.GetComponentInChildren<ReticleController>();

        heldItems = new List<IItemBehavior>();

        ChangeColor(colorID);

        selectedItemIdx = 0;
        isCoolingDown = false;
        i_move = Vector2.zero;
        rotation = Vector3.zero;

        //set default stats
        defaultScale = transform.localScale;
        defaultMass = rb.mass;
        defaultChargeStrength = chargeStrength;
        defaultKnockbackMultiplier = knockbackMultiplier;
    }

    // Update is called once per frame
    void Update()
    {
        CoolDown();
    }

    void FixedUpdate()
    {
        DirectionalInfluence();
    }

    //handle players colliding
    void OnCollisionEnter2D(Collision2D col)
    {

        FindObjectOfType<AudioManager>().PlayRandom("Impact");


        Debug.Log("Player colliding! Collision LayerMask name: " + LayerMask.LayerToName(col.gameObject.layer));

        if (LayerMask.LayerToName(col.gameObject.layer) == "Players")
        {
            Debug.Log("Player" + idx + " collided with Player " + col.gameObject.GetComponent<PlayerController>().idx);
            
            GetComponentInChildren<TrailRenderer>().emitting = false; //cancel wall item on impact

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

    void ResetDefaultStats()
    {
        transform.localScale = defaultScale;
        rb.mass = defaultMass;
        chargeStrength = defaultChargeStrength;
        knockbackMultiplier = defaultKnockbackMultiplier;

        rb.angularVelocity = 0;
        transform.rotation = quaternion.identity;
    }

    //REWORK NEEDED
    public void Deactivate()
    {
        ResetDefaultStats();

        rb.velocity = Vector2.zero;
        i_move = Vector2.zero;
        chargeTime = 0;
        charging = false;
        specialCharging = false;
        ClearInventory();
        this.GetComponent<CircleCollider2D>().enabled = false;
        this.GetComponent<SpriteRenderer>().enabled = false;
        GetComponentInChildren<TrailRenderer>().Clear();
        //REWORK THIS!!
        //tg.RemoveMember(this.transform);
    }

    //REWORK THIS
    public void Reactivate()
    {
        ResetDefaultStats();

        i_move= Vector2.zero;
        chargeTime = 0;
        charging = false;
        specialCharging = false;
        
        this.GetComponent<CircleCollider2D>().enabled = true;
        this.GetComponent<SpriteRenderer>().enabled = true;
        GetComponentInChildren<TrailRenderer>().Clear();
        
        //REWORK
        tg = FindObjectOfType<CinemachineTargetGroup>();
        if(tg != null)
        {
            tg.AddMember(this.transform, 1, 1);
        }
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
        if(ctx.ReadValue<Vector2>().magnitude != 0 && gm.battleStarted)
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


    //allows players to slightly alter their trajectory while moving / being launched
    //called in FixedUpdate()
    void DirectionalInfluence()
    {
        if(isCoolingDown)
        {
            rb.AddForce(i_move * DIStrength);
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
        if(ctx.performed && pm.playerList[idx].isInBounds) //charging
        {
            Debug.Log("charging!!");
            
            if(!gm.battleStarted && !pm.playerList[idx].isActive && GameObject.Find("LevelController").GetComponent<ILevelController>().GetLevelType() != 1)
            {
                Debug.Log("Reconnect!!");
                OnControllerReconnect(GetComponent<PlayerInput>());
            }

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
        if(gm.battleStarted && pm.playerList[idx].isActive && pm.playerList[idx].isAlive)
        {
            StartCoroutine(rc.RenderReticle());
        }

        //Charging
        while (charging)
        {
            if (!isCoolingDown && pm.playerList[idx].isInBounds)
            {
                sr.sprite = spriteSet[2]; //charging sprite
                chargeTime += Time.deltaTime;
            } else
            {
                //failsafe prevents chargehold while stunned
                //MIGHT CAUSE PROBLEMS idk
                chargeTime = 0;
            }
            

            //readyUp if held for 1 secs before game
            if(chargeTime > 1 && !gm.battleStarted && !pm.playerList[idx].isReady)
            {
                ReadyUp();
            }
            yield return null;
        }

        //After charging
        if(gm.battleStarted && !isCoolingDown && pm.playerList[idx].isInBounds /*&& !specialCharging*/) //perform movement during match
        {
            Move(0);
        } else if (!gm.battleStarted && !pm.playerList[idx].isReady)
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

        FindObjectOfType<AudioManager>().PlayRandom("Move");

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
            case 2: //exception for powerups that launch full distance
                charge = specialChargeTime;
                break;
            default:
                charge = chargeTime;
                break;
        }

        float xmod = .4f;
        float ymod = 1.37f;

        float chargeCurve = MathF.Log10(maxChargeTime) + 1;
        float chargeScale = 3; //max charge
        float chargeFactor = chargeScale / chargeCurve;
        float minCharge = .5f;

        Vector2 moveForce = i_move * chargeStrength * Math.Clamp(ymod*chargeFactor * (MathF.Log10(xmod*Math.Clamp(charge, 0, maxChargeTime)) + 1), minCharge, 100);
        
        Debug.Log("MOVEFORCE: " + moveForce.magnitude);

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
        if(gm.battleStarted && pm.playerList[idx].isActive && pm.playerList[idx].isAlive)
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

            //drop out if held for 1 secs before game
            if(specialChargeTime > 1 && !gm.battleStarted && GameObject.Find("LevelController").GetComponent<ILevelController>().GetLevelType() != 1)
            {
                OnControllerDisconnect(gameObject.GetComponent<PlayerInput>());
            }

            yield return null;
        }

        //After special charging
        if(gm.battleStarted && !isCoolingDown) //use item during match
        {
            if(heldItems.Count != 0 && heldItems[selectedItemIdx] != null)
            {
                if(heldItems[selectedItemIdx].GetItemType() == "Wall")
                {
                    Debug.Log("WALL MOVEMENT");
                    Move(2);
                } else
                {
                    Move(1);
                }
                
            }else
            {
                Move(1);
            }

            UseItem(selectedItemIdx);
        } else if (!gm.battleStarted && !pm.playerList[idx].isReady)
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
            if(!gm.battleStarted) //change color only before match
            {
                colorID = (colorID - 1);
                if(colorID < 0) {colorID = pm.colorsCount;}
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
            if(!gm.battleStarted) //change color only before match
            {
                colorID = (colorID + 1);
                if(colorID > pm.colorsCount) {colorID = 0;}
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
        spriteSet = pm.playerSprites[color];
        sr.sprite = spriteSet[0];

        pm.SetPlayerColor(idx, color);
    }

    public void ReadyUp()
    {
        sr.sprite = spriteSet[2]; //charging sprite doubles as ready sprite
        pm.ReadyPlayer(idx);
    }


    //NEW movement system stuff

    //called in Update
    void MovementTick()
    {
        //if time is almost up start deccel
        //if(moveTime < )
        //{

        //}




        if(moveTime > 0)
        {
            moveTime -= Time.deltaTime;
        } else if(moveTime < 0)
        {
            moveTime = 0;
        }

    }


    void ApplyMove(Vector2 direction, float time, float speed, float power)
    {
        moveSpeed = speed;
        moveTime = time;
        movePower = power;

        rb.velocity = Vector2.zero;
        rb.AddForce(direction * speed, ForceMode2D.Impulse);
    }





}
