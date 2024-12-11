using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System;
using Cinemachine;
using Unity.Mathematics;
using Unity.VisualScripting;
using System.Security.Cryptography;
//using UnityEditor.UIElements;
using UnityEngine.Assertions.Must;
using UnityEngine.Timeline;
using UnityEngine.InputSystem.Switch;
using System.Linq;
using UnityEngine.InputSystem.UI;
using System.Runtime.InteropServices;
//using Unity.Barracuda;
//using System.Numerics;
//using System.Numerics;

public class PlayerController : MonoBehaviour
{
    [HideInInspector] public GameManager gm;
    [HideInInspector] public PlayerManager pm;
    [HideInInspector] public InputManager im;
    [SerializeField] public PlayerInput pi;
    public CircleCollider2D SolidCol;
    public CircleCollider2D HurtBoxTrigger;
    [HideInInspector] public Gamepad gp;
    [SerializeField] RumbleController rumbleCon;

    [SerializeField] public ReticleController rc;
    [SerializeField] public Transform WarpPoint;
    public Rigidbody2D rb;
    [HideInInspector] public SpriteRenderer sr;
    public SpriteRenderer eyeSR;
    [SerializeField] public ParticleSystem bubblePart;
    [SerializeField] public GameObject hitPart;

    public Sprite[] SpriteSet;
    public Sprite[] EyeSpriteSet;
    CinemachineTargetGroup tg;

    //saved "real" velocity value for when velocity is temporarily modified (hitstop)
    [HideInInspector] public Vector2 storedVelocity;

    [HideInInspector] public Vector2 i_move;
    [HideInInspector] public Vector2 true_i_move;
    [HideInInspector] public Vector3 rotation;

    [HideInInspector] public int idx;
    [HideInInspector] public int colorID;
    [HideInInspector] public List<ItemBehavior> heldItems;
    [HideInInspector] public int selectedItemIdx;
    [HideInInspector] public bool charging;
    [HideInInspector] public float chargeTime;
    [HideInInspector] public bool specialCharging;
    [HideInInspector] public float specialChargeTime;

    float staticTimer = 0;
    [SerializeField] public int rewindSize;
    [HideInInspector] public Queue<PlayerState> prevStates;
    [HideInInspector] public List<Vector2> prevPos; //this is redundant idgaf

    //NEW movement system
    [SerializeField] public AnimationCurve moveCurve; //speed:time movement curve
    [SerializeField] public AnimationCurve speedCurve; //scaling of max speed : charge in movement
    [SerializeField] public AnimationCurve timeCurve; //scaling of time spent moving : charge in movement
    [SerializeField] public AnimationCurve powerCurve; //scaling of movement kb power : charge in movement
    [SerializeField] public AnimationCurve knockbackCurve; //scaling of relative power (movePower-otherPlayer.movePower) : "knockback-charge" applied when colliding
    [SerializeField] public AnimationCurve directnessKBCurve;
    [SerializeField] public AnimationCurve hitstopCurve; //scaling of knockback power : hitstop applied when colliding
    
    [SerializeField] public float maxMoveSpeed;
    [SerializeField] public float maxMoveTime;
    [SerializeField] public float maxMovePower;
    [SerializeField] public float maxHitstop;


    [HideInInspector] public Vector2 DIMod = Vector2.zero;

    [HideInInspector] public int movePriority = 0;

    [HideInInspector] public float moveTime; //total time to spend in the current movement instance
    [HideInInspector] public float moveTimer; //timer counting time to spend moving
    [HideInInspector] public float hitStopTimer; //timer counting time to spend in hitstop
    [HideInInspector] public float hitStopTime; //total hitstop time
    [HideInInspector] public float moveSpeed; //speed of movement
    [HideInInspector] public float movePower; //knockback to apply on collision
    [HideInInspector] public float glidePower;
    [HideInInspector] public bool isMoving;
    [HideInInspector] public bool isGliding;
    [HideInInspector] public bool isKnockback;
    [HideInInspector] public bool isHitStop;

    [HideInInspector] public float lastChargePress = 0;
    public int wallTechFrameWindow; //frame data for open wallTechWindow
    [HideInInspector] public bool canWallTech = false; //wallTech window open/closed bool






