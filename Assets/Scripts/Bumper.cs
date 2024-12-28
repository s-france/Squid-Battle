using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Bumper : MonoBehaviour
{
    public CircleCollider2D TriggerHB;

    List<Transform> collisions;
    [HideInInspector] public List<Vector2> prevPos;

    // Start is called before the first frame update
    void Start()
    {
        collisions = new List<Transform>();

        prevPos = new List<Vector2>(3);
        prevPos.Add(transform.position);
        prevPos.Add(transform.position);
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void FixedUpdate()
    {
        TrackStatesTick();
    }

    //tracks previous states 
    //used in trigger collision correction
    //only really needed if bumper is set to move around
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

    void OnCollisionEnter2D(Collision2D col)
    {
        if (!collisions.Contains(col.transform))
        {
            collisions.Add(col.transform);

            if (LayerMask.LayerToName(col.gameObject.layer) == "Players")
            {
                //Debug.Log("Player" + idx + " Bumped");
                PlayerController pc = col.gameObject.GetComponent<PlayerController>();

                //I literally forgot how PEMDAS works idk how to fix this

                float power = (2*pc.movePower) + (pc.movePower * (1.5f*(pc.moveTime/pc.moveTimer)));

                
                Vector2 direction = transform.position - col.transform.position;

                pc.ApplyMove(1, direction, power);
            }

        }

    }

    void OnCollisionExit2D(Collision2D col)
    {
        StartCoroutine(DelayRemove(col.transform));
        //collisions.Remove(col.transform);
    }

    IEnumerator DelayRemove(Transform t)
    {
        WaitForFixedUpdate fuWait = new WaitForFixedUpdate();
        int count = 0;

        while(count < 2)
        {
            count++;
            yield return fuWait;
        }

        collisions.Remove(t);
    }


    void OnTriggerEnter2D(Collider2D col)
    {

    }

}
