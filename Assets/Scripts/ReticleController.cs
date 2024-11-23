using System;
using System.Collections;
using System.Collections.Generic;
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




    public Color32[] colorSet;

    Vector3 reticlePosition;

    // Start is called before the first frame update
    void Start()
    {
        pc = GetComponentInParent<PlayerController>();
        lr = GetComponent<LineRenderer>();
        
        ChangeColor(pc.colorID);

        lr.enabled = false;
        reticlePosition = Vector3.zero;
    }

    // Update is called once per frame
    void Update()
    {
        RenderNormalReticle();

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

    public void RenderNormalReticle()
    {
        if(pc.charging)
        {
            if(pc.chargeTime > pc.minCharge)
            {
                ActivateReticle();
                lr.positionCount = 2;

                //int chargeType = 0;

                if(!pc.isCoolingDown && pc.isInBounds)
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
            DeactivateReticle();
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

            if(!pc.isCoolingDown && pc.isInBounds)
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

        float reticleLength = pc.CalcMoveForce(chargeType).magnitude * .575f;

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
                wr.position += (Vector3)pc.true_i_move * warpSpeed * Time.deltaTime;
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
