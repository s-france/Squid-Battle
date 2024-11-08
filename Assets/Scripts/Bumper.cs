using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bumper : MonoBehaviour
{
    List<Transform> collisions;

    // Start is called before the first frame update
    void Start()
    {
        collisions = new List<Transform>();
        
    }

    // Update is called once per frame
    void Update()
    {
        
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

}
