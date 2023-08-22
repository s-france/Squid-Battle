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
        SetReticle(Vector2.zero);
        lr.enabled = true;
    }

    void DeactivateReticle()
    {
        SetReticle(Vector2.zero);
        lr.enabled = false;
    }

    public void ChangeColor(int colorID)
    {
        lr.material.SetColor("_Color", colorSet[colorID]);
    }


    public IEnumerator RenderReticle()
    {
        //CHANGE 0 TO MINCHARGETIME
        yield return new WaitUntil(() => pc.chargeTime > 0);
        ActivateReticle();

        while (pc.charging /*&& !pc.isCoolingDown*/)
        {
            if(!pc.isCoolingDown)
            {
                lr.enabled = true;

                //SetReticle(pc.i_move);
                SetReticle(Vector2.up);
            } else
            {
                lr.enabled = false;
            }


            yield return null;
        }

        DeactivateReticle();

    }

    void SetReticle(Vector2 point)
    {
        reticlePosition.x = point.x;
        reticlePosition.y = point.y;

        float reticleLength = pc.calcMoveForce().magnitude;

        lr.SetPosition(1, reticlePosition * reticleLength * .08f);
        //Debug.Log("reticlePosition: " + reticlePosition);
    }

}
