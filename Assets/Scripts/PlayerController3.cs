using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController3 : PlayerController
{
    float lastWallTech = 0; //tracks last successful wallTech
    float lastSolidCollision = 0; // tracks last solid collision
    

    bool chargePressed = false;
    //int movePriority = 0;
    int movePhase = 0;


    //bool isGliding = false;
    float glideTimer = 0;
    float glideTime = 0;
    //float glidePower = 0;

    [SerializeField] AnimationCurve glideDeccelDICurve;

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
       

       //tick functions
        InputBufferTick();
        PeerPriorityTick();
        ChargeTick();
        MovementTick();
        HitStopTick();


    }


    //tracks time-of-last-input for various inputs
    void InputBufferTick()
    {
        lastChargePress += Time.fixedDeltaTime;
        lastWallTech += Time.fixedDeltaTime;
        lastSolidCollision += Time.fixedDeltaTime;

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
        if(ctx.performed)
        {
            if(lastChargePress <= 3 * wallTechFrameWindow * Time.fixedDeltaTime)
            {
                //repeat inut closes window
                canWallTech = false;
            } else if(lastWallTech >= 20 * wallTechFrameWindow * Time.fixedDeltaTime)
            {
                canWallTech = true;
            } else
            {
                canWallTech = false;
            }

            //reset buffer timer
            lastChargePress = 0;
        }

        
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
                DIMod = 1.1f * DIStrength * ((1.2f * lateralDIStrength * lateralDI) + (forwardDIStrength * forwardDI)) /*true_i_move*/;
            } else
            {
                DIMod = (1.2f * lateralDIStrength * lateralDI) * DIStrength;
            }

            if(moveTimer <= moveTime) //MOVING
            {
                isMoving = true;
                movePhase = 2;

                if(isKnockback)
                {
                    movePriority = 2;
                } else
                {
                    movePriority = 3;
                }
                
                rb.velocity = moveCurve.Evaluate(moveTimer/moveTime) * moveSpeed * (rb.velocity.normalized + DIMod).normalized;

                if(isMoving && !isKnockback && isCoolingDown)
                {
                    RotateSprite(rb.velocity.normalized);
                    //not a great fix but it works
                    sr.sprite = spriteSet[1];
                }

                moveTimer += Time.fixedDeltaTime;
                
                
            } else if(glideTimer <= glideTime) //GLIDING
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

                movePriority = 1;

                //scale power down with speed
                movePower = moveCurve.Evaluate(1 + (glideTimer/glideTime)) * glidePower;
                
                movePhase = 1;
                isGliding = true;

                
                DIMod = DIStrength * (lateralDIStrength * lateralDI);                

                //rate of glideTimer tick
                /// >1 => quicker deccel, shorter glide
                /// <1 => slower deccel, longer glide
                float glideRate = 1;

                //max glide speed for curve evals
                float glideSpeed = moveSpeed;

                float chargeMod = 1;

                //neutral DI + charging state
                if(charging && true_i_move == Vector2.zero)
                {
                    glideRate = 1.2f;
                }

                if(forwardDI != Vector2.zero)
                {
                    //holding back
                    if(Vector2.Angle(rb.velocity, true_i_move) > 90)
                    {
                        if(charging)
                        {
                            chargeMod = 6.5f;
                        }

                        //EDIT THIS CONSTANT
                        glideRate = 1 + ((glideDeccelDICurve.Evaluate(forwardDI.magnitude) * .7f) / chargeMod);

                    //holding forward
                    } else if(Vector2.Angle(rb.velocity, true_i_move) < 90)
                    {
                        if(charging)
                        {
                            chargeMod = 4;
                        }

                        //EDIT THIS CONSTANT
                        glideRate = 1 - ((forwardDI.magnitude * .3f) / chargeMod);
                    }
                }

                //extend glide duration for lateral control
                if(lateralDI != Vector2.zero)
                {
                    //EDIT THIS CONSTANT
                    glideRate -= (lateralDI.magnitude * .25f);
                }

                //is this really necessary???
                if(charging)
                {
                    glideSpeed = .8f * moveSpeed;
                    glideRate *= .8f;

                }
                

                rb.velocity = moveCurve.Evaluate(1 + (glideTimer/glideTime)) * glideSpeed * (rb.velocity.normalized + DIMod).normalized;

                glideTimer += (Time.fixedDeltaTime * glideRate);                


                //may cause problems in the future
                //sr.sprite = spriteSet[0];
            } else //done - no movement
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

                movePriority = 0;
                
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
        
        //is this needed still??
        //failsafe
        if(!isMoving && !isKnockback && !isGliding && !isRewind)
        {
            movePriority = 0;
            rb.velocity = Vector2.zero;
        }

    }

    //new hitstop system tracked in FixedUpdate()
    void HitStopTick()
    {
        if(hitStopTimer < hitStopTime)
        {
            /*
            if(!isHitStop) //first frame of hitstop
            {
                storedVelocity = rb.velocity;
            }
            */

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


    //applies HitStop
    //type 0: additive
    //type 1: overwrite
    void ApplyHitStop(float time, int type)
    {
        if(!isHitStop)
        {
            isHitStop = true;
            storedVelocity = rb.velocity;
        }
        

        switch (type)
        {
            case 0: //additive
                //prevent overstacking
                Mathf.Clamp(hitStopTime, 0, maxHitstop);
                if(hitStopTimer < hitStopTime)
                {
                    hitStopTime += time;
                } else
                {
                    hitStopTime = time;
                    hitStopTimer = 0;
                }
                break;
            case 1: //overwrite
                hitStopTime = time;
                hitStopTimer = 0;
                break;
            default:
                break;
        }
    }


    //tracks peer priority timers
    void PeerPriorityTick()
    {   
        if(!isHitStop)
        {
            //timer > 0 => this player overpowers otherPlayer
            int idx = 0;

            //if not attacking
            if(!isMoving || isKnockback)
            {
                //clear overpower prio table
                foreach(float peer in OverpowerPeerPrioTable)
                {
                    OverpowerPeerPrioTable[idx] = 0;
                    idx++;
                }
            }

            /* //old timer behavior
            foreach(float peer in OverpowerPeerPrioTable)
            {
                if(OverpowerPeerPrioTable[idx] > 0)
                {
                    OverpowerPeerPrioTable[idx] -= Time.fixedDeltaTime;
                }

                if(OverpowerPeerPrioTable[idx] < 0)
                {
                    OverpowerPeerPrioTable[idx] = 0;
                }

                idx++;
            }
            */


            //timer > 0 => this player is intangible to player[idx]
            idx = 0;
            foreach(float peer in IntangiblePeerPrioTable)
            {
                if(IntangiblePeerPrioTable[idx] > 0)
                {
                    IntangiblePeerPrioTable[idx] -= Time.fixedDeltaTime;
                }

                if(IntangiblePeerPrioTable[idx] < 0)
                {
                    IntangiblePeerPrioTable[idx] = 0;
                }

                idx++;
            }

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
        } else if(type == 2) //glide - weak tap
        {
            //ADD THIS
            //gonna have to do something completely different here


        }

        //max speed reached in this movement
        moveSpeed = maxMoveSpeed * speedCurve.Evaluate(charge/maxChargeTime);
        //total time to spend moving in this movement
        moveTime = maxMoveTime * timeCurve.Evaluate(charge/maxChargeTime);
        //knockback power of this movement
        movePower = maxMovePower * powerCurve.Evaluate(charge/maxChargeTime);

        //new glide stuff
        glidePower = /*.5f * */movePower;
        glideTime = 2.7f * moveTime;

        //probably not needed in this context
        rb.velocity = direction.normalized;

        //reset moveTimer to beginning
        moveTimer = 0;
        glideTimer = 0;

        EmitBubbles(.9f * moveTime);
    }

    //modifies the current movement stats
    //useful for movement impedance/extension
    //type => new movement type to set
    //directionMod => modifier applied to current rb direction
    //durationMod => modifier applied to current moveTime + moveTimer vars
    //powerMod => modifier applied to current movePower
    void ModifyMove(int type, Vector2 directionMod, float durationMod, float speedMod, float powerMod)
    {
        //mod direction
        Vector2 direction = (directionMod + rb.velocity.normalized).normalized;
        rb.velocity = rb.velocity.magnitude * direction;

        //mod timers
        moveTime *= durationMod;
        moveTimer *= durationMod;

        //doing too much
        //glideTime *= durationMod;
        //glideTimer *= durationMod;

        moveSpeed *= speedMod;

        //mod power
        movePower *= powerMod;
        glidePower *= powerMod;
    }


    //applies knockback effects to this player - does nothing to other colliding player
    //called in OnCollisionEnter2D when colliding with opponent
    public override IEnumerator ApplyKnockback(int otherPriority, float otherPower, Rigidbody2D otherRB)
    {
        //Debug.Log("ApplyKnockback!");

        PlayerController otherPC = otherRB.GetComponent<PlayerController>();
        
        
        //exit if this player is intangible to KB from otherPlayer
        if(IntangiblePeerPrioTable[otherPC.idx] > 0)
        {
            yield break;
        }


        //REPLACE THIS WITH STUNNED SPRITE!!!!!!!!!!!!
        sr.sprite = spriteSet[0];

        GetComponentInChildren<TrailRenderer>().emitting = false; //cancel wall item on impact

        //difference in priority
        int priorityDiff = movePriority - otherPriority;

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

        //calc hitstop
        float hitstop = (strength > otherStrength) ? (strength/maxMovePower) : (otherStrength/maxMovePower);

        Vector2 otherImpactDirection = otherRB.velocity.normalized;

        direction = otherPosDiff; //replicates movement2.0 style knockback

        //wait one tick so other player's knockback calculations can finish
        //IMPORTANT: all variables used in KB calcs must be set before this yield
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
            

            //ADD PARRY HERE
            if(otherPC.canWallTech && otherPC.lastChargePress < otherPC.wallTechFrameWindow * Time.fixedDeltaTime)
            {
                //give this player intangible priority from otherPlayer
                //EDIT THIS: int constant = invol frame data
                IntangiblePeerPrioTable[otherPC.idx] = 12 * Time.fixedDeltaTime;
                //give otherPlayer intangible priority from this player
                //EDIT THIS: int constant = invol frame data
                otherPC.IntangiblePeerPrioTable[idx] = 12 * Time.fixedDeltaTime;

                //KB starts from parrying player's position
                transform.position = otherPC.transform.position;

                //apply parry KB to this player
                ApplyMove(1, otherPC.true_i_move, /* (moveTimer/moveTime) * */ knockbackMultiplier * movePower);

                //ADD THIS: increase hitstop for parry indication
                //apply KB impact hitstop
                //StartCoroutine(HitStop(maxHitstop * hitstopCurve.Evaluate(hitstop)));
                ApplyHitStop(maxHitstop * hitstopCurve.Evaluate(hitstop), 1);

                yield break;
            }

            //if this player overpowers otherPlayer
            if(OverpowerPeerPrioTable[otherPC.idx] > 0)
            {
                //do different KB behavior - barrel through
                //"barrel through"
                //impedance based on otherStrength

                //give this player intangible priority from otherPlayer
                //EDIT THIS: int constant = invol frame data
                IntangiblePeerPrioTable[otherPC.idx] = 7 * Time.fixedDeltaTime;

                //give this player overpower priority over otherPlayer
                //EDIT THIS: int constant = overPower frame data
                OverpowerPeerPrioTable[otherPC.idx] = 7 * Time.fixedDeltaTime;

                //max possible glidestrength = maxMovePower * .1
                Debug.Log("otherStrength = " + otherStrength);
                //alter travel distance
                float impedanceFactor = 1 - Mathf.Clamp(1 * otherStrength, .1f, 1);

                //alter direction
                //factors: otherPosDiff, otherRB direction, 
                Vector2 directionMod = ((2 * otherPosDiff.normalized) + (5 * otherRB.velocity.normalized)).normalized * Mathf.Clamp((3 * otherStrength), .4f, 1);

                ModifyMove(0, directionMod, impedanceFactor, impedanceFactor, .9f);

                //apply KB impact hitstop
                //StartCoroutine(HitStop(maxHitstop * hitstopCurve.Evaluate(hitstop)));
                ApplyHitStop(maxHitstop * hitstopCurve.Evaluate(hitstop), 1);

                yield break;
            }

            switch (movePriority)
            {
                case 0: //standing still
                    if(otherPriority <= 1) //other standing still / gliding
                    {
                        //ADD THIS:
                        //weak gliding KB nudge
                        //use both glidePowers in calc

                        ApplyMove(1, direction, knockbackMultiplier * otherStrength);


                    } else if(otherPriority == 2) //other being KB launched
                    {
                        //powerful attack KB
                        ApplyMove(1, direction, knockbackMultiplier * otherStrength);

                    } else if(otherPriority == 3) //other moving attacking
                    {
                        //give this player intangible priority from otherPlayer
                        //EDIT THIS: int constant = invol frame data
                        IntangiblePeerPrioTable[otherPC.idx] = 7 * Time.fixedDeltaTime;

                        //give otherPlayer overpower priority over this Player
                        //EDIT THIS: int constant = overPower frame data
                        otherPC.OverpowerPeerPrioTable[idx] = 7 * Time.fixedDeltaTime;

                        //TWEAK THIS - calc should be more biased to otherPlayer
                        //powerful attack KB
                        ApplyMove(1, direction, knockbackMultiplier * otherStrength);
                    }

                    break;
                case 1: //gliding
                    if(otherPriority <= 1)
                    {
                        //ADD THIS:
                        //weak gliding KB nudge
                        //use both glidePowers in calc

                        ApplyMove(1, direction, knockbackMultiplier * otherStrength);

                    } else if(otherPriority == 2)
                    {
                        //powerful attack KB
                        ApplyMove(1, direction, knockbackMultiplier * otherStrength);


                    } else if(otherPriority ==3)
                    {
                        //give this player intangible priority from otherPlayer
                        //EDIT THIS: int constant = invol frame data
                        IntangiblePeerPrioTable[otherPC.idx] = 7 * Time.fixedDeltaTime;
                        
                        //give otherPlayer overpower priority over this Player
                        //EDIT THIS: int constant = overPower frame data
                        otherPC.OverpowerPeerPrioTable[idx] = 7 * Time.fixedDeltaTime;

                        //powerful attack KB
                        ApplyMove(1, direction, knockbackMultiplier * otherStrength);
                    }

                    break;
                case 2: //knockback launch
                    if(otherPriority <= 1)
                    {
                        //old "8ball" behaviour works for now
                        //^^BRITISH???
                        ApplyMove(1, direction, knockbackMultiplier * otherStrength);

                        //TRY THIS:
                        //bounce off with slightly increased moveTime
                        //(same as wallbounce behavior)
                        //
                        //idk


                    } else if(otherPriority == 2)
                    {
                        //Equal KB exchange
                        ApplyMove(1, direction, knockbackMultiplier * otherStrength);

                        //TRY THIS:
                        //impede stronger player's KB more

                    } else if(otherPriority == 3)
                    {
                        //receive full KB
                        ApplyMove(1, direction, knockbackMultiplier * otherStrength);

                    }

                    break;
                case 3: //moving launch
                    if(otherPriority <= 1)
                    {
                        //"barrel through"
                        //impedance based on otherStrength

                        //give this player intangible priority from otherPlayer
                        //EDIT THIS: int constant = invol frame data
                        IntangiblePeerPrioTable[otherPC.idx] = 7 * Time.fixedDeltaTime;

                        //give this player overpower priority over otherPlayer
                        //EDIT THIS: int constant = overPower frame data
                        OverpowerPeerPrioTable[otherPC.idx] = 7 * Time.fixedDeltaTime;
                        
                        //max possible glidestrength = maxMovePower * .1
                        Debug.Log("otherStrength = " + otherStrength);
                        //alter travel distance
                        float impedanceFactor = 1 - Mathf.Clamp(1 * otherStrength, .1f, 1);

                        //alter direction
                        //factors: otherPosDiff, otherRB direction, 
                        Vector2 directionMod = ((2 * otherPosDiff.normalized) + (5 * otherRB.velocity.normalized)).normalized * Mathf.Clamp((3 * otherStrength), .4f, 1);

                        ModifyMove(0, directionMod, impedanceFactor, impedanceFactor, .9f);                        

                    } else if(otherPriority == 2)
                    {
                        //ADD THIS
                        //apply reduced knockback based on powerDiff

                        ApplyMove(1, direction, knockbackMultiplier * otherStrength);



                    } else if(otherPriority == 3)
                    {
                        //Equal KB exchange
                        ApplyMove(1, direction, knockbackMultiplier * otherStrength);

                    }

                    break;
            }

            //old default behavior
            //ApplyMove(1, direction, knockbackMultiplier * otherStrength);
        }

        //apply KB impact hitstop
        //(must be at the end)
        //StartCoroutine(HitStop(maxHitstop * hitstopCurve.Evaluate(hitstop)));
        ApplyHitStop(maxHitstop * hitstopCurve.Evaluate(hitstop), 1);

    }


    public override void OnCollisionEnter2D(Collision2D col)
    {
        Debug.Log("solid player collider colliding! col Layer: " + LayerMask.LayerToName(col.gameObject.layer));

        FindFirstObjectByType<AudioManager>().PlayRandom("Impact");


        if(LayerMask.LayerToName(col.gameObject.layer) == "Solids")
        {
            
            
            StartCoroutine(OnSolidCollision(col));
            

            
        }


    }

    IEnumerator OnSolidCollision(Collision2D col)
    {
        //Debug.Log("OnSolidCollision!");
        
        WaitForFixedUpdate fuWait = new WaitForFixedUpdate();

        
        if(lastSolidCollision < 5 * Time.fixedDeltaTime)
        {
            //extend movement slightly
            ModifyMove(0, Vector2.zero, 1.2f, .95f, 1);

            //hitstop calculation
            float hitstop = Mathf.Clamp(.4f * maxHitstop * hitstopCurve.Evaluate(movePower/maxMovePower), 4*Time.fixedDeltaTime, maxHitstop);
            Debug.Log("solid collision hitstop: " + hitstop);

            //apply hitstop
            //StartCoroutine(HitStop(hitstop));
            ApplyHitStop(hitstop, 0);
        }
        

        //wall tech timer
        float timer = 0;
        while (timer <= 2 * Time.fixedDeltaTime) //2 frames of wiggle room for late inputs
        {
            float window = wallTechFrameWindow * Time.fixedDeltaTime;
            if(isKnockback)
            {
                window = 3 * wallTechFrameWindow * Time.fixedDeltaTime;
            }

            //wall tech frame window
            if(canWallTech && lastChargePress <= window)
            {
                StartCoroutine(WallTech());
                break;
            }

            //waiting
            timer += Time.fixedDeltaTime;
            yield return fuWait;
        }

    }

    //wallbounce tech
    IEnumerator WallTech()
    {
        WaitForFixedUpdate fuWait = new WaitForFixedUpdate();

        //extra hitstop for player feedback
        float hitstop = Mathf.Clamp(.8f * maxHitstop * hitstopCurve.Evaluate(movePower/maxMovePower), 8*Time.fixedDeltaTime, maxHitstop);
        //StartCoroutine(HitStop(hitstop));
        ApplyHitStop(hitstop, 0);

        //read DI input up until launch (for responsiveness)
        while(isHitStop)
        {
            storedVelocity = storedVelocity.magnitude * true_i_move.normalized;
            Debug.Log("wallTech storedVelocity: " + storedVelocity);
            yield return fuWait;
        }

        //ADD THIS:
        //constrain new direction based off wall normal

        //reset cooldown timer
        lastWallTech = 0;

        //add extra distance
        ModifyMove(0, Vector2.zero, 1.4f, .95f, 1.05f);
        Debug.Log("P" + idx + " WALLTECH!");
    }


    public override void OnHurtboxTriggerEnter(Collider2D col)
    {
        FindFirstObjectByType<AudioManager>().PlayRandom("Impact");

        Debug.Log("PlayerHB colliding! Collision LayerMask name: " + LayerMask.LayerToName(col.gameObject.layer));

        if (LayerMask.LayerToName(col.gameObject.layer) == "Players")
        {
            if(col.GetComponent<PlayerHurtBox>() != null)
            {
                if(col == col.GetComponent<PlayerHurtBox>().hb)
                {
                    Debug.Log("Player" + idx + " collided with Player " + col.gameObject.GetComponentInParent<PlayerController>().idx);

                    //positional collision hack
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

                    StartCoroutine(ApplyKnockback(otherPC.movePriority, otherPC.movePower, otherPC.rb));
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
