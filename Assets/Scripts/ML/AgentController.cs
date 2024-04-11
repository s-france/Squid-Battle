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
//using System.Numerics;
//using System.Numerics;

public class AgentController : PlayerController
{
    [SerializeField] MLAgent mla;
    [SerializeField] Sprite[] agentSprites;
    /*
    
    InputManager im;
    [SerializeField] PlayerInput pi;
    //[HideInInspector] public Gamepad gp;

    [SerializeField] ReticleController rc;
    //public Rigidbody2D rb;
    [SerializeField] private SpriteRenderer sr;
    [SerializeField] ParticleSystem bubblePart;
    [SerializeField] GameObject hitPart;

    [SerializeField] private Sprite[] spriteSet;

    //[HideInInspector] public Vector2 i_move;
    //[HideInInspector] public Vector2 true_i_move;
    Vector3 rotation;

    public int idx;
    //[HideInInspector] public int colorID;
    //[HideInInspector] public bool charging;
    //[HideInInspector] public float chargeTime;

    //NEW movement system
    [SerializeField] AnimationCurve moveCurve; //speed:time movement curve
    [SerializeField] AnimationCurve speedCurve; //scaling of max speed : charge in movement
    [SerializeField] AnimationCurve timeCurve; //scaling of time spent moving : charge in movement
    [SerializeField] AnimationCurve powerCurve; //scaling of movement kb power : charge in movement
    [SerializeField] AnimationCurve knockbackCurve; //scaling of relative power (movePower-otherPlayer.movePower) : "knockback-charge" applied when colliding
    [SerializeField] AnimationCurve directnessKBCurve;
    [SerializeField] AnimationCurve hitstopCurve; //scaling of knockback power : hitstop applied when colliding
    
    //[SerializeField] public float maxMoveSpeed;
    //[SerializeField] public float maxMoveTime;
    //[SerializeField] public float maxMovePower;
    //[SerializeField] public float maxHitstop;


    //[HideInInspector] public float moveTime; //total time to spend in the current movement instance
    //[HideInInspector] public float moveTimer; //timer counting time to spend moving
    float hitStopTimer; //timer counting time to spend in hitstop
    float moveSpeed; //speed of movement
    //[HideInInspector] public float movePower; //knockback to apply on collision
    //[HideInInspector] public bool isMoving;
    bool isKnockback;
    bool isHitStop;
    Coroutine MoveCR;


    //[HideInInspector] public bool isCoolingDown;


    //[SerializeField] public float chargeStrength;
    //[SerializeField] public float maxChargeTime;
    //[SerializeField] public float minCharge;
    [SerializeField] float maxChargeHoldTime;
    //[SerializeField] public float knockbackMultiplier;
    [SerializeField] float coolDownFactor;
    [SerializeField] float moveCoolDown;
    [SerializeField] float coolDownVelocity;

    //[HideInInspector] public Vector3 defaultScale;
    //[HideInInspector] public float defaultChargeStrength;

    //[HideInInspector] public float defaultMaxMoveSpeed;
    //[HideInInspector] public float defaultMaxMoveTime;
    //[HideInInspector] public float defaultMaxMovePower;
    //[HideInInspector] public float defaultMaxHitstop;

    */
    
    //[HideInInspector] public bool isInBounds;
    [HideInInspector] public Vector2 v_move = Vector2.zero;
    [HideInInspector] public int i_charge = 0;

    void Awake()
    {
        isInBounds = true;
        spriteSet = agentSprites;
    }

