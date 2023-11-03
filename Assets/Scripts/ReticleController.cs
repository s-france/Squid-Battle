using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReticleController : MonoBehaviour
{
    PlayerController pc;
    LineRenderer lr;

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

    }


    void ActivateReticle()
    {
        SetReticle(0, Vector2.zero);
        lr.enabled = true;
    }

    void DeactivateReticle()
    {
        SetReticle(0, Vector2.zero);
        lr.enabled = false;
    }

    public void ChangeColor(int colorID)
    {
        lr.material.SetColor("_Color", colorSet[colorID]);
    }


    public IEnumerator RenderReticle()
    {
        Debug.Log("starting RenderReticle()!");
        Debug.Log("charging: "+ pc.charging);
        Debug.Log("specialCharging: " + pc.specialCharging);

        //CHANGE 0 TO MINCHARGETIME
        yield return new WaitUntil(() => pc.chargeTime > 0 || pc.specialChargeTime > 0);
        ActivateReticle();
        int chargeType;

        if(pc.charging)
        {
            Debug.Log("normal charge rendering!");
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
                Debug.Log("specialcharge Rendering!");
                chargeType = 1;
            }
            
        } else
        {
            chargeType = 0;
        }


        while (pc.charging || pc.specialCharging)
        {

            if(!pc.isCoolingDown)
            {
                lr.enabled = true;

                //SetReticle(pc.i_move);
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

        float reticleLength = pc.calcMoveForce(chargeType).magnitude;

        lr.SetPosition(1, reticlePosition * reticleLength * .08f / pc.GetComponent<Rigidbody2D>().mass);
        //Debug.Log("reticlePosition: " + reticlePosition*reticleLength*.08f);
    }

}
