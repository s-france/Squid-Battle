using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor;
using UnityEngine.UIElements;

public class ShotObj : MonoBehaviour
{
    [HideInInspector] public int parentID;
    [HideInInspector] public int immunityID;
    [SerializeField] float immunityTime;
    [HideInInspector] public float immunityTimer = 0;
    [HideInInspector] public bool timerStarted = false;

    [HideInInspector] public float activeTimer;
    public Rigidbody2D rb;

    public CircleCollider2D TriggerHB;
    [HideInInspector] public List<Vector2> prevPos;


    [SerializeField] float maxChargeTime;
    [SerializeField] float maxSpeed;
    public float maxPower;

    [SerializeField] AnimationCurve powerCurve;
    [SerializeField] AnimationCurve speedCurve;

    float shotSpeed;
    [HideInInspector] public float shotPower;
    [HideInInspector] public bool isHitStop;
    float hitStopTimer;
    float hitStopTime;
    [HideInInspector] public Vector2 storedVelocity;

    // Start is called before the first frame update
    void Start()
    {
        rb = this.GetComponent<Rigidbody2D>();
        activeTimer = 0;

        prevPos = new List<Vector2>(3);
        prevPos.Add(transform.position);
        prevPos.Add(transform.position);


        //shotSpeed = 0;
        //shotPower = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if(timerStarted)
        {
            immunityTimer += Time.deltaTime;
        } else
        {
            immunityTimer = 0;
        }

        if(immunityTimer > immunityTime)
        {
            //remove immunity
            immunityID = -1;
        }

        activeTimer += Time.deltaTime;
    }

    void FixedUpdate()
    {
        rb.velocity = rb.velocity.normalized * shotSpeed;

        TrackStatesTick();
        HitStopTick();
        MovementTick();

    }


    //tracks previous states 
    //used in trigger collision correction
    void TrackStatesTick()
    {
        //storing previous position for use in trigger collision corrections
        //prevent overfilling
        while(prevPos.Count >= 2)
        {
            prevPos.RemoveAt(0);
        }
        //save previous position
        prevPos.Add(transform.position);

        //Debug.Log("P" + idx + " transform.position " + transform.position);
        //Debug.Log("P" + idx + " prevPos[0]: " + prevPos[0]);
        //Debug.Log("P" + idx + " prevPos[1]: " + prevPos[1]);
    }


    void MovementTick()
    {
        if(!isHitStop)
        {
            //constant speed
            rb.velocity = shotSpeed * rb.velocity.normalized;
            //rotate sprite
            RotateSprite(-rb.velocity.normalized);
        } 

    }


    //new hitstop system tracked in FixedUpdate()
    void HitStopTick()
    {
        if(hitStopTimer < hitStopTime)
        {
            isHitStop = true;            
            rb.velocity = Vector2.zero;
            hitStopTimer += Time.fixedDeltaTime;
        } else
        {
            if(isHitStop) //first frame out of hitstop
            {
                rb.velocity = storedVelocity;
            }

            isHitStop = false;
        }
    }


    public void ApplyHitStop(float time)
    {
        if(!isHitStop)
        {
            isHitStop = true;
            storedVelocity = rb.velocity;
            rb.velocity = Vector2.zero;
        }

        hitStopTime = time;
        hitStopTimer = 0;

    }


    //simplified ApplyMove() used when bouncing off players and other trigger HBs
    public void ApplyMove(Vector2 direction)
    {
        //change direction
        rb.velocity = rb.velocity.magnitude * direction;
        storedVelocity = storedVelocity.magnitude * direction;
    }


    /*//old movement sys behavior
    void OnCollisionEnter2D(Collision2D col)
    {
        if (LayerMask.LayerToName(col.gameObject.layer) == "Players")
        {
            PlayerController pc = col.gameObject.GetComponent<PlayerController>();

            pc.StartCoroutine(pc.ApplyKnockback(2, shotPower, rb));
            //StartCoroutine(HitStop());
        }
    }
    */

    //sets shot speed and power based on chargeTime
    public void Shoot(float chargeTime, Vector2 direction)
    {
        chargeTime = Mathf.Clamp(chargeTime, 0, maxChargeTime);

        rb = this.GetComponent<Rigidbody2D>();
        rb.velocity = direction.normalized;

        shotPower = maxPower * powerCurve.Evaluate(chargeTime/maxChargeTime);
        shotSpeed = maxSpeed * speedCurve.Evaluate(chargeTime/maxChargeTime);
        
        Debug.Log("SHOT SPEED: " + shotSpeed);
    }


    void OnTriggerExit2D(Collider2D col)
    {
        PlayerController pc = col.GetComponentInParent<PlayerController>();
        if(pc != null && pc.idx == immunityID)
        {
            //start immunity timer
            timerStarted = true;
        }

    }

    void OnTriggerEnter2D(Collider2D col)
    {
        PlayerController pc = col.GetComponentInParent<PlayerController>();
        if(pc != null && pc.idx == immunityID)
        {
            //reset timer
            timerStarted = false;

        }

    }


    public IEnumerator HitStop(float time)
    {
        if(!isHitStop)
        {
            Debug.Log("Shot Hitstopping!!");
            
            WaitForFixedUpdate fuWait = new WaitForFixedUpdate();
            //for collisions just in case
            yield return fuWait;

            isHitStop = true;
            Vector2 initialVelocity = rb.velocity;

            float timer = 0;
            while (timer < time)
            {
                rb.velocity = Vector2.zero;

                timer += Time.fixedDeltaTime;
                yield return fuWait;
            }
            isHitStop = false;

            rb.velocity = initialVelocity;
        
        
        }
    }

    public void RotateSprite(Vector2 dir)
    {
        rb.angularVelocity = 0;
        float angle = Vector2.SignedAngle(Vector2.up, dir);
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }
}