    //use this instead of Awake()
    public override void Init()
    {
        DontDestroyOnLoad(this);
        spriteSet = agentSprites;

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


        gp = pi.GetDevice<Gamepad>();
        if (gp != null)
        {
            Debug.Log("got GP!!");
            Debug.Log("device type: " + gp.GetType().ToString());
        }


        colorID = idx;
        Debug.Log("player idx (from playercontroller): " + idx);


        sr = this.gameObject.GetComponent<SpriteRenderer>();
        if(sr != null)
        {
            Debug.Log("got SR!!");
        }
        rc = this.GetComponentInChildren<ReticleController>();


        isCoolingDown = false;
        i_move = Vector2.zero;
        true_i_move = Vector2.zero;
        rotation = Vector3.zero;

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


    public override void Update()
    {
        base.Update();
    }

    public override void FixedUpdate()
    {
        MovementTick();
    }

    //handle players colliding
    public override void OnCollisionEnter2D(Collision2D col)
    {
        base.OnCollisionEnter2D(col);
    }



    public override void ResetDefaultStats()
    {
        base.ResetDefaultStats();
    }

    //REWORK NEEDED
    public override void Deactivate()
    {
        ResetDefaultStats();

        bubblePart.gameObject.SetActive(false);

        rb.velocity = Vector2.zero;
        i_move = Vector2.zero;
        true_i_move = Vector2.zero;
        chargeTime = 0;
        charging = false;
        this.GetComponent<CircleCollider2D>().enabled = false;
        this.GetComponent<SpriteRenderer>().enabled = false;
        GetComponentInChildren<TrailRenderer>().Clear();

        
    }

    //REWORK THIS
    public override void Reactivate()
    {
        ResetDefaultStats();

        bubblePart.gameObject.SetActive(true);

        i_move= Vector2.zero;
        true_i_move = Vector2.zero;
        chargeTime = 0;
        charging = false;
        
        this.GetComponent<CircleCollider2D>().enabled = true;
        this.GetComponent<SpriteRenderer>().enabled = true;
        GetComponentInChildren<TrailRenderer>().Clear();
        
    }



    //called when move (stick) input received
    public override void OnMove(InputAction.CallbackContext ctx)
    {
        v_move = ctx.ReadValue<Vector2>();
    }

    public virtual void OnMove(Vector2 Vmove)
    {
        //save move input
        if(Vmove.magnitude != 0)
        {
            i_move = Vmove.normalized;
        }

        true_i_move = Vmove.normalized;

        //Debug.Log(i_move);

        //rotate sprite
        if(!isCoolingDown)
        {
            RotateSprite(i_move);
        }
    }

    public override void OnCharge(InputAction.CallbackContext ctx)
    {
        if(ctx.performed)
        {
            i_charge = 1;
        } else if(ctx.canceled)
        {
            i_charge = 0;
        }
    }

    //called when charge (button) input received
    public virtual void OnCharge(int Icharge)
    {
        //Debug.Log(ctx.phase);
        if(Icharge == 1 && isInBounds) //charging
        {
            Debug.Log("charging!!");
            if(!charging)
            {
                chargeTime = 0;
                charging = true;
                StartCoroutine(Charge());
            }
            

        } else if (Icharge == 0) //released
        {
            charging = false;
        }
    }

    //called when OnCharge() performed - handles player charging
    public override IEnumerator Charge()
    {

        StartCoroutine(rc.RenderReticle());

        //Charging
        while (charging)
        {
            yield return null;

            if (!isCoolingDown && isInBounds)
            {
                sr.sprite = spriteSet[2]; //charging sprite
                chargeTime += Time.deltaTime;

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
            
            
        }

        //After charging
        if(!isCoolingDown && isInBounds /*&& !specialCharging*/) //perform movement during match
        {
            ApplyMove(0, i_move, chargeTime);
            i_charge = 0;
        }


        //reset chargeTime
        chargeTime = 0;
    }


    //type of movement - 0:normal 1:special
    //^^stupid workaround for demo sprint MUST FIX LATER
    public override Vector2 CalcMoveForce(int type)
    {
        return CalcMoveForce();
    }

    Vector2 CalcMoveForce()
    {
       return base.CalcMoveForce(0);        
    }


    //NEW movement system stuff below


    //processes player movement
    //called in FixedUpdate()
    public override void MovementTick()
    {
        if(moveTimer <= moveTime)
        {
            if(!isHitStop)
            {
                
                rb.velocity = moveCurve.Evaluate(moveTimer/moveTime) * moveSpeed * rb.velocity.normalized;

                //ADD THIS: should only rotate sprite if in player-inputted movement, i.e. NOT in knockback state
                if(isMoving && !isKnockback)
                {
                    RotateSprite(rb.velocity.normalized);
                    //not a great fix but it works
                    sr.sprite = spriteSet[1];
                }

                moveTimer += Time.fixedDeltaTime;
            }
            
        } else 
        {
            //movement is over -> reset movement tracking variables
            if(isMoving)
            {
                isMoving = false;
                moveTime = 0;
                movePower = 0;
                moveSpeed = 0;

                //may cause problems in the future
                rb.velocity = Vector2.zero;
                sr.sprite = spriteSet[0];
            }

            if(isKnockback)
            {
                isKnockback = false;
                moveTime = 0;
                movePower = 0;
                moveSpeed = 0;

                //may cause problems in the future
                rb.velocity = Vector2.zero;
                sr.sprite = spriteSet[0];
            }


        }

        //failsafe
        if(!isMoving && !isKnockback)
        {
            rb.velocity = Vector2.zero;
        }

        

    }

    //basic movement function
    //updates movement variables which are processed in MovementTick()
    //pauses during hitstop -> continues after stop is over
    //type 0 -> player-inputted movement
    //type 1 -> attack knockback/launching
    public override void ApplyMove(int type, Vector2 direction, float charge)
    {
        base.ApplyMove(type, direction, charge);
    }


    //applies knockback effects to this player - does nothing to other colliding player
    //called in OnCollisionEnter2D when colliding with opponent
    public override IEnumerator ApplyKnockback(float otherPower, Rigidbody2D otherRB)
    {
        return base.ApplyKnockback(otherPower, otherRB);
    }


    public virtual IEnumerator PlayerKillClock(float timer)
    {
        float clock = 0;
        
        while(clock < timer && !isInBounds)
        {
            //add dying animation here
            
            
            clock += Time.deltaTime;
            yield return null;
        }

        if(!isInBounds)
        {
            mla.KillAgent();
        }

    }
 

}
