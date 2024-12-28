using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using Unity.Mathematics;
using UnityEngine;

public class ReticleController : MonoBehaviour
{
    [SerializeField] PlayerController pc;
    [SerializeField] LineRenderer lr; //default linerenderer reticle
    [SerializeField] Transform wr; //warp reticle
    //[SerializeField] LineRenderer rlr; //rewind reticle
    [SerializeField] float warpSpeed;
    [SerializeField] SpriteRenderer warpSR;


    //rewind tick variables
    int prevTC = 0;
    Stack<PlayerState> states;




    public Color32[] colorSet;

    Vector3 reticlePosition;

    // Start is called before the first frame update
    void Start()
    {
        pc = GetComponentInParent<PlayerController>();
        lr = GetComponent<LineRenderer>();

        states = new Stack<PlayerState>(pc.prevStates.ToArray());
        
        ChangeColor(pc.colorID);

        lr.enabled = false;
        reticlePosition = Vector3.zero;
    }

    // Update is called once per frame
    void Update()
    {
        warpSR.enabled = false;

        //hide reticle if dead
        if(!pc.pm.playerList[pc.idx].isAlive)
        {
            DeactivateReticle();
            return;
        }


        RenderNormalReticle();

        if(pc.heldItems.Any() && pc.heldItems[pc.selectedItemIdx] != null)
        {
            RenderSpecialReticle(pc.heldItems[pc.selectedItemIdx].GetItemType());
        }


    }


    void ActivateReticle()
    {
        SetReticle(0, Vector2.zero);
        lr.enabled = true;
    }

    public void DeactivateReticle()
    {
        SetReticle(0, Vector2.zero);
        lr.enabled = false;
        warpSR.enabled = false;
    }

    public void ChangeColor(int colorID)
    {
        warpSR.color = colorSet[colorID];

        lr.startColor = colorSet[colorID];
        lr.endColor = colorSet[colorID];
    }

    //New Movement 3.0
    public void RenderNormalReticle()
    {
        if(pc.charging)
        {
            if(pc.chargeTime > pc.minCharge)
            {
                transform.SetParent(pc.transform);
                transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
                ActivateReticle();
                lr.positionCount = 2;
                lr.SetPosition(0, Vector2.zero);

                //int chargeType = 0;

                if(!pc.isCoolingDown)
                {
                    lr.enabled = true;

                    SetReticle(0, Vector2.up);
                } else
                {
                    lr.enabled = false;
                    DeactivateReticle();
                }

            } else
            {
                DeactivateReticle();
            }
        } else
        {
            //DeactivateReticle();
            lr.enabled = false;
        }
    }