    [HideInInspector] public bool isCoolingDown;
    [HideInInspector] public bool isGrown;
    [HideInInspector] public bool isRewind;
    [HideInInspector] public bool isInBounds; //copy of pm.playerList[idx].isInBounds


    [SerializeField] public int inventorySize; //amount of items that can be held at once
    [SerializeField] public float chargeStrength;
    [SerializeField] public float maxChargeTime;
    [SerializeField] public float minCharge;
    [SerializeField] public float maxChargeHoldTime;
    [SerializeField] public float specialMoveMod;
    [SerializeField] public float OOBMoveMod;

    [SerializeField] public float knockbackMultiplier;
    //[SerializeField] float coolDownFactor;
    //[SerializeField] float moveCoolDown;
    //[SerializeField] float coolDownVelocity;
    public float DIStrength;
    public float forwardDIStrength;
    public float lateralDIStrength;
    public float maxRumbleStrength;
    public AnimationCurve rumbleCurve;

    [HideInInspector] public Vector3 defaultScale;
    [HideInInspector] public float defaultChargeStrength;

    [HideInInspector] public float defaultMaxMoveSpeed;
    [HideInInspector] public float defaultMaxMoveTime;
    [HideInInspector] public float defaultMaxMovePower;
    [HideInInspector] public float defaultMaxHitstop;


    //priority tables
    [HideInInspector] public float[] OverpowerPeerPrioTable; //overpower priority
    [HideInInspector] public float[] IntangiblePeerPrioTable; //intangible priority

    


    void Awake()
    {
        
    }

    //use this instead of Awake()
    public virtual void Init()
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

        gp = pi.GetDevice<Gamepad>();
        if (gp != null)
        {
            Debug.Log("got GP!!");
            Debug.Log("device type: " + gp.GetType().ToString());
        }

        //pi.uiInputModule = FindFirstObjectByType<InputSystemUIInputModule>();

        transform.parent = gm.transform.GetChild(0);

        idx = pm.playerList.FindIndex(i => i.input == this.gameObject.GetComponent<PlayerInput>());
        colorID = pm.FindFirstAvailableColorID(idx, 1);
        Debug.Log("player idx (from playercontroller): " + idx);


        sr = this.gameObject.GetComponent<SpriteRenderer>();
        if(sr != null)
        {
            Debug.Log("got SR!!");
        }
        rc = this.GetComponentInChildren<ReticleController>();

        OverpowerPeerPrioTable = new float[6];
        IntangiblePeerPrioTable = new float[6];

        heldItems = new List<ItemBehavior>();

        prevStates = new Queue<PlayerState>(rewindSize);
        prevPos = new List<Vector2>(3);

        ChangeColor(colorID);

        selectedItemIdx = 0;
        isCoolingDown = false;
        i_move = Vector2.zero;
        true_i_move = Vector2.zero;
        rotation = Vector3.zero;

        isRewind = false;

        //set default stats
        defaultScale = transform.localScale;
        defaultChargeStrength = chargeStrength;

        defaultMaxMoveSpeed = maxMoveSpeed;
        defaultMaxMoveTime = maxMoveTime;
        defaultMaxMovePower = maxMovePower;
        defaultMaxHitstop = maxHitstop;
        

