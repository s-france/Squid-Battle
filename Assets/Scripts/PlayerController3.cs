using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Cinemachine;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController3 : PlayerController
{

    //absolute max speed possible in game.
    [SerializeField] float moveSpeedCap; //Anything higher will be capped at this val
    [SerializeField] float moveTimeCap; //safety cap for moveTime

    float lastWallTech = 0; //tracks last successful wallTech
    Vector2 lastWallTechNormal; //normal of the most recent wallTech performed
    float lastSolidCollision = 0; // tracks last solid collision

    bool chargePressed = false;
    bool specialChargePressed = false;
    //int movePriority = 0;
    int movePhase = 0;

    public float maxStamina; //how long the player can survive offstage
    [HideInInspector] public float stamina = 5; //how long the player has been offstage
    float oobGraceTimer = 0; //grace period before oobTimer starts passively ticking


    float glideTimer = 0;
    float glideTime = 0;

    //dodge has constant maxSpeed + time unlike movement
    //because it always has the same distance + frame data
    [SerializeField] float dodgeSpeed;
    [SerializeField] float dodgeTime;
    float dodgeTimer = 10;
    Vector2 dodgeDirection = Vector2.zero; //direction player is dodging in
    
    bool isDodging = false;

    [SerializeField] AnimationCurve dodgeCurve; //dodge movement curve
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
       //tick functions
        TrackStatesTick();

        ArmorTick();
        
        InputBufferTick();
        PeerPriorityTick();
        ChargeTick();
        SpecialChargeTick();
        MovementTick();
        HitStopTick();
        OutOfBoundsTick();
        KillCreditTick();

        MovementSafetyTick();
    }

    //tracks who should receive kill credit, should this player die rn
    void KillCreditTick()
    {
        //kill credit id is usually assigned in ApplyKnockback and it's results (parry)
        //hold credit + suspend/reset timer while isknockback
        if(isKnockback)
        {
            killCreditTimer = 0;
        } else if(isInBounds)
        {
            killCreditTimer += Time.fixedDeltaTime;

        }

        //reset kill credit to self if inbounds and not refreshed in last 1sec
        if(killCreditTimer >= 1)
        {
            //suicide
            killCredit = idx;
        }


    }


    //prevents unintended extreme movement cases
    void MovementSafetyTick()
    {
        if(rb.velocity.magnitude > moveSpeedCap)
        {
            Debug.Log("Velocity exceeding SpeedCap!! Speed = " + rb.velocity.magnitude);
            rb.velocity = rb.velocity.normalized * moveSpeedCap;
        }

        if(storedVelocity.magnitude > moveSpeedCap)
        {
            Debug.Log("Velocity exceeding SpeedCap!! Speed = " + storedVelocity.magnitude);
            storedVelocity = storedVelocity.normalized * moveSpeedCap;
        }

        if(moveTime > moveTimeCap)
        {
            Debug.Log("moveTime exceeding moveTimeCap!! moveTime = " + moveTime);
            moveTime = moveTimeCap;
        }

    }


    //tracks time-of-last-input for various inputs
    void InputBufferTick()
    {
        lastChargePress += Time.fixedDeltaTime;
        lastWallTech += Time.fixedDeltaTime;
        lastSolidCollision += Time.fixedDeltaTime;

        lastIMove += Time.fixedDeltaTime;
        if(true_i_move.magnitude != 0)
        {
            lastIMove = 0;
        }

    }


    //tracks player's previous states
    //for use in collision correction + Rewind
    public override void TrackStatesTick()
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


        if(!isHitStop)
        {
            //always store state if moving (Rewind)
            if(((Vector2)transform.position - lastPrevStatePos).magnitude > .1f)
            {
                staticTimer = 0;

                PlayerState s = new PlayerState(transform.position.x, transform.position.y, movePower, rb.velocity);
                prevStates.Enqueue(s);

                lastPrevStatePos.x = s.xPos;
                lastPrevStatePos.y = s.yPos;

            } else
            {
                //store .1s of states when stationary
                if(staticTimer <= .1f)
                {
                    PlayerState s = new PlayerState(transform.position.x, transform.position.y, movePower, rb.velocity);
                    prevStates.Enqueue(s);
                }
                staticTimer += Time.fixedDeltaTime;
            }
        }
        

        //prevent overfilling
        while(prevStates.Count > rewindSize)
        {
            prevStates.Dequeue();
        }

        //Debug.Log("prevStates.Count: " + prevStates.Count);
        //Debug.Log("prevStates: " + prevStates);
    }

    //tick armor stats
    //runs in FixedUpdate()
    void ArmorTick()
    {
        if(passiveArmor < maxPassiveArmor)
        {
            passiveArmor = Mathf.Clamp(passiveArmor + Time.fixedDeltaTime, 0, maxPassiveArmor);
        } else
        {
            passiveArmor = maxPassiveArmor;
        }

    }


    //called when move (stick) input received
    public override void OnMove(InputAction.CallbackContext ctx)
    {
        true_i_move = ctx.ReadValue<Vector2>();

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

        //Debug.Log(i_move);

        //rotate sprite
        if(!isCoolingDown && !isHitStop)
        {
            RotateSprite(i_move);
        }


        //copy input on clones
        for(int i=0; i<Clones.Count; i++)
        {
            Clones[i].OnMove(ctx);
        }
    }



    //called when charge (button) input received
    public override void OnCharge(InputAction.CallbackContext ctx)
    {
        if(ctx.performed)
        {
            //THIS DOESN'T WORK
            //NEED TO REWORK THIS!!
            if(lastChargePress <= 40 * Time.fixedDeltaTime) //40 frame repeat-input lockout time
            {
                //repeat input closes window
                canWallTech = false;
            } else
            {
                canWallTech = true;
            }

            Debug.Log("time from last charge press: " + lastChargePress);

            //reset buffer timer
            lastChargePress = 0;
        }

        
        Debug.Log("charge: " + ctx.phase);
        if(ctx.performed /*&& pm.playerList[idx].isInBounds*/) //charging
        {
            //Debug.Log("charging!!");
            

            //chargeTime = 0;
            //charging = true;

            //prevents new charge after stamina depletion
            if(isInBounds || stamina > 0)
            {
                chargePressed = true;
            } else
            {
                chargePressed = false;
            }

        } else if (ctx.canceled) //released
        {
            //charging = false;
            chargePressed = false;

            if(/*!isMoving &&*/ !isKnockback && !isHitStop && !isRewind)
            {
                if(isMoving)
                {
                    if(movePower > maxMovePower * powerCurve.Evaluate(minCharge/maxChargeTime) && moveTimer/moveTime > .4f)
                    {
                        ApplyMove(0, i_move, chargeVal);
                    }
                } else
                {
                    ApplyMove(0, i_move, chargeVal);
                }
            }
        }

        //copy input on clones
        for(int i=0; i<Clones.Count; i++)
        {
            Clones[i].OnCharge(ctx);

        }


    }


    
    public override void OnSpecial(InputAction.CallbackContext ctx)
    {
        if(ctx.performed) //special charging
        {
            //maybe:
            //if(!isInBounds || stamina > 0)
            
            specialChargePressed = true;
            
            
        } else if (ctx.canceled) //released
        {
            specialChargePressed = false;

            if(/*!isMoving*/ !isKnockback && !isHitStop && !isRewind)
            {
                UseItem(selectedItemIdx);
            }
        }

        //copy input on clones
        for(int i=0; i<Clones.Count; i++)
        {
            Clones[i].OnSpecial(ctx);

        }
    }


    public override void OnSelectL(InputAction.CallbackContext ctx)
    {
        if(ctx.performed)
        {
            if(!isDodging && !isHitStop && !isKnockback && !isRewind)
            {
                //ApplyDodge(true_i_move.normalized);
            }
        }

        //copy input on clones
        for(int i=0; i<Clones.Count; i++)
        {
            Clones[i].OnSelectL(ctx);

        }
    }

    public override void OnSelectR(InputAction.CallbackContext ctx)
    {
        OnSelectL(ctx);
    }


    //makes the player dodge in direction
    void ApplyDodge(Vector2 direction)
    {
        Debug.Log("APPLY DODGE!");

        isDodging = true;
        
        //disable player collision
        HurtBoxTrigger.enabled = false;

        //cancel other movements
        isGliding = false;
        isMoving = false;
        glideTime = 0;
        moveTime = 0;
        
        //reset dodge timer
        dodgeTimer = 0;

        //set direction
        dodgeDirection = direction;
        rb.velocity = direction.normalized;
    }
    

    
    void ChargeTick()
    {
        if(chargePressed)
        {
            if(/*!isMoving &&*/ !isKnockback && !isHitStop && !isRewind/*charge conditions: end of movement or gliding, not hitstop, not stunned in any way*/ )
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


                sr.sprite = SpriteSet[2]; //charging sprite
                chargeTime += Time.fixedDeltaTime;
                
                if(isInBounds || stamina >0)
                {
                    chargeVal += Time.fixedDeltaTime;
                }
                

                RotateSprite(i_move);

                if(gm.battleStarted && gp != null)
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
                    //force launch
                    if(charging && !isMoving && !isHitStop)
                    {
                        ApplyMove(0, i_move, chargeVal);
                    }

                    //cancel charge
                    chargeTime = 0;
                    chargeVal = 0;
                    charging = false;
                    
                }

            }
            

        } else //charge button not pressed
        {
            chargeTime = 0;
            chargeVal = 0;
            charging = false;

        }

    }

    void SpecialChargeTick()
    {
        if(specialChargePressed)
        {
            if(/*!isMoving &&*/ !isKnockback && !isHitStop && !isRewind/*charge conditions: end of movement or gliding, not hitstop, not stunned in any way*/ )
            {
                specialCharging = true;
            } else
            {
                specialCharging = false;
            }

            if (specialCharging)
            {
                //REPLACE WITH THIS!!!:
                //charge function
                ///contained within held ItemBehavior
                ////render reticle function
                

                //rc.RenderNormalReticle();


                sr.sprite = SpriteSet[2]; //charging sprite
                specialChargeTime += Time.fixedDeltaTime;

                if(isInBounds || stamina > 0)
                {
                    specialChargeVal += Time.fixedDeltaTime;
                }

                //MOVE THIS TO ITEMBEHAVIOR 
                //some items face forward some face back
                RotateSprite(i_move);

                if(gm.battleStarted && gp != null)
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
                    //force item
                    if(specialCharging && !isMoving && !isHitStop)
                    {
                        UseItem(selectedItemIdx);
                    }

                    //cancel charge
                    specialChargeTime = 0;
                    specialChargeVal = 0;
                    specialCharging = false;
                    
                }

            }
            

        } else //special charge button not pressed
        {
            specialChargeTime = 0;
            specialChargeVal = 0;
            specialCharging = false;
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

        if(!isHitStop && !isRewind)
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
                DIMod = 1.1f * DIStrength * ((1.1f * lateralDIStrength * lateralDI) + (forwardDIStrength * forwardDI));
            } else
            {
                DIMod = (/*1.2f * */ lateralDIStrength * lateralDI) * DIStrength;
            }

            //account for tick rate
            DIMod *= Time.fixedDeltaTime;

            
            if(dodgeTimer < dodgeTime) //DODGING
            {

                if(isGliding)
                {
                    glideTime = 0;
                    isGliding = false;
                }

                if(isMoving)
                {
                    moveTime = 0;
                    isMoving = false;
                }


                isDodging = true;


                //2 frames of vulnerability on startup
                //12 frames of vulnerability on end
                if(.8f > dodgeTimer/dodgeTime && dodgeTimer/dodgeTime > 2 * Time.fixedDeltaTime)
                {
                    //player is invulnerable
                    //ADD THIS
                    //visuals to show invulnerability

                    //disable player collision
                    HurtBoxTrigger.enabled = false;
                } else
                {
                    //dumb band-aid fix
                    if(isAlive)
                    {
                        //player is vulnerable (startup / end lag)
                        HurtBoxTrigger.enabled = true;
                    }
                    
                }

                moveArmor = 0;

                movePriority = 0;
                
                rb.velocity = dodgeCurve.Evaluate(dodgeTimer/dodgeTime) * dodgeSpeed * dodgeDirection.normalized;

                RotateSprite(rb.velocity.normalized);
                
                dodgeTimer += Time.fixedDeltaTime;

            } else if(moveTimer < moveTime) //MOVING
            {
                if(isDodging)
                {
                    //dumb band-aid fix
                    if(isAlive)
                    {
                        HurtBoxTrigger.enabled = true;
                    }
                    
                    isDodging = false;
                }

                isMoving = true;
                movePhase = 2;

                if(isKnockback)
                {
                    eyeSR.sprite = EyeSpriteSet[3];
                    sr.sprite = SpriteSet[0];

                    movePriority = 2;
                } else
                {
                    eyeSR.sprite = EyeSpriteSet[0];
                    sr.sprite = SpriteSet[1];

                    movePriority = 3;
                }
                
                rb.velocity = moveCurve.Evaluate(moveTimer/moveTime) * moveSpeed * (rb.velocity.normalized + DIMod).normalized;

                if(isMoving && !isKnockback && isCoolingDown)
                {
                    RotateSprite(rb.velocity.normalized);
                }

                moveTimer += Time.fixedDeltaTime;
                
                
            } else if(glideTimer < glideTime) //GLIDING
            {
                if(isDodging)
                {
                    //dumb band-aid fix
                    if(isAlive)
                    {
                        HurtBoxTrigger.enabled = true;
                    }
                    isDodging = false;
                }

                //movement is over -> reset movement tracking variables
                if(isMoving)
                {
                    isMoving = false;
                }
                if(isKnockback)
                {
                    isKnockback = false;
                }

                moveArmor = 0;

                movePriority = 1;

                //scale power down with speed
                movePower = moveCurve.Evaluate(1 + (glideTimer/glideTime)) * glidePower;
                
                movePhase = 1;
                isGliding = true;

                if(!charging && !specialCharging)
                {
                    eyeSR.sprite = EyeSpriteSet[0];
                    sr.sprite = SpriteSet[0];
                }
                
                DIMod = DIStrength * (lateralDIStrength * lateralDI);
                DIMod *= Time.fixedDeltaTime;

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

                //holding back
                if(forwardDI != Vector2.zero)
                {
                    if(Vector2.Angle(rb.velocity, true_i_move) > 90)
                    {
                        if(charging)
                        {
                            chargeMod = 5f; //old val = 6.5
                        }

                        //extend momentum
                        //EDIT THIS CONSTANT
                        glideRate = 1 + ((glideDeccelDICurve.Evaluate(forwardDI.magnitude) * .7f) / chargeMod);

                    //holding forward
                    } else if(Vector2.Angle(rb.velocity, true_i_move) < 90)
                    {
                        if(charging)
                        {
                            chargeMod = 4;
                        }

                        //cut momentum
                        //EDIT THIS CONSTANT
                        glideRate = 1 - ((forwardDI.magnitude * .3f) / chargeMod);
                    }
                }

                //extend glide duration for lateral control
                if(lateralDI != Vector2.zero)
                {
                    //EDIT THIS CONSTANT (very sensitive value)
                    glideRate -= (lateralDI.magnitude * .12f); //old val = .25
                }

                //is this really necessary??? - yes
                if(charging)
                {
                    //charging slows momentum
                    glideSpeed = .8f * moveSpeed;
                    //extend duration to compensate
                    glideRate *= .7f;

                }
                
                rb.velocity = moveCurve.Evaluate(1 + (glideTimer/glideTime)) * glideSpeed * (rb.velocity.normalized + DIMod).normalized;

                glideTimer += (Time.fixedDeltaTime * glideRate);

            } else //done - no movement
            {
                if(isDodging)
                {
                    //dumb band-aid fix
                    if(isAlive)
                    {
                        HurtBoxTrigger.enabled = true;
                    }
                    isDodging = false;
                }

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

                moveArmor = 0;

                movePriority = 0;
                
                glideTime = 0;
                moveTime = 0;
                moveSpeed = 0;

                dodgeTimer = 100;

                if(!isRewind)
                {
                    movePower = 0;
                    glidePower = 0;
                    rb.velocity = Vector2.zero;
                }

                //may cause problems in the future
                //sr.sprite = SpriteSet[0];

            }

        }
        
        //is this needed still??
        //failsafe
        if(!isMoving && !isKnockback && !isGliding && !isDodging && !isRewind)
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
            rb.velocity = Vector2.zero;
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

    //handles player behavior while offstage
    void OutOfBoundsTick()
    {
        if(!isInBounds) //Offstage
        {
            //if time runs out
            if(movePower <= (maxMovePower * powerCurve.Evaluate((1.8f * minCharge)/maxChargeTime))  && !charging && !specialCharging && !isRewind && !isWarp && stamina <= 0 && isAlive && alc.roundOver == false)
            {
                //kill this player instance
                KillPlayer();

                //check if all clones dead then do pm.killplayer
                CheckIfAlive();
            }
            
            //(stats modified in ArenaBoundary.OnTriggerExit/Enter)

            //tick OOB clock while not launching / rewind / item
            if((movePriority < 2 || (movePower < (maxMovePower * powerCurve.Evaluate((1.8f * minCharge)/maxChargeTime)))) && !isRewind /*&& !specialCharging*/)
            {
                if( (charging && chargeTime <= maxChargeTime) || (specialCharging && specialChargeTime <= maxChargeTime))
                {
                    stamina -= (.75f * Time.fixedDeltaTime);
                } else if(oobGraceTimer < 1) //1 sec grace period
                {
                    oobGraceTimer += Time.fixedDeltaTime;
                } else
                {
                    //passive stamina depletion
                    stamina -= (.35f * Time.fixedDeltaTime);
                }

            } else
            {
                oobGraceTimer = 0;
            }



        } else //Onstage
        {
            //restore oob time while onstage
            if(stamina < maxStamina)
            {
                //onstage stamina regen rate
                stamina += .35f * Time.fixedDeltaTime;
            }
            
        }

        //cap stamina at max
        if(stamina > maxStamina)
        {
            stamina = maxStamina;
        }

        //min stamina = 0
        if(stamina <0)
        {
            stamina = 0;
        }


    }


    //tracks peer priority timers
    void PeerPriorityTick()
    {
        if(!isHitStop)
        {
            //timer > 0 => this player overpowers otherPlayer
            //int idx = 0;

            //if not attacking
            if((!isMoving || isKnockback) && !isHitStop)
            {
                //clear overpower prio table
                /*
                foreach(float peer in OverpowerPeerPrioTable)
                {
                    OverpowerPeerPrioTable[idx] = 0;
                    idx++;
                }
                */

                for(int i=0; i<OverpowerPeerPrioTable.Count; i++)
                {
                    OverpowerPeerPrioTable[OverpowerPeerPrioTable.Keys.ElementAt(i)] = 0;
                }
                /*
                foreach (var pair in OverpowerPeerPrioTable)
                {
                    OverpowerPeerPrioTable[pair.Key] = 0;
                }
                */

                //OverpowerPeerPrioTable.Clear();
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
            /*
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
            */

            for(int i=0; i<IntangiblePeerPrioTable.Count; i++)
            {
                if(IntangiblePeerPrioTable[IntangiblePeerPrioTable.Keys.ElementAt(i)] > 0)
                {
                    IntangiblePeerPrioTable[IntangiblePeerPrioTable.Keys.ElementAt(i)] -= Time.fixedDeltaTime;
                }

                if(IntangiblePeerPrioTable[IntangiblePeerPrioTable.Keys.ElementAt(i)] < 0)
                {
                    IntangiblePeerPrioTable[IntangiblePeerPrioTable.Keys.ElementAt(i)] = 0;
                }
            }

            /*
            foreach (var pair in IntangiblePeerPrioTable)
            {
                if(IntangiblePeerPrioTable[pair.Key] > 0)
                {
                    IntangiblePeerPrioTable[pair.Key] -= Time.fixedDeltaTime;
                }

                if(IntangiblePeerPrioTable[pair.Key] < 0)
                {
                    IntangiblePeerPrioTable[pair.Key] = 0;
                }

            }
            */


        }
        

    }


    public override void SetPeerPriority(Dictionary<PlayerController, float> prioTable, PlayerController pc, float time)
    {
        if(prioTable.ContainsKey(pc))
        {
            prioTable[pc] = time;
        } else
        {
            prioTable.Add(pc, time);
        }
    }


    public override void ApplyMove(int type, Vector2 direction, float charge)
    {
        //might cause problems in the future...
        //remove waltech lockout
        lastWallTechNormal = Vector2.zero;

        //Debug.Log("player" + idx + " moving!");
        //Debug.Log("type = " + type);
        //Debug.Log("direction = " + direction);
        //Debug.Log("charge = " + charge);

        if(type == 0) //movement
        {
            chargePressed = false;

            FindFirstObjectByType<AudioManager>().PlayRandom("Move");

            isMoving = true;
            sr.sprite = SpriteSet[1];
            charge = Mathf.Clamp(charge, minCharge, maxChargeTime);
        } else if(type == 1) //knockback
        {
            isKnockback = true;
            charge = Mathf.Clamp(charge, 0, maxChargeTime * 8); //8 is max overcharge potential on movement curves
            
            //knockback has no moveArmor
            moveArmor = 0;

            chargeTime = 0;
            chargeVal = 0;
        } else if(type == 2) //glide - weak tap
        {
            //ADD THIS
            //gonna have to do something completely different here

        } else if(type == -1) //uncapped movement
        {
            chargePressed = false;

            FindFirstObjectByType<AudioManager>().PlayRandom("Move");

            isMoving = true;
            sr.sprite = SpriteSet[1];
            //charge = Mathf.Clamp(charge, minCharge, maxChargeTime);
        }

        //cancel dodge
        dodgeTimer = 100;
        isDodging = false;
        //dumb band-aid fix
        if(isAlive)
        {
            HurtBoxTrigger.enabled = true;
        }

        //max speed reached in this movement
        moveSpeed = maxMoveSpeed * speedCurve.Evaluate(charge/maxChargeTime);
        //total time to spend moving in this movement
        moveTime = maxMoveTime * timeCurve.Evaluate(charge/maxChargeTime);
        //knockback power of this movement
        movePower = maxMovePower * powerCurve.Evaluate(charge/maxChargeTime);
        //apply armor if launching
        if(type == 0)
        {
            moveArmor = maxMoveArmor * armorCurve.Evaluate(charge/maxChargeTime);
        } else
        {
            moveArmor = 0;
        }

        //new glide stuff
        glidePower = /*.5f * */movePower;
        glideTime = 2.7f * Mathf.Clamp(moveTime,0, timeCurve.Evaluate(1.2f));

        //probably not needed in this context
        rb.velocity = direction.normalized;
        //definitely needed
        if(isHitStop)
        {
            storedVelocity = direction.normalized;
        }

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
        Vector2 direction; 
        
        if(isHitStop)
        {
            direction = (directionMod + storedVelocity.normalized).normalized;
            storedVelocity = storedVelocity.magnitude * direction;
        } else
        {
            direction = (directionMod + rb.velocity.normalized).normalized;
            rb.velocity = rb.velocity.magnitude * direction;
        }
        //

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


    //applies knockback received from Bumpers
    //called in OnHurtbxTriggerEnter()
    void ApplyBumperKnockback(Bumper bumper)
    {
        //I literally forgot how PEMDAS works idk how to fix this
        float power = (.5f * movePower) + (movePower * (1.5f * (1- (moveTimer/moveTime))));

        Vector2 direction = transform.position - bumper.transform.position;

        ApplyMove(1, direction, power);
    }


    //applies knockback recieved from Shot projectile
    //called in main ApplyKnockback() function
    void ApplyShotKnockBack(ShotObj shot)
    {
        //true velocity at impact
        Vector2 velocity = isHitStop ? storedVelocity : rb.velocity;
        Vector2 otherVelocity = shot.isHitStop ?  shot.storedVelocity : shot.rb.velocity;
        //direction of shot at impact
        Vector2 impactDirection = velocity.normalized;
        Vector2 otherImpactDirection = otherVelocity.normalized;

        //relative positions
        //vector pointing from player's position to otherPlayer's position i.e. relative position (direction only)
        Vector2 posDiff = (shot.transform.position - transform.position).normalized;
        //vector pointing from otherPlayer's position to this player's position
        Vector2 otherPosDiff = (transform.position - shot.transform.position).normalized;

        //how much of a "direct hit" it is
            //1 = direct hit
            //0 = indirect hit
        //animationCurve used to level out "almost direct hits" and keep value > 0
        float directness = 0;
        if(velocity != Vector2.zero)
        {
            directness = directnessKBCurve.Evaluate(1 - (Vector2.Angle(posDiff, velocity) / 180));
        }

        float otherDirectness = 0;
        if(otherVelocity != Vector2.zero)
        {
            otherDirectness = 1 - (Vector2.Angle(otherPosDiff, otherVelocity) / 180);
        }

        float strength = directness * movePower;
        float otherStrength = otherDirectness * shot.shotPower;

        //calc + apply hitstop to BOTH
        float hitstop = (strength > otherStrength) ? (strength/maxMovePower) : (otherStrength/shot.maxPower);

        Debug.Log("SHOT HITSTOP = " + hitstop);

        ApplyHitStop(maxHitstop * hitstopCurve.Evaluate(hitstop), 1);
        shot.ApplyHitStop(maxHitstop * hitstopCurve.Evaluate(hitstop));

        if(hitstop >= .3f)
        {
            GameObject part = Instantiate(hitPart, (transform.position + shot.transform.position)/2, Quaternion.identity);

            part.GetComponent<HitEffect>().Init(.5f, maxHitstop * hitstopCurve.Evaluate(hitstop)*1.1f);
        }

        //applyMove to BOTH
        Vector2 direction = ((otherDirectness * otherImpactDirection) + (otherPosDiff * (1/otherDirectness))).normalized;
        
        if(!isRewind)
        {
            ApplyMove(1, direction, knockbackMultiplier * otherStrength);
            
            //assign killcredit to shot's owner
            killCredit = shot.parentID;
        } else
        {
            //give temp immunity from shot
            shot.immunityID = idx;
            shot.immunityTimer = 0;
            shot.timerStarted = true;
        }
        
        if(shot.activeTimer > .75f)
        {
            shot.ApplyMove(posDiff.normalized);
        }else
        {
            shot.ApplyMove((2 * posDiff.normalized) + otherVelocity.normalized);
        }

    }


    //applies knockback effects to this player - does nothing to other colliding player
    //called in OnCollisionEnter2D when colliding with opponent
    public override IEnumerator ApplyKnockback(/*int otherPC.movePriority,*/ float otherPower, Rigidbody2D otherRB)
    {
        //ADD THIS
        //isIntangible check -> exit


        
        if(otherRB.TryGetComponent<PlayerController>(out PlayerController otherPC))
        {
            //receiving KB from other player
            //continue below

        } else if(otherRB.TryGetComponent<ShotObj>(out ShotObj shot))
        {
            //receiving KB from shot projectile
            //use ShotKB function
            ApplyShotKnockBack(shot);
            yield break;
        } else
        {
            //applying knockback from something that shouldn't be
            Debug.Log("ERROR no defined ApplyKnockback behavior!!!");
            yield break;
        }
        

        //exit if this player is intangible to KB from otherPlayer
        if(IntangiblePeerPrioTable.ContainsKey(otherPC) && IntangiblePeerPrioTable[otherPC] > 0)
        {
            yield break;
        }

        //IntangiblePeerPrioTable[otherPC] = 1;
        SetPeerPriority(IntangiblePeerPrioTable, otherPC, 1);


        
        WaitForFixedUpdate fuWait = new WaitForFixedUpdate();

        //Debug.Log("ApplyKnockback!");

        //KB eye sprites
        int eyeSprite = movePower == 0 ? 2 : 1;

        //true velocity at impact
        Vector2 velocity = isHitStop ? storedVelocity : rb.velocity;
        Vector2 otherVelocity = otherPC.isHitStop ?  otherPC.storedVelocity : otherRB.velocity;
        
        int mPrio = movePriority;
        int otherMPrio = otherPC.movePriority;
        

        //cancel charge on impact
        if(otherMPrio > 1 && otherPC.movePower > powerCurve.Evaluate(2f * minCharge))
        {
            chargeTime = 0;
            chargeVal = 0;
        }


        GetComponentInChildren<TrailRenderer>().emitting = false; //cancel wall item on impact

        //difference in priority
        int priorityDiff = mPrio - otherMPrio;

        //difference in powers
        float powerDiff = movePower - otherPower;
        //ratio
        float powerRatio = movePower / otherPower;

        //angle players are colliding:
            //1 = moving straight at each other
            //0 = moving in same direction
        float directionDiff = Vector2.Angle(velocity, otherVelocity) / 180;

        //relative positions
        //vector pointing from player's position to otherPlayer's position i.e. relative position (direction only)
        Vector2 posDiff = (otherPC.transform.position - transform.position).normalized;
        //vector pointing from otherPlayer's position to this player's position
        Vector2 otherPosDiff = (transform.position - otherPC.transform.position).normalized;

        //how much of a "direct hit" it is
            //1 = direct hit
            //0 = indirect hit
        //animationCurve used to level out "almost direct hits" and keep value > 0
        float directness = 0;
        if(velocity != Vector2.zero)
        {
            directness = directnessKBCurve.Evaluate(1 - (Vector2.Angle(posDiff, velocity) / 180));
        }

        float otherDirectness = 0;
        if(otherVelocity != Vector2.zero)
        {
            otherDirectness = directnessKBCurve.Evaluate(1 - (Vector2.Angle(otherPosDiff, otherVelocity) / 180));
        }

        float directnessRatio = otherDirectness/directness;

        //Debug.Log("Player" + idx + " directness: " + directness);
        //Debug.Log("Player" + idx + " movePower: " + movePower);


        Debug.Log("P" + idx + " pos = " + transform.position + ", otherPos = " + otherPC.transform.position);
        Debug.Log("P" + idx + " RBvelocity = " + rb.velocity + ", otherRBVelocity = " + otherPC.rb.velocity);
        Debug.Log("P" + idx + " velocity = " + velocity + ", otherVelocity = " + otherVelocity);
        Debug.Log("P" + idx + " posDiff = " + posDiff + ", otherPosDiff = " + otherPosDiff);
        Debug.Log("P" + idx + " movePower = " + movePower + ", otherPower = " + otherPower);
        Debug.Log("P" + idx + " directness = " + directness + ", otherDirectness = " + otherDirectness);

        //overall strength: takes directness and power into account
        float strength = directness * movePower;
        float otherStrength = otherDirectness * otherPower;

        //armor calcs
        float armoredStrength = strength - (otherPC.passiveArmor + otherPC.moveArmor) + (passiveArmor + moveArmor);
        float armoredOtherStrength = otherStrength - (passiveArmor + moveArmor) + (otherPC.passiveArmor + otherPC.moveArmor);

        

        Debug.Log("P"+idx + " otherStrength: " + otherStrength + " (armor calcs)");
        float armor = (passiveArmor + moveArmor) - otherStrength;
        float otherArmor = (otherPC.passiveArmor + otherPC.moveArmor) - strength;
        Debug.Log("P"+idx + " armor: " + armor);
        Debug.Log("P"+idx + " otherArmor: " + otherArmor);

        //trying this...
        //otherStrength = Mathf.Clamp(armoredOtherStrength, .1f, 100);
        //Debug.Log("P"+idx + " armoredOtherStrength: " + otherStrength);

        float strengthDiff = otherStrength - strength;
        float strengthRatio = otherStrength / strength;


        //calculate knockback direction
        Vector2 direction;

        //calc hitstop
        float hitstop = (strength > otherStrength) ? (strength/maxMovePower) : (otherStrength/maxMovePower);

        Vector2 impactDirection = velocity.normalized;
        Vector2 otherImpactDirection = otherVelocity.normalized;


        //TRYING THIS
        //works for when one player overpowers other               
        direction = ((otherDirectness * otherImpactDirection) + (otherPosDiff * (1/otherDirectness))).normalized;


        
        //armor priority recalculations
        //other player is armored
        if (otherArmor > 0 && otherArmor > armor)
        {
            if(otherMPrio <= 1)
            {
                Debug.Log("P"+idx + " armored Priority: " + -1);

                //deflected
                mPrio = -1;
            } else
            {
                Debug.Log("P"+idx + " armored Priority: " + 1);

                //overpowered
                mPrio = 1;
            }

        //armor overpower
        } else if (armor > 0)
        {
            if (otherMPrio <= 1)
            {

            } else
            {
                Debug.Log("P"+idx + " armored Priority: " + 4);

                mPrio = 4;
            }

            
        }
        

        //always lose to rewind
        if(otherPC.isRewind)
        {
            mPrio = 0;
        }


        //hit particle effect
        GameObject part = null;
        if(idx < otherRB.gameObject.GetComponent<PlayerController>().idx)
        {
            if(hitstop >=.3f)
            {
                part = Instantiate(hitPart, (transform.position + otherRB.transform.position)/2, Quaternion.identity);
                part.GetComponent<HitEffect>().Init(.5f, maxHitstop * hitstopCurve.Evaluate(hitstop)*1.1f);
            }
        }



        //apply impact hitstop
        ApplyHitStop(maxHitstop * hitstopCurve.Evaluate(hitstop), 1);



        //wait one tick so other player's knockback calculations can finish
        //IMPORTANT: all variables used in KB calcs must be set before this yield
        yield return fuWait;
        
        //movearmor gets decreased permanently
        float remainingAttack = moveArmor - otherStrength;

        moveArmor = Mathf.Clamp(moveArmor - otherStrength, 0, 10000);

        //passive armor gets decreased by any overflow Strength
        if(moveArmor <= 0 && remainingAttack < 0)
        {
            passiveArmor = Mathf.Clamp(passiveArmor + remainingAttack, 0, 10000);
        }


        if(!isRewind)
        {

            //apply movement - type 1
            //need to calculate direction and charge

            //Debug.Log("player" + idx + " knockback direction: " + direction);
            //Debug.Log("player" + idx + " knockback strength: " + (knockbackMultiplier * otherStrength));
            
            //late parry window
            float timer = 0;
            while (timer < 5 * Time.fixedDeltaTime && isHitStop && !isRewind) //5 frames of leeway for inputting parry after impact
            {
                //someone parrys
                if(((!isKnockback && canWallTech && lastChargePress < parryFrameWindow * Time.fixedDeltaTime) || (!otherPC.isKnockback && otherPC.canWallTech && otherPC.lastChargePress < otherPC.parryFrameWindow * Time.fixedDeltaTime)) && (isMoving || otherPC.isMoving) && (movePower > (maxMovePower * powerCurve.Evaluate((3 * minCharge)/maxChargeTime)) || otherPower > (otherPC.maxMovePower * otherPC.powerCurve.Evaluate((3 * otherPC.minCharge)/otherPC.maxChargeTime))))
                {
                    //reset peer priority
                    //OverpowerPeerPrioTable[otherPC] = 0;
                    //IntangiblePeerPrioTable[otherPC] = 0;
                    SetPeerPriority(OverpowerPeerPrioTable, otherPC, 0);
                    SetPeerPriority(IntangiblePeerPrioTable, otherPC, 0);

                    //receive parry effects
                    if(part != null)
                    {
                        part.GetComponent<HitEffect>().SetColor(Color.white);
                    }

                    //play Parry soundfx
                    FindFirstObjectByType<AudioManager>().PlayRandom("Parry");
                    
                    StartCoroutine(ParryLaunch(otherPC));
                    yield break;
                }

                //impact particle effect
                SpawnImpactParticles((Vector2)transform.position + (Vector2)((otherPC.transform.position - transform.position)/2), otherPosDiff, sr.color);

                eyeSR.sprite = EyeSpriteSet[eyeSprite];

                timer += Time.fixedDeltaTime;
                yield return fuWait;
            }

            //play Impact soundfx
            FindFirstObjectByType<AudioManager>().PlayRandom("Impact");

            //KB calc vars
            float impedanceFactor;
            Vector2 directionMod;

            //if this player overpowers otherPlayer
            if(OverpowerPeerPrioTable.ContainsKey(otherPC) && OverpowerPeerPrioTable[otherPC] > 0)
            {
                Debug.Log("P"+idx + " overpowering!");

                //do different KB behavior - barrel through
                //"barrel through"
                //impedance based on otherStrength

                //give this player intangible priority from otherPlayer
                //EDIT THIS: int constant = invol frame data
                //IntangiblePeerPrioTable[otherPC] = 8 * Time.fixedDeltaTime;
                SetPeerPriority(IntangiblePeerPrioTable, otherPC, 8 * Time.fixedDeltaTime);



                //give this player overpower priority over otherPlayer
                //EDIT THIS: int constant = overPower frame data
                //OverpowerPeerPrioTable[otherPC] = 6 * Time.fixedDeltaTime;
                SetPeerPriority(OverpowerPeerPrioTable, otherPC, 6 * Time.fixedDeltaTime);

                //max possible glidestrength = maxMovePower * .1
                //Debug.Log("otherStrength = " + otherStrength);
                //alter travel distance
                impedanceFactor = 1 - Mathf.Clamp(1 * otherStrength, .1f, 1);

                //alter direction
                //factors: otherPosDiff, otherRB direction, 
                directionMod = ((2 * otherPosDiff.normalized) + (5 * otherVelocity.normalized)).normalized * Mathf.Clamp((3 * otherStrength), .4f, 1);

                ModifyMove(0, directionMod, impedanceFactor, impedanceFactor, .9f);

                //apply KB impact hitstop
                //StartCoroutine(HitStop(maxHitstop * hitstopCurve.Evaluate(hitstop)));

                yield break;
            }


            Debug.Log("P"+idx + " mPrio: " + mPrio);
            Debug.Log("P"+idx + " otherMPrio: " + otherMPrio);


            //trying this
            //sort of works, needs tuning (too much KB mitigation)
            otherStrength = .5f * (otherStrength + armoredOtherStrength);
            strength = .5f * (strength + armoredStrength);

            switch (mPrio)
            {
                case -1: //armor deflect
                    
                    //give this player intangible priority from otherPlayer
                    //EDIT THIS: int constant = invol frame data
                    //IntangiblePeerPrioTable[otherPC] = 8 * Time.fixedDeltaTime;
                    SetPeerPriority(IntangiblePeerPrioTable, otherPC, 8 * Time.fixedDeltaTime);

                    //give otherPlayer overpower priority over this Player
                    //EDIT THIS: int constant = overPower frame data
                    //otherPC.OverpowerPeerPrioTable[idx] = 6 * Time.fixedDeltaTime;


                    Debug.Log("P"+idx + " deflected! (armor deflect)");

                    //currently in hitstop, use -storedvelocity to cancel out current direction
                    directionMod = (Vector2.Reflect(storedVelocity, otherPosDiff) * 200);
                    Debug.Log("P"+idx + " directionMod: " + directionMod);

                    ModifyMove(1, directionMod, 1.1f, .95f, .8f);

                    //assign killCredit
                    killCredit = otherPC.idx;
                    killCreditTimer = 0;
                    

                    break;
                case 0: //standing still
                    if(otherMPrio <= 1) //other standing still / gliding
                    {
                        //ADD THIS:
                        //weak gliding KB nudge
                        //use both glidePowers in calc
                        eyeSprite = 1;

                        ApplyMove(1, direction, knockbackMultiplier * otherStrength);

                        if (otherMPrio ==1)
                        {
                            //give otherpc killcredit if bumping on glide
                            killCredit = otherPC.idx;
                            killCreditTimer = 0; 
                        }

                    } else if(otherMPrio == 2) //other being KB launched
                    {
                        eyeSprite = 2;

                        //use other player's stats for KB
                        direction = ((otherDirectness * otherImpactDirection) + (otherPosDiff * (1/otherDirectness))).normalized;


                        //powerful attack KB
                        ApplyMove(1, direction, knockbackMultiplier * otherStrength);

                        //give original attacker killcredit
                        killCredit = otherPC.killCredit;
                        killCreditTimer = 0;

                    } else if(otherMPrio >= 3) //other moving attacking
                    {
                        //give this player intangible priority from otherPlayer
                        //EDIT THIS: int constant = invol frame data
                        //IntangiblePeerPrioTable[otherPC] = 8 * Time.fixedDeltaTime;
                        SetPeerPriority(IntangiblePeerPrioTable, otherPC, 8 * Time.fixedDeltaTime);

                        //give otherPlayer overpower priority over this Player
                        //EDIT THIS: int constant = overPower frame data
                        //otherPC.OverpowerPeerPrioTable[idx] = 6 * Time.fixedDeltaTime;

                        eyeSprite = 2;

                        //use other player's stats for KB
                        direction = ((otherDirectness * otherImpactDirection) + (otherPosDiff * (1/otherDirectness))).normalized;

                        //TWEAK THIS - calc should be more biased to otherPlayer
                        //powerful attack KB
                        ApplyMove(1, direction, knockbackMultiplier * otherStrength);

                        //otherpc killcredit
                        killCredit = otherPC.idx;
                        killCreditTimer = 0;
                    }

                    break;
                case 1: //gliding
                    if(otherMPrio <= 1)
                    {
                        //ADD THIS:
                        //weak gliding KB nudge
                        //use both glidePowers in calc

                        eyeSprite = 1;

                        ApplyMove(1, direction, knockbackMultiplier * otherStrength);
                        
                        //otherpc gets killcredit
                        killCredit = otherPC.idx;
                        killCreditTimer = 0;

                    } else if(otherMPrio == 2)
                    {
                        eyeSprite = 2;

                        //use other player's stats for KB
                        direction = ((otherDirectness * otherImpactDirection) + (otherPosDiff * (1/otherDirectness))).normalized;
                        
                        //powerful attack KB
                        ApplyMove(1, direction, knockbackMultiplier * otherStrength);

                        //give original attacker killcredit
                        killCredit = otherPC.killCredit;
                        killCreditTimer = 0;

                    } else if(otherMPrio >= 3)
                    {
                        //give this player intangible priority from otherPlayer
                        //EDIT THIS: int constant = invol frame data
                        //IntangiblePeerPrioTable[otherPC] = 8 * Time.fixedDeltaTime;
                        SetPeerPriority(IntangiblePeerPrioTable, otherPC, 8 * Time.fixedDeltaTime);

                        //give otherPlayer overpower priority over this Player
                        //EDIT THIS: int constant = overPower frame data
                        //otherPC.OverpowerPeerPrioTable[idx] = 6 * Time.fixedDeltaTime;

                        eyeSprite = 2;

                        //use other player's stats for KB
                        direction = ((otherDirectness * otherImpactDirection) + (otherPosDiff * (1/otherDirectness))).normalized;

                        //powerful attack KB
                        ApplyMove(1, direction, knockbackMultiplier * otherStrength);
                        
                        //otherpc killcredit
                        killCredit = otherPC.idx;
                        killCreditTimer = 0;
                    }

                    break;
                case 2: //knockback launch
                    if(otherMPrio <= 1)
                    {
                        eyeSprite = 1;

                        //"barrel through"
                        //impedance based on otherStrength

                        //give this player intangible priority from otherPlayer
                        //EDIT THIS: int constant = invol frame data
                        //IntangiblePeerPrioTable[otherPC] = 8 * Time.fixedDeltaTime;
                        SetPeerPriority(IntangiblePeerPrioTable, otherPC, 8 * Time.fixedDeltaTime);


                        //give this player overpower priority over otherPlayer
                        //EDIT THIS: int constant = overPower frame data
                        //OverpowerPeerPrioTable[otherPC.idx] = 6 * Time.fixedDeltaTime;

                        //max possible glidestrength = maxMovePower * .1
                        Debug.Log("otherStrength = " + otherStrength);
                        //alter travel distance
                        impedanceFactor = 1 - Mathf.Clamp(1 * otherStrength, .1f, 1);

                        //alter direction
                        //factors: otherPosDiff, otherRB direction, 
                        directionMod = ((2 * otherPosDiff.normalized) + (5 * otherVelocity.normalized)).normalized * Mathf.Clamp((3 * otherStrength), .4f, 1);

                        ModifyMove(0, directionMod, 1.1f, .95f, 1);      

                        //killcredit unchanged



                        //old "8ball" behaviour works for now
                        //^^BRITISH???
                        //ApplyMove(1, direction, knockbackMultiplier * otherStrength);

                        //TRY THIS:
                        //bounce off with slightly increased moveTime
                        //(similar to wallbounce behavior)
                        //ApplyMove(1,)
                        //direction = (velocity.normalized) + Vector2.Reflect(velocity.normalized, otherPosDiff);
                        //ModifyMove(1, direction, 1.1f, .95f, 1);

                        //idk


                    } else if(otherMPrio == 2)
                    {
                        eyeSprite = 2;


                        //this should be good (same as 3,3 impact)
                        direction = (((3 * Mathf.Clamp(otherDirectness, (otherPower/otherPC.maxMovePower), 2)) * otherImpactDirection) + (otherPosDiff * (1/Mathf.Clamp(otherDirectness, (otherPower/otherPC.maxMovePower), 2))) + (.7f * (1 - (Vector2.Angle(impactDirection, otherImpactDirection)/180)) * powerRatio * impactDirection)).normalized;

                        //Equal KB exchange
                        ApplyMove(1, direction, knockbackMultiplier * otherStrength);

                        //ADD THIS: assign killcredit either to current or otherPc original attacker
                        //based on new KB direction: angle>90 otherPc original, <90 current attacker
                        //killCredit = 
                        killCreditTimer = 0;

                        //TRY THIS:
                        //impede stronger player's KB more

                    } else if(otherMPrio >= 3)
                    {
                        eyeSprite = 2;


                        //TUNE THIS
                        //use other player's direction in calc

                        direction = (((3 * Mathf.Clamp(otherDirectness, (otherPower/otherPC.maxMovePower), 2)) * otherImpactDirection) + (otherPosDiff * (1/Mathf.Clamp(otherDirectness, (otherPower/otherPC.maxMovePower), 2))) + (.7f * (1 - (Vector2.Angle(impactDirection, otherImpactDirection)/180)) * powerRatio * impactDirection)).normalized;

                        //receive full KB
                        ApplyMove(1, direction, knockbackMultiplier * otherStrength);

                        //otherpc killcredit
                        killCredit = otherPC.idx;
                        killCreditTimer = 0;
                    }

                    break;
                case 3: //moving launch
                    if(otherMPrio <= 1)
                    {
                        //"barrel through"
                        //impedance based on otherStrength

                        //give this player intangible priority from otherPlayer
                        //EDIT THIS: int constant = invol frame data
                        //IntangiblePeerPrioTable[otherPC] = 8 * Time.fixedDeltaTime;
                        SetPeerPriority(IntangiblePeerPrioTable, otherPC, 8 * Time.fixedDeltaTime);


                        //give this player overpower priority over otherPlayer
                        //EDIT THIS: int constant = overPower frame data
                        //OverpowerPeerPrioTable[otherPC] = 6 * Time.fixedDeltaTime;
                        SetPeerPriority(OverpowerPeerPrioTable, otherPC, 6 * Time.fixedDeltaTime);


                        eyeSprite = 1;
                        
                        //max possible glidestrength = maxMovePower * .1
                        Debug.Log("otherStrength = " + otherStrength);
                        //alter travel distance
                        impedanceFactor = 1 - Mathf.Clamp(1 * otherStrength, .1f, 1);

                        //alter direction
                        //factors: otherPosDiff, otherRB direction, 
                        directionMod = ((2 * otherPosDiff.normalized) + (5 * otherVelocity.normalized)).normalized * Mathf.Clamp((3 * otherStrength), .4f, 1);

                        ModifyMove(0, directionMod, impedanceFactor, impedanceFactor, .9f);                        

                    } else if(otherMPrio == 2)
                    {
                        eyeSprite = 1;
                        
                        //ADD THIS
                        //apply reduced knockback based on powerDiff



                        //new direction calc
                        //ADD THIS:
                        //tweak (powerRatio * impactDirection vals)
                        //to allow overpowering players in weak KB launch
                        direction = (((3 * Mathf.Clamp(otherDirectness, (otherPower/otherPC.maxMovePower), 2)) * otherImpactDirection) + (otherPosDiff * (1/Mathf.Clamp(otherDirectness, (otherPower/otherPC.maxMovePower), 2))) + (.7f * (1 - (Vector2.Angle(impactDirection, otherImpactDirection)/180)) * powerRatio * impactDirection)).normalized;


                        ApplyMove(1, direction, knockbackMultiplier * otherStrength);

                        //original attacker killcredit
                        killCredit = otherPC.killCredit;
                        killCreditTimer = 0;

                    } else if(otherMPrio >= 3)
                    {
                        eyeSprite = 2;

                        //ADD THIS: update strength calc
                        //less weight on otherdirectness
                        //add weight from (1/otherDirectness) * this player's moveTimer/moveTime
                        
                        //otherStrength = otherDirectness * otherPower;

                        //new direction calc
                        //GOOD:
                        direction = (((3 * Mathf.Clamp(otherDirectness, (otherPower/otherPC.maxMovePower), 2)) * otherImpactDirection) + (otherPosDiff * (1/Mathf.Clamp(otherDirectness, (otherPower/otherPC.maxMovePower), 2))) + (.7f * (1 - (Vector2.Angle(impactDirection, otherImpactDirection)/180)) * powerRatio * impactDirection)).normalized;
                        Debug.Log("P" + idx + " KB launch direction: " + direction);
                        //DONE LFG

                        //Equal KB exchange
                        ApplyMove(1, direction, knockbackMultiplier * otherStrength);

                        killCredit = otherPC.idx;
                        killCreditTimer = 0;
                    }

                    break;

                case 4: //armored
                    Debug.Log("P" + idx + " Armored!");

                    if(otherMPrio == -1)
                    {

                    } else
                    {
                        //barrel through
                        //"barrel through"
                        //impedance based on otherStrength

                        //give this player intangible priority from otherPlayer
                        //EDIT THIS: int constant = invol frame data
                        //IntangiblePeerPrioTable[otherPC] = 8 * Time.fixedDeltaTime;
                        SetPeerPriority(IntangiblePeerPrioTable, otherPC, 8 * Time.fixedDeltaTime);

                        //give this player overpower priority over otherPlayer
                        //EDIT THIS: int constant = overPower frame data
                        //OverpowerPeerPrioTable[otherPC] = 6 * Time.fixedDeltaTime;
                        SetPeerPriority(OverpowerPeerPrioTable, otherPC, 6 * Time.fixedDeltaTime);


                        eyeSprite = 1;
                        
                        //max possible glidestrength = maxMovePower * .1
                        Debug.Log("otherStrength = " + otherStrength);
                        //alter travel distance
                        impedanceFactor = 1 - Mathf.Clamp(.3f * otherStrength, .2f, 1);

                        //alter direction
                        //factors: otherPosDiff, otherRB direction, 
                        directionMod = ((2 * otherPosDiff.normalized) + (4 * otherVelocity.normalized)).normalized * Mathf.Clamp((.5f * otherStrength), .2f, .6f);
                        ModifyMove(0, directionMod, impedanceFactor, impedanceFactor, .9f);
                    }
                    

                    break;
                default:
                    break;
            }
            
            //animate impact
            while(isHitStop)
            {
                eyeSR.sprite = EyeSpriteSet[eyeSprite];
                yield return fuWait;
            }

        } else //this player is rewinding: grant peer priority
        {
            //give this player intangible priority from otherPlayer
            //EDIT THIS: int constant = invol frame data
            //IntangiblePeerPrioTable[otherPC] = 8 * Time.fixedDeltaTime;
            SetPeerPriority(IntangiblePeerPrioTable, otherPC, 8 * Time.fixedDeltaTime);

            //give this player overpower priority over otherPlayer
            //EDIT THIS: int constant = overPower frame data
            //OverpowerPeerPrioTable[otherPC] = 6 * Time.fixedDeltaTime;
            SetPeerPriority(OverpowerPeerPrioTable, otherPC, 6 * Time.fixedDeltaTime);

        }

    }


    public override void OnCollisionEnter2D(Collision2D col)
    {
        Debug.Log("solid player collider colliding! col Layer: " + LayerMask.LayerToName(col.gameObject.layer));


        //impact soundfx
        //FindFirstObjectByType<AudioManager>().PlayRandom("Impact");


        if(LayerMask.LayerToName(col.gameObject.layer) == "Solids")
        {
            
            
            StartCoroutine(OnSolidCollision(col));
            

            
        }


    }

    IEnumerator OnSolidCollision(Collision2D col)
    {
        //Debug.Log("OnSolidCollision!");
        
        WaitForFixedUpdate fuWait = new WaitForFixedUpdate();

        //cooldown prevents infinite speed gain hopefully
        if(lastSolidCollision > 5 * Time.fixedDeltaTime)
        {
            //extend movement slightly
            ModifyMove(0, Vector2.zero, 1.2f, .95f, 1);
        }

        //hitstop calculation
        float hitstop = Mathf.Clamp(.3f * maxHitstop * hitstopCurve.Evaluate(movePower/maxMovePower), 3*Time.fixedDeltaTime, maxHitstop);
        Debug.Log("solid collision hitstop: " + hitstop);

        //apply hitstop
        //StartCoroutine(HitStop(hitstop));
        ApplyHitStop(hitstop, 0);

        lastSolidCollision = 0;

        //save wall normal
        ContactPoint2D point = col.GetContact(0);
        Vector2 solidNorm = point.normal.normalized;

        //player color for impact particle fx
        Color color = sr.color;

        //wall tech timer
        float timer = 0;
        while (timer <= 3 * Time.fixedDeltaTime) //2 frames of wiggle room after impact for late inputs
        {
            float window = wallTechFrameWindow * Time.fixedDeltaTime;
            if(isKnockback)
            {
                window = 1.5f * wallTechFrameWindow * Time.fixedDeltaTime;
            }

            //wall tech frame window
            if(canWallTech && lastChargePress <= window && solidNorm != lastWallTechNormal.normalized)
            {
                //change fx color for visual feedback
                color = Color.white;

                StartCoroutine(WallTech(col));
                break;
            }

            //waiting
            timer += Time.fixedDeltaTime;
            yield return fuWait;
        }

        //spawn impact particles

        SpawnImpactParticles(point.point, solidNorm, color);


    }

    //wallbounce tech
    IEnumerator WallTech(Collision2D wall)
    {
        WaitForFixedUpdate fuWait = new WaitForFixedUpdate();

        //FIX THIS player tp bug still happens on same-frame
        //important that we only call this once at the beginning
        //ContactPoint2D contact = wall.GetContact(0);
        Debug.Log("wall.contactCount: " + wall.contactCount);

        
        //why doesn't this work??
        //(CREATES MEMORY GARBAGE) ^^unrelated
        ContactPoint2D[] contacts = new ContactPoint2D[wall.contactCount];
        wall.GetContacts(contacts);
        ContactPoint2D contact = Array.Find<ContactPoint2D>(contacts, p => p.otherCollider == SolidCol);
        

        Debug.Log("contacts[0]: " + wall.GetContact(0).point);
        //Debug.Log("contacts[1]: " + wall.GetContact(1).point);
        //Debug.Log("contacts[2]: " + wall.GetContact(2).point);


        Vector2 contactPoint = contact.point;
        Vector2 normal = contact.normal;


        //extra hitstop for player feedback
        float hitstop = Mathf.Clamp(.7f * maxHitstop * hitstopCurve.Evaluate(movePower/maxMovePower), 5*Time.fixedDeltaTime, maxHitstop);
        //StartCoroutine(HitStop(hitstop));
        ApplyHitStop(hitstop, 0);

        //read DI input up until launch (for responsiveness)
        while(isHitStop)
        {
            sr.sprite = SpriteSet[4];
            RotateSprite(normal);

            transform.position = contactPoint + (.2f * normal.normalized);
            //Debug.Log("P" + idx + " tech pos: " + transform.position);

            //five frame direction input buffer
            Vector2 moveDirection = Vector2.zero;
            if(true_i_move.magnitude != 0)
            {
                moveDirection = true_i_move.normalized;
            } else if(lastIMove < 5 * Time.fixedDeltaTime) //DI leeway room
            {
                moveDirection = i_move.normalized;
            }

             //constrain new direction based off wall normal
            if(Vector2.Angle(moveDirection, normal) > 90)
            {
                storedVelocity = storedVelocity.magnitude * Vector2.Reflect(moveDirection, normal).normalized;
            } else
            {
                storedVelocity = storedVelocity.magnitude * moveDirection.normalized;
            }

            Debug.Log("wallTech storedVelocity: " + storedVelocity);
            yield return fuWait;
        }


        //save norm
        lastWallTechNormal = normal.normalized;

        //reset cooldown timer
        lastWallTech = 0;

        if(Vector2.Angle(true_i_move, normal) > 105) //a little leeway for parallels
        {
            //player is holding into wall
            //decrease distance
            ModifyMove(0, Vector2.zero, .9f, .9f, 1.05f);
        } else
        {
            //player is holding away from wall
            //add extra distance
            ModifyMove(0, Vector2.zero, 1.4f, .95f, 1.05f);
        }
        
        Debug.Log("P" + idx + " WALLTECH!");
    }


    //launch KB received when this player is parried by otherPlayer
    IEnumerator ParryLaunch(PlayerController otherPC)
    {
        WaitForFixedUpdate fuWait = new WaitForFixedUpdate();

        //give this player intangible priority from otherPlayer
        //EDIT THIS: int constant = invol frame data
        //IntangiblePeerPrioTable[otherPC] = 8 * Time.fixedDeltaTime;
        SetPeerPriority(IntangiblePeerPrioTable, otherPC, 8 * Time.fixedDeltaTime);

        //give otherPlayer intangible priority from this player
        //EDIT THIS: int constant = invol frame data
        //otherPC.IntangiblePeerPrioTable[this] = 8 * Time.fixedDeltaTime;
        otherPC.SetPeerPriority(otherPC.IntangiblePeerPrioTable, this, 8 * Time.fixedDeltaTime);
        
        //save pre-impact stats for KB calcs
        float mTime = moveTime;
        float mTimer = moveTimer;
        float mPower = movePower;
        float otherMTime = otherPC.moveTime;
        float otherMTimer = otherPC.moveTimer;
        float otherMPower = otherPC.movePower;
        int mPrio = movePriority;
        int otherMPrio = otherPC.movePriority;


        float hitstop;
        if(movePower > otherPC.movePower)
        {
            hitstop = Mathf.Clamp(/*.8f * */ maxHitstop * hitstopCurve.Evaluate(movePower/maxMovePower), 4*Time.fixedDeltaTime, 2*maxHitstop);
        }else
        {
            hitstop = Mathf.Clamp(/*.8f * */ maxHitstop * hitstopCurve.Evaluate(otherPC.movePower/maxMovePower), 4*Time.fixedDeltaTime, 2*maxHitstop);
        }

        //extra hitstop for player feedback
        ApplyHitStop(hitstop, 0); //applying hitstop additively


        //this player parrying otherPlayer
        if(canWallTech && lastChargePress < parryFrameWindow * Time.fixedDeltaTime)
        {
            Debug.Log("P" + idx + " parried P" + otherPC.idx);

            bool doubleParry = false;
            //double parry
            if(otherPC.canWallTech && otherPC.lastChargePress < otherPC.parryFrameWindow * Time.fixedDeltaTime)
            {
                doubleParry = true;

                //boost recoil on double parry
                otherMPower *= 1.5f;
            }

            //save pre-impact otherVelocity
            Vector2 otherDirection = otherPC.rb.velocity.normalized;

            //impact particle effect
            SpawnImpactParticles((Vector2)transform.position + (Vector2)((otherPC.transform.position - transform.position)/2), (transform.position - otherPC.transform.position), Color.white);

            //wait to read this player's DI input up until launch (for responsiveness)
            while(isHitStop)
            {
                sr.sprite = SpriteSet[3];
                eyeSR.sprite = EyeSpriteSet[4];

                RotateSprite(otherPC.transform.position - transform.position);

                //parrying players are inputting launch direction during this time...
                yield return fuWait;
            }

            


            //direction this player is aiming parry
            //and direction other player is launched
            Vector2 parryDirection = Vector2.zero; //neutral parry has no recoil
            if(true_i_move.magnitude != 0)
            {
                parryDirection = true_i_move.normalized;
            } else if(lastIMove < 5 * Time.fixedDeltaTime) //DI leeway room
            {
                parryDirection = i_move.normalized;
            } else if(doubleParry)
            {
                Debug.Log("double parry! P" + idx + " neutral");

                //neutral double parry has recoil
                parryDirection = (transform.position - otherPC.transform.position).normalized;
            }

            Debug.Log("P" + idx + " parryDirection: " + parryDirection);

            //recoil on non-neutral parry (or neutral double parry)
            if(parryDirection != Vector2.zero)
            {
                //successful parry recoil
                Vector2 direction = ((.9f * Mathf.Clamp((otherMPower/maxMovePower), .5f, 2) * -(parryDirection.normalized)) + (.1f * otherMPower * otherDirection)).normalized;

                Debug.Log("P" + idx + " Parry recoil direction: " + direction);

                //apply parry recoil KB to this player in opposite direction of parry
                ApplyMove(1, direction,  Mathf.Clamp(.2f * otherMPower, (.1f * maxMovePower), (.5f * maxMovePower)));
            } else //no recoil on neutral parry
            {
                rb.velocity = Vector2.zero;
            }
            

            //reset cooldown timer
            lastWallTech = 0;

        //otherPlayer parrying this player
        } else if(otherPC.canWallTech && otherPC.lastChargePress < otherPC.parryFrameWindow * Time.fixedDeltaTime)
        {
            //impact particle effect
            SpawnImpactParticles((Vector2)transform.position + (Vector2)((otherPC.transform.position - transform.position)/2), (transform.position - otherPC.transform.position), sr.color);

            //wait to read players' DI input up until launch (for responsiveness)
            while(isHitStop)
            {
                //parrying players are inputting launch direction during this time...
                
                yield return fuWait;
            }

            Vector2 otherParryDirection;
            if(otherPC.true_i_move.magnitude != 0)
            {
                otherParryDirection = otherPC.true_i_move.normalized;

                //KB starts from parrying player's position
                transform.position = otherPC.transform.position;
            } else if(otherPC.lastIMove < 5 * Time.fixedDeltaTime)
            {
                otherParryDirection = otherPC.i_move.normalized;

                //KB starts from parrying player's position
                transform.position = otherPC.transform.position;
            } else
            {
                //use posDiff if neutral parry
                otherParryDirection = (transform.position - otherPC.transform.position).normalized;
            }


            //apply parry KB to this player in direction inputted by otherPlayer
            if(mPower >= otherMPower) //player is launching (defensive parry) use this player's power
            {
                Debug.Log("defensive parry");
                Debug.Log("defense parry power: " + /*.7f * */ Mathf.Clamp((1 - (mTimer/mTime)), .6f, 1)  * knockbackMultiplier * mPower);
                ApplyMove(1, otherParryDirection,  /*.7f * */ Mathf.Clamp((1 - (mTimer/mTime)), .6f, 1) * knockbackMultiplier * mPower);
            } else //player is standing still (aggressive parry) use attacker's power
            {
                Debug.Log("aggressive parry");
                Debug.Log("aggro parry power: " + /*.7f * */ Mathf.Clamp((1 - (otherMTimer/otherMTime)), .6f, 1) * knockbackMultiplier * otherMPower);
                ApplyMove(1, otherParryDirection,  /*.7f * */ Mathf.Clamp((1 - (otherMTimer/otherMTime)), .6f, 1) * knockbackMultiplier * otherMPower);
            }
        }

        //assign killcredit to otherPC (on impact regardless of who parried)
        killCredit = otherPC.idx;
        
    }


    //called in OnTriggerEnter2D(
    public override void OnHurtboxTriggerEnter(Collider2D col)
    {
        //FindFirstObjectByType<AudioManager>().PlayRandom("Impact");

        Debug.Log("PlayerHB colliding! Collision LayerMask name: " + LayerMask.LayerToName(col.gameObject.layer));

        if (LayerMask.LayerToName(col.gameObject.layer) == "Players")
        {
            if(col.GetComponent<PlayerHurtBox>() != null)
            {
                if(col == col.GetComponent<PlayerHurtBox>().hb)
                {
                    PlayerController otherPC = col.GetComponentInParent<PlayerController>();

                    //ignore collision if peer-intangible
                    if(IntangiblePeerPrioTable.ContainsKey(otherPC) && IntangiblePeerPrioTable[otherPC] > 0 /* || otherPC.IntangiblePeerPrioTable[idx] > 0 */)
                    {
                        Debug.Log("Intangible onCollision. Exiting.");
                        return;
                    }

                    Debug.Log("Player" + idx + " collided with Player " + col.gameObject.GetComponentInParent<PlayerController>().idx);

                    //positional collision hack
                    //previous position
                    
                    /*
                    Vector2 prev = transform.position;
                    if(prevPos[1] != (Vector2)transform.position)
                    {
                        prev = prevPos[1];
                    } else if(prevPos[0] != (Vector2)transform.position)
                    {
                        prev = prevPos[0];
                    }

                    Vector2 otherPrev = otherPC.transform.position;
                    if(otherPC.prevPos[1] != (Vector2)otherPC.transform.position)
                    {
                        otherPrev = otherPC.prevPos[1];
                    } else if(otherPC.prevPos[0] != (Vector2)otherPC.transform.position)
                    {
                        otherPrev = otherPC.prevPos[0];
                    }
                    */
                    /*
                    //safety
                    if(prevPos.Count == 0)
                    {
                        prevPos.Add(transform.position);
                    }
                    if(prevPos.Count == 1)
                    {
                        prevPos.Add(transform.position);
                    }

                    //safety
                    if(otherPC.prevPos.Count == 0)
                    {
                        otherPC.prevPos.Add(otherPC.transform.position);
                    }
                    if(otherPC.prevPos.Count == 1)
                    {
                        otherPC.prevPos.Add(otherPC.transform.position);
                    }
                    */

                    Vector2 prev = prevPos[1];                    
                    Vector2 otherPrev = otherPC.prevPos[1];
                    
                    /* //my descent into madness:
                    Debug.Log("ishitstop: " + isHitStop);
                    Debug.Log("otherIsHitstop: " + otherPC.isHitStop);

                    Debug.Log("transform.pos: " + transform.position);
                    Debug.Log("prevPos[0]: " + prevPos[0]);
                    Debug.Log("prevPos[1]: " + prevPos[1]);
                    Debug.Log("otherTransform.pos: " + otherPC.transform.position);
                    Debug.Log("otherPrevPos[0]: " + otherPC.prevPos[0]);
                    Debug.Log("otherPrevPos[1]: " + otherPC.prevPos[1]);

                    Debug.Log("prev: " + prev);
                    Debug.Log("otherPrev: " + otherPrev);
                    */
                    
                    //collision correction safety check
                    if((prev - otherPrev).magnitude > HurtBoxTrigger.radius + otherPC.HurtBoxTrigger.radius)
                    {
                        //correct collision position
                        var (pos, otherPos) = EstimateCircleTriggerCollision(HurtBoxTrigger.radius * transform.localScale.x, otherPC.HurtBoxTrigger.radius * otherPC.transform.localScale.x, transform.position, prev, otherPC.transform.position, otherPrev);

                        Debug.Log("newPos: " + pos);
                        Debug.Log("otherNewPos: " + otherPos);

                        transform.position = pos;
                        otherPC.transform.position = otherPos; //is this needed?
                    }

                    StartCoroutine(ApplyKnockback(/*otherPC.movePriority,*/ otherPC.movePower, otherPC.rb));
                }
            }
            
        } else if(LayerMask.LayerToName(col.gameObject.layer) == "ItemObjs")
        {
            //colliding with ShotObj
            if(col.gameObject.name == "ShotTriggerHurtBox")
            {
                ShotObj shot = col.GetComponentInParent<ShotObj>();
                if(shot.immunityID != idx)
                {
                    Vector2 prev = prevPos[1];
                    Vector2 shotPrev = shot.prevPos[1];

                    //correct collision position
                    var (pos, otherPos) = EstimateCircleTriggerCollision(HurtBoxTrigger.radius, shot.TriggerHB.radius, transform.position, prev, shot.transform.position, shotPrev);
                    transform.position = pos;
                    shot.transform.position = otherPos;
                    
                    StartCoroutine(ApplyKnockback(/*3,*/ shot.shotPower, shot.GetComponent<Rigidbody2D>()));
                }
                
            }
        } else if(LayerMask.LayerToName(col.gameObject.layer) == "Interactables")
        {
            if(col.TryGetComponent<Bumper>(out Bumper bumper))
            {
                Vector2 prev = prevPos[1];
                Vector2 bPrev = bumper.prevPos[1];

                //correct collision position
                //causes stack overflow... leaving it out for now
                //var (pos, otherPos) = EstimateCircleTriggerCollision(HurtBoxTrigger.radius, bumper.TriggerHB.radius, transform.position, prev, bumper.transform.position, bPrev);
                //transform.position = pos;
                //bumper.transform.position = otherPos;

                ApplyBumperKnockback(bumper);
            }

        }


    }

    (Vector2 pos, Vector2 otherPos) EstimateCircleTriggerCollision(float radius1, float radius2, Vector2 pos, Vector2 prev, Vector2 otherPos, Vector2 otherPrev)
    {
        //exit condition
        if(((radius1 + radius2)+.01f >= (prev-otherPrev).magnitude) && ((prev-otherPrev).magnitude >= (radius1 + radius2)-.01f))
        {
            return(prev, otherPrev);
        }


        //recursive divide + conquer loop
        Vector2 dist = pos - prev; //dist points toward pos
        Vector2 otherDist = otherPos - otherPrev;

        //Vector2 newPrev;
        //Vector2 otherNewPrev;

        //plus 1/2 or -1/2 for closer or farther
        if((prev-otherPrev).magnitude > radius1 + radius2)
        {
            prev += (.5f * dist);
            otherPrev += (.5f * otherDist);
        }else if((prev-otherPrev).magnitude < radius1 + radius2)
        {
            prev -= (.5f * dist);
            otherPrev -= (.5f * otherDist);
        }

        //repeat loop
        return EstimateCircleTriggerCollision(radius1, radius2, pos, prev, otherPos, otherPrev);

    }


    public override void Deactivate()
    {
        ResetDefaultStats();

        prevStates.Clear();

        StopAllCoroutines();
        

        bubblePart.gameObject.SetActive(false);
        
        rb.velocity = Vector2.zero;
        i_move = Vector2.zero;
        true_i_move = Vector2.zero;
        chargeTime = 0;
        chargeVal = 0;
        charging = false;
        specialCharging = false;
        isRewind = false;
        ClearInventory();
        this.GetComponent<CircleCollider2D>().enabled = false;
        HurtBoxTrigger.enabled = false;
        sr.enabled = false;
        eyeSR.enabled = false;
        hatSR.enabled = false;
        GetComponentInChildren<TrailRenderer>().emitting = false;
        GetComponentInChildren<TrailRenderer>().Clear();
        rc.DeactivateReticle();
        
        if(gp != null)
        {
            gp.SetMotorSpeeds(0,0);
        }


        tg = FindFirstObjectByType<CinemachineTargetGroup>();
        if(tg != null)
        {
            tg.RemoveMember(this.transform);
        }
    }

    public override void Reactivate()
    {
        ResetDefaultStats();

        stamina = maxStamina;

        bubblePart.gameObject.SetActive(true);

        i_move= Vector2.zero;
        true_i_move = Vector2.zero;
        chargeTime = 0;
        chargeVal = 0;
        charging = false;
        specialCharging = false;
        
        this.GetComponent<CircleCollider2D>().enabled = true;
        HurtBoxTrigger.enabled = true;
        sr.enabled = true;
        eyeSR.enabled = true;

        //enable hat for team battle
        if(gm.gameMode == 1 || gm.gameMode == 2)
        {
            hatSR.enabled = true;
        }

        GetComponentInChildren<TrailRenderer>().emitting = false;
        GetComponentInChildren<TrailRenderer>().Clear();
        rc.DeactivateReticle();

        if(gp != null)
        {
            gp.SetMotorSpeeds(0,0);

        }

        tg = FindFirstObjectByType<CinemachineTargetGroup>();
        if(tg != null)
        {
            tg.AddMember(this.transform, 1, 1);
        }
    }



    //spawns impact particles at pos facing in direction
    void SpawnImpactParticles(Vector2 pos, Vector2 direction, Color color)
    {
        float angle = Vector2.SignedAngle(Vector2.up, direction);
        GameObject part = Instantiate(impactPart, pos, Quaternion.AngleAxis(angle, Vector3.forward));

        ParticleSystem.MainModule main = part.GetComponent<ImpactParticle>().partSys.main;
        main.startColor = color;
    }

    void SpawnDeathParticles(Vector2 pos, Color color)
    {
        GameObject part = Instantiate(deathPart, pos, Quaternion.identity);
        
        ParticleSystem.MainModule main = part.GetComponent<ParticleSystem>().main;
        main.startColor = color;
    }


    //used for killing this player specifically (not eliminating from game if clones are alive)
    public override void KillPlayer()
    {
        SpawnDeathParticles(transform.position, pm.PlayerList[killCredit].playerScript.sr.color);

        isAlive = false;
        Deactivate();
        FindFirstObjectByType<AudioManager>().Play("Fall");
    }


    //true killplayer
    //pm.kill player if all clones are dead
    public virtual void CheckIfAlive()
    {
        //true if any clones are alive
        if(!(isAlive || Clones.Any(dummy => dummy.isAlive)))
        {
            Debug.Log("Player" + idx + " killed by Player" + killCredit);
            pm.KillPlayer(idx, killCredit);
        }
    }





}