    //new Item Reticle func for movement3.0
    public void RenderSpecialReticle(string item)
    {
        if(pc.specialCharging)
        {
            if(pc.specialChargeTime > pc.minCharge)
            {
                switch (item)
                {
                    case "Shot":
                        ActivateReticle();
                        lr.positionCount = 2;

                        //int chargeType = 1;

                        if(!pc.isCoolingDown /*&& pc.isInBounds*/)
                        {
                            //dumb that this is here and not pc but idgaf
                            pc.RotateSprite(-pc.i_move);

                            lr.enabled = true;

                            SetReticle(1, Vector2.up);
                        } else
                        {
                            lr.enabled = false;
                            DeactivateReticle();
                        }

                        break;
                    
                    case "Ink":
                        ActivateReticle();
                        lr.positionCount = 2;

                        //int chargeType = 1;

                        if(!pc.isCoolingDown /*&& pc.isInBounds*/)
                        {
                            //dumb that this is here and not pc but idgaf
                            pc.RotateSprite(-pc.i_move);

                            lr.enabled = true;

                            SetReticle(1, Vector2.up);
                        } else
                        {
                            lr.enabled = false;
                            DeactivateReticle();
                        }   
                    
                        break;
                    
                    case "Wall":
                        ActivateReticle();
                        lr.positionCount = 2;

                        //int chargeType = 0;

                        if(!pc.isCoolingDown /*&& pc.isInBounds*/)
                        {
                            lr.enabled = true;

                            SetReticle(2, Vector2.up);
                        } else
                        {
                            lr.enabled = false;
                            DeactivateReticle();
                        }

                        break;
                    
                    case "Warp":
                        
                        break;

                    case "Rewind":

                        int tickCount = Mathf.RoundToInt(.6f * (Mathf.Clamp(pc.specialChargeTime, 0, pc.maxChargeTime)/pc.maxChargeTime) * pc.rewindSize);
                        
                        //DURING
                        if(!pc.isCoolingDown)
                        {
                            //START
                            //only do this once at start
                            if(transform.parent != null)
                            {
                                Debug.Log("initing rewind reticle!");

                                //temporarily free reticle from player parent
                                transform.SetParent(null);
                                transform.SetPositionAndRotation(Vector2.zero, Quaternion.identity);

                                //convert queue to stack
                                states.Clear();
                                states = new Stack<PlayerState>(pc.prevStates.ToArray());
                                //Vector2 pos = Vector2.zero;

                                lr.positionCount = 1;
                            }

                            lr.SetPosition(0, pc.transform.position);

                            lr.enabled = true;

                            if(Mathf.Clamp(pc.specialChargeTime, 0, pc.maxChargeTime)/pc.maxChargeTime < 1)
                            {
                                while(lr.positionCount < tickCount && states.Count > 0)
                                {
                                    //render reticle stuff here!!!!:
                                    PlayerState ps = states.Pop();
                                    Vector2 pos = new Vector2(ps.xPos, ps.yPos);

                                    lr.positionCount++;
                                    lr.SetPosition(lr.positionCount-1, pos);
                                }
                                
                            }


                        } else //cooling down
                        {
                            if(transform.parent == null)
                            {
                                transform.SetParent(pc.transform);
                                transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
                            }

                            //transform.position = pc.transform.position;
                            //lr.SetPosition(0, pc.transform.position);

                            lr.positionCount = 2;
                            lr.enabled = false;
                        }

                        break;
                    
                    case "Grow":

                        break;



                    default:
                        
                        ActivateReticle();
                        lr.positionCount = 2;

                        //int chargeType = 1;

                        if(!pc.isCoolingDown)
                        {
                            lr.enabled = true;

                            SetReticle(1, Vector2.up);
                        } else
                        {
                            lr.enabled = false;
                            DeactivateReticle();
                        }
                        

                        break;
                }
            }
            
            //items that ignore mincharge go here    
            switch(item)
            {
                case "Warp":
                    //START
                    //do this only once
                    if(wr.parent != null)
                    {
                        wr.position = pc.transform.position;
                        wr.SetParent(null);
                    }
                    
                    
                    //DURING
                    if(!pc.isCoolingDown)
                    {
                        warpSR.enabled = true;
                        wr.position +=  warpSpeed * Time.deltaTime * (Vector3)pc.true_i_move;

                        //save warp position
                        WarpBehavior wb = (WarpBehavior)pc.heldItems[pc.selectedItemIdx];
                        wb.warpPoint = wr.position;
                    } else
                    {
                        warpSR.enabled = false;
                        wr.position = pc.transform.position;
                    }

                    break;
                

                
                default:
                    break;
            }


            
        } else //not charging
        {
            //handled in normal charge function:
            //DeactivateReticle();

            //reset parent
            transform.SetParent(pc.transform);
            transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            lr.positionCount = 2;
            lr.SetPosition(0, Vector2.zero);

            //deactivate warp reticle
            warpSR.enabled = false;
            if(item == "Warp")
            {   
                //hide reticle
                wr.gameObject.GetComponent<SpriteRenderer>().enabled = false;
                wr.SetParent(pc.transform);
                wr.localPosition = Vector3.zero;
            }

            





        }
        


    }