        moveTime = 0;
        moveTimer = 0;
        hitStopTimer = 0;
        isMoving = false;
        isKnockback = false;
        isHitStop = false;
        ApplyMove(0, Vector2.zero, 0);
    }

    // Update is called once per frame
    public virtual void Update()
    {
        CoolDown();
    }

    public virtual void FixedUpdate()
    {
        if(gm.battleStarted)
        {
            TrackStatesTick();
        }

        MovementTick();
    }

    //handle players colliding
    public virtual void OnCollisionEnter2D(Collision2D col)
    {

        FindFirstObjectByType<AudioManager>().PlayRandom("Impact");


        //Debug.Log("Player colliding! Collision LayerMask name: " + LayerMask.LayerToName(col.gameObject.layer));



        if (LayerMask.LayerToName(col.gameObject.layer) == "Players")
        {
            //Debug.Log("Player" + idx + " collided with Player " + col.gameObject.GetComponent<PlayerController>().idx);

            StartCoroutine(ApplyKnockback(col.gameObject.GetComponent<PlayerController>().movePriority, col.gameObject.GetComponent<PlayerController>().movePower, col.gameObject.GetComponent<PlayerController>().rb));

        } /*else if(LayerMask.LayerToName(col.gameObject.layer) == "Items")
        {

        }*/

    }


    public virtual void ResetDefaultStats()
    {
        transform.localScale = defaultScale;
        chargeStrength = defaultChargeStrength;

        maxMoveSpeed = defaultMaxMoveSpeed;
        maxMoveTime = defaultMaxMoveTime;
        maxMovePower = defaultMaxMovePower;
        maxHitstop = defaultMaxHitstop;

        rc.transform.localScale = Vector2.one;

        rb.angularVelocity = 0;
        transform.rotation = quaternion.identity;
    }

    //REWORK NEEDED
    public virtual void Deactivate()
    {
        ResetDefaultStats();

        prevStates.Clear();

        StopAllCoroutines();
        

        bubblePart.gameObject.SetActive(false);
        
        rb.velocity = Vector2.zero;
        i_move = Vector2.zero;
        true_i_move = Vector2.zero;
        chargeTime = 0;
        charging = false;
        specialCharging = false;
        isRewind = false;
        ClearInventory();
        this.GetComponent<CircleCollider2D>().enabled = false;
        this.GetComponent<SpriteRenderer>().enabled = false;
        GetComponentInChildren<TrailRenderer>().emitting = false;
        GetComponentInChildren<TrailRenderer>().Clear();
        rc.DeactivateReticle();
        

        gp.SetMotorSpeeds(0,0);


        tg = FindFirstObjectByType<CinemachineTargetGroup>();
        if(tg != null)
        {
            tg.RemoveMember(this.transform);
        }
        
    }

    //REWORK THIS
    public virtual void Reactivate()
    {
        ResetDefaultStats();

        bubblePart.gameObject.SetActive(true);

        i_move= Vector2.zero;
        true_i_move = Vector2.zero;
        chargeTime = 0;
        charging = false;
        specialCharging = false;
        
        this.GetComponent<CircleCollider2D>().enabled = true;
        this.GetComponent<SpriteRenderer>().enabled = true;
        GetComponentInChildren<TrailRenderer>().emitting = false;
        GetComponentInChildren<TrailRenderer>().Clear();
        rc.DeactivateReticle();
        gp.SetMotorSpeeds(0,0);

        tg = FindFirstObjectByType<CinemachineTargetGroup>();
        if(tg != null)
        {
            tg.AddMember(this.transform, 1, 1);
        }
    }

    //handle player leaving in conjunction with im.OnPlayerLeave()
    public void OnControllerDisconnect(PlayerInput pi)
    {
        im.OnPlayerLeave(pi);

        gm.lc.OnPlayerLeave(idx);
    }

    //runs when d/c controller reconnected
    public void OnControllerReconnect(PlayerInput pi)
    {
        im.OnPlayerReconnect(pi);
        
        gm.lc.OnPlayerReconnect(idx);
    }

    //called when move (stick) input received
    public virtual void OnMove(InputAction.CallbackContext ctx)
    {
        //save move input
        if(ctx.ReadValue<Vector2>().magnitude != 0 && gm.battleStarted)
        {
            //holds last non-zero input, normalized
            i_move = ctx.ReadValue<Vector2>().normalized;
            
            //inverts projectile aim
            /*
            if(specialCharging && !isCoolingDown && heldItems.Any() && heldItems[selectedItemIdx].GetItemType() != "Wall" && heldItems[selectedItemIdx].GetItemType() != "Grow")
            {
                i_move = -ctx.ReadValue<Vector2>();
            }
            */

        }

        //not normalized, can equal 0
        true_i_move = ctx.ReadValue<Vector2>();

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

    //makes player look in direction
    public void RotateSprite(Vector2 dir)
    {
        rb.angularVelocity = 0;
        float angle = Vector2.SignedAngle(Vector2.up, dir);
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    //called when charge (button) input received
    public virtual void OnCharge(InputAction.CallbackContext ctx)
    {
        Debug.Log("charge: " + ctx.phase);
        if(ctx.performed && pm.playerList[idx].isInBounds) //charging
        {
            //Debug.Log("charging!!");
            

            chargeTime = 0;
            charging = true;
            StartCoroutine(Charge());

        } else if (ctx.canceled) //released
        {
            charging = false;
        }
    }

    //called when OnCharge() performed - handles player charging
    public virtual IEnumerator Charge()
    {
        if(gm.battleStarted && pm.playerList[idx].isActive && pm.playerList[idx].isAlive)
        {
            //StartCoroutine(rc.RenderReticle());
        }

        //Charging
        while (charging)
        {
            if (!isCoolingDown && pm.playerList[idx].isInBounds)
            {
                sr.sprite = SpriteSet[2]; //charging sprite
                chargeTime += Time.deltaTime;

                if(gm.battleStarted)
                {
                    
                    RotateSprite(i_move);

                    if(chargeTime/maxChargeHoldTime >= .85f)
                    {
                        gp.SetMotorSpeeds(maxRumbleStrength * 7, maxRumbleStrength * 7);
                    } else
                    {
                        gp.SetMotorSpeeds(rumbleCurve.Evaluate(Mathf.Clamp(chargeTime/maxChargeTime, 0, 1)) * maxRumbleStrength, rumbleCurve.Evaluate(Mathf.Clamp(chargeTime/maxChargeTime, 0, 1)) * maxRumbleStrength);
                    }
                }
                


                if(chargeTime > maxChargeHoldTime)
                {
                    charging = false;
                }

            } else
            {
                //failsafe prevents chargehold while stunned
                //MIGHT CAUSE PROBLEMS idk
                chargeTime = 0;
            }
            

            yield return null;
        }

        //After charging
        if(gm.battleStarted && !isCoolingDown && pm.playerList[idx].isInBounds /*&& !specialCharging*/) //perform movement during match
        {
            ApplyMove(0, i_move, chargeTime);
        } else if (!gm.battleStarted && !pm.playerList[idx].isReady)
        {
            sr.sprite = SpriteSet[0];
        }

        gp.SetMotorSpeeds(0, 0);


        //reset chargeTime
        chargeTime = 0;
    }

    //called in OnCharge - handles launch movement
    //type of movement - 0:normal 1:special
    //^^stupid workaround for demo sprint MUST FIX LATER
    /*OLD MOVEMENT SYSTEM
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
    */

    //type of movement - 0:normal 1:special
    //^^stupid workaround for demo sprint MUST FIX LATER
    public virtual Vector2 CalcMoveForce(int type)
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

        //THIS IS A PROBLEM!!!!
        charge = Mathf.Clamp(charge, minCharge, maxChargeTime);

        //max speed reached in this movement
        float calcSpeed = maxMoveSpeed * speedCurve.Evaluate(charge/maxChargeTime);
        //total time to spend moving in this movement
        float calcTime = maxMoveTime * timeCurve.Evaluate(charge/maxChargeTime);


        Vector2 moveVector = i_move.normalized * calcSpeed * calcTime;
        
        //Debug.Log("MOVEFORCE: " + moveForce.magnitude);

        if (type == 1) //cut special move length
        {
            //return moveForce * .35f;
            return moveVector * specialMoveMod;

        } else
        {
            return moveVector;
        }

        
    }


    void CoolDown()
    {
        //TEMP FIX - NEEDS MORE SIGNIFICANT REWORKING
        if(moveTimer/moveTime < .9f || isRewind)
        {
            isCoolingDown = true;
        } else
        {
            isCoolingDown = false;

            //reset sprite after cooldown
            if(!charging && !isHitStop)
            {
                sr.sprite = SpriteSet[0];
            }
        }
    }

    public virtual void OnSpecial(InputAction.CallbackContext ctx)
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
            if(heldItems.Any() && heldItems[selectedItemIdx].GetItemType() == "Warp")
            {
                StartCoroutine(rc.RenderWarpReticle());
            } else if(heldItems.Any() && heldItems[selectedItemIdx].GetItemType() == "Rewind")
            {
                StartCoroutine(rc.RenderRewindReticle());
            } else
            {
                StartCoroutine(rc.RenderReticle());
            }
        }


        //Special Charging
        while (specialCharging)
        {
            if (!isCoolingDown)
            {
                sr.sprite = SpriteSet[2]; //charging sprite
                specialChargeTime += Time.deltaTime;

                if(gm.battleStarted)
                {
                    if(specialChargeTime/maxChargeHoldTime >= .85f)
                    {
                        gp.SetMotorSpeeds(maxRumbleStrength * 7, maxRumbleStrength * 7);
                    } else
                    {
                        gp.SetMotorSpeeds(rumbleCurve.Evaluate(Mathf.Clamp(specialChargeTime/maxChargeTime, 0, 1)) * maxRumbleStrength, rumbleCurve.Evaluate(Mathf.Clamp(specialChargeTime/maxChargeTime, 0, 1)) * maxRumbleStrength);
                    }
                }


                if(specialChargeTime > maxChargeHoldTime)
                {
                    specialCharging = false;
                }

            } else
            {
                specialChargeTime = 0;
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
                    //Debug.Log("WALL MOVEMENT");
                    ApplyMove(0, i_move, Mathf.Clamp(specialChargeTime, 0, maxChargeTime));
                } else if(heldItems[selectedItemIdx].GetItemType() != "Warp")
                {
                    ApplyMove(0, i_move, specialMoveMod * Mathf.Clamp(specialChargeTime, 0, maxChargeTime));
                }
                
            }else if(isInBounds)
            {
                ApplyMove(0, i_move, specialMoveMod * Mathf.Clamp(specialChargeTime, minCharge, maxChargeTime));
            }

            UseItem(selectedItemIdx);
        } else if (!gm.battleStarted && !pm.playerList[idx].isReady)
        {
            sr.sprite = SpriteSet[0];
        }

        gp.SetMotorSpeeds(0, 0);

        //reset chargeTime
        specialChargeTime = 0;
    }

    public void GainItem(ItemBehavior item)
    {
        heldItems.Add(item);
        //Debug.Log("player" + idx + " gained an item!");
        
    }

    //uses item in heldItems[idx]
    void UseItem(int idx)
    {
        if(heldItems.Any())
        {
            heldItems[idx].UseItem(specialChargeTime);
            heldItems.RemoveAt(idx);
        }
    }

    void ClearInventory()
    {
        
        foreach (ItemBehavior item in heldItems)
        {
            if(item != null)
            {
                item.DestroyItem();
            }
        }
        heldItems.Clear();
        
    }

    public void OnSelectL(InputAction.CallbackContext ctx)
    {

        //whatever L does during gameplay
        //maybe swaps between items idk

    }

    public void OnSelectR(InputAction.CallbackContext ctx)
    {
        //whatever R does during gameplay
        //maybe swaps between items idk
      
    }

    //change color - updates sprite
    public void ChangeColor(int color)
    {
        colorID = color;

        sr.color = pm.playerColors[color];
        sr.sprite = SpriteSet[0];

        rc.ChangeColor(color);

        pm.SetPlayerColor(idx, color);
    }

    //This is reduntant.  Use pm.ReadyPlayer(idx)
    /*public void ReadyUp()
    {
        sr.sprite = spriteSet[2]; //charging sprite doubles as ready sprite
        pm.ReadyPlayer(idx);
    }*/



    //NEW movement system stuff below


    //processes player movement
    //called in FixedUpdate()
    public virtual void MovementTick()
    {
        if(moveTimer <= moveTime)
        {
            if(!isHitStop)
            {

                if(isKnockback)
                {
                    DIMod = 1.1f * DIStrength * true_i_move;
                } else
                {
                    DIMod = true_i_move * DIStrength;
                }
                
                rb.velocity = moveCurve.Evaluate(moveTimer/moveTime) * moveSpeed * (rb.velocity.normalized + DIMod);

                //ADD THIS: should only rotate sprite if in player-inputted movement, i.e. NOT in knockback state
                if(isMoving && !isKnockback && isCoolingDown)
                {
                    RotateSprite(rb.velocity.normalized);
                    //not a great fix but it works
                    sr.sprite = SpriteSet[1];
                }

                moveTimer += Time.fixedDeltaTime;
            }
            
        } else 
        {
            //movement is over -> reset movement tracking variables
            if(isMoving)
            {
                isMoving = false;
            }

            if(isKnockback)
            {
                isKnockback = false;
            }
            moveTime = 0;
            moveSpeed = 0;

            if(!isRewind)
            {
                movePower = 0;
                rb.velocity = Vector2.zero;
            }

            //may cause problems in the future
            sr.sprite = SpriteSet[0];

        }

        //failsafe
        if(!isMoving && !isKnockback && !isRewind)
        {
            rb.velocity = Vector2.zero;

            //strafing test
            if(true_i_move.magnitude > 0)
            {
                //add strafe speed var here
                rb.velocity = true_i_move * .5f;

            }

        }

    }

    //basic movement function
    //updates movement variables which are processed in MovementTick()
    //pauses during hitstop -> continues after stop is over
    //type 0 -> player-inputted movement
    //type 1 -> attack knockback/launching    
    public virtual void ApplyMove(int type, Vector2 direction, float charge)
    {
        //Debug.Log("player" + idx + " moving!");
        //Debug.Log("type = " + type);
        //Debug.Log("direction = " + direction);
        //Debug.Log("charge = " + charge);

        if(type == 0) //movement
        {
            FindFirstObjectByType<AudioManager>().PlayRandom("Move");

            isMoving = true;
            sr.sprite = SpriteSet[1];
            charge = Mathf.Clamp(charge, minCharge, maxChargeTime);
        } else if(type == 1) //knockback
        {
            isKnockback = true;
            //SET SPRITE TO STUNNED
            charge = Mathf.Clamp(charge, 0, maxChargeTime * 2);
        }

        //max speed reached in this movement
        moveSpeed = maxMoveSpeed * speedCurve.Evaluate(charge/maxChargeTime);
        //total time to spend moving in this movement
        moveTime = maxMoveTime * timeCurve.Evaluate(charge/maxChargeTime);
        //knockback power of this movement
        movePower = maxMovePower * powerCurve.Evaluate(charge/maxChargeTime);

        //probably not needed in this context
        rb.velocity = direction.normalized;

        //reset moveTimer to beginning
        moveTimer = 0;

        EmitBubbles(.9f * moveTime);
    }




    //coroutine applies hitstop for specified time
    public IEnumerator HitStop(float time)
    {
        if(!isHitStop)
        {
            Debug.Log("P" + idx + " entering hitstop for " + time + " seconds");

            WaitForFixedUpdate fuWait = new WaitForFixedUpdate();
            //for collisions just in case
            //yield return fuWait;

            isHitStop = true;
            storedVelocity = rb.velocity;

            float timer = 0;
            while (timer < time)
            {
                rb.velocity = Vector2.zero;

                timer += Time.fixedDeltaTime;
                yield return fuWait;
            }
            rb.velocity = storedVelocity;
            isHitStop = false;

        }
    }


    //applies knockback effects to this player - does nothing to other colliding player
    //called in OnCollisionEnter2D when colliding with opponent
    public virtual IEnumerator ApplyKnockback(int otherPriority, float otherPower, Rigidbody2D otherRB)
    {
        //Debug.Log("ApplyKnockback!");

        //REPLACE THIS WITH STUNNED SPRITE!!!!!!!!!!!!
        sr.sprite = SpriteSet[0];

        GetComponentInChildren<TrailRenderer>().emitting = false; //cancel wall item on impact


        //difference in powers
        float powerDiff = movePower - otherPower;

        //angle players are colliding:
            //1 = moving straight at each other
            //0 = moving in same direction
        float directionDiff = Vector2.Angle(rb.velocity, otherRB.velocity) / 180;

        //relative positions
        //vector pointing from player's position to otherPlayer's position i.e. relative position (direction only)
        Vector2 posDiff = (otherRB.position - rb.position).normalized;
        //vector pointing from otherPlayer's position to this player's position
        Vector2 otherPosDiff = (rb.position - otherRB.position).normalized;

        //how much of a "direct hit" it is
            //1 = direct hit
            //0 = indirect hit
        //animationCurve used to level out "almost direct hits" and keep value > 0
        float directness = 0;
        if(rb.velocity != Vector2.zero)
        {
            directness = directnessKBCurve.Evaluate(Vector2.Angle(posDiff, rb.velocity) / 180);
        }

        float otherDirectness = 0;
        if(otherRB.velocity != Vector2.zero)
        {
            otherDirectness = directnessKBCurve.Evaluate(Vector2.Angle(otherPosDiff, otherRB.velocity) / 180);
        }

        float directnessRatio = otherDirectness/directness;

        //Debug.Log("Player" + idx + " directness: " + directness);
        //Debug.Log("Player" + idx + " movePower: " + movePower);


        //overall strength: takes directness and power into account
        float strength = directness * movePower;
        float otherStrength = otherDirectness * otherPower;

        float strengthDiff = otherStrength - strength;
        float strengthRatio = otherStrength / strength;

        //calculate knockback direction
        Vector2 direction;

        //if(powerDiff and directness are strong enough)
        //TWEAK THESE VALUES
        /*
        if(otherDirectness > .9 && powerDiff < 0)
        {
            //launch in direction of impact
            direction = otherRB.velocity.normalized;
        } else
        {
            //TRY CHANGING THIS
            //direction = (rb.velocity.normalized * movePower) + (otherPlayer.rb.velocity.normalized * otherPlayer.movePower);
            direction = rb.velocity.normalized;
        }
        */
        //apply hitstop
        float hitstop = (strength > otherStrength) ? (strength/maxMovePower) : (otherStrength/maxMovePower);
        StartCoroutine(HitStop(maxHitstop * hitstopCurve.Evaluate(hitstop)));

        Vector2 otherImpactDirection = otherRB.velocity.normalized;
        yield return new WaitForFixedUpdate();

        //direction = (otherStrength * pre-impact otherPlayer.direction) + (strenght * post-impact thisPlayer.direction)
        //direction = ((movePower * rb.velocity.normalized) + (otherStrength * otherImpactDirection)).normalized;
        direction = rb.velocity.normalized;

        //calculate knockback strength/charge



        

        //impact particle effect
        

        //particle behavior for non-player hitstop collisions
        if(LayerMask.LayerToName(otherRB.gameObject.layer) != "Players")
        {
            if(otherRB.TryGetComponent<ShotObj>(out ShotObj shot))
            {
                shot.StartCoroutine(shot.HitStop(maxHitstop * hitstopCurve.Evaluate(hitstop)));

                if(hitstop >= .3f)
                {
                    GameObject part = Instantiate(hitPart, (transform.position + otherRB.transform.position)/2, Quaternion.identity);
                    part.GetComponent<HitEffect>().Init(.5f, maxHitstop * hitstopCurve.Evaluate(hitstop)*1.1f);
                }
            }
        } else if(idx < otherRB.gameObject.GetComponent<PlayerController>().idx)
            {
                if(hitstop >=.3f)
                {
                    GameObject part = Instantiate(hitPart, (transform.position + otherRB.transform.position)/2, Quaternion.identity);
                    part.GetComponent<HitEffect>().Init(.5f, maxHitstop * hitstopCurve.Evaluate(hitstop)*1.1f);
                }
            }

        
        if(!isRewind)
        {
            //apply movement - type 1
            //need to calculate direction and charge
            ApplyMove(1, direction, knockbackMultiplier * otherStrength);
        }
        

    }


    public void EmitBubbles(float time)
    {
        //Debug.Log("bubbles emitting");
        bubblePart.Stop();
        var main = bubblePart.main;
        main.duration = time;
        bubblePart.Play();
    }

    //called in FixedUpdate if gm.gamestarted
    void TrackStatesTick()
    {
        //always store state if moving
        if(isMoving || isKnockback || isRewind)
        {
            staticTimer = 0;

            PlayerState s = new PlayerState(transform.position.x, transform.position.y, movePower);
            prevStates.Enqueue(s);
        } else
        {
            //store .2s of states when stationary
            if(staticTimer <= .25f)
            {
                PlayerState s = new PlayerState(transform.position.x, transform.position.y, movePower);
                prevStates.Enqueue(s);
            }
            staticTimer += Time.fixedDeltaTime;
        }

        //prevent overfilling
        while(prevStates.Count > rewindSize)
        {
            prevStates.Dequeue();
        }

        //Debug.Log("prevStates.Count: " + prevStates.Count);
        //Debug.Log("prevStates: " + prevStates);
    }

    public virtual void OnHurtboxTriggerEnter(Collider2D col)
    {
        return;
    }


}
