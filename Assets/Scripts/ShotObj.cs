using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor;

public class ShotObj : MonoBehaviour
{
    float activeTimer;
    Rigidbody2D rb;

    [SerializeField] float maxChargeTime;
    [SerializeField] float maxSpeed;
    [SerializeField] float maxPower;

    [SerializeField] AnimationCurve powerCurve;
    [SerializeField] AnimationCurve speedCurve;

    float shotSpeed;
    float shotPower;
    bool isHitStop;

    // Start is called before the first frame update
    void Start()
    {
        rb = this.GetComponent<Rigidbody2D>();
        activeTimer = 0;

        //shotSpeed = 0;
        //shotPower = 0;
    }

    // Update is called once per frame
    void Update()
    {
        activeTimer += Time.deltaTime;
    }

    void FixedUpdate()
    {
        rb.velocity = rb.velocity.normalized * shotSpeed;
        Debug.Log("shotspeed = " + shotSpeed);
        Debug.Log("velocity = " + rb.velocity);
    }


    void OnCollisionEnter2D(Collision2D col)
    {
        if (LayerMask.LayerToName(col.gameObject.layer) == "Players")
        {
            PlayerController pc = col.gameObject.GetComponent<PlayerController>();

            pc.StartCoroutine(pc.ApplyKnockback(shotPower, rb));
        }
    }

    //sets shot speed and power based on chargeTime
    public void Shoot(float chargeTime)
    {  
        chargeTime = Mathf.Clamp(chargeTime, 0, maxChargeTime);

        rb = this.GetComponent<Rigidbody2D>();
        rb.velocity = -transform.up;

        shotPower = maxPower * powerCurve.Evaluate(chargeTime/maxChargeTime);
        shotSpeed = maxSpeed * speedCurve.Evaluate(chargeTime/maxChargeTime);
        
        Debug.Log("SHOT SPEED: " + shotSpeed);
    }


    IEnumerator HitStop(float time)
    {
        if(!isHitStop)
        {
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
}