    public IEnumerator RenderReticle()
    {
        //Debug.Log("starting RenderReticle()!");
        //Debug.Log("charging: "+ pc.charging);
        //Debug.Log("specialCharging: " + pc.specialCharging);

        //CHANGE 0 TO MINCHARGETIME
        yield return new WaitUntil(() => pc.chargeTime > pc.minCharge || pc.specialChargeTime > pc.minCharge);
        ActivateReticle();
        int chargeType;

        lr.positionCount = 2;


        if(pc.charging)
        {
            //Debug.Log("normal charge rendering!");
            chargeType = 0;
        } else if(pc.specialCharging)
        {
            if(pc.heldItems.Count != 0 && pc.heldItems[pc.selectedItemIdx] != null)
            {
                if(pc.heldItems[pc.selectedItemIdx].GetItemType() == "Wall")
                {
                    chargeType = 2;
                } else
                {
                    chargeType = 1;
                }
            } else
            {
                //Debug.Log("specialcharge Rendering!");
                chargeType = 1;
            }
            
        } else
        {
            chargeType = 0;
        }


        while (pc.charging || pc.specialCharging)
        {

            if(!pc.isCoolingDown /*&& pc.isInBounds*/)
            {
                lr.enabled = true;

                SetReticle(chargeType, Vector2.up);
            } else
            {
                lr.enabled = false;
            }


            yield return null;
        }

        DeactivateReticle();

    }

    //type 1: normal charge, type2: specialCharge
    void SetReticle(int chargeType, Vector2 point)
    {
        reticlePosition.x = point.x;
        reticlePosition.y = point.y;

        float reticleLength = pc.CalcMoveForce(chargeType).magnitude * .65f;

        //Debug.Log("SET RETICLE!!");

        lr.SetPosition(1, reticlePosition * reticleLength);
        //Debug.Log("reticlePosition: " + reticlePosition*reticleLength*.08f);
    }


    public IEnumerator RenderWarpReticle()
    {
        wr.position = pc.transform.position;
        wr.SetParent(null);
        //set reticle visible
        warpSR.enabled = true;

        while (pc.specialCharging)
        {
            if(pc.isCoolingDown)
            {
                warpSR.enabled = false;
                wr.position = pc.transform.position;
            } else
            {
                warpSR.enabled = true;
                wr.position +=  warpSpeed * Time.deltaTime * (Vector3)pc.true_i_move;
            }

            yield return null;
        }
        //save warp position
        WarpBehavior wb = (WarpBehavior)pc.heldItems[pc.selectedItemIdx];

        wb.warpPoint = wr.position;

        //hide reticle
        wr.gameObject.GetComponent<SpriteRenderer>().enabled = false;
        wr.SetParent(pc.transform);
        wr.localPosition = Vector3.zero;
    }


    public IEnumerator RenderRewindReticle()
    {
        //temporarily free reticle from player parent
        transform.SetParent(null);
        transform.position = Vector2.zero;
        transform.rotation = Quaternion.identity;

        bool rCharging = false;

        WaitForFixedUpdate fuwait = new WaitForFixedUpdate();
        //convert queue to stack
        Stack<PlayerState> states = new Stack<PlayerState>(pc.prevStates.ToArray());
        PlayerState ps;
        Vector2 pos = Vector2.zero;

        float chargeTime = 0;

        int tickCount = (int)(chargeTime/pc.maxChargeTime * pc.rewindSize);
        int prevtc = tickCount;

        lr.positionCount = 1;

        while (pc.specialCharging)
        {
            print("tickCount: " + tickCount);
            print("prevtc: " + prevtc);

            if(!pc.isCoolingDown)
            {
                if(rCharging == false)
                {
                    rCharging = true;
                    states.Clear();
                    states = new Stack<PlayerState>(pc.prevStates.ToArray());
                }


                lr.SetPosition(0, pc.transform.position);

                lr.enabled = true;

                if(chargeTime/pc.maxChargeTime < 1 && tickCount != prevtc)
                {
                    prevtc = tickCount;

                    //render reticle stuff here!!!!:
                    ps = states.Pop();
                    pos.x = ps.xPos;
                    pos.y = ps.yPos;

                    lr.positionCount++;
                    lr.SetPosition(lr.positionCount-1, pos);
                }
                

                chargeTime += Time.fixedDeltaTime;

                //SetReticle(chargeType, Vector2.up);
            } else
            {
                //transform.position = pc.transform.position;
                lr.SetPosition(0, pc.transform.position);

                rCharging = false;

                chargeTime = 0;
                lr.positionCount = 1;
                lr.enabled = false;
            }

            tickCount = (int)(chargeTime/pc.maxChargeTime * pc.rewindSize);

            yield return fuwait;
        }

        lr.positionCount = 2;
        lr.enabled = false;

        transform.SetParent(pc.transform);

        lr.SetPosition(0, Vector2.zero);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
    }

}
