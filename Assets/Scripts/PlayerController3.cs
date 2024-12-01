using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;



//using System.Numerics;
using System.Runtime.Serialization;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController3 : PlayerController
{
    bool chargePressed = false;
    int movePriority = 0;
    int movePhase = 0;


    bool isGliding = false;
    float glideTimer = 0;
    float glideTime = 0;
    float glidePower = 0;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    /*
    // Update is called once per frame
    void Update()
    {
        
    }
    */

    public override void FixedUpdate()
    {
        //storing previous position for use in trigger collision corrections
        //prevent overfilling
        while(prevPos.Count >= 2)
        {
            prevPos.RemoveAt(0);
        }
        //save previous position
        prevPos.Add(transform.position);
       
        

        ChargeTick();
        MovementTick();


    }


    //called when move (stick) input received
    public override void OnMove(InputAction.CallbackContext ctx)
    {
        //save move input
        if(ctx.ReadValue<Vector2>().magnitude != 0 && gm.battleStarted)
        {
            i_move = ctx.ReadValue<Vector2>();
            
            //inverts projectile aim
            /*
            if(specialCharging && !isCoolingDown && heldItems.Any() && heldItems[selectedItemIdx].GetItemType() != "Wall" && heldItems[selectedItemIdx].GetItemType() != "Grow")
            {
                i_move = -ctx.ReadValue<Vector2>();
            }
            */

        }

        true_i_move = ctx.ReadValue<Vector2>();

        //Debug.Log(i_move);

        //rotate sprite
        if(!isCoolingDown)
        {
            RotateSprite(i_move);
        }
    }



    //called when charge (button) input received
    public override void OnCharge(InputAction.CallbackContext ctx)
    {
        
        Debug.Log("charge: " + ctx.phase);
        if(ctx.performed && pm.playerList[idx].isInBounds) //charging
        {
            //Debug.Log("charging!!");
            

            //chargeTime = 0;
            //charging = true;
            //StartCoroutine(Charge());
            chargePressed = true;

        } else if (ctx.canceled) //released
        {
            //charging = false;
            chargePressed = false;

            if(!isMoving)
            {
                ApplyMove(0, i_move, chargeTime);
            }
        }
    }


    /*
    public override void OnSpecial(InputAction.CallbackContext ctx)
    {
        if(ctx.performed) //special charging
        {
            specialChargeTime = 0;
            specialCharging = true;
            //StartCoroutine(SpecialCharge());
            
        } else if (ctx.canceled) //released
        {
            specialCharging = false;
        }
    }
    */

    
    void ChargeTick()
    {
        if(chargePressed)
        {
            if(pm.playerList[idx].isInBounds && !isMoving && !isKnockback && !isHitStop && !isRewind/*charge conditions: end of movement or gliding, not hitstop, not stunned in any way*/ )
            {
                charging = true;
            } else
            {
                charging = false;
            }

            if (charging)
            {
                //REPLACE WITH THIS!!!:
                //charge function
                ///based on state: normal, big, offstage, turbo, etc
                ///render reticle function
                ///


                //rc.RenderNormalReticle();


                sr.sprite = spriteSet[2]; //charging sprite
                chargeTime += Time.deltaTime;

                RotateSprite(i_move);

                if(gm.battleStarted)
                {
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

            }
            

        } else //charge button not pressed
        {
            chargeTime = 0;
            charging = false;

        }

    }
    

    public override void MovementTick()
    {
        /*
        if(isHitStop)
        {
            return;
        }
        */

        if(!isHitStop)
        {
            Vector2 direction = rb.velocity.normalized;

            //DI stuff
            Vector2 lateralDI;
            Vector2 forwardDI;
            if(true_i_move != Vector2.zero)
            {
                lateralDI = Vector2.Perpendicular(rb.velocity).normalized * Mathf.Sin(Mathf.Deg2Rad * Vector2.SignedAngle(rb.velocity, true_i_move));
                forwardDI = rb.velocity.normalized * Mathf.Cos(Mathf.Deg2Rad * Vector2.SignedAngle(rb.velocity, true_i_move));
            } else
            {
                lateralDI = Vector2.zero;
                forwardDI = Vector2.zero;
            }
            
            if(isKnockback)
            {
                DIMod = 1.1f * DIStrength * ((lateralDIStrength * lateralDI) + (forwardDIStrength * forwardDI)) /*true_i_move*/;
            } else
            {
                DIMod = (lateralDIStrength * lateralDI) * DIStrength;
            }

            if(moveTimer <= moveTime)
            {
                isMoving = true;
                movePhase = 2;
                
                rb.velocity = moveCurve.Evaluate(moveTimer/moveTime) * moveSpeed * (rb.velocity.normalized + DIMod);

                if(isMoving && !isKnockback && isCoolingDown)
                {
                    RotateSprite(rb.velocity.normalized);
                    //not a great fix but it works
                    sr.sprite = spriteSet[1];
                }

                moveTimer += Time.fixedDeltaTime;
                
                
            } else if(glideTimer <= glideTime)
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

                //moveTime = 0;
                //moveSpeed = 0;

                movePhase = 1;
                isGliding = true;

                rb.velocity = moveCurve.Evaluate(1 + (glideTimer/glideTime)) * moveSpeed * rb.velocity.normalized + DIMod;

                glideTimer += Time.fixedDeltaTime;                


                //may cause problems in the future
                //sr.sprite = spriteSet[0];
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

                if(isGliding)
                {
                    isGliding = false;
                }
                
                glideTime = 0;
                moveTime = 0;
                moveSpeed = 0;

                if(!isRewind)
                {
                    movePower = 0;
                    glidePower = 0;
                    rb.velocity = Vector2.zero;
                }

                //may cause problems in the future
                sr.sprite = spriteSet[0];


            }



        }
        

        //failsafe
        if(!isMoving && !isKnockback && !isGliding && !isRewind)
        {
            rb.velocity = Vector2.zero;
        }

    }


    public override void ApplyMove(int type, Vector2 direction, float charge)
    {
        //Debug.Log("player" + idx + " moving!");
        //Debug.Log("type = " + type);
        //Debug.Log("direction = " + direction);
        //Debug.Log("charge = " + charge);

        if(type == 0) //movement
        {
            FindFirstObjectByType<AudioManager>().PlayRandom("Move");

            isMoving = true;
            sr.sprite = spriteSet[1];
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

        //new glide stuff
        glidePower = .25f * movePower;
        glideTime = 3 * moveTime;

        //probably not needed in this context
        rb.velocity = direction.normalized;

        //reset moveTimer to beginning
        moveTimer = 0;
        glideTimer = 0;

        EmitBubbles(.9f * moveTime);
    }


    //applies knockback effects to this player - does nothing to other colliding player
    //called in OnCollisionEnter2D when colliding with opponent
    public override IEnumerator ApplyKnockback(float otherPower, Rigidbody2D otherRB)
    {
        //Debug.Log("ApplyKnockback!");

        //REPLACE THIS WITH STUNNED SPRITE!!!!!!!!!!!!
        sr.sprite = spriteSet[0];

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
            directness = directnessKBCurve.Evaluate(1 - (Vector2.Angle(posDiff, rb.velocity) / 180));
        }

        float otherDirectness = 0;
        if(otherRB.velocity != Vector2.zero)
        {
            otherDirectness = directnessKBCurve.Evaluate(1 - (Vector2.Angle(otherPosDiff, otherRB.velocity) / 180));
        }

        float directnessRatio = otherDirectness/directness;

        //Debug.Log("Player" + idx + " directness: " + directness);
        //Debug.Log("Player" + idx + " movePower: " + movePower);


        Debug.Log("P" + idx + " movePower = " + movePower + ", otherPower = " + otherPower);
        Debug.Log("P" + idx + " directness = " + directness + ", otherDirectness = " + otherDirectness);
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
        //FIX THIS!!
        float hitstop = (strength > otherStrength) ? (strength/maxMovePower) : (otherStrength/maxMovePower);
        //StartCoroutine(HitStop(maxHitstop * hitstopCurve.Evaluate(hitstop)));

        Vector2 otherImpactDirection = otherRB.velocity.normalized;

        direction = otherPosDiff; //replicates movement2.0 style knockback

        //wait one tick so other player's knockback calculations can finish
        yield return new WaitForFixedUpdate();

        //direction = (otherStrength * pre-impact otherPlayer.direction) + (strenght * post-impact thisPlayer.direction)
        //direction = ((movePower * rb.velocity.normalized) + (otherStrength * otherImpactDirection)).normalized;
        
        //direction = rb.velocity.normalized;


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

            Debug.Log("player" + idx + " knockback direction: " + direction);
            Debug.Log("player" + idx + " knockback strength: " + (knockbackMultiplier * otherStrength));

            ApplyMove(1, direction, knockbackMultiplier * otherStrength);
        }

        //apply hitstop
        StartCoroutine(HitStop(maxHitstop * hitstopCurve.Evaluate(hitstop)));
    }


    public override void OnCollisionEnter2D(Collision2D col)
    {
        Debug.Log("solid player collider colliding!");


    }


    public override void OnHurtboxTriggerEnter(Collider2D col)
    {
        FindFirstObjectByType<AudioManager>().PlayRandom("Impact");

        Debug.Log("Player colliding! Collision LayerMask name: " + LayerMask.LayerToName(col.gameObject.layer));

        if (LayerMask.LayerToName(col.gameObject.layer) == "Players")
        {
            if(col.GetComponent<PlayerHurtBox>() != null)
            {
                if(col == col.GetComponent<PlayerHurtBox>().hb)
                {
                    Debug.Log("Player" + idx + " collided with Player " + col.gameObject.GetComponentInParent<PlayerController>().idx);

                    //DO THIS NEXT:
                    //insert positional collision hack here
                    
                    //previous position
                    Vector2 prev = transform.position;
                    if(prevPos[1] != (Vector2)transform.position)
                    {
                        prev = prevPos[1];
                    } else if(prevPos[0] != (Vector2)transform.position)
                    {
                        prev = prevPos[0];
                    }

                    PlayerController otherPC = col.GetComponentInParent<PlayerController>();
                    Vector2 otherPrev = otherPC.transform.position;
                    if(otherPC.prevPos[1] != (Vector2)otherPC.transform.position)
                    {
                        otherPrev = otherPC.prevPos[1];
                    } else if(otherPC.prevPos[0] != (Vector2)otherPC.transform.position)
                    {
                        otherPrev = otherPC.prevPos[0];
                    }
                    
                    //correct collision position
                    var (pos, otherPos) = EstimateCircleTriggerCollision(HurtBoxTrigger.radius, transform.position, prev, otherPC.transform.position, otherPrev);
                    transform.position = pos;
                    otherPC.transform.position = otherPos; //is this needed?

                    StartCoroutine(ApplyKnockback(col.gameObject.GetComponentInParent<PlayerController>().movePower, col.gameObject.GetComponentInParent<PlayerController>().rb));
                }
            }
            
        } /*else if(LayerMask.LayerToName(col.gameObject.layer) == "Items")
        {

        }*/


    }

    (Vector2 pos, Vector2 otherPos) EstimateCircleTriggerCollision(float radius, Vector2 pos, Vector2 prev, Vector2 otherPos, Vector2 otherPrev)
    {
        //exit condition
        if(((2*radius)+.001f >= (prev-otherPrev).magnitude) && ((prev-otherPrev).magnitude >= (2 * radius)-.001f))
        {
            return(prev, otherPrev);
        }


        //recursive divide + conquer loop
        Vector2 dist = pos - prev; //dist points toward pos
        Vector2 otherDist = otherPos - otherPrev;

        //Vector2 newPrev;
        //Vector2 otherNewPrev;

        //plus 1/2 or -1/2 for closer or farther
        if((prev-otherPrev).magnitude > 2*radius)
        {
            prev += (.5f * dist);
            otherPrev += (.5f * otherDist);
        }else if((prev-otherPrev).magnitude < 2*radius)
        {
            prev -= (.5f * dist);
            otherPrev -= (.5f * otherDist);
        }

        //repeat loop
        return EstimateCircleTriggerCollision(radius, pos, prev, otherPos, otherPrev);

    }





}
